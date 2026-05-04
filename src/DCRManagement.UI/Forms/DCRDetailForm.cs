using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.Domain.Enums;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Windows.Forms;

namespace DCRManagement.UI.Forms;

public partial class DCRDetailForm : BaseUserControl, IDCRDetailView
{
    private readonly DCRDetailPresenter _presenter;
    private bool _hasUnsavedChanges;

    public event EventHandler? DataChanged;

    // ─── IDCRDetailView — field bindings ──────────────────────────────────────

    /// <summary>Maps to Drawing no. field (_txtTitle). Presenter uses DCRTitle for this.</summary>
    public string DCRTitle
    {
        get => _txtTitle.Text.Trim();
        set => _txtTitle.Text = value;
    }

    public string Description
    {
        get => _rtbDescription.Text.Trim();
        set => _rtbDescription.Text = value;
    }

    /// <summary>Hidden backing field — presenter sets AffectedParts separately.</summary>
    public string AffectedParts
    {
        get => _rtbAffectedParts.Text.Trim();
        set => _rtbAffectedParts.Text = value;
    }

    public string Reason
    {
        get => _rtbReason.Text.Trim();
        set => _rtbReason.Text = value;
    }

    public string ImpactAnalysis
    {
        get => _rtbImpactAnalysis.Text.Trim();
        set => _rtbImpactAnalysis.Text = value;
    }

    public string Priority
    {
        get => _cmbPriority.SelectedItem?.ToString() ?? string.Empty;
        set
        {
            var idx = _cmbPriority.Items.IndexOf(value);
            _cmbPriority.SelectedIndex = idx >= 0 ? idx : 0;
        }
    }

    public DateTime? TargetDate
    {
        get => _chkNoTargetDate.Checked ? null : _dtpTargetDate.Value;
        set
        {
            if (value.HasValue)
            {
                _chkNoTargetDate.Checked = false;
                _dtpTargetDate.Value     = value.Value;
            }
            else
            {
                _chkNoTargetDate.Checked = true;
            }
        }
    }

    // ─── IDCRDetailView — events ──────────────────────────────────────────────
    public event EventHandler? SaveRequested;
    public event EventHandler? SubmitRequested;
    public event EventHandler<ApprovalAction>? WorkflowActionRequested;
    public event EventHandler? AddAttachmentRequested;
    public event EventHandler<int>? RemoveAttachmentRequested;
    public event EventHandler? CloseRequested;

    // ─── Constructor ──────────────────────────────────────────────────────────
    public DCRDetailForm(
        DCRService dcrService,
        WorkflowService workflowService,
        UserService userService,
        ILogger<DCRDetailPresenter> logger)
    {
        InitializeComponent();
        ApplyStyling();

        _presenter = new DCRDetailPresenter(this, dcrService, workflowService, userService, logger);
        _presenter.DataChanged += (s, e) => DataChanged?.Invoke(this, EventArgs.Empty);

        WireEvents();
    }

    // ─── Public init ──────────────────────────────────────────────────────────

    public void OpenCreate()
    {
        Text = "New DCR";
        _presenter.InitCreate();
    }

    public async Task OpenViewAsync(int dcrId)
    {
        Text = "DCR Detail — View";
        await RunAsync(() => _presenter.InitViewAsync(dcrId), "load DCR");
    }

    public async Task OpenEditAsync(int dcrId)
    {
        Text = "DCR Detail — Edit";
        await RunAsync(() => _presenter.InitEditAsync(dcrId), "load DCR for editing");
    }

    // ─── IDCRDetailView implementation ────────────────────────────────────────

    public void SetHeaderInfo(string dcrNumber, string status, string createdBy, DateTime createdAt)
    {
        InvokeIfRequired(() =>
        {
            // Topbar: large DCR number
            _lblDCRNumber.Text  = dcrNumber;
            Text                = $"DCR — {dcrNumber}";

            // Status badge
            _lblStatusBadge.Text      = status;
            _lblStatusBadge.BackColor = ThemeManager.GetStatusColor(status.Replace(" ", ""));

            // Hint text
            _lblHint.Text = $"Created by: {createdBy}  |  {createdAt:dd/MM/yyyy}";

            // Form strip read-only labels
            _lblDCRNumber2.Text = dcrNumber;
            _lblCreatedBy.Text  = createdBy;
            _lblCreatedAt.Text  = createdAt.ToString("dd/MM/yyyy");
        });
    }

    public void SetWorkflowHistory(IEnumerable<ApprovalHistoryDto> history)
    {
        InvokeIfRequired(() =>
        {
            _gridHistory.DataSource = history.ToList();
        });
    }

    public void SetAttachments(IEnumerable<AttachmentDto> attachments)
    {
        InvokeIfRequired(() =>
        {
            _gridAttachments.DataSource  = attachments.ToList();
            _tabAttachments.Text         = $"Attachments ({attachments.Count()})";
        });
    }

