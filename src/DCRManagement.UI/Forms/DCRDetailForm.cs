using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.Domain.Enums;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.Logging;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace DCRManagement.UI.Forms;

public partial class DCRDetailForm : BaseUserControl, IDCRDetailView
{
    private readonly DCRDetailPresenter _presenter;
    private bool _hasUnsavedChanges;

    // Image gallery panels for multiple images support
    private ImageGalleryPanel _galleryBefore;
    private ImageGalleryPanel _galleryAfter;

    public event EventHandler? DataChanged;

    // ─── IDCRDetailView ───────────────────────────────────────────────────────
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
                _dtpTargetDate.Value = value.Value;
            }
            else
            {
                _chkNoTargetDate.Checked = true;
            }
        }
    }

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

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Set SplitterDistance after layout is complete to avoid constraint violations
        if (_splitMain.Width > _splitMain.Panel1MinSize + _splitMain.Panel2MinSize)
        {
            _splitMain.SplitterDistance = 580;
        }
    }

    // ─── Public init (called by MainForm/DCRListForm) ─────────────────────────

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
            _lblDCRNumber.Text = dcrNumber;
            _lblStatusBadge.Text = status;
            _lblStatusBadge.BackColor = ThemeManager.GetStatusColor(status.Replace(" ", ""));
            _lblCreatedBy.Text = $"Created by: {createdBy}";
            _lblCreatedAt.Text = $"On: {createdAt:dd/MM/yyyy HH:mm}";
            Text = $"DCR — {dcrNumber}";
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
            _gridAttachments.DataSource = attachments.ToList();
            _tabAttachments.Text = $"📎  Attachments ({attachments.Count()})";
        });
    }

    public void SetAvailableActions(IEnumerable<ApprovalAction> actions)
    {
        InvokeIfRequired(() =>
        {
            // Clear dynamically-added workflow buttons (keep Save/Edit/Submit/Close)
            var dynamicBtns = _flowActions.Controls
                .OfType<Button>()
                .Where(b => b.Tag is ApprovalAction)
                .ToList();

            foreach (var btn in dynamicBtns)
                _flowActions.Controls.Remove(btn);

            foreach (var action in actions)
            {
                var btn = CreateWorkflowButton(action);
                _flowActions.Controls.Add(btn);
            }
        });
    }

    public void SetMode(DetailMode mode)
    {
        InvokeIfRequired(() =>
        {
            bool isEditing = mode is DetailMode.Create or DetailMode.Edit;
            bool isView = mode == DetailMode.View;

            // Field editability
            _txtTitle.ReadOnly = isView;
            _rtbDescription.ReadOnly = isView;
            _rtbAffectedParts.ReadOnly = isView;
            _rtbReason.ReadOnly = isView;
            _rtbImpactAnalysis.ReadOnly = isView;
            _cmbPriority.Enabled = isEditing;
            _dtpTargetDate.Enabled = isEditing;
            _chkNoTargetDate.Enabled = isEditing;

            // Image gallery editability
            _galleryBefore?.SetReadOnly(isView);
            _galleryAfter?.SetReadOnly(isView);

            // Background tint for read-only fields
            var bg = isView
                ? Color.FromArgb(248, 249, 250)
                : ThemeManager.SurfaceColor;
            _txtTitle.BackColor = bg;
            _rtbDescription.BackColor = bg;
            _rtbAffectedParts.BackColor = bg;
            _rtbReason.BackColor = bg;
            _rtbImpactAnalysis.BackColor = bg;

            // Buttons
            _btnSave.Visible = isEditing;
            _btnEdit.Visible = isView &&
                SessionContext.Instance.CanCreateDCR;
            _btnSubmit.Visible = false;

            _hasUnsavedChanges = false;
        });
    }

    public void SetSubmitVisible(bool visible)
    {
        InvokeIfRequired(() =>
        {
            _btnSubmit.Visible = visible;
        });
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private void ApplyStyling()
    {
        ThemeManager.StylePrimaryButton(_btnSave);
        ThemeManager.StyleSecondaryButton(_btnEdit);
        ThemeManager.StyleSecondaryButton(_btnSubmit);
        ThemeManager.StyleSecondaryButton(_btnClose);
        ThemeManager.StyleSecondaryButton(_btnAddAttachment);
    }

    private void WireEvents()
    {
        _btnSave.Click += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
        _btnEdit.Click += (s, e) => SetMode(DetailMode.Edit);
        _btnSubmit.Click += (s, e) => SubmitRequested?.Invoke(this, EventArgs.Empty);
        _btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

        _btnAddAttachment.Click += (s, e) =>
            AddAttachmentRequested?.Invoke(this, EventArgs.Empty);

        _gridAttachments.CellClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_gridAttachments.Columns[e.ColumnIndex].HeaderText == "" &&
                _gridAttachments.Rows[e.RowIndex].DataBoundItem is AttachmentDto att)
                RemoveAttachmentRequested?.Invoke(this, att.Id);
        };

        // Initialize image galleries
        _galleryBefore = new ImageGalleryPanel();
        _galleryAfter = new ImageGalleryPanel();

        // Replace old picture boxes with galleries in layout
        ReplaceBeforeAfterControls();

        // Gallery event handlers
        _galleryBefore.ImagesChanged += (s, e) => MarkDirty(null, EventArgs.Empty);
        _galleryAfter.ImagesChanged += (s, e) => MarkDirty(null, EventArgs.Empty);

        // Dirty tracking
        _txtTitle.TextChanged += MarkDirty;
        _rtbDescription.TextChanged += MarkDirty;
        _rtbAffectedParts.TextChanged += MarkDirty;
        _rtbReason.TextChanged += MarkDirty;
        _rtbImpactAnalysis.TextChanged += MarkDirty;

        // Toggle date picker
        _chkNoTargetDate.CheckedChanged += (s, e) =>
            _dtpTargetDate.Enabled = !_chkNoTargetDate.Checked;
    }

    /// <summary>Replace old PictureBox controls with ImageGalleryPanel for multiple image support.</summary>
    private void ReplaceBeforeAfterControls()
    {
        if (_pnlBeforeImage == null) return;

        _pnlBeforeImage.Controls.Clear();
        _galleryBefore.Dock = DockStyle.Fill;
        _pnlBeforeImage.Controls.Add(_galleryBefore);

        if (_pnlAfterImage == null) return;

        _pnlAfterImage.Controls.Clear();
        _galleryAfter.Dock = DockStyle.Fill;
        _pnlAfterImage.Controls.Add(_galleryAfter);
    }

    /// <summary>Get all "Before" images as a list of file paths.</summary>
    public IEnumerable<GalleryImage> GetBeforeImages() => _galleryBefore?.GetImages() ?? [];

    /// <summary>Get all "After" images as a list of file paths.</summary>
    public IEnumerable<GalleryImage> GetAfterImages() => _galleryAfter?.GetImages() ?? [];

    private void MarkDirty(object? sender, EventArgs e) =>
        _hasUnsavedChanges = true;

    private Button CreateWorkflowButton(ApprovalAction action)
    {
        var (label, style) = action switch
        {
            ApprovalAction.StartReview => ("▶  Start Review", "secondary"),
            ApprovalAction.SendToApproval => ("📨  Send to Approval", "secondary"),
            ApprovalAction.Approve => ("✅  Approve", "success"),
            ApprovalAction.Reject => ("❌  Reject", "danger"),
            ApprovalAction.Close => ("🔒  Close DCR", "secondary"),
            ApprovalAction.Cancel => ("🚫  Cancel", "danger"),
            _ => (action.ToString(), "secondary")
        };

        var btn = new Button
        {
            Text = label,
            Size = new Size(160, 34),
            Tag = action,
            Margin = new Padding(0, 0, 8, 0)
        };

        switch (style)
        {
            case "success": ThemeManager.StyleSuccessButton(btn); break;
            case "danger": ThemeManager.StyleDangerButton(btn); break;
            default: ThemeManager.StyleSecondaryButton(btn); break;
        }

        btn.Click += (s, e) => WorkflowActionRequested?.Invoke(this, action);
        return btn;
    }
}
