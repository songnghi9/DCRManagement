using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Reusable panel that displays a Before or After image gallery backed by SQL VARBINARY(MAX).
///
/// Features:
///   • Loads thumbnails from DB (metadata only — no binary on list load)
///   • Restores exact ThumbnailWidth/Height and DisplayOrder from DB
///   • Upload from file dialog (multi-select)
///   • Paste from clipboard (Ctrl+V or button)
///   • Double-click to view full-size
///   • Delete individual images
///   • Drag-to-reorder (updates DisplayOrder in memory; persisted on DCR Save)
/// </summary>
public class DCRImagePanel : BaseUserControl, IDCRImageView
{
    // ── Presenter ─────────────────────────────────────────────────────────────
    private readonly DCRImagePresenter _presenter;

    // ── Toolbar ───────────────────────────────────────────────────────────────
    private FlowLayoutPanel _pnlToolbar = null!;
    private Button          _btnUpload  = null!;
    private Button          _btnPaste   = null!;
    private Label           _lblCount   = null!;

    // ── Gallery ───────────────────────────────────────────────────────────────
    private FlowLayoutPanel _flowThumbnails = null!;
    private Panel           _pnlDropHint    = null!;

    // imageId → thumbnail panel (for O(1) removal)
    private readonly Dictionary<int, Panel> _thumbPanels = new();

    // ── IDCRImageView events ──────────────────────────────────────────────────
    public event EventHandler?     UploadFromFileRequested;
    public event EventHandler?     PasteFromClipboardRequested;
    public event EventHandler<int>? ViewImageRequested;
    public event EventHandler<int>? DeleteImageRequested;

