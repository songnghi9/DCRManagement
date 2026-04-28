using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.Domain.Enums;
using DCRManagement.UI.Common;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace DCRManagement.UI.Presenters;

public enum DetailMode { Create, Edit, View }

public interface IDCRDetailView : IView
{
    // ── Field bindings ────────────────────────────────────────────────────────
    string DCRTitle { get; set; }
    string Description { get; set; }
    string AffectedParts { get; set; }
    string Reason { get; set; }
    string ImpactAnalysis { get; set; }
    string Priority { get; set; }
    DateTime? TargetDate { get; set; }

    // ── Display-only ──────────────────────────────────────────────────────────
    void SetHeaderInfo(string dcrNumber, string status, string createdBy, DateTime createdAt);
    void SetWorkflowHistory(IEnumerable<ApprovalHistoryDto> history);
    void SetAttachments(IEnumerable<AttachmentDto> attachments);
    void SetAvailableActions(IEnumerable<ApprovalAction> actions);
    void SetMode(DetailMode mode);

    // ── Events ────────────────────────────────────────────────────────────────
    event EventHandler SaveRequested;
    event EventHandler SubmitRequested;
    event EventHandler<ApprovalAction> WorkflowActionRequested;
    event EventHandler AddAttachmentRequested;
    event EventHandler<int> RemoveAttachmentRequested;
    event EventHandler CloseRequested;
}

public class DCRDetailPresenter
{
    private readonly IDCRDetailView _view;
    private readonly DCRService _dcrService;
    private readonly WorkflowService _workflowService;
    private readonly UserService _userService;
    private readonly ILogger<DCRDetailPresenter> _logger;

    private DCRDto? _currentDcr;
    private DetailMode _mode;

    /// <summary>Raised after any successful mutation so the list can refresh.</summary>
    public event EventHandler? DataChanged;

    public DCRDetailPresenter(
        IDCRDetailView view,
        DCRService dcrService,
        WorkflowService workflowService,
        UserService userService,
        ILogger<DCRDetailPresenter> logger)
    {
        _view = view;
        _dcrService = dcrService;
        _workflowService = workflowService;
        _userService = userService;
        _logger = logger;

        _view.SaveRequested += async (s, e) => await OnSaveAsync();
        _view.SubmitRequested += async (s, e) => await OnSubmitAsync();
        _view.WorkflowActionRequested += async (s, action) => await OnWorkflowActionAsync(action);
        _view.AddAttachmentRequested += async (s, e) => await OnAddAttachmentAsync();
        _view.RemoveAttachmentRequested += async (s, id) => await OnRemoveAttachmentAsync(id);
        _view.CloseRequested += (s, e) => { /* form handles close */ };
    }

    // ─── Public init methods ──────────────────────────────────────────────────

    public void InitCreate()
    {
        _mode = DetailMode.Create;
        _view.SetMode(DetailMode.Create);
        _view.SetHeaderInfo("(new)", "Draft", SessionContext.Instance.FullName, DateTime.Now);
        _view.SetWorkflowHistory([]);
        _view.SetAttachments([]);
        _view.SetAvailableActions([]);
    }

    public async Task InitViewAsync(int dcrId)
    {
        _mode = DetailMode.View;
        await LoadDcrAsync(dcrId);
        _view.SetMode(DetailMode.View);
    }

    public async Task InitEditAsync(int dcrId)
    {
        _mode = DetailMode.Edit;
        await LoadDcrAsync(dcrId);
        _view.SetMode(DetailMode.Edit);
    }

    // ─── Private handlers ─────────────────────────────────────────────────────

