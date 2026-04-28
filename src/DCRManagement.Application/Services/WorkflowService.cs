using DCRManagement.Application.Common;
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCRManagement.Application.Services;

/// <summary>
/// Owns all state-machine logic for the DCR approval workflow.
/// Centralising transitions here means no workflow logic leaks into Forms or DCRService.
///
/// State machine:
///   Draft ──[Submit]──► PendingReview ──[StartReview]──► UnderReview
///     ──[SendToApproval]──► PendingApproval ──[Approve]──► Approved ──[Close]──► Closed
///   Any non-terminal ──[Reject]──► Rejected
///   Any non-terminal ──[Cancel]──► Cancelled
/// </summary>
public class WorkflowService
{
    private readonly IDCRRepository _dcrRepository;
    private readonly IApprovalHistoryRepository _historyRepository;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<WorkflowService> _logger;

    // Defines which roles can perform each action — single source of truth for authorization
    private static readonly Dictionary<ApprovalAction, UserRole[]> ACTION_PERMISSIONS = new()
    {
        { ApprovalAction.Submit,         [UserRole.Engineer, UserRole.Admin] },
        { ApprovalAction.StartReview,    [UserRole.Reviewer, UserRole.Admin] },
        { ApprovalAction.SendToApproval, [UserRole.Reviewer, UserRole.Admin] },
        { ApprovalAction.Approve,        [UserRole.Approver, UserRole.Admin] },
        { ApprovalAction.Reject,         [UserRole.Reviewer, UserRole.Approver, UserRole.Admin] },
        { ApprovalAction.Close,          [UserRole.Admin, UserRole.Approver] },
        { ApprovalAction.Cancel,         [UserRole.Engineer, UserRole.Admin] }
    };

    // Valid (FromStatus → Action) pairs — prevents illegal transitions
    private static readonly HashSet<(DCRStatus, ApprovalAction)> VALID_TRANSITIONS = new()
    {
        (DCRStatus.Draft,            ApprovalAction.Submit),
        (DCRStatus.PendingReview,    ApprovalAction.StartReview),
        (DCRStatus.PendingReview,    ApprovalAction.Reject),
        (DCRStatus.UnderReview,      ApprovalAction.SendToApproval),
        (DCRStatus.UnderReview,      ApprovalAction.Reject),
        (DCRStatus.PendingApproval,  ApprovalAction.Approve),
        (DCRStatus.PendingApproval,  ApprovalAction.Reject),
        (DCRStatus.Approved,         ApprovalAction.Close),
        (DCRStatus.Draft,            ApprovalAction.Cancel),
        (DCRStatus.PendingReview,    ApprovalAction.Cancel),
        (DCRStatus.Rejected,         ApprovalAction.Submit), // Allow re-submission after rejection
    };

    // Maps action → resulting status
    private static readonly Dictionary<ApprovalAction, DCRStatus> ACTION_RESULT_STATUS = new()
    {
        { ApprovalAction.Submit,         DCRStatus.PendingReview },
        { ApprovalAction.StartReview,    DCRStatus.UnderReview },
        { ApprovalAction.SendToApproval, DCRStatus.PendingApproval },
        { ApprovalAction.Approve,        DCRStatus.Approved },
        { ApprovalAction.Reject,         DCRStatus.Rejected },
        { ApprovalAction.Close,          DCRStatus.Closed },
        { ApprovalAction.Cancel,         DCRStatus.Cancelled }
    };

