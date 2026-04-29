using DCRManagement.UI.Common;
using System.Windows.Forms;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Reusable UserControl for displaying and managing multiple images in a gallery view.
/// Supports drag-drop, paste from clipboard, and single/multiple file selection.
/// </summary>
public partial class ImageGalleryPanel : UserControl
{
    private readonly FlowLayoutPanel _gallery = new();
    private readonly Panel _toolbar = new();
    private readonly Button _btnAdd = new() { Text = "➕  Add Images" };
    private readonly Button _btnPaste = new() { Text = "📋  Paste from Clipboard" };
    private readonly Button _btnClear = new() { Text = "🗑  Clear All" };
    private readonly Label _lblImageCount = new();

    private List<GalleryImage> _images = [];
    private const int THUMBNAIL_SIZE = 120;
    private const int PADDING = 8;

    public event EventHandler? ImagesChanged;

    public ImageGalleryPanel()
    {
        BuildLayout();
        WireEvents();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>Get all images currently in the gallery.</summary>
    public IEnumerable<GalleryImage> GetImages() => _images.AsReadOnly();

    /// <summary>Add a single image from file path.</summary>
    public void AddImage(string filePath)
    {
        try
        {
            if (!IsValidImageFile(filePath))
            {
                MessageBox.Show("Invalid image format. Supported: JPG, PNG, BMP, GIF", "Error");
                return;
            }

            var image = System.Drawing.Image.FromFile(filePath);
            var galleryImage = new GalleryImage(Guid.NewGuid().ToString(), filePath, image);
            _images.Add(galleryImage);
            RenderGallery();
            ImagesChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load image: {ex.Message}", "Error");
        }
    }

    /// <summary>Add multiple images from file paths.</summary>
    public void AddImages(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
            AddImage(filePath);
    }

    /// <summary>Add image from clipboard.</summary>
    public void AddImageFromClipboard()
    {
        try
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("Clipboard does not contain an image.", "Info");
                return;
            }

            var image = Clipboard.GetImage();
            if (image != null)
            {
                var galleryImage = new GalleryImage(Guid.NewGuid().ToString(), "(clipboard)", image);
                _images.Add(galleryImage);
                RenderGallery();
                ImagesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to paste image: {ex.Message}", "Error");
        }
    }

    /// <summary>Clear all images from the gallery.</summary>
    public void ClearAll()
    {
        if (_images.Count == 0) return;

        if (MessageBox.Show(
            $"Remove all {_images.Count} image(s)?",
            "Clear Gallery",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes)
        {
            foreach (var img in _images)
                img.Image?.Dispose();

            _images.Clear();
            RenderGallery();
            ImagesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Get count of images in gallery.</summary>
    public int GetImageCount() => _images.Count;

    /// <summary>Set read-only mode (disable add/remove buttons).</summary>
    public void SetReadOnly(bool readOnly)
    {
        _btnAdd.Enabled = !readOnly;
        _btnPaste.Enabled = !readOnly;
        _btnClear.Enabled = !readOnly;

        foreach (var item in _gallery.Controls.OfType<Panel>())
        {
            if (item.Tag is Panel removeBtn)
                removeBtn.Visible = !readOnly;
        }
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
        ThemeManager.StyleSecondaryButton(_btnPaste);
        ThemeManager.StyleSecondaryButton(_btnClear);

        _btnAdd.Size = new Size(130, 30);
        _btnAdd.Location = new Point(8, 7);

        _btnPaste.Size = new Size(160, 30);
        _btnPaste.Location = new Point(146, 7);

        _btnClear.Size = new Size(120, 30);
        _btnClear.Location = new Point(314, 7);

        _lblImageCount.AutoSize = true;
        _lblImageCount.Location = new Point(442, 12);
        _lblImageCount.Font = new Font(Font, FontStyle.Regular);
        _lblImageCount.ForeColor = ThemeManager.TextSecondary;

        _toolbar.Controls.AddRange([_btnAdd, _btnPaste, _btnClear, _lblImageCount]);

        // Gallery
        _gallery.Dock = DockStyle.Fill;
        _gallery.FlowDirection = FlowDirection.LeftToRight;
        _gallery.WrapContents = true;
        _gallery.Padding = new Padding(PADDING);
        _gallery.AutoScroll = true;
        _gallery.BackColor = Color.White;

        Controls.Add(_gallery);
        Controls.Add(_toolbar);
    }

    private void WireEvents()
    {
        _btnAdd.Click += (s, e) => OnAddImages();
        _btnPaste.Click += (s, e) => AddImageFromClipboard();
        _btnClear.Click += (s, e) => ClearAll();

        // Drag-drop support
        AllowDrop = true;
        DragOver += (s, e) => e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        DragDrop += (s, e) => OnImagesDrop(e);
    }

    private void OnAddImages()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select images",
            Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*",
            Multiselect = true
        };

        if (ofd.ShowDialog() == DialogResult.OK)
            AddImages(ofd.FileNames);
    }

    private void OnImagesDrop(DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddImages(files.Where(IsValidImageFile));
        }
    }