    private async Task OnSaveAsync()
    {
        if (!ValidateFields()) return;

        _view.SetBusy(true);
        try
        {
            Result<DCRDto> result;

            if (_mode == DetailMode.Create)
            {
                result = await _dcrService.CreateAsync(new CreateDCRDto(
                    _view.DCRTitle,
                    _view.Description,
                    NullIfEmpty(_view.AffectedParts),
                    NullIfEmpty(_view.Reason),
                    NullIfEmpty(_view.ImpactAnalysis),
                    NullIfEmpty(_view.Priority),
                    _view.TargetDate));
            }
            else
            {
                result = await _dcrService.UpdateAsync(new UpdateDCRDto(
                    _currentDcr!.Id,
                    _view.DCRTitle,
                    _view.Description,
                    NullIfEmpty(_view.AffectedParts),
                    NullIfEmpty(_view.Reason),
                    NullIfEmpty(_view.ImpactAnalysis),
                    NullIfEmpty(_view.Priority),
                    _view.TargetDate));
            }

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Save Failed");
                return;
            }

            _currentDcr = result.Value;
            _view.SetHeaderInfo(
                _currentDcr!.DCRNumber,
                _currentDcr.StatusDisplay,
                _currentDcr.CreatedBy,
                _currentDcr.CreatedAt);

            _view.ShowInfo(
                _mode == DetailMode.Create
                    ? $"DCR {_currentDcr.DCRNumber} created successfully."
                    : "DCR updated successfully.",
                "Saved");

            // Switch to view mode after save
            _mode = DetailMode.View;
            _view.SetMode(DetailMode.View);
            RefreshWorkflowButtons();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    private async Task OnSubmitAsync()
    {
        if (_currentDcr is null)
        {
            _view.ShowError("Please save the DCR before submitting.", "Not Saved");
            return;
        }

        // Show dialog to pick reviewer + approver
        var reviewers = (await _userService.GetReviewersAsync()).ToList();
        var approvers = (await _userService.GetApproversAsync()).ToList();

        if (!reviewers.Any())
        {
            _view.ShowError("No active Reviewer users found. Ask Admin to create one.", "No Reviewers");
            return;
        }
        if (!approvers.Any())
        {
            _view.ShowError("No active Approver users found. Ask Admin to create one.", "No Approvers");
            return;
        }

        using var dialog = new SubmitDCRDialog(reviewers, approvers);
        if (dialog.ShowDialog() != DialogResult.OK) return;

        _view.SetBusy(true);
        try
        {
            var result = await _dcrService.SubmitAsync(new SubmitDCRDto(
                _currentDcr.Id,
                dialog.SelectedReviewerId,
                dialog.SelectedApproverId));

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Submit Failed");
                return;
            }

            _view.ShowInfo("DCR submitted for review successfully.", "Submitted");
            await LoadDcrAsync(_currentDcr.Id);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    private async Task OnWorkflowActionAsync(ApprovalAction action)
    {
        if (_currentDcr is null) return;

        var actionLabel = action switch
        {
            ApprovalAction.Approve => "approve",
            ApprovalAction.Reject => "reject",
            ApprovalAction.StartReview => "start review on",
            ApprovalAction.SendToApproval => "send for approval",
            ApprovalAction.Close => "close",
            ApprovalAction.Cancel => "cancel",
            _ => action.ToString().ToLower()
        };

        // Reject and Cancel require a mandatory comment
        string? comment = null;
        if (action is ApprovalAction.Reject or ApprovalAction.Cancel)
        {
            comment = PromptComment($"Please enter a reason for {actionLabel}:", required: true);
            if (comment is null) return; // user cancelled dialog
        }
        else
        {
            if (!_view.Confirm($"Are you sure you want to {actionLabel} DCR {_currentDcr.DCRNumber}?",
                               "Confirm Action"))
                return;

            comment = PromptComment("Optional comment:", required: false);
        }

        _view.SetBusy(true);
        try
        {
            var result = await _workflowService.ExecuteActionAsync(
                _currentDcr.Id, action, SessionContext.Instance.UserId, comment);

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Action Failed");
                return;
            }

            _view.ShowInfo($"Action '{actionLabel}' completed successfully.", "Done");
            await LoadDcrAsync(_currentDcr.Id);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    private async Task OnAddAttachmentAsync()
    {
        if (_currentDcr is null)
        {
            _view.ShowError("Please save the DCR before adding attachments.", "Not Saved");
            return;
        }
        // Phase 6 — AttachmentPanel
        _view.ShowInfo("Attachment upload will be available in the next update.", "Coming Soon");
        await Task.CompletedTask;
    }

    private async Task OnRemoveAttachmentAsync(int attachmentId)
    {
        if (!_view.Confirm("Remove this attachment?", "Confirm")) return;
        // Phase 6
        await Task.CompletedTask;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task LoadDcrAsync(int dcrId)
    {
        var result = await _dcrService.GetByIdAsync(dcrId);
        if (!result.IsSuccess)
        {
            _view.ShowError(result.ErrorMessage!, "Load Error");
            return;
        }

        _currentDcr = result.Value!;

        _view.DCRTitle = _currentDcr.Title;
        _view.Description = _currentDcr.Description;
        _view.AffectedParts = _currentDcr.AffectedParts ?? string.Empty;
        _view.Reason = _currentDcr.Reason ?? string.Empty;
        _view.ImpactAnalysis = _currentDcr.ImpactAnalysis ?? string.Empty;
        _view.Priority = _currentDcr.Priority ?? string.Empty;
        _view.TargetDate = _currentDcr.TargetCompletionDate;

        _view.SetHeaderInfo(
            _currentDcr.DCRNumber,
            _currentDcr.StatusDisplay,
            _currentDcr.CreatedBy,
            _currentDcr.CreatedAt);

        _view.SetWorkflowHistory(_currentDcr.History);
        _view.SetAttachments(_currentDcr.Attachments);
        RefreshWorkflowButtons();
    }

    private void RefreshWorkflowButtons()
    {
        if (_currentDcr is null) return;

        var session = SessionContext.Instance;
        // Need DCR entity for assignment checks — rebuild lightweight check from DTO
        // WorkflowService.GetAvailableActions needs DCR entity; call it via domain data in DTO
        var availableActions = GetAvailableActionsForCurrentUser(_currentDcr, session);
        _view.SetAvailableActions(availableActions);
    }

    private static IEnumerable<ApprovalAction> GetAvailableActionsForCurrentUser(
        DCRDto dcr, SessionContext session)
    {
        // Mirror WorkflowService logic using DTO data (avoids extra DB call)
        var role = session.Role;
        var userId = session.UserId;
        var status = dcr.Status;

        var candidates = new List<ApprovalAction>();

        if (status == DCRStatus.Draft || status == DCRStatus.Rejected)
        {
            if (role is UserRole.Engineer or UserRole.Admin)
                candidates.Add(ApprovalAction.Submit);
            if (role is UserRole.Engineer or UserRole.Admin)
                candidates.Add(ApprovalAction.Cancel);
        }
        if (status == DCRStatus.PendingReview)
        {
            if (role is UserRole.Reviewer or UserRole.Admin)
                candidates.Add(ApprovalAction.StartReview);
            if (role is UserRole.Reviewer or UserRole.Approver or UserRole.Admin)
                candidates.Add(ApprovalAction.Reject);
            if (role is UserRole.Engineer or UserRole.Admin)
                candidates.Add(ApprovalAction.Cancel);
        }
        if (status == DCRStatus.UnderReview)
        {
            if (role is UserRole.Reviewer or UserRole.Admin)
                candidates.Add(ApprovalAction.SendToApproval);
            if (role is UserRole.Reviewer or UserRole.Approver or UserRole.Admin)
                candidates.Add(ApprovalAction.Reject);
        }
        if (status == DCRStatus.PendingApproval)
        {
            if (role is UserRole.Approver or UserRole.Admin)
                candidates.Add(ApprovalAction.Approve);
            if (role is UserRole.Approver or UserRole.Admin)
                candidates.Add(ApprovalAction.Reject);
        }
        if (status == DCRStatus.Approved)
        {
            if (role is UserRole.Admin or UserRole.Approver)
                candidates.Add(ApprovalAction.Close);
        }

        return candidates;
    }

    private bool ValidateFields()
    {
        if (string.IsNullOrWhiteSpace(_view.DCRTitle))
        {
            _view.ShowError("Title is required.", "Validation");
            return false;
        }
        if (_view.DCRTitle.Length > 300)
        {
            _view.ShowError("Title must not exceed 300 characters.", "Validation");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_view.Description))
        {
            _view.ShowError("Description is required.", "Validation");
            return false;
        }
        if (_view.TargetDate.HasValue && _view.TargetDate.Value < DateTi