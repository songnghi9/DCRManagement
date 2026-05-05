using DCRManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views;

// ── Thumbnail view model ──────────────────────────────────────────────────────

public sealed record ImageThumbVm
{
    public int       Index     { get; init; }
    public int       ImageId   { get; init; }   // DB id (0 = not yet saved)
    public string    FileName  { get; init; } = string.Empty;
    public BitmapImage Thumbnail { get; init; } = null!;
    public byte[]    FullData  { get; init; } = [];  // full-res bytes
}

// ── UserControl ───────────────────────────────────────────────────────────────

public partial class DCRImagePanelView : UserControl
{
    private readonly List<ImageThumbVm> _images = [];
    private int _selectedIndex = -1;

    // Raised when images are added/removed so DCRDetailView can mark dirty
    public event EventHandler? ImagesChanged;

    public DCRImagePanelView()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
        Focusable = true;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public string GalleryTitle
    {
        get => GalleryTitleText.Text;
        set => GalleryTitleText.Text = value;
    }

    /// <summary>Load images from byte arrays (called by DCRDetailView after DB load).</summary>
    public void LoadImages(IEnumerable<(string FileName, byte[] Data)> items)
    {
        _images.Clear();
        int idx = 0;
        foreach (var (name, data) in items)
            _images.Add(BuildVm(idx++, 0, name, data));

        Refresh();
        SelectImage(0);
    }

    /// <summary>Returns all current images as (FileName, Data) pairs for saving.</summary>
    public IReadOnlyList<(string FileName, byte[] Data)> GetImages() =>
        _images.Select(v => (v.FileName, v.FullData)).ToList();

    public void ClearImages()
    {
        _images.Clear();
        Refresh();
    }

    // ── Toolbar handlers ──────────────────────────────────────────────────────

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            Title       = "Select images",
            Filter      = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif",
            Multiselect = true
        };
        if (ofd.ShowDialog() != true) return;

        foreach (var path in ofd.FileNames)
        {
            try
            {
                var data = File.ReadAllBytes(path);
                AddImageBytes(Path.GetFileName(path), data);
            }
            catch { /* skip bad files */ }
        }
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e) => PasteFromClipboard();

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _images.Count) return;
        _images.RemoveAt(_selectedIndex);
        Refresh();
        SelectImage(Math.Min(_selectedIndex, _images.Count - 1));
        ImagesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ExpandButton_Click(object sender, RoutedEventArgs e)
    {
        if (_images.Count == 0) return;
        OpenLightbox(_selectedIndex);
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void PrevButton_Click(object sender, RoutedEventArgs e) =>
        SelectImage(_selectedIndex - 1);

    private void NextButton_Click(object sender, RoutedEventArgs e) =>
        SelectImage(_selectedIndex + 1);

    private void Thumbnail_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
            SelectImage(idx);
    }

    private void MainImage_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2) OpenLightbox(_selectedIndex);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Left  || e.Key == Keys.Up)   { SelectImage(_selectedIndex - 1); e.Handled = true; }
        if (e.Key == Keys.Right || e.Key == Keys.Down)  { SelectImage(_selectedIndex + 1); e.Handled = true; }
        if (e.Key == Keys.Delete)                        { DeleteButton_Click(sender, e);   e.Handled = true; }
        if (e.Key == Keys.V && Keyboard.Modifiers == ModifierKeys.Control)
        { PasteFromClipboard(); e.Handled = true; }
    }

    // ── Core helpers ──────────────────────────────────────────────────────────

    private void AddImageBytes(string fileName, byte[] data)
    {
        var vm = BuildVm(_images.Count, 0, fileName, data);
        _images.Add(vm);
        Refresh();
        SelectImage(_images.Count - 1);
        ImagesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PasteFromClipboard()
    {
        if (!Clipboard.ContainsImage()) return;
        try
        {
            var src = Clipboard.GetImage();
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(src));
            using var ms = new MemoryStream();
            enc.Save(ms);
            AddImageBytes($"Paste_{DateTime.Now:yyyyMMdd_HHmmss}.png", ms.ToArray());
        }
        catch { /* ignore */ }
    }

    private void SelectImage(int index)
    {
        if (_images.Count == 0) { _selectedIndex = -1; UpdateMainViewer(); return; }
        _selectedIndex = Math.Max(0, Math.Min(index, _images.Count - 1));
        UpdateMainViewer();
        HighlightThumbnail(_selectedIndex);
    }

    private void UpdateMainViewer()
    {
        bool hasImages = _images.Count > 0 && _selectedIndex >= 0;

        EmptyState.Visibility  = hasImages ? Visibility.Collapsed : Visibility.Visible;
        MainImage.Visibility   = hasImages ? Visibility.Visible   : Visibility.Collapsed;
        PrevButton.Visibility  = hasImages && _images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        NextButton.Visibility  = hasImages && _images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        IndexBadge.Visibility  = hasImages && _images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        DeleteButton.IsEnabled = hasImages;

        if (!hasImages) { MainImage.Source = null; return; }

        var vm = _images[_selectedIndex];
        MainImage.Source = vm.Thumbnail;   // use full-res for main view
        IndexText.Text   = $"{_selectedIndex + 1} / {_images.Count}";
    }

    private void HighlightThumbnail(int index)
    {
        // Walk ItemsControl visual tree to update border colors
        for (int i = 0; i < ThumbnailList.Items.Count; i++)
        {
            var container = ThumbnailList.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not ContentPresenter cp) continue;
            if (VisualTreeHelper.GetChildrenCount(cp) == 0) continue;
            var border = VisualTreeHelper.GetChild(cp, 0) as Border;
            if (border is null) continue;
            border.BorderBrush = i == index
                ? (Brush)FindResource("PrimaryBrush")
                : Brushes.Transparent;
        }
    }

    private void Refresh()
    {
        // Rebuild index on each vm
        for (int i = 0; i < _images.Count; i++)
        {
            var old = _images[i];
            _images[i] = old with { Index = i };
        }

        ThumbnailList.ItemsSource = null;
        ThumbnailList.ItemsSource = _images;

        CountText.Text = _images.Count.ToString();
        CountBadge.Background = _images.Count > 0
            ? (Brush)FindResource("PrimaryBrush")
            : (Brush)FindResource("MutedTextBrush");
    }

    // ── Lightbox popup ────────────────────────────────────────────────────────

    private void OpenLightbox(int startIndex)
    {
        if (_images.Count == 0) return;
        var lb = new ImageLightboxWindow(_images, startIndex);
        lb.Owner = Window.GetWindow(this);
        lb.ShowDialog();
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    private static ImageThumbVm BuildVm(int index, int dbId, string fileName, byte[] data)
    {
        var bmp = LoadBitmap(data);
        return new ImageThumbVm
        {
            Index     = index,
            ImageId   = dbId,
            FileName  = fileName,
            Thumbnail = bmp,
            FullData  = data
        };
    }

    private static BitmapImage LoadBitmap(byte[] data)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.StreamSource    = new MemoryStream(data);
        bmp.CacheOption     = BitmapCacheOption.OnLoad;
        bmp.CreateOptions   = BitmapCreateOptions.None;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}

// ── Keyboard key alias (avoid System.Windows.Forms dependency) ────────────────
file static class Keys
{
    public const Key Left   = Key.Left;
    public const Key Right  = Key.Right;
    public const Key Up     = Key.Up;
    public const Key Down   = Key.Down;
    public const Key Delete = Key.Delete;
    public const Key V      = Key.V;
}
