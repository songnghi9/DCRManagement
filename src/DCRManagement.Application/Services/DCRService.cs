using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCRManagement.Application.Services;

public class DCRService
{
    private readonly IDCRRepository _dcrRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApprovalHistoryRepository _historyRepository;
    private readonly WorkflowService _workflowService;
    private readonly ILogger<DCRService> _logger;

    public DCRService(
        IDCRRepository dcrRepository,
        IUserRepository userRepository,
        IApprovalHistoryRepository historyRepository,
        WorkflowService workflowService,
        ILogger<DCRService> logger)
    {
        _dcrRepository = dcrRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new DCR in Draft status. DCRNumber is auto-generated.
    /// </summary>
    public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)
    {
        try
        {
            var validationResult = ValidateCreateDto(dto);
            if (!validationResult.IsSuccess)
                return Result<DCRDto>.Failure(validationResult.ErrorMessage!, validationResult.ErrorCode);

            var dcrNumber = await _dcrRepository.GenerateNextDCRNumberAsync();
            var currentUserId = SessionContext.Instance.UserId;

            var dcr = new DCR
            {
                DCRNumber = dcrNumber,
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                AffectedParts = dto.AffectedParts?.Trim(),
                Reason = dto.Reason?.Trim(),
                ImpactAnalysis = dto.ImpactAnalysis?.Trim(),
                Priority = dto.Priority,
                TargetCompletionDate = dto.TargetCompletionDate,
                Status = DCRStatus.Draft,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _dcrRepository.AddAsync(dcr);

            _logger.LogInformation("Created DCR {DCRNumber} by user {UserId}", dcrNumber, currentUserId);

            return await MapToDtoAsync(dcr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DCR");
            throw;
        }
    }

    /// <summary>
    /// Updates editable fields. Only allowed when DCR is in Draft or Rejected status.
    /// </summary>
    public async Task<Result<DCRDto>> UpdateAsync(UpdateDCRDto dto)
    {
        try
        {
            var dcr = await _dcrRepository.GetByIdAsync(dto.Id);
            if (dcr is null)
                return Result<DCRDto>.Failure($"DCR {dto.Id} not found.", "NOT_FOUND");

            if (!new DCRDto(dcr.Id, dcr.DCRNumber, dcr.Title, dcr.Description,
                    dcr.AffectedParts, dcr.Reason, dcr.ImpactAnalysis, dcr.Status,
                    dcr.Priority, dcr.TargetCompletionDate, dcr.ActualCompletionDate,
                    string.Empty, DateTime.MinValue, null, null, [], []).IsEditable)
                return Result<DCRDto>.Failure(
                    $"DCR cannot be edited in '{dcr.Status}' status. Only Draft or Rejected DCRs are editable.",
                    "NOT_EDITABLE");

            // Ownership check — only creator or Admin can edit
            if (dcr.CreatedById != SessionContext.Instance.UserId && !SessionContext.Instance.IsAdmin)
                return Result<DCRDto>.Failure("You can only edit DCRs you created.", "UNAUTHORIZED");

            dcr.Title = dto.Title.Trim();
            dcr.Description = dto.Description.Trim();
            dcr.AffectedParts = dto.AffectedParts?.Trim();
            dcr.Reason = dto.Reason?.Trim();
            dcr.ImpactAnalysis = dto.ImpactAnalysis?.Trim();
            dcr.Priority = dto.Priority;
            dcr.TargetCompletionDate = dto.TargetCompletionDate;
            dcr.UpdatedAt = DateTime.UtcNow;
            dcr.UpdatedById = SessionContext.Instance.UserId;

            await _dcrRepository.UpdateAsync(dcr);

            return await MapToDtoAsync(dcr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DCR {DCRId}", dto.Id);
            throw;
        }
    }

    /// <summary>
    /// Submits a Draft DCR for review, assigning reviewer and approver at submission time.
    /// Delegates state transition to WorkflowService.
    /// </summary>
    public async Task<Result> SubmitAsync(SubmitDCRDto dto)
    {
        try
        {
            var dcr = await _dcrRepository.GetByIdAsync(dto.DCRId);
            if (dcr is null)
                return Result.Failure($"DCR {dto.DCRId} not found.", "NOT_FOUND");

            if (dcr.Status != DCRStatus.Draft && dcr.Status != DCRStatus.Rejected)
                return Result.Failure("Only Draft or Rejected DCRs can be submitted.", "INVALID_STATE");

            var reviewer = await _userRepository.GetByIdAsync(dto.AssignedReviewerId);
            if (reviewer is null || reviewer.Role != UserRole.Reviewer)
                return Result.Failure("Invalid reviewer selected.", "INVALID_REVIEWER");

            var approver = await _userRepository.GetByIdAsync(dto.AssignedApproverId);
            if (approver is null || approver.Role != UserRole.Approver)
                return Result.Failure("Invalid approver selected.", "INVALID_APPROVER");

            dcr.AssignedReviewerId = dto.AssignedReviewerId;
            dcr.AssignedApproverId = dto.AssignedApproverId;
            await _dcrRepository.UpdateAsync(dcr);

            return await _workflowService.ExecuteActionAsync(
                dto.DCRId, ApprovalAction.Submit, SessionContext.Instance.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting DCR {DCRId}", dto.DCRId);
            throw;
        }
    }

    public async Task<Result<DCRDto>> GetByIdAsync(int dcrId)
    {
        try
        {
            var dcr = await _dcrRepository.GetWithFullDetailsAsync(dcrId);
            if (dcr is null)
                return Result<DCRDto>.Failure($"DCR {dcrId} not found.", "NOT_FOUND");

            return await MapToDtoAsync(dcr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching DCR {DCRId}", dcrId);
            throw;
        }
    }

    public async Task<IEnumerable<DCRDto>> GetAllForCurrentUserAsync()
    {
        var session = SessionContext.Instance;

        // Admin sees everything; others see their own + assigned
        var dcrs = session.IsAdmin
            ? await _dcrRepository.GetAllAsync()
            : (await _dcrRepository.GetByCreatorAsync(session.UserId))
              .Concat(await _dcrRepository.GetAssignedToUserAsync(session.UserId))
              .DistinctBy(d => d.Id);

        var tasks = dcrs.Select(MapToDtoAsync);
        return await Task.WhenAll(tasks);
    }

    public async Task<IEnumerable<DCRDto>> GetByStatusAsync(DCRStatus status)
    {
        var dcrs = await _dcrRepository.GetByStatusAsync(status);
        var tasks = dcrs.Select(MapToDtoAsync);
        return await Task.WhenAll(tasks);
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private static Result ValidateCreateDto(CreateDCRDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result.Failure("Title is required.", "VALIDATION_ERROR");

        if (dto.Title.Length > 300)
            return Result.Failure("Title must not exceed 300 characters.", "VALIDATION_ERROR");

        if (string.IsNullOrWhiteSpace(dto.Description))
            return Result.Failure("Description is required.", "VALIDATION_ERROR");

        if (dto.TargetCompletionDate.HasValue && dto.TargetCompletionDate.Value < DateTime.Today)
            return Result.Failure("Target completion date cannot be in the past.", "VALIDATION_ERROR");

        return Result.Success();
    }

    private async Task<DCRDto> MapToDtoAsync(DCR dcr)
    {
        // Load relations if not already eager-loaded
        var creator = await _userRepository.GetByIdAsync(dcr.CreatedById);

        var history = dcr.ApprovalHistories?.Select(h => new ApprovalHistoryDto(
            h.Id, h.DCRId,
            h.Actor?.FullName ?? "Unknown",
            h.Actor?.Role.ToString() ?? string.Empty,
            h.Action, h.FromStatus, h.ToStatus,
            h.Comment, h.ActionDate)) ?? [];

        var attachments = dcr.Attachments?.Select(a => new AttachmentDto(
            a.Id, a.DCRId, a.FileName, a.FilePath, a.FileSizeBytes,
            a.ContentType,
            creator?.FullName ?? "Unknown",
            a.CreatedAt)) ?? [];

        return new DCRDto(
            dcr.Id, dcr.DCRNumber, dcr.Title, dcr.Description,
            dcr.AffectedParts, dcr.Reason, dcr.ImpactAnalysis,
            dcr.Status, dcr.Priority,
            dcr.TargetCompletionDate, dcr.ActualCompletionDate,
            creator?.FullName ?? "Unknown",
            dcr.CreatedAt,
            dcr.AssignedReviewer?.FullName,
            dcr.AssignedApprover?.FullName,
            history, attachments);
    }
}