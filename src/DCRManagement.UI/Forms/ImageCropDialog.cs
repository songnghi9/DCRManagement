using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Modal crop dialog — rubber-band selection, live preview, zoom-to-fit.
/// Layout uses TableLayoutPanel (no SplitContainer) to avoid SplitterDistance constraint errors.
/// </summary>
public sealed class ImageCropDialog : Form
{
    // ── Public result ─────────────────────────────────────────────────────────
    public Bitmap? CroppedImage { get; private set; }

    // ── Source image ──────────────────────────────────────────────────────────
    private readonly Bitmap _source;

    // ── Canvas ────────────────────────────────────────────────────────────────
    private Panel  _pnlCanvas;
    private Bitmap _displayBmp;
    private float  _scale;
    private Point  _imgOffset;

    // Rubber-band
    private bool      _dragging;
    private Point     _dragStart;
    private Point     _dragCurrent;
    private Rectangle _selCanvas;
    private Rectangle _selSource;

    // ── Preview ───────────────────────────────────────────────────────────────
    private PictureBox _picPreview;

    // ── Bottom bar ────────────────────────────────────────────────────────────
    private Button _btnCrop;
    private Button _btnReset;
    private Button _btnCancel;
    private Label  _lblInfo;

    // ── GDI resources ─────────────────────────────────────────────────────────
    private static readonly Pen         SelPen  = new Pen(Color.FromArgb(0, 120, 215), 1.5f) { DashStyle = DashStyle.Dash };
    private static readonly SolidBrush  DimBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0));

    // ── Constructor ───────────────────────────────────────────────────────────
    public ImageCropDialog(Image source)
    {
        _source     = new Bitmap(source);
        _displayBmp = _source;
        BuildLayout();
    }

    // ── Layout (no SplitContainer) ────────────────────────────────────────────
    private void BuildLayout()
    {
        Text            = "Crop Image";
        Size            = new Size(920, 640);
        MinimumSize     = new Size(640, 480);
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = Color.FromArgb(30, 30, 30);
        Font            = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.Sizable;

        // ── Bottom action bar ─────────────────────────────────────────────────
        var pnlBottom = new FlowLayoutPanel
        {
            Dock          = DockStyle.Bottom,
            Height        = 46,
            BackColor     = Color.FromArgb(45, 45, 45),
            FlowDirection = FlowDirection.RightToLeft,
            Padding       = new Padding(8, 7, 8, 7),
            WrapContents  = false
        };

        _btnCancel = MakeBtn("Cancel",   false);
        _btnCrop   = MakeBtn("✔  Crop",  true);
        _btnReset  = MakeBtn("↺  Reset", false);

        _lblInfo = new Label
        {
            AutoSize  = true,
            ForeColor = Color.FromArgb(180, 180, 180),
            Font      = new Font("Segoe UI", 8.5F),
            Margin    = new Padding(0, 9, 8, 0),
            Text      = "Drag to select crop area"
        };

        pnlBottom.Controls.AddRange(new Control[] { _btnCancel, _btnCrop, _btnReset, _lblInfo });

        // ── Body: TableLayoutPanel 2 columns (no SplitContainer) ─────────────
        var table = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 1,
            BackColor   = Color.FromArgb(30, 30, 30),
            Padding     = new Padding(0)
        };
        // Canvas takes ~75%, preview takes ~25%
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Canvas
        _pnlCanvas = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 20, 20),
            Cursor    = Cursors.Cross,
            Margin    = new Padding(0)
        };
        _pnlCanvas.Paint     += Canvas_Paint;
        _pnlCanvas.MouseDown += Canvas_MouseDown;
        _pnlCanvas.MouseMove += Canvas_MouseMove;
        _pnlCanvas.MouseUp   += Canvas_MouseUp;
        _pnlCanvas.Resize    += (s, e) => { RebuildDisplay(); _pnlCanvas.Invalidate(); };

        // Preview panel
        var pnlPreview = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 40, 40),
            Padding   = new Padding(8),
            Margin    = new Padding(4, 0, 0, 0)
        };
        var lblTitle = new Label
        {
            Text      = "Preview",
            Dock      = DockStyle.Top,
            Height    = 24,
            ForeColor = Color.FromArgb(180, 180, 180),
            Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _picPreview = new PictureBox
        {
            Dock        = DockStyle.Fill,
            SizeMode    = PictureBoxSizeMode.Zoom,
            BackColor   = Color.FromArgb(20, 20, 20),
            BorderStyle = BorderStyle.None
        };
        pnlPreview.Controls.Add(_picPreview);
        pnlPreview.Controls.Add(lblTitle);

        table.Controls.Add(_pnlCanvas,  0, 0);
        table.Controls.Add(pnlPreview,  1, 0);

        Controls.Add(table);
        Controls.Add(pnlBottom);

        // ── Wire ─────────────────────────────────────────────────────────────
        _btnCrop.Click   += BtnCrop_Click;
        _btnReset.Click  += (s, e) => ResetSelection();
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        KeyPreview = true;
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            if (e.KeyCode == Keys.Enter && _selSource != Rectangle.Empty) BtnCrop_Click(s, e);
        };

        // RebuildDisplay after the form is fully laid out
        Shown += (s, e) => RebuildDisplay();
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void RebuildDisplay()
    {
        if (_pnlCanvas.Width <= 0 || _pnlCanvas.Height <= 0) return;

        float sx = (float)_pnlCanvas.Width  / _source.Width;
        float sy = (float)_pnlCanvas.Height / _source.Height;
        _scale = Math.Min(sx, sy) * 0.95f;

        int dw = (int)(_source.Width  * _scale);
        int dh = (int)(_source.Height * _scale);
        _imgOffset = new Point((_pnlCanvas.Width - dw) / 2, (_pnlCanvas.Height - dh) / 2);

        if (_displayBmp != _source) _displayBmp.Dispose();
        _displayBmp = new Bitmap(_source, dw, dh);

        ResetSelection();
    }

    private void ResetSelection()
    {
        _selCanvas        = Rectangle.Empty;
        _selSource        = Rectangle.Empty;
        _picPreview.Image = null;
        UpdateInfoLabel();
        _pnlCanvas.Invalidate();
    }

    // ── Canvas paint ──────────────────────────────────────────────────────────

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(_displayBmp, _imgOffset.X, _imgOffset.Y, _displayBmp.Width, _displayBmp.Height);

        if (_selCanvas.Width > 2 && _selCanvas.Height > 2)
        {
            var imgRect = new Rectangle(_imgOffset.X, _imgOffset.Y, _displayBmp.Width, _displayBmp.Height);
            using var region = new Region(imgRect);
            region.Exclude(_selCanvas);
            g.FillRegion(DimBrush, region);

            g.DrawRectangle(SelPen, _selCanvas);
            DrawHandle(g, _selCanvas.Left,  _selCanvas.Top);
            DrawHandle(g, _selCanvas.Right, _selCanvas.Top);
            DrawHandle(g, _selCanvas.Left,  _selCanvas.Bottom);
            DrawHandle(g, _selCanvas.Right, _selCanvas.Bottom);
        }
    }

    private static void DrawHandle(Graphics g, int x, int y)
    {
        const int hs = 6;
        g.FillRectangle(Brushes.White,     x - hs / 2, y - hs / 2, hs, hs);
        g.DrawRectangle(Pens.DodgerBlue,   x - hs / 2, y - hs / 2, hs, hs);
    }

    // ── Mouse ─────────────────────────────────────────────────────────────────

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _dragging         = true;
        _dragStart        = ClampToImage(e.Location);
        _dragCurrent      = _dragStart;
        _selCanvas        = Rectangle.Empty;
        _selSource        = Rectangle.Empty;
        _picPreview.Image = null;
    }

    private void Canvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        _dragCurrent = ClampToImage(e.Location);
        UpdateSelection();
        _pnlCanvas.Invalidate();
    }

    private void Canvas_MouseUp(object? sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        _dragging    = false;
        _dragCurrent = ClampToImage(e.Location);
        UpdateSelection();
        UpdatePreview();
        _pnlCanvas.Invalidate();
    }

    private void UpdateSelection()
    {
        int x = Math.Min(_dragStart.X, _dragCurrent.X);
        int y = Math.Min(_dragStart.Y, _dragCurrent.Y);
        int w = Math.Abs(_dragCurrent.X - _dragStart.X);
        int h = Math.Abs(_dragCurrent.Y - _dragStart.Y);

        if (w < 2 || h < 2) { _selCanvas = Rectangle.Empty; _selSource = Rectangle.Empty; return; }

        _selCanvas = new Rectangle(x, y, w, h);

        int sx = Math.Max(0, (int)((x - _imgOffset.X) / _scale));
        int sy = Math.Max(0, (int)((y - _imgOffset.Y) / _scale));
        int sw = Math.Max(1, (int)(w / _scale));
        int sh = Math.Max(1, (int)(h / _scale));

        sx = Math.Min(sx, _source.Width  - 1);
        sy = Math.Min(sy, _source.Height - 1);
        sw = Math.Min(sw, _source.Width  - sx);
        sh = Math.Min(sh, _source.Height - sy);

        _selSource = new Rectangle(sx, sy, sw, sh);
        UpdateInfoLabel();
    }

    private void UpdatePreview()
    {
        if (_selSource.Width < 1 || _selSource.Height < 1) return;
        _picPreview.Image?.Dispose();
        _picPreview.Image = _source.Clone(_selSource, _source.PixelFormat);
    }

    private void UpdateInfoLabel()
    {
        _lblInfo.Text = _selSource == Rectangle.Empty
            ? "Drag to select crop area"
            : $"Selection: {_selSource.Width} × {_selSource.Height} px";
    }

    private Point ClampToImage(Point p) => new Point(
        Math.Max(_imgOffset.X, Math.Min(p.X, _imgOffset.X + _displayBmp.Width)),
        Math.Max(_imgOffset.Y, Math.Min(p.Y, _imgOffset.Y + _displayBmp.Height)));

    // ── Crop ──────────────────────────────────────────────────────────────────

    private void BtnCrop_Click(object? sender, EventArgs e)
    {
        if (_selSource.Width < 1 || _selSource.Height < 1)
        {
            MessageBox.Show("Please drag to select a crop area first.",
                "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        CroppedImage = _source.Clone(_selSource, _source.PixelFormat);
        DialogResult = DialogResult.OK;
        Close();
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    private static Button MakeBtn(string text, bool primary) => new Button
    {
        Text      = text,
        Size      = new Size(primary ? 100 : 80, 30),
        Margin    = new Padding(0, 0, 6, 0),
        FlatStyle = FlatStyle.Flat,
        BackColor = primary ? Color.FromArgb(0, 120, 215) : Color.FromArgb(70, 70, 70),
        ForeColor = Color.White,
        Font      = new Font("Segoe UI", 9F, primary ? FontStyle.Bold : FontStyle.Regular),
        Cursor    = Cursors.Hand,
        FlatAppearance = { BorderSize = 0 }
    };

    // ── Cleanup ───────────────────────────────────────────────────────────────

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _source.Dispose();
            if (_displayBmp != _source) _displayBmp.Dispose();
            _picPreview.Image?.Dispose();
        }
        base.Dispose(disposing);
    }
}