    public void SetAvailableActions(IEnumerable<ApprovalAction> actions)
    {
        InvokeIfRequired(() =>
        {
            // Remove previously added dynamic workflow buttons
            var dynamicBtns = _flowActions.Controls
                .OfType<Button>()
                .Where(b => b.Tag is ApprovalAction)
                .ToList();
            foreach (var btn in dynamicBtns)
                _flowActions.Controls.Remove(btn);

            foreach (var action in actions)
                _flowActions.Controls.Add(CreateWorkflowButton(action));
        });
    }

    public void SetMode(DetailMode mode)
    {
        InvokeIfRequired(() =>
        {
            bool isEditing = mode is DetailMode.Create or DetailMode.Edit;
            bool isView    = mode == DetailMode.View;

            // Field editability
            _txtTitle.ReadOnly          = isView;
            _txtMachine.ReadOnly        = isView;
            _rtbDescription.ReadOnly    = isView;
            _rtbAffectedParts.ReadOnly  = isView;
            _rtbReason.ReadOnly         = isView;
            _rtbImpactAnalysis.ReadOnly = isView;
            _rtbLeadTimeNote.ReadOnly   = isView;
            _rtbSafetyNote.ReadOnly     = isView;
            _rtbComplianceNote.ReadOnly = isView;
            _rtbDecisionComment.ReadOnly = isView;
            _cmbPriority.Enabled        = isEditing;
            _dtpTargetDate.Enabled      = isEditing;
            _chkNoTargetDate.Enabled    = isEditing;
            _cmbLeadTimeRisk.Enabled    = isEditing;
            _cmbSafetyRisk.Enabled      = isEditing;
            _cmbComplianceRisk.Enabled  = isEditing;
            _txtSaving.ReadOnly         = isView;
            _txtCostToChange.ReadOnly   = isView;
            _cmbSavingCurrency.Enabled  = isEditing;
            _cmbCostCurrency.Enabled    = isEditing;
            _cmbConsultant.Enabled      = isEditing;
            _cmbConsultantDecision.Enabled = isEditing;
            _dtpDecisionDate.Enabled    = isEditing;
            _cmbDCRDecision.Enabled     = isEditing;

            // Gallery editability
            _galleryBefore.Enabled = isEditing;
            _galleryAfter.Enabled  = isEditing;

            // Background tint for read-only text fields
            var bg = isView ? Color.FromArgb(248, 249, 250) : Color.FromArgb(247, 249, 252);
            _txtTitle.BackColor          = bg;
            _txtMachine.BackColor        = bg;
            _rtbDescription.BackColor    = bg;
            _rtbReason.BackColor         = bg;
            _rtbImpactAnalysis.BackColor = bg;
            _rtbLeadTimeNote.BackColor   = bg;
            _rtbSafetyNote.BackColor     = bg;
            _rtbComplianceNote.BackColor = bg;
            _rtbDecisionComment.BackColor = bg;

            // Topbar buttons
            _btnSave.Visible     = isEditing;
            _btnSaveSide.Visible = isEditing;
            _btnEdit.Visible     = isView && SessionContext.Instance.CanCreateDCR;
            _btnSubmit.Visible   = false;   // controlled by SetSubmitVisible

            _hasUnsavedChanges = false;
        });
    }

    public void SetSubmitVisible(bool visible)
    {
        InvokeIfRequired(() =>
        {
            _btnSubmit.Visible     = visible;
            _btnSubmitSide.Visible = visible;
        });
    }

    // ─── Approval log toggle ──────────────────────────────────────────────────

    private bool _logExpanded = false;

    private void ToggleApprovalLog()
    {
        _logExpanded = !_logExpanded;
        _pnlApprovalLog.Height = _logExpanded ? 260 : 44;
        _gridHistory.Visible   = _logExpanded;
        _btnToggleLog.Text     = _logExpanded ? "▲ Collapse" : "▼ Expand";
    }

    // ─── Styling ──────────────────────────────────────────────────────────────

    private void ApplyStyling()
    {
        // Primary buttons
        ThemeManager.StylePrimaryButton(_btnSubmit);
        ThemeManager.StylePrimaryButton(_btnSubmitSide);

        // Secondary buttons
        ThemeManager.StyleSecondaryButton(_btnSave);
        ThemeManager.StyleSecondaryButton(_btnSaveSide);
        ThemeManager.StyleSecondaryButton(_btnEdit);
        ThemeManager.StyleSecondaryButton(_btnClose);
        ThemeManager.StyleSecondaryButton(_btnAddAttachment);

        // Status badge pill appearance
        _lblStatusBadge.Padding = new Padding(8, 2, 8, 2);
    }

    // ─── Event wiring ─────────────────────────────────────────────────────────

