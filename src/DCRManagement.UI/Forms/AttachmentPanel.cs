using DCRManagement.Application.DTOs;
using DCRManagement.Infrastructure.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Reusable UserControl that owns the attachment grid + add/remove/open actions.
/// Drop it onto any Form that needs attachment management.
/// Replaces the placeholder implementation in Phase 5.
/// </summary>
public partial class AttachmentPanel : UserControl, IAttachmentView
{
    private readonly AttachmentPresenter _presenter;

    private readonly DataGridView _grid = new();
    private readonly Button _btnAdd = new() { Text = "➕  Add File" };
    private readonly Panel _toolbar = new();

    // ─── IAttachmentView ──────────────────────────────────────────────────────
    public event EventHandler? AddRequested;
    public event EventHandler<int>? RemoveRequested;
    public event EventHandler<int>? OpenRequested;

    /// <summary>
    /// Fired when attachments change — parent form subscribes to update tab badge.
    /// </summary>
    public event EventHandler? AttachmentsChanged;

    public AttachmentPanel(
        AttachmentService attachmentService,
        ILogger<AttachmentPresenter> logger)
    {
        _presenter = new AttachmentPresenter(this, attachmentService, logger);
        _presenter.AttachmentsChanged += (s, e) => AttachmentsChanged?.Invoke(this, EventArgs.Empty);

        BuildLayout();
        WireEvents();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void Load(int dcrId, IEnumerable<AttachmentDto> existing) =>
        _presenter.Load(dcrId, existing);

    public void UpdateAttachments(IEnumerable<AttachmentDto> attachments) =>
        _presenter.UpdateAttachments(attachments);

    public void SetReadOnly(bool readOnly)
    {
        _btnAdd.Visible = !readOnly;

        // Hide Remove column in read-only mode
        if (_grid.Columns.Contains("colRemove"))
            _grid.Columns["colRemove"]!.Visible = !readOnly;
    }

    // ─── IView ────────────────────────────────────────────────────────────────
    public void ShowError(string message, string title = "Error") =>
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public void ShowInfo(string message, string title = "Information") =>
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public bool Confirm(string message, string title = "Confirm") =>
        MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            == DialogResult.Yes;

    public void SetBusy(bool isBusy)
    {
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
        _btnAdd.Enabled = !isBusy;
        _grid.Enabled = !isBusy;
    }

    // ─── IAttachmentView ──────────────────────────────────────────────────────
    public void BindAttachments(IEnumerable<AttachmentDto> attachments)
    {
        if (InvokeRequired) { Invoke(() => BindAttachments(attachments)); return; }
        _grid.DataSource = attachments.ToList();
    }

    public void UpdateTabBadge(int count)
    {
        // Parent form may subscribe to AttachmentsChanged to update its own tab text
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private void BuildLayout()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        // Toolbar
        _toolbar.Dock = DockStyle.Top;
        _toolbar.Height = 44;
        _toolbar.Padding = new Padding(8, 6, 8, 6);
        _toolbar.BackColor = Color.White;

        ThemeManager.StyleSecondaryButton(_btnAdd);
        _btnAdd.Size = new Size(110, 30);
        _btnAdd.Location = new Point(8, 7);
        _toolbar.Controls.Add(_btnAdd);

        // Grid
        _grid.Dock = DockStyle.Fill;
        ThemeManager.StyleDataGrid(_grid);
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                Name = "colFileName",
                HeaderText = "File Name",
                DataPropertyName = "FileName",
                FillWeight = 50
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colSize",
                HeaderText = "Size",
                DataPropertyName = "FileSizeDisplay",
                Width = 80
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = "Type",
                DataPropertyName = "ContentType",
                Width = 130
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colUploadedBy",
                HeaderText = "Uploaded By",
                DataPropertyName = "UploadedBy",
                Width = 140
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "Date",
                DataPropertyName = "UploadedAt",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            },
            new DataGridViewButtonColumn
            {
                Name = "colOpen",
                HeaderText = "",
                Text = "📂 Open",
                UseColumnTextForButtonValue = true,
                Width = 80
            },
            new DataGridViewButtonColumn
            {
                Name = "colRemove",
                HeaderText = "",
                Text = "🗑 Remove",
                UseColumnTextForButtonValue = true,
                Width = 90
            }
        );

        Controls.Add(_grid);
        Controls.Add(_toolbar);
    }

    private void WireEvents()
    {
        _btnAdd.Click += (s, e) => AddRequested?.Invoke(this, EventArgs.Empty);

        _grid.CellClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is not AttachmentDto att) return;

            switch (_grid.Columns[e.ColumnIndex].Name)
            {
                case "colOpen":
                    OpenRequested?.Invoke(this, att.Id);
                    break;
                case "colRemove":
                    RemoveRequested?.Invoke(this, att.Id);
                    break;
            }
        };

        // Double-click row → open
        _grid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is AttachmentDto att)
                OpenRequested?.Invoke(this, att.Id);
        };
    }
}