using System.Drawing;
using System.Windows.Forms;

namespace DCRManagement.UI.Forms;

/// <summary>
/// A custom control for displaying and managing multiple images with drag-and-drop support.
/// </summary>
public class ImageGalleryControl : UserControl
{
    private FlowLayoutPanel _flowImages;
    private Panel _pnlDropZone;
    private Label _lblDropHint;
    private Panel _pnlToolbar;
    private readonly List<ImageThumbnail> _images = new();
    private Button _btnAddImage;
    private Button _btnClear;

    public event EventHandler<Image>? ImageAdded;
    public event EventHandler<ImageThumbnail>? ImageRemoved;

    public ImageGalleryControl()
    {
        InitializeComponent();
        AllowDrop = true;
        DragOver += (s, e) => e.Effect = DragDropEffects.Copy;
        DragDrop += HandleDragDrop;
    }

    private void InitializeComponent()
    {
        // Toolbar with buttons
        _pnlToolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 42,
            BackColor = Color.White,
            Padding = new Padding(4, 4, 4, 6)
        };

        _btnAddImage = new Button
        {
            Text = "📁  Add Image",
            Size = new Size(120, 32),
            Margin = new Padding(0, 0, 4, 0),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F)
        };
        _btnAddImage.Click += (s, e) => AddImageViaDialog();

        _btnClear = new Button
        {
            Text = "🗑  Clear",
            Size = new Size(100, 32),
            Margin = new Padding(0),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F)
        };
        _btnClear.Click += (s, e) => ClearImages();

        _pnlToolbar.Controls.Add(_btnAddImage);
        _pnlToolbar.Controls.Add(_btnClear);

        // Image gallery
        _flowImages = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(250, 250, 250),
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(8)
        };

        _pnlDropZone = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250),
            BorderStyle = BorderStyle.FixedSingle,
            Visible = true
        };

        _lblDropHint = new Label
        {
            Text = "Drag images here or\nuse buttons above",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(128, 128, 128),
            Font = new Font("Segoe UI", 10F),
            Dock = DockStyle.Fill,
            AutoSize = false
        };

        _pnlDropZone.Controls.Add(_lblDropHint);
        Controls.Add(_flowImages);
        Controls.Add(_pnlToolbar);
    }

    public void AddImage(Image image)
    {
        if (image == null) return;

        var thumbnail = new ImageThumbnail(image);
        thumbnail.RemoveRequested += (s, e) =>
        {
            _flowImages.Controls.Remove(thumbnail);
            _images.Remove(thumbnail);
            ImageRemoved?.Invoke(this, thumbnail);
            UpdateDropZoneVisibility();
        };

        _flowImages.Controls.Add(thumbnail);
        _images.Add(thumbnail);
        ImageAdded?.Invoke(this, image);
        UpdateDropZoneVisibility();
    }

    public void ClearImages()
    {
        foreach (var img in _images)
            img.Dispose();
        _images.Clear();
        _flowImages.Controls.Clear();
        UpdateDropZoneVisibility();
    }

    public List<Image> GetImages()
    {
        return _images.Select(t => t.GetImage()).ToList();
    }

    private void AddImageViaDialog()
    {
        using (var ofd = new OpenFileDialog())
        {
            ofd.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp, *.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files (*.*)|*.*";
            ofd.Multiselect = true;
            ofd.Title = "Select images";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var filePath in ofd.FileNames)
                {
                    try
                    {
                        var image = System.Drawing.Image.FromFile(filePath);
                        AddImage(image);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load {Path.GetFileName(filePath)}: {ex.Message}", "Error");
                    }
                }
            }
        }
    }

    private void HandleDragDrop(object sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var validImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

            foreach (var filePath in files)
            {
                var extension = Path.GetExtension(filePath).ToLower();
                if (validImageExtensions.Contains(extension))
                {
                    try
                    {
                        var image = System.Drawing.Image.FromFile(filePath);
                        AddImage(image);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load {Path.GetFileName(filePath)}: {ex.Message}", "Error");
                    }
                }
            }
        }
        else if (e.Data?.GetDataPresent(DataFormats.Bitmap) == true)
        {
            try
            {
                var image = (Image)e.Data.GetData(DataFormats.Bitmap);
                AddImage(image);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to paste image: {ex.Message}", "Error");
            }
        }
    }

    private void UpdateDropZoneVisibility()
    {
        _pnlDropZone.Visible = _images.Count == 0;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V && Clipboard.ContainsImage())
        {
            try
            {
                var image = Clipboard.GetImage();
                AddImage(image);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to paste image: {ex.Message}", "Error");
            }
        }
        base.OnKeyDown(e);
    }
}

/// <summary>
/// Represents a single image thumbnail with remove capability and resize support.
/// </summary>
public class ImageThumbnail : Panel
{
    private PictureBox _picThumbnail;
    private Button _btnRemove;
    private Panel _resizeHandle;
    private Image _image;
    private bool _isResizing;
    private Point _lastMousePos;
    private const int MinSize = 80;
    private const int MaxSize = 300;
    private const int ResizeHandleSize = 12;

