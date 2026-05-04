using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Image gallery: Add Image | Clear | Size controls | 🔒 lock-ratio
/// Supports: multi-select dialog, drag-and-drop, Ctrl+V paste, crop on add, individual removal.
/// </summary>
public class ImageGalleryControl : UserControl
{
    // ── Toolbar ───────────────────────────────────────────────────────────────
    private FlowLayoutPanel _pnlToolbar;
    private Button          _btnAddImage;
    private Button          _btnClear;
    private Label           _lblSep;
    private Label           _lblSize;
    private ComboBox        _cmbPreset;
    private Label           _lblW;
    private NumericUpDown   _nudWidth;
    private Label           _lblX;
    private NumericUpDown   _nudHeight;
    private Label           _lblPx;
    private Button          _btnLock;

    // ── Body ──────────────────────────────────────────────────────────────────
    private Panel           _pnlBody;
    private Panel           _pnlDropZone;
    private Label           _lblDropHint;
    private FlowLayoutPanel _flowImages;

    // ── State ─────────────────────────────────────────────────────────────────
    private readonly List<ImageThumbnail> _images = new();
    private bool _lockAspect        = true;
    private bool _suppressSizeEvents = false;

    // Message filter for global Ctrl+V capture
    private GalleryKeyFilter? _keyFilter;

    // Drag-to-reorder state
    private ImageThumbnail? _dragThumb;      // thumbnail being dragged
    private Point           _dragOffset;     // cursor offset within the thumbnail
    private Panel?          _dragGhost;      // semi-transparent overlay following the cursor
    private int             _dropIndex = -1; // current insertion index (highlighted)

    private const int DefaultThumbSize = 140;
    private const int MinThumbSize     = 60;
    private const int MaxThumbSize     = 400;

    private static readonly (string Label, int W, int H)[] Presets =
    {
        ("Small",   80,  80),
        ("Medium", 140, 140),
        ("Large",  220, 220),
        ("Custom",   0,   0),
    };

    // ── Events ────────────────────────────────────────────────────────────────
    public event EventHandler<Image>?          ImageAdded;
    public event EventHandler<ImageThumbnail>? ImageRemoved;

    // ── Constructor ───────────────────────────────────────────────────────────
    public ImageGalleryControl()
    {
        InitializeComponent();

        AllowDrop = true;
        DragEnter += OnDragEnter;
        DragDrop  += OnDragDrop;

        _pnlDropZone.AllowDrop  = true;
        _pnlDropZone.DragEnter += OnDragEnter;
        _pnlDropZone.DragDrop  += OnDragDrop;

        // Install message filter when handle is created; remove on dispose
        HandleCreated  += (s, e) => InstallKeyFilter();
        HandleDestroyed += (s, e) => RemoveKeyFilter();
    }

    // ── Layout ────────────────────────────────────────────────────────────────
    private void InitializeComponent()
    {
        SuspendLayout();

        _pnlToolbar = new FlowLayoutPanel
        {
            Dock          = DockStyle.Top,
            Height        = 40,
            BackColor     = Color.White,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            Padding       = new Padding(4, 5, 4, 4)
        };

        _btnAddImage = MakeBtn("📁  Add Image", 118);
        _btnAddImage.Click += (s, e) => AddImageViaDialog();

        _btnClear = MakeBtn("🗑  Clear", 82);
        _btnClear.Click += (s, e) => ConfirmAndClearImages();

        _lblSep = new Label
        {
            Text = "|", ForeColor = Color.FromArgb(200, 200, 200),
            Font = new Font("Segoe UI", 12F), Size = new Size(12, 30),
            Margin = new Padding(4, 0, 4, 0), TextAlign = ContentAlignment.MiddleCenter
        };

        _lblSize = new Label
        {
            Text = "Size:", Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(60, 60, 60), Size = new Size(34, 30),
            Margin = new Padding(0, 0, 2, 0), TextAlign = ContentAlignment.MiddleRight
        };

        _cmbPreset = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(82, 28), Margin = new Padding(0, 1, 6, 0),
            Font = new Font("Segoe UI", 9F), FlatStyle = FlatStyle.Flat
        };
        foreach (var p in Presets) _cmbPreset.Items.Add(p.Label);
        _cmbPreset.SelectedIndex = 1;
        _cmbPreset.SelectedIndexChanged += CmbPreset_Changed;

