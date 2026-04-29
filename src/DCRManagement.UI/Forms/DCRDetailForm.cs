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

            // Image button visibility
            _btnInsertBefore.Enabled = isEditing;
            _btnClearBefore.Enabled = isEditing;
            _btnInsertAfter.Enabled = isEditing;
            _btnClearAfter.Enabled = isEditing;

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

        // Image handling events
        _btnInsertBefore.Click += (s, e) => InsertImage(_picBefore);
        _btnClearBefore.Click += (s, e) => ClearImage(_picBefore);
        _btnInsertAfter.Click += (s, e) => InsertImage(_picAfter);
        _btnClearAfter.Click += (s, e) => ClearImage(_picAfter);

        _picBefore.DragOver += (s, e) => e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        _picBefore.DragDrop += (s, e) => HandleImageDrop(e, _picBefore);
        _picBefore.KeyDown += (s, e) => HandleImagePaste(e, _picBefore);

        _picAfter.DragOver += (s, e) => e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        _picAfter.DragDrop += (s, e) => HandleImageDrop(e, _picAfter);
        _picAfter.KeyDown += (s, e) => HandleImagePaste(e, _picAfter);

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

    // ─── Image handling ───────────────────────────────────────────────────────

    private void InsertImage(PictureBox targetPicture)
    {
        using (var openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp, *.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files (*.*)|*.*";
            openFileDialog.Title = "Select an image";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var image = System.Drawing.Image.FromFile(openFileDialog.FileName);
                    targetPicture.Image?.Dispose();
                    targetPicture.Image = image;
                    MarkDirty(null, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to load image: {ex.Message}", "Error");
                }
            }
        }
    }

    private void ClearImage(PictureBox targetPicture)
    {
        if (targetPicture.Image != null)
        {
            targetPicture.Image.Dispose();
            targetPicture.Image = null;
            MarkDirty(null, EventArgs.Empty);
        }
    }

    private void HandleImageDrop(DragEventArgs e, PictureBox targetPicture)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                var filePath = files[0];
                var validImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                var extension = Path.GetExtension(filePath).ToLower();

                if (validImageExtensions.Contains(extension))
                {
                    try
                    {
                        var image = System.Drawing.Image.FromFile(filePath);
                        targetPicture.Image?.Dispose();
                        targetPicture.Image = image;
                        MarkDirty(null, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Failed to load image: {ex.Message}", "Error");
                    }
                }
                else
                {
                    ShowError("Please drop a valid image file (jpg, png, bmp, gif)", "Invalid file");
                }
            }
        }
    }

    private void HandleImagePaste(KeyEventArgs e, PictureBox targetPicture)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    targetPicture.Image?.Dispose();
                    targetPicture.Image = image;
                    MarkDirty(null, EventArgs.Empty);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to paste image: {ex.Message}", "Error");
            }
        }
    }
}