    public event EventHandler? RemoveRequested;

    public ImageThumbnail(Image image)
    {
        _image = image;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Width = 140;
        Height = 140;
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = Color.White;
        Margin = new Padding(4);

        _picThumbnail = new PictureBox
        {
            Dock = DockStyle.Fill,
            Image = _image,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(250, 250, 250),
            Cursor = Cursors.Hand
        };

        _btnRemove = new Button
        {
            Text = "✕",
            Width = 24,
            Height = 24,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.None,
            Cursor = Cursors.Default
        };
        _btnRemove.Location = new Point(Width - 28, 4);
        _btnRemove.Click += (s, e) => RemoveRequested?.Invoke(this, EventArgs.Empty);

        // Resize handle in bottom-right corner
        _resizeHandle = new Panel
        {
            Width = ResizeHandleSize,
            Height = ResizeHandleSize,
            BackColor = Color.FromArgb(70, 130, 180),
            Cursor = Cursors.SizeNWSE,
            Dock = DockStyle.None
        };
        _resizeHandle.Location = new Point(Width - ResizeHandleSize, Height - ResizeHandleSize);

        Controls.Add(_picThumbnail);
        Controls.Add(_btnRemove);
        Controls.Add(_resizeHandle);

        MouseEnter += (s, e) => 
        {
            _btnRemove.Visible = true;
            _resizeHandle.Visible = true;
        };
        MouseLeave += (s, e) => 
        {
            _btnRemove.Visible = false;
            _resizeHandle.Visible = false;
        };
        _btnRemove.Visible = false;
        _resizeHandle.Visible = false;

        // Resize handle events
        _resizeHandle.MouseDown += ResizeHandle_MouseDown;
        _resizeHandle.MouseMove += ResizeHandle_MouseMove;
        _resizeHandle.MouseUp += ResizeHandle_MouseUp;

        // Also allow resizing from the main panel edges
        MouseDown += Thumbnail_MouseDown;
        MouseMove += Thumbnail_MouseMove;
        MouseUp += Thumbnail_MouseUp;
    }

    private void ResizeHandle_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isResizing = true;
            _lastMousePos = e.Location;
        }
    }

    private void ResizeHandle_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isResizing && e.Button == MouseButtons.Left)
        {
            int deltaX = e.X - _lastMousePos.X;
            int deltaY = e.Y - _lastMousePos.Y;

            int newWidth = Width + deltaX;
            int newHeight = Height + deltaY;

            // Maintain aspect ratio - use the larger delta
            int delta = Math.Max(deltaX, deltaY);
            newWidth = Width + delta;
            newHeight = Height + delta;

            // Apply size constraints
            newWidth = Math.Max(MinSize, Math.Min(MaxSize, newWidth));
            newHeight = Math.Max(MinSize, Math.Min(MaxSize, newHeight));

            if (newWidth != Width || newHeight != Height)
            {
                Size = new Size(newWidth, newHeight);
                UpdateResizeHandlePosition();
                _lastMousePos = e.Location;
            }
        }
    }

    private void ResizeHandle_MouseUp(object? sender, MouseEventArgs e)
    {
        _isResizing = false;
    }

    private void Thumbnail_MouseDown(object? sender, MouseEventArgs e)
    {
        // Check if clicking near the bottom-right corner for resizing
        if (e.Button == MouseButtons.Left && 
            e.X >= Width - 20 && e.Y >= Height - 20)
        {
            _isResizing = true;
            _lastMousePos = e.Location;
        }
    }

    private void Thumbnail_MouseMove(object? sender, MouseEventArgs e)
    {
        // Update cursor based on position
        if (e.X >= Width - 20 && e.Y >= Height - 20)
        {
            Cursor = Cursors.SizeNWSE;
        }
        else
        {
            Cursor = Cursors.Default;
        }

        if (_isResizing && e.Button == MouseButtons.Left)
        {
            int deltaX = e.X - _lastMousePos.X;
            int deltaY = e.Y - _lastMousePos.Y;

            int delta = Math.Max(deltaX, deltaY);
            int newWidth = Width + delta;
            int newHeight = Height + delta;

            newWidth = Math.Max(MinSize, Math.Min(MaxSize, newWidth));
            newHeight = Math.Max(MinSize, Math.Min(MaxSize, newHeight));

            if (newWidth != Width || newHeight != Height)
            {
                Size = new Size(newWidth, newHeight);
                UpdateResizeHandlePosition();
                _lastMousePos = e.Location;
            }
        }
    }

    private void Thumbnail_MouseUp(object? sender, MouseEventArgs e)
    {
        _isResizing = false;
    }

    private void UpdateResizeHandlePosition()
    {
        if (_resizeHandle != null)
        {
            _resizeHandle.Location = new Point(Width - ResizeHandleSize, Height - ResizeHandleSize);
        }
    }

    public Image GetImage() => _image;
}