        _lblW    = MakeLbl("W:");
        _nudWidth = MakeSpinner();
        _nudWidth.Value = DefaultThumbSize;
        _nudWidth.ValueChanged += NudWidth_Changed;

        _lblX = MakeLbl("×");

        _nudHeight = MakeSpinner();
        _nudHeight.Value = DefaultThumbSize;
        _nudHeight.ValueChanged += NudHeight_Changed;

        _lblPx = MakeLbl("px");
        _lblPx.Margin = new Padding(2, 0, 6, 0);

        _btnLock = new Button
        {
            Text = "🔒", Size = new Size(30, 28), Margin = new Padding(0, 1, 0, 0),
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(230, 240, 255),
            Font = new Font("Segoe UI", 10F), Cursor = Cursors.Hand
        };
        _btnLock.FlatAppearance.BorderColor = Color.FromArgb(160, 190, 230);
        _btnLock.Click += BtnLock_Click;
        new ToolTip().SetToolTip(_btnLock, "Lock aspect ratio (W = H)");

        _pnlToolbar.Controls.AddRange(new Control[]
        {
            _btnAddImage, _btnClear, _lblSep,
            _lblSize, _cmbPreset,
            _lblW, _nudWidth, _lblX, _nudHeight, _lblPx, _btnLock
        });