    private void RenderGallery()
    {
        _gallery.Controls.Clear();

        foreach (var img in _images)
        {
            var thumbnail = CreateThumbnail(img);
            _gallery.Controls.Add(thumbnail);
        }

        _lblImageCount.Text = $"({_images.Count} image{(_images.Count != 1 ? "s" : "")})";
    }

    private Panel CreateThumbnail(GalleryImage galleryImage)
    {
        var container = new Panel
        {
            Size = new Size(THUMBNAIL_SIZE + 4, THUMBNAIL_SIZE + 50),
            Margin = new Padding(PADDING / 2),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };

        // Thumbnail image
        var picBox = new PictureBox
        {
            Image = galleryImage.Image,
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(THUMBNAIL_SIZE, THUMBNAIL_SIZE),
            Location = new Point(2, 2),
            Cursor = Cursors.Hand
        };

        // File name label
        var lblName = new Label
        {
            Text = Path.GetFileName(galleryImage.FilePath),
            AutoSize = false,
            Size = new Size(THUMBNAIL_SIZE, 40),
            Location = new Point(2, THUMBNAIL_SIZE + 2),
            Font = new Font(Font.FontFamily, 7),
            ForeColor = ThemeManager.TextSecondary,
            TextAlign = ContentAlignment.TopLeft
        };

        // Remove button
        var btnRemove = new Button
        {
            Text = "✕",
            Size = new Size(20, 20),
            Location = new Point(THUMBNAIL_SIZE - 20, 0),
            Font = new Font(Font.FontFamily, 9, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Red,
            Cursor = Cursors.Hand,
            Tag = galleryImage.Id
        };

        btnRemove.Click += (s, e) => RemoveImage(galleryImage.Id);

        container.Controls.AddRange([picBox, lblName, btnRemove]);
        container.Tag = galleryImage.Id;

        // Double-click to view full size
        picBox.DoubleClick += (s, e) => ViewFullSize(galleryImage.Image);

        return container;
    }

    private void RemoveImage(string id)
    {
        var toRemove = _images.FirstOrDefault(i => i.Id == id);
        if (toRemove != null)
        {
            _images.Remove(toRemove);
            toRemove.Image?.Dispose();
            RenderGallery();
            ImagesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ViewFullSize(System.Drawing.Image image)
    {
        var form = new Form
        {
            Text = "Image Preview",
            Size = new Size(800, 600),
            StartPosition = FormStartPosition.CenterScreen,
            BackColor = Color.Black
        };

        var picBox = new PictureBox
        {
            Image = image,
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill,
            BackColor = Color.Black
        };

        form.Controls.Add(picBox);
        form.ShowDialog();
    }

    private static bool IsValidImageFile(string filePath)
    {
        try
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            return validExtensions.Contains(Path.GetExtension(filePath).ToLower());
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>Represents a single image in the gallery.</summary>
public class GalleryImage
{
    public string Id { get; }
    public string FilePath { get; }
    public System.Drawing.Image Image { get; }

    public GalleryImage(string id, string filePath, System.Drawing.Image image)
    {
        Id = id;
        FilePath = filePath;
        Image = image;
    }
}