    public WorkflowService(
        IDCRRepository dcrRepository,
        IApprovalHistoryRepository historyRepository,
        IEmailService emailService,
        IUserRepository userRepository,
        ILogger<WorkflowService> logger)
    {
        _dcrRepository = dcrRepository;
        _historyRepository = historyRepository;
        _emailService = emailService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes a workflow action against a DCR, enforcing role-based authorization
    /// and valid state-machine transitions before persisting.
    /// </summary>
    /// <param name="dcrId">Target DCR identifier</param>
    /// <param name="action">The action to perform</param>
    /// <param name="actorId">User performing the action</param>
    /// <param name="comment">Optional comment recorded in history</param>
    public async Task<Result> ExecuteActionAsync(
        int dcrId,
        ApprovalAction action,
        int actorId,
        string? comment = null)
    {
        try
        {
            var dcr = await _dcrRepository.GetByIdAsync(dcrId);
            if (dcr is null)
                return Result.Failure($"DCR with ID {dcrId} not found.", "DCR_NOT_FOUND");

            var actor = await _userRepository.GetByIdAsync(actorId);
            if (actor is null)
                return Result.Failure("Actor user not found.", "USER_NOT_FOUND");

            var authResult = CheckAuthorization(actor.Role, action, dcr, actorId);
            if (!authResult.IsSuccess)
                return authResult;

            var transitionResult = CheckTransition(dcr.Status, action);
            if (!transitionResult.IsSuccess)
                return transitionResult;

            var fromStatus = dcr.Status;
            var toStatus = ACTION_RESULT_STATUS[action];

            dcr.Status = toStatus;
            dcr.UpdatedAt = DateTime.UtcNow;
            dcr.UpdatedById = actorId;

            if (toStatus == DCRStatus.Closed)
                dcr.ActualCompletionDate = DateTime.UtcNow;

            await _dcrRepository.UpdateAsync(dcr);

            await _historyRepository.AddAsync(new ApprovalHistory
            {
                DCRId = dcrId,
                ActorId = actorId,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Comment = comment,
                ActionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedById = actorId
            });

            // Fire-and-forget notifications — failures are logged, not rethrown
            _ = NotifyStakeholdersAsync(dcr, actor.FullName, toStatus, comment);

            _logger.LogInformation(
                "DCR {DCRNumber} transitioned {From} → {To} by {Actor}",
                dcr.DCRNumber, fromStatus, toStatus, actor.Username);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action {Action} on DCR {DCRId}", action, dcrId);
            throw;
        }
    }

    /// <summary>
    /// Returns which actions the given role can perform on a DCR in its current state.
    /// Used by UI to show/hide action buttons dynamically.
    /// </summary>
    public IEnumerable<ApprovalAction> GetAvailableActions(DCRStatus currentStatus, UserRole role, int userId, DCR dcr)
    {
        return VALID_TRANSITIONS
            .Where(t => t.Item1 == currentStatus)
            .Select(t => t.Item2)
            .Where(action =>
            {
                if (!ACTION_PERMISSIONS.TryGetValue(action, out var allowedRoles))
                    return false;

                if (!allowedRoles.Contains(role))
                    return false;

                // Reviewer can only act on DCRs assigned to them (unless Admin)
                if (action is ApprovalAction.StartReview or ApprovalAction.SendToApproval
                    && role == UserRole.Reviewer && dcr.AssignedReviewerId != userId)
                    return false;

                // Approver can only act on DCRs assigned to them (unless Admin)
                if (action is ApprovalAction.Approve or ApprovalAction.Reject
                    && role == UserRole.Approver && dcr.AssignedApproverId != userId)
                    return false;

                return true;
            });
    }

    private static Result CheckAuthorization(UserRole role, ApprovalAction action, DCR dcr, int actorId)
    {
        if (!ACTION_PERMISSIONS.TryGetValue(action, out var allowedRoles))
            return Result.Failure($"Unknown action: {action}", "UNKNOWN_ACTION");

        if (!allowedRoles.Contains(role))
            return Result.Failure(
                $"Your role ({role}) is not permitted to perform '{action}'.",
                "UNAUTHORIZED");

        // Reviewer assignment check
        if (action is ApprovalAction.StartReview or ApprovalAction.SendToApproval
            && role == UserRole.Reviewer
            && dcr.AssignedReviewerId != actorId)
            return Result.Failure("You are not the assigned reviewer for this DCR.", "NOT_ASSIGNED");

        // Approver assignment check
        if (action is ApprovalAction.Approve
            && role == UserRole.Approver
            && dcr.AssignedApproverId != actorId)
            return Result.Failure("You are not the assigned approver for this DCR.", "NOT_ASSIGNED");

        return Result.Success();
    }

    private static Result CheckTransition(DCRStatus current, ApprovalAction action)
    {
        if (!VALID_TRANSITIONS.Contains((current, action)))
            return Result.Failure(
                $"Action '{action}' is not valid when DCR is in '{current}' status.",
                "INVALID_TRANSITION");

        return Result.Success();
    }

    private async Task NotifyStakeholdersAsync(DCR dcr, string actorName, DCRStatus newStatus, string? comment)
    {
        try
        {
            var recipients = new List<(string Email, string Name)>();

            // Collect all stakeholders to notify
            if (dcr.AssignedReviewer is not null)
                recipients.Add((dcr.AssignedReviewer.Email, dcr.AssignedReviewer.FullName));

            if (dcr.AssignedApprover is not null)
                recipients.Add((dcr.AssignedApprover.Email, dcr.AssignedApprover.FullName));

            var creator = await _userRepository.GetByIdAsync(dcr.CreatedById);
            if (creator is not null)
                recipients.Add((creator.Email, creator.FullName));

            // Deduplicate before sending
            foreach (var (email, name) in recipients.DistinctBy(r => r.Email))
            {
                await _emailService.SendDCRStatusChangedAsync(
                    email, name, dcr.DCRNumber, newStatus.ToString(), comment);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notification failed for DCR {DCRNumber} — workflow unaffected", dcr.DCRNumber);
        }
    }
}