    private void WireEvents()
    {
        _btnSave.Click         += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
        _btnSaveSide.Click     += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
        _btnEdit.Click         += (s, e) => SetMode(DetailMode.Edit);
        _btnSubmit.Click       += (s, e) => SubmitRequested?.Invoke(this, EventArgs.Empty);
        _btnSubmitSide.Click   += (s, e) => SubmitRequested?.Invoke(this, EventArgs.Empty);
        _btnClose.Click        += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
        _btnToggleLog.Click    += (s, e) => ToggleApprovalLog();
        _btnAddAttachment.Click += (s, e) => AddAttachmentRequested?.Invoke(this, EventArgs.Empty);

        _gridAttachments.CellClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_gridAttachments.Columns[e.ColumnIndex].HeaderText == "" &&
                _gridAttachments.Rows[e.RowIndex].DataBoundItem is AttachmentDto att)
                RemoveAttachmentRequested?.Invoke(this, att.Id);
        };

        // Gallery change tracking
        _galleryBefore.ImageAdded   += (s, e) => MarkDirty(null, EventArgs.Empty);
        _galleryBefore.ImageRemoved += (s, e) => MarkDirty(null, EventArgs.Empty);
        _galleryAfter.ImageAdded    += (s, e) => MarkDirty(null, EventArgs.Empty);
        _galleryAfter.ImageRemoved  += (s, e) => MarkDirty(null, EventArgs.Empty);

        // Dirty tracking for text fields
        _txtTitle.TextChanged          += MarkDirty;
        _txtMachine.TextChanged        += MarkDirty;
        _rtbDescription.TextChanged    += MarkDirty;
        _rtbAffectedParts.TextChanged  += MarkDirty;
        _rtbReason.TextChanged         += MarkDirty;
        _rtbImpactAnalysis.TextChanged += MarkDirty;

        // Toggle date picker
        _chkNoTargetDate.CheckedChanged += (s, e) =>
            _dtpTargetDate.Enabled = !_chkNoTargetDate.Checked;
    }

    private void MarkDirty(object? sender, EventArgs e) =>
        _hasUnsavedChanges = true;

    // ─── Dynamic workflow buttons ─────────────────────────────────────────────

    private Button CreateWorkflowButton(ApprovalAction action)
    {
        var (label, style) = action switch
        {
            ApprovalAction.StartReview    => ("▶  Start Review",      "secondary"),
            ApprovalAction.SendToApproval => ("📨  Send to Approval", "secondary"),
            ApprovalAction.Approve        => ("✅  Approve",          "success"),
            ApprovalAction.Reject         => ("❌  Reject",           "danger"),
            ApprovalAction.Close          => ("🔒  Close DCR",        "secondary"),
            ApprovalAction.Cancel         => ("🚫  Cancel",           "danger"),
            _                             => (action.ToString(),       "secondary")
        };

        var btn = new Button
        {
            Text     = label,
            Height   = 34,
            AutoSize = true,
            Padding  = new Padding(12, 0, 12, 0),
            Tag      = action,
            Margin   = new Padding(0, 0, 8, 0),
            Cursor   = Cursors.Hand
        };

        switch (style)
        {
            case "success": ThemeManager.StyleSuccessButton(btn); break;
            case "danger":  ThemeManager.StyleDangerButton(btn);  break;
            default:        ThemeManager.StyleSecondaryButton(btn); break;
        }

        btn.Click += (s, e) => WorkflowActionRequested?.Invoke(this, action);
        return btn;
    }

    // ─── Image gallery methods ────────────────────────────────────────────────

    public List<System.Drawing.Image> GetBeforeImages() => _galleryBefore.GetImages();
    public List<System.Drawing.Image> GetAfterImages()  => _galleryAfter.GetImages();

    public (int Width, int Height) GetBeforeThumbnailSize() => _galleryBefore.GetCurrentSize();
    public (int Width, int Height) GetAfterThumbnailSize()  => _galleryAfter.GetCurrentSize();

    public void SetBeforeThumbnailSize(int width, int height)
    {
        InvokeIfRequired(() => _galleryBefore.SetSize(width, height));
    }

    public void SetAfterThumbnailSize(int width, int height)
    {
        InvokeIfRequired(() => _galleryAfter.SetSize(width, height));
    }

    public void SetBeforeImages(IEnumerable<System.Drawing.Image> images)
    {
        InvokeIfRequired(() =>
        {
            _galleryBefore.ClearImages();
            foreach (var img in images) _galleryBefore.AddImage(img);
        });
    }

    public void SetAfterImages(IEnumerable<System.Drawing.Image> images)
    {
        InvokeIfRequired(() =>
        {
            _galleryAfter.ClearImages();
            foreach (var img in images) _galleryAfter.AddImage(img);
        });
    }

    public void ClearBeforeImages() => _galleryBefore.ClearImages();
    public void ClearAfterImages()  => _galleryAfter.ClearImages();

    public void AddBeforeImage(System.Drawing.Image image) => _galleryBefore.AddImage(image);
    public void AddAfterImage(System.Drawing.Image image)  => _galleryAfter.AddImage(image);
}

