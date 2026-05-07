using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views;

// ── Tile view model ───────────────────────────────────────────────────────────

public sealed class ImageTileVm : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isSelected;
    private double _tileSize = 140;

    public int         Index        { get; set; }
    public string      FileName     { get; init; } = string.Empty;
    public BitmapImage Thumbnail    { get; init; } = null!;
    public byte[]      FullData     { get; init; } = [];
    public string      DisplayIndex => (Index + 1).ToString();

    public double TileSize
    {
        get => _tileSize;
        set { _tileSize = value; OnPropertyChanged(nameof(TileSize)); }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
}

// ── UserControl ───────────────────────────────────────────────────────────────

public partial class DCRImagePanelView : UserControl
{
    private readonly List<ImageTileVm> _tiles = [];
    private int    _selectedIndex = -1;
    private double _tileSize      = 140;

    /// <summary>Raised when images are added/removed — parent can mark dirty.</summary>
    public event EventHandler? ImagesChanged;

    /// <summary>Gallery label shown in the toolbar (e.g. "Before" / "After").</summary>
    public string GalleryTitle
    {
        get => GalleryTitleText.Text;
        set => GalleryTitleText.Text = value;
    }

    public DCRImagePanelView()
    {
        InitializeComponent();
        Focusable = true;
        KeyDown  += OnKeyDown;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Load images from byte arrays (called after DB load).</summary>
    public void LoadImages(IEnumerable<(string FileName, byte[] Data)> items)
    {
        _tiles.Clear();
        int i = 0;
        foreach (var (name, data) in items)
            _tiles.Add(BuildTile(i++, name, data));
        Refresh();
        Select(0);
    }

    /// <summary>Returns all current images for saving.</summary>
    public IReadOnlyList<(string FileName, byte[] Data)> GetImages() =>
        _tiles.Select(t => (t.FileName, t.FullData)).ToList();

    public void ClearImages()
    {
        _tiles.Clear();
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
            try { AddBytes(System.IO.Path.GetFileName(path), File.ReadAllBytes(path)); }
            catch { /* skip bad files */ }
        }
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e) => PasteClipboard();

    private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteSelected();

    private void ExpandButton_Click(object sender, RoutedEventArgs e)
    {
        // Find sibling DCRImagePanelView (Before/After) for side-by-side
        var sibling = FindSiblingPanel();
        var lb = new ImageLightboxWindow(_tiles, _selectedIndex, sibling?._tiles);
        lb.Owner = Window.GetWindow(this);
        lb.ShowDialog();
    }

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _tileSize = e.NewValue;
        foreach (var t in _tiles) t.TileSize = _tileSize;
    }

    // ── Tile interaction ──────────────────────────────────────────────────────

    private void Tile_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
        {
            Select(idx);
            if (e.ClickCount >= 2)
            {
                var lb = new ImageLightboxWindow(_tiles, idx, null);
                lb.Owner = Window.GetWindow(this);
                lb.ShowDialog();
            }
        }
    }

    private void TileDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
        {
            e.Handled = true;
            _tiles.RemoveAt(idx);
            Refresh();
            Select(Math.Min(idx, _tiles.Count - 1));
            ImagesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
            case Key.Up:
                Select(_selectedIndex - 1);
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Down:
                Select(_selectedIndex + 1);
                e.Handled = true;
                break;
            case Key.Delete:
                DeleteSelected();
                e.Handled = true;
                break;
            case Key.V when Keyboard.Modifiers == ModifierKeys.Control:
                PasteClipboard();
                e.Handled = true;
                break;
            case Key.Enter:
            case Key.Space:
                if (_selectedIndex >= 0)
                {
                    var lb = new ImageLightboxWindow(_tiles, _selectedIndex, null);
                    lb.Owner = Window.GetWindow(this);
                    lb.ShowDialog();
                }
                e.Handled = true;
                break;
        }
    }

    // ── Core helpers ──────────────────────────────────────────────────────────

    private void AddBytes(string fileName, byte[] data)
    {
        _tiles.Add(BuildTile(_tiles.Count, fileName, data));
        Refresh();
        Select(_tiles.Count - 1);
        ImagesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PasteClipboard()
    {
        if (!Clipboard.ContainsImage()) return;
        try
        {
            var src = Clipboard.GetImage();
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(src));
            using var ms = new MemoryStream();
            enc.Save(ms);
            AddBytes($"Paste_{DateTime.Now:yyyyMMdd_HHmmss}.png", ms.ToArray());
        }
        catch { /* ignore */ }
    }

    private void DeleteSelected()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _tiles.Count) return;
        _tiles.RemoveAt(_selectedIndex);
        Refresh();
        Select(Math.Min(_selectedIndex, _tiles.Count - 1));
        ImagesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Select(int index)
    {
        foreach (var t in _tiles) t.IsSelected = false;
        if (_tiles.Count == 0) { _selectedIndex = -1; UpdateDeleteButton(); return; }
        _selectedIndex = Math.Max(0, Math.Min(index, _tiles.Count - 1));
        _tiles[_selectedIndex].IsSelected = true;
        UpdateDeleteButton();
    }

    private void UpdateDeleteButton() =>
        DeleteButton.IsEnabled = _selectedIndex >= 0 && _tiles.Count > 0;

    private void Refresh()
    {
        // Re-index
        for (int i = 0; i < _tiles.Count; i++)
        {
            _tiles[i].Index    = i;
            _tiles[i].TileSize = _tileSize;
        }

        bool hasImages = _tiles.Count > 0;
        EmptyState.Visibility = hasImages ? Visibility.Collapsed : Visibility.Visible;
        ImageGrid.Visibility  = hasImages ? Visibility.Visible   : Visibility.Collapsed;

        ImageGrid.ItemsSource = null;
        ImageGrid.ItemsSource = _tiles;

        CountText.Text = _tiles.Count.ToString();
        CountBadge.Background = _tiles.Count > 0
            ? (System.Windows.Media.Brush)FindResource("PrimaryBrush")
            : (System.Windows.Media.Brush)FindResource("MutedTextBrush");
    }

    /// <summary>
    /// Finds the sibling DCRImagePanelView in the same parent Grid
    /// (used for side-by-side fullscreen).
    /// </summary>
    private DCRImagePanelView? FindSiblingPanel()
    {
        if (Parent is not System.Windows.Controls.Panel panel) return null;
        return panel.Children.OfType<DCRImagePanelView>()
                              .FirstOrDefault(p => p != this);
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    private ImageTileVm BuildTile(int index, string fileName, byte[] data)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.StreamSource  = new MemoryStream(data);
        bmp.CacheOption   = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.None;
        bmp.EndInit();
        bmp.Freeze();

        return new ImageTileVm
        {
            Index    = index,
            FileName = fileName,
            Thumbnail = bmp,
            FullData  = data,
            TileSize  = _tileSize
        };
    }
}