        _pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(250, 250, 250) };

        _pnlDropZone = new Panel
        {
            Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250), Visible = true
        };
        _lblDropHint = new Label
        {
            Text = "📂  Drag images here, paste (Ctrl+V)\nor click  📁 Add Image",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(150, 150, 150),
            Font = new Font("Segoe UI", 9.5F), Dock = DockStyle.Fill, AutoSize = false
        };
        _pnlDropZone.Controls.Add(_lblDropHint);

        _flowImages = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.FromArgb(250, 250, 250),
            WrapContents = true, AutoScroll = true, Padding = new Padding(6), Visible = false
        };

        _pnlBody.Controls.Add(_flowImages);
        _pnlBody.Controls.Add(_pnlDropZone);
        Controls.Add(_pnlBody);
        Controls.Add(_pnlToolbar);

        ResumeLayout(false);
    }

    // ── Ctrl+V message filter ─────────────────────────────────────────────────

    /// <summary>
    /// Installs a WndProc-level message filter so Ctrl+V is caught regardless of
    /// which child control has keyboard focus, as long as the mouse is over this gallery
    /// or any child of it.
    /// </summary>
    private void InstallKeyFilter()
    {
        _keyFilter = new GalleryKeyFilter(this, PasteFromClipboard);
        System.Windows.Forms.Application.AddMessageFilter(_keyFilter);
    }

    private void RemoveKeyFilter()
    {
        if (_keyFilter != null)
        {
            System.Windows.Forms.Application.RemoveMessageFilter(_keyFilter);
            _keyFilter = null;
        }
    }

    // ── Paste ─────────────────────────────────────────────────────────────────

    /// <summary>Called by the message filter when Ctrl+V is pressed while this gallery is active.</summary>
    internal void PasteFromClipboard()
    {
        if (!Clipboard.ContainsImage()) return;
        try
        {
            var raw = Clipboard.GetImage();
            if (raw == null) return;
            var independent = CloneImage(raw);
            // Don't dispose raw — clipboard owns it
            AddImageWithOptionalCrop(independent);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot paste image:\n{ex.Message}",
                "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ── Size controls ─────────────────────────────────────────────────────────

    private void CmbPreset_Changed(object? sender, EventArgs e)
    {
        int idx = _cmbPreset.SelectedIndex;
        if (idx < 0 || idx >= Presets.Length) return;
        var (_, w, h) = Presets[idx];
        if (w == 0) return;

        _suppressSizeEvents = true;
        _nudWidth.Value  = w;
        _nudHeight.Value = h;
        _suppressSizeEvents = false;
        ApplySizeToAll(w, h);
    }

    private void NudWidth_Changed(object? sender, EventArgs e)
    {
        if (_suppressSizeEvents) return;
        int w = (int)_nudWidth.Value;
        if (_lockAspect)
        {
            _suppressSizeEvents = true;
            _nudHeight.Value = w;
            _suppressSizeEvents = false;
        }
        SetCustomPreset();
        ApplySizeToAll(w, (int)_nudHeight.Value);
    }

    private void NudHeight_Changed(object? sender, EventArgs e)
    {
        if (_suppressSizeEvents) return;
        int h = (int)_nudHeight.Value;
        if (_lockAspect)
        {
            _suppressSizeEvents = true;
            _nudWidth.Value = h;
            _suppressSizeEvents = false;
        }
        SetCustomPreset();
        ApplySizeToAll((int)_nudWidth.Value, h);
    }

    private void BtnLock_Click(object? sender, EventArgs e)
    {
        _lockAspect = !_lockAspect;
        _btnLock.Text      = _lockAspect ? "🔒" : "🔓";
        _btnLock.BackColor = _lockAspect ? Color.FromArgb(230, 240, 255) : Color.FromArgb(245, 245, 245);
        _btnLock.FlatAppearance.BorderColor = _lockAspect
            ? Color.FromArgb(160, 190, 230) : Color.FromArgb(200, 200, 200);
    }

    private void SetCustomPreset()
    {
        _suppressSizeEvents = true;
        _cmbPreset.SelectedIndex = Presets.Length - 1;
        _suppressSizeEvents = false;
    }

    private void ApplySizeToAll(int w, int h)
    {
        _flowImages.SuspendLayout();
        foreach (var t in _images) t.ResizeTo(w, h);
        _flowImages.ResumeLayout();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Add image directly (no crop dialog). Used when loading saved data.</summary>
    public void AddImage(Image image)
    {
        if (image == null) return;
        int w = (int)_nudWidth.Value;
        int h = (int)_nudHeight.Value;

        var thumb = new ImageThumbnail(image, w, h);
        thumb.RemoveRequested += (s, e) =>
        {
            _flowImages.Controls.Remove(thumb);
            _images.Remove(thumb);
            thumb.Dispose();
            ImageRemoved?.Invoke(this, thumb);
            UpdateDropZoneVisibility();
        };

        // Drag-to-reorder support — thumbnail notifies gallery on MouseDown
        thumb.DragStarted += Thumb_DragStarted;

        _flowImages.Controls.Add(thumb);
        _images.Add(thumb);
        ImageAdded?.Invoke(this, image);
        UpdateDropZoneVisibility();
    }

    public void ClearImages()
    {
        foreach (var t in _images) t.Dispose();
        _images.Clear();
        _flowImages.Controls.Clear();
        UpdateDropZoneVisibility();
    }

    public List<Image> GetImages() => _images.Select(t => t.GetImage()).ToList();

    /// <summary>Returns the current W×H from the size spinners.</summary>
    public (int Width, int Height) GetCurrentSize() =>
        ((int)_nudWidth.Value, (int)_nudHeight.Value);

    /// <summary>
    /// Restores the size spinners and resizes all existing thumbnails.
    /// Called when loading a saved DCR so the gallery looks identical to when it was saved.
    /// </summary>
    public void SetSize(int width, int height)
    {
        _suppressSizeEvents = true;
        _nudWidth.Value  = Math.Max(MinThumbSize, Math.Min(MaxThumbSize, width));
        _nudHeight.Value = Math.Max(MinThumbSize, Math.Min(MaxThumbSize, height));
        _suppressSizeEvents = false;

        // Update preset combo to match (or show Custom)
        var match = Array.FindIndex(Presets, p => p.W == width && p.H == height && p.W != 0);
        _suppressSizeEvents = true;
        _cmbPreset.SelectedIndex = match >= 0 ? match : Presets.Length - 1;
        _suppressSizeEvents = false;

        // Resize all existing thumbnails to the restored size
        ApplySizeToAll(width, height);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Opens the crop dialog. If user confirms, adds the cropped image.
    /// If user cancels crop, adds the original image unchanged.
    /// </summary>
    private void AddImageWithOptionalCrop(Image image)
    {
        using var dlg = new ImageCropDialog(image);
        var result = dlg.ShowDialog(FindForm() ?? (IWin32Window)this);

        if (result == DialogResult.OK && dlg.CroppedImage != null)
        {
            image.Dispose();            // discard original
            AddImage(dlg.CroppedImage);
        }
        else
        {
            AddImage(image);            // add as-is
        }
    }

    private void ConfirmAndClearImages()
    {
        if (_images.Count == 0) return;
        var r = MessageBox.Show(
            $"Remove all {_images.Count} image(s) from this gallery?",
            "Clear Gallery", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (r == DialogResult.Yes) ClearImages();
    }

    private void AddImageViaDialog()
    {
        using var ofd = new OpenFileDialog
        {
            Filter      = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.webp|All files|*.*",
            Multiselect = true,
            Title       = "Select images"
        };
        if (ofd.ShowDialog() != DialogResult.OK) return;
        foreach (var f in ofd.FileNames) LoadImageFromFile(f);
    }

    private void LoadImageFromFile(string filePath)
    {
        try
        {
            using var tmp = Image.FromFile(filePath);
            AddImageWithOptionalCrop(CloneImage(tmp));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot load \"{System.IO.Path.GetFileName(filePath)}\":\n{ex.Message}",
                "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = (e.Data?.GetDataPresent(DataFormats.FileDrop) == true ||
                    e.Data?.GetDataPresent(DataFormats.Bitmap)   == true)
            ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files    = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            var validExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };
            foreach (var f in files)
                if (validExt.Contains(System.IO.Path.GetExtension(f)))
                    LoadImageFromFile(f);
        }
        else if (e.Data?.GetDataPresent(DataFormats.Bitmap) == true)
        {
            try
            {
                var src = (Image)e.Data.GetData(DataFormats.Bitmap)!;
                AddImageWithOptionalCrop(CloneImage(src));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot load dragged image:\n{ex.Message}",
                    "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    // ── Drag-to-reorder ───────────────────────────────────────────────────────
    // Strategy: thumbnail fires DragStarted → gallery captures mouse on _flowImages
    // so MouseMove/MouseUp always arrive even when cursor leaves the thumbnail.

    private void Thumb_DragStarted(object? sender, DragStartedEventArgs e)
    {
        if (sender is not ImageThumbnail thumb) return;

        _dragThumb  = thumb;
        // Convert the click offset from thumbnail coords to flowImages coords
        var thumbPosInFlow = _flowImages.PointToClient(thumb.PointToScreen(Point.Empty));
        _dragOffset = new Point(e.ClickOffset.X, e.ClickOffset.Y);
        _dropIndex  = _images.IndexOf(thumb);

        // Build ghost
        _dragGhost = new Panel
        {
            Size        = thumb.Size,
            BackColor   = Color.FromArgb(180, 210, 240),
            BorderStyle = BorderStyle.FixedSingle
        };
        var ghostPic = new PictureBox
        {
            Dock      = DockStyle.Fill,
            Image     = thumb.GetImage(),
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        _dragGhost.Controls.Add(ghostPic);

        // Position ghost at thumb's current location
        _dragGhost.Location = thumbPosInFlow;
        _flowImages.Controls.Add(_dragGhost);
        _dragGhost.BringToFront();

        thumb.SetDragging(true);

        // Capture mouse on _flowImages so Move/Up fire regardless of cursor position
        _flowImages.MouseMove += Flow_MouseMove;
        _flowImages.MouseUp   += Flow_MouseUp;
        _flowImages.Capture    = true;
    }

    private void Flow_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragThumb == null || _dragGhost == null) return;

        // Move ghost
        _dragGhost.Location = new Point(e.X - _dragOffset.X, e.Y - _dragOffset.Y);

        // Hit-test drop index
        int newIndex = HitTestDropIndex(e.Location);
        if (newIndex != _dropIndex)
        {
            _dropIndex = newIndex;
            RefreshDropIndicators();
        }
    }

    private void Flow_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_dragThumb == null) return;

        // Release capture first
        _flowImages.Capture    = false;
        _flowImages.MouseMove -= Flow_MouseMove;
        _flowImages.MouseUp   -= Flow_MouseUp;

        int src = _images.IndexOf(_dragThumb);
        int dst = _dropIndex < 0 ? src : Math.Max(0, Math.Min(_dropIndex, _images.Count - 1));

        if (src != dst)
        {
            _images.RemoveAt(src);
            _images.Insert(dst, _dragThumb);
            _flowImages.Controls.SetChildIndex(_dragThumb, dst);
        }

        CancelDrag();
    }

    private void CancelDrag()
    {
        _flowImages.Capture    = false;
        _flowImages.MouseMove -= Flow_MouseMove;
        _flowImages.MouseUp   -= Flow_MouseUp;

        if (_dragGhost != null)
        {
            _flowImages.Controls.Remove(_dragGhost);
            _dragGhost.Dispose();
            _dragGhost = null;
        }
        if (_dragThumb != null)
        {
            _dragThumb.SetDragging(false);
            _dragThumb = null;
        }
        ClearDropIndicators();
        _dropIndex = -1;
    }

    /// <summary>Returns insertion index (0..Count) based on cursor position in flow coords.</summary>
    private int HitTestDropIndex(Point posInFlow)
    {
        for (int i = 0; i < _images.Count; i++)
        {
            var t = _images[i];
            if (t == _dragThumb) continue;
            var r = t.Bounds;
            // Insert before this thumb if cursor is in its left half on the same row,
            // or on any row above it
            if (posInFlow.Y < r.Bottom)
            {
                if (posInFlow.Y >= r.Top && posInFlow.X < r.Left + r.Width / 2)
                    return i;
                if (posInFlow.Y < r.Top)
                    return i;
            }
        }
        return _images.Count;
    }

    private void RefreshDropIndicators()
    {
        ClearDropIndicators();
        if (_dropIndex < 0 || _dragThumb == null) return;

        int target = Math.Min(_dropIndex, _images.Count - 1);
        if (target >= 0 && _images[target] != _dragThumb)
        {
            int targetIdx = _images.IndexOf(_images[target]);
            _images[target].ShowDropIndicator(_dropIndex <= targetIdx);
        }
    }

    private void ClearDropIndicators()
    {
        foreach (var t in _images) t.ClearDropIndicator();
    }

    private void UpdateDropZoneVisibility()
    {
        _pnlDropZone.Visible = _images.Count == 0;
        _flowImages.Visible  = _images.Count > 0;
    }

    internal static Bitmap CloneImage(Image src)
    {
        var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.DrawImage(src, 0, 0, src.Width, src.Height);
        return bmp;
    }

    // ── Factory helpers ───────────────────────────────────────────────────────

    private static Button MakeBtn(string text, int width) => new Button
    {
        Text = text, Size = new Size(width, 28), Margin = new Padding(0, 1, 6, 0),
        BackColor = Color.White, FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F), Cursor = Cursors.Hand,
        FlatAppearance = { BorderColor = Color.FromArgb(180, 180, 180) }
    };

    private static Label MakeLbl(string text) => new Label
    {
        Text = text, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(60, 60, 60),
        Size = new Size(text == "×" ? 14 : 22, 30), Margin = new Padding(2, 0, 2, 0),
        TextAlign = ContentAlignment.MiddleCenter
    };

    private static NumericUpDown MakeSpinner() => new NumericUpDown
    {
        Minimum = MinThumbSize, Maximum = MaxThumbSize, Increment = 10,
        Size = new Size(58, 28), Margin = new Padding(0, 1, 0, 0),
        Font = new Font("Segoe UI", 9F), TextAlign = HorizontalAlignment.Center,
        BorderStyle = BorderStyle.FixedSingle
    };

    // ── Enabled state ─────────────────────────────────────────────────────────
    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        _btnAddImage.Enabled = Enabled;
        _btnClear.Enabled    = Enabled;
        _cmbPreset.Enabled   = Enabled;
        _nudWidth.Enabled    = Enabled;
        _nudHeight.Enabled   = Enabled;
        _btnLock.Enabled     = Enabled;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) RemoveKeyFilter();
        base.Dispose(disposing);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// WndProc-level message filter: intercepts WM_KEYDOWN Ctrl+V globally and
/// routes it to the gallery when the gallery (or any descendant) is under the mouse
/// OR contains keyboard focus.
/// </summary>
internal sealed class GalleryKeyFilter : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;
    private const int VK_V       = 0x56;

    private readonly ImageGalleryControl _gallery;
    private readonly Action              _onPaste;

    public GalleryKeyFilter(ImageGalleryControl gallery, Action onPaste)
    {
        _gallery = gallery;
        _onPaste = onPaste;
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_KEYDOWN) return false;
        if ((int)m.WParam != VK_V) return false;
        if ((Control.ModifierKeys & Keys.Control) == 0) return false;

        // Fire only when gallery or a child has focus, OR mouse is over the gallery
        bool hasFocus  = _gallery.ContainsFocus || _gallery.Focused;
        bool mouseOver = _gallery.ClientRectangle.Contains(
                             _gallery.PointToClient(Cursor.Position));

        if (!hasFocus && !mouseOver) return false;

        _onPaste();
        return true;   // consume the keystroke
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Event args carrying the click offset within the thumbnail when drag starts.</summary>
public class DragStartedEventArgs : EventArgs
{
    public Point ClickOffset { get; }
    public DragStartedEventArgs(Point offset) => ClickOffset = offset;
}

/// <summary>Single image thumbnail with hover-reveal remove button and drag-to-reorder support.</summary>
public class ImageThumbnail : Panel
{
    private readonly PictureBox _pic;
    private readonly Button     _btnRemove;
    private readonly Image      _image;

    // Drop indicator state
    private bool _showDropLeft;
    private bool _showDropRight;

    // Drag threshold — don't start drag on tiny accidental moves
    private Point _mouseDownPos;
    private bool  _mayDrag;
    private const int DragThreshold = 5;

    public event EventHandler?              RemoveRequested;
    public event EventHandler<DragStartedEventArgs>? DragStarted;

    public ImageThumbnail(Image image, int width = 140, int height = 140)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
        Size        = new Size(width, height);
        BorderStyle = BorderStyle.FixedSingle;
        BackColor   = Color.White;
        Margin      = new Padding(4);
        Cursor      = Cursors.SizeAll;

        _pic = new PictureBox
        {
            Dock      = DockStyle.Fill,
            Image     = _image,
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(250, 250, 250),
            Cursor    = Cursors.SizeAll
        };

        _btnRemove = new Button
        {
            Text      = "✕",
            Width     = 22, Height = 22,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Default,
            Visible   = false
        };
        _btnRemove.FlatAppearance.BorderSize = 0;
        _btnRemove.Location = new Point(width - 26, 4);
        _btnRemove.Click   += (s, e) => RemoveRequested?.Invoke(this, EventArgs.Empty);

        Controls.Add(_pic);
        Controls.Add(_btnRemove);
        _btnRemove.BringToFront();

        // Hover: show/hide remove button
        MouseEnter           += (s, e) => _btnRemove.Visible = true;
        MouseLeave           += (s, e) => HideIfLeft();
        _pic.MouseEnter      += (s, e) => _btnRemove.Visible = true;
        _pic.MouseLeave      += (s, e) => HideIfLeft();
        _btnRemove.MouseLeave += (s, e) => HideIfLeft();

        // Drag detection on both panel and picture box
        MouseDown      += OnThumbMouseDown;
        MouseMove      += OnThumbMouseMove;
        MouseUp        += OnThumbMouseUp;
        _pic.MouseDown += OnThumbMouseDown;
        _pic.MouseMove += OnThumbMouseMove;
        _pic.MouseUp   += OnThumbMouseUp;
    }

    public Image GetImage() => _image;

    public void ResizeTo(int w, int h)
    {
        Size = new Size(w, h);
        _btnRemove.Location = new Point(w - 26, 4);
    }

    public void SetDragging(bool dragging)
    {
        BackColor      = dragging ? Color.FromArgb(210, 220, 235) : Color.White;
        _pic.BackColor = dragging ? Color.FromArgb(200, 210, 225) : Color.FromArgb(250, 250, 250);
        _btnRemove.Visible = false;
        _mayDrag = false;
    }

    public void ShowDropIndicator(bool onLeft)
    {
        _showDropLeft  = onLeft;
        _showDropRight = !onLeft;
        Invalidate();
    }

    public void ClearDropIndicator()
    {
        if (!_showDropLeft && !_showDropRight) return;
        _showDropLeft = _showDropRight = false;
        Invalidate();
    }

    // ── Mouse drag detection ──────────────────────────────────────────────────

    private void OnThumbMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        // Convert to panel coords if event came from _pic
        _mouseDownPos = sender == _pic
            ? _pic.PointToScreen(e.Location).Let(p => PointToClient(p))
            : e.Location;
        _mayDrag = true;
    }

    private void OnThumbMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_mayDrag || (e.Button & MouseButtons.Left) == 0) return;

        var cur = sender == _pic
            ? _pic.PointToScreen(e.Location).Let(p => PointToClient(p))
            : e.Location;

        if (Math.Abs(cur.X - _mouseDownPos.X) >= DragThreshold ||
            Math.Abs(cur.Y - _mouseDownPos.Y) >= DragThreshold)
        {
            _mayDrag = false;
            DragStarted?.Invoke(this, new DragStartedEventArgs(_mouseDownPos));
        }
    }

    private void OnThumbMouseUp(object? sender, MouseEventArgs e)
    {
        _mayDrag = false;
    }

    // ── Paint drop indicator ──────────────────────────────────────────────────

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (!_showDropLeft && !_showDropRight) return;

        int x = _showDropLeft ? 1 : Width - 3;
        using var pen = new Pen(Color.FromArgb(0, 120, 215), 3f);
        e.Graphics.DrawLine(pen, x, 4, x, Height - 4);

        // Small triangle arrow
        using var brush = new SolidBrush(Color.FromArgb(0, 120, 215));
        int ax = _showDropLeft ? 1 : Width - 2;
        e.Graphics.FillPolygon(brush, new[]
        {
            new Point(ax - 5, 4), new Point(ax + 5, 4), new Point(ax, 10)
        });
    }

    private void HideIfLeft()
    {
        if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
            _btnRemove.Visible = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _image.Dispose(); _pic.Dispose(); }
        base.Dispose(disposing);
    }
}

/// <summary>Extension to allow inline Point transformation without temp variable.</summary>
internal static class PointExt
{
    public static T Let<T>(this T value, Func<T, T> fn) => fn(value);
}