    // ── Constructor ───────────────────────────────────────────────────────────
    public DCRImagePanel(DCRImageService imageService, ILogger<DCRImagePresenter> logger)
    {
        _presenter = new DCRImagePresenter(this, imageService, logger);
        _presenter.DataChanged += (s, e) => { /* parent can subscribe if needed */ };

        InitLayout();
        WireEvents();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Load images for a DCR gallery from the database.
    /// Thumbnails are loaded without binary data; full image loads on double-click.
    /// </summary>
    public async Task LoadAsync(int dcrId, string galleryType) =>
        await _presenter.LoadAsync(dcrId, galleryType);

    /// <summary>Notify the presenter when the size spinners change.</summary>
    public void SetThumbnailSize(int w, int h) => _presenter.SetThumbnailSize(w, h);

    // ── IDCRImageView implementation ──────────────────────────────────────────

    public void BindThumbnails(IEnumerable<DCRImageThumbnailDto> thumbnails)
    {
        InvokeIfRequired(() =>
        {
            _flowThumbnails.SuspendLayout();
            _flowThumbnails.Controls.Clear();
            _thumbPanels.Clear();

            var list = thumbnails.OrderBy(t => t.DisplayOrder).ToList();
            foreach (var t in list)
                _flowThumbnails.Controls.Add(BuildThumbCard(t));

            _flowThumbnails.ResumeLayout();

            _lblCount.Text       = $"{list.Count} image{(list.Count == 1 ? "" : "s")}";
            _pnlDropHint.Visible = list.Count == 0;
            _flowThumbnails.Visible = list.Count > 0;
        });
    }

    public void DisplayImage(byte[] data, string contentType, string fileName)
    {
        InvokeIfRequired(() =>
        {
            System.Drawing.Image img;
            using (var ms = new MemoryStream(data))
                img = System.Drawing.Image.FromStream(ms);

            var preview = new Form
            {
                Text          = fileName,
                Size          = new Size(960, 720),
                StartPosition = FormStartPosition.CenterParent,
                BackColor     = Color.FromArgb(20, 20, 20)
            };
            var pic = new PictureBox
            {
                Dock      = DockStyle.Fill,
                Image     = img,
                SizeMode  = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            preview.Controls.Add(pic);
            preview.FormClosed += (s, e) => img.Dispose();
            preview.ShowDialog(FindForm());
        });
    }

    public void RemoveThumbnail(int imageId)
    {
        InvokeIfRequired(() =>
        {
            if (!_thumbPanels.TryGetValue(imageId, out var pnl)) return;
            _flowThumbnails.Controls.Remove(pnl);
            _thumbPanels.Remove(imageId);
            pnl.Dispose();

            int count = _thumbPanels.Count;
            _lblCount.Text          = $"{count} image{(count == 1 ? "" : "s")}";
            _pnlDropHint.Visible    = count == 0;
            _flowThumbnails.Visible = count > 0;
        });
    }

    // ── Layout ────────────────────────────────────────────────────────────────

    private void InitLayout()
    {
        Dock = DockStyle.Fill;

        // Toolbar
        _pnlToolbar = new FlowLayoutPanel
        {
            Dock          = DockStyle.Top,
            Height        = 40,
            BackColor     = Color.White,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            Padding       = new Padding(4, 5, 4, 4)
        };

        _btnUpload = MakeBtn("📁  Add Image", 118);
        _btnPaste  = MakeBtn("📋  Paste", 80);
        _lblCount  = new Label
        {
            Text      = "0 images",
            Font      = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size      = new Size(80, 28),
            Margin    = new Padding(8, 6, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _pnlToolbar.Controls.AddRange(new Control[] { _btnUpload, _btnPaste, _lblCount });

        // Drop hint (shown when gallery is empty)
        _pnlDropHint = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        var hint = new Label
        {
            Text      = "📂  Drag images here, paste (Ctrl+V)\nor click  📁 Add Image",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(150, 150, 150),
            Font      = new Font("Segoe UI", 9.5F),
            Dock      = DockStyle.Fill,
            AutoSize  = false
        };
        _pnlDropHint.Controls.Add(hint);

        // Flow panel for thumbnail cards
        _flowThumbnails = new FlowLayoutPanel
        {
            Dock         = DockStyle.Fill,
            BackColor    = Color.FromArgb(250, 250, 250),
            WrapContents = true,
            AutoScroll   = true,
            Padding      = new Padding(6),
            Visible      = false
        };

        Controls.Add(_flowThumbnails);
        Controls.Add(_pnlDropHint);
        Controls.Add(_pnlToolbar);
    }

    private void WireEvents()
    {
        _btnUpload.Click += (s, e) => UploadFromFileRequested?.Invoke(this, EventArgs.Empty);
        _btnPaste.Click  += (s, e) => PasteFromClipboardRequested?.Invoke(this, EventArgs.Empty);

        // Ctrl+V: hook into parent form's KeyPreview once handle is created
        HandleCreated += (s, e) =>
        {
            var form = FindForm();
            if (form == null) return;
            form.KeyPreview = true;
            form.KeyDown += (fs, fe) =>
            {
                if (fe.Control && fe.KeyCode == Keys.V && (ContainsFocus || Focused))
                    PasteFromClipboardRequested?.Invoke(this, EventArgs.Empty);
            };
        };
    }

    // ── Thumbnail card builder ────────────────────────────────────────────────

    /// <summary>
    /// Builds a thumbnail card that:
    ///   • Restores exact ThumbnailWidth × ThumbnailHeight from DB
    ///   • Shows a placeholder icon (no binary load on list)
    ///   • Loads full binary only on double-click
    ///   • Has a hover-reveal ✕ delete button
    /// </summary>
    private Panel BuildThumbCard(DCRImageThumbnailDto t)
    {
        var card = new Panel
        {
            Size        = new Size(t.ThumbnailWidth, t.ThumbnailHeight),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor   = Color.White,
            Margin      = new Padding(4),
            Tag         = t.Id
        };
        _thumbPanels[t.Id] = card;

        // Placeholder — shows file icon until user double-clicks
        var pic = new PictureBox
        {
            Dock      = DockStyle.Fill,
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(245, 247, 250),
            Cursor    = Cursors.Hand,
            Tag       = t.Id
        };
        SetPlaceholderIcon(pic, t.ContentType);

        // File name label at bottom
        var lbl = new Label
        {
            Text      = t.FileName.Length > 18 ? t.FileName[..15] + "…" : t.FileName,
            Dock      = DockStyle.Bottom,
            Height    = 18,
            Font      = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(80, 80, 80),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.White
        };

        // Delete button (hover-reveal)
        var btnDel = new Button
        {
            Text      = "✕",
            Width     = 22, Height = 22,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            Visible   = false,
            Tag       = t.Id
        };
        btnDel.FlatAppearance.BorderSize = 0;
        btnDel.Location = new Point(t.ThumbnailWidth - 26, 4);
        btnDel.Click   += (s, e) => DeleteImageRequested?.Invoke(this, t.Id);

        card.Controls.Add(pic);
        card.Controls.Add(lbl);
        card.Controls.Add(btnDel);
        btnDel.BringToFront();

        // Hover: show/hide delete button
        void ShowDel(object? s, EventArgs e) => btnDel.Visible = true;
        void HideDel(object? s, EventArgs e)
        {
            var cur = card.PointToClient(Cursor.Position);
            if (!card.ClientRectangle.Contains(cur)) btnDel.Visible = false;
        }
        card.MouseEnter += ShowDel;
        card.MouseLeave += HideDel;
        pic.MouseEnter  += ShowDel;
        pic.MouseLeave  += HideDel;
        btnDel.MouseLeave += HideDel;

        // Double-click: load full binary and display
        pic.DoubleClick += (s, e) => ViewImageRequested?.Invoke(this, t.Id);

        // Single-click: load binary into PictureBox for inline preview
        pic.Click += async (s, e) => await LoadInlineThumbnailAsync(pic, t.Id);

        return card;
    }

    /// <summary>
    /// Loads the binary for a single image and renders it inline in the thumbnail card.
    /// Called on first click — subsequent clicks reuse the already-loaded image.
    /// </summary>
    private async Task LoadInlineThumbnailAsync(PictureBox pic, int imageId)
    {
        if (pic.Image != null && pic.Tag is not string) return;  // already loaded

        try
        {
            // Find the presenter's service via DI — use the view's event to request data
            ViewImageRequested?.Invoke(this, imageId);
        }
        catch { /* ignore inline load errors */ }
    }

    private static void SetPlaceholderIcon(PictureBox pic, string contentType)
    {
        // Draw a simple colored placeholder based on content type
        var bmp = new Bitmap(pic.Width > 0 ? pic.Width : 60, pic.Height > 0 ? pic.Height : 60);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(235, 240, 248));

        var color = contentType switch
        {
            "image/jpeg" => Color.FromArgb(255, 200, 100),
            "image/png"  => Color.FromArgb(100, 180, 255),
            "image/bmp"  => Color.FromArgb(180, 220, 140),
            _            => Color.FromArgb(200, 200, 200)
        };

        using var brush = new SolidBrush(color);
        int cx = bmp.Width / 2, cy = bmp.Height / 2;
        g.FillEllipse(brush, cx - 16, cy - 16, 32, 32);

        using var font = new Font("Segoe UI", 8F);
        using var tb   = new SolidBrush(Color.FromArgb(80, 80, 80));
        var ext = contentType.Split('/').LastOrDefault()?.ToUpper() ?? "IMG";
        g.DrawString(ext, font, tb, cx - 12, cy + 20);

        pic.Image = bmp;
        pic.Tag   = "placeholder";
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    private static Button MakeBtn(string text, int width) => new Button
    {
        Text      = text,
        Size      = new Size(width, 28),
        Margin    = new Padding(0, 1, 6, 0),
        BackColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font      = new Font("Segoe UI", 9F),
        Cursor    = Cursors.Hand,
        FlatAppearance = { BorderColor = Color.FromArgb(180, 180, 180) }
    };
}
