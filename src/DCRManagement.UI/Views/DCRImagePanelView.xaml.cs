using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DCRManagement.UI.Views;

// ── Thumbnail view-model ──────────────────────────────────────────────────────

public sealed class ImageThumbVm : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isSelected;

    public int         Index     { get; set; }
    public int         ImageId   { get; init; }
    public string      FileName  { get; init; } = string.Empty;
    public BitmapImage Thumbnail { get; init; } = null!;
    public byte[]      FullData  { get; init; } = [];
    public double      ThumbSize { get; init; } = 64;

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}

// ── View mode ─────────────────────────────────────────────────────────────────

public enum ImageViewMode { Single, Grid2, Grid3, Strip }

// ── UserControl ───────────────────────────────────────────────────────────────

public partial class DCRImagePanelView : UserControl
{
    private readonly List<ImageThumbVm> _images = [];
    private int           _selectedIndex     = -1;
    private double        _zoomFactor        = 1.0;
    private bool          _thumbStripVisible = true;
    private ImageViewMode _viewMode          = ImageViewMode.Single;

    /// <summary>Set externally by DCRDetailView to enable side-by-side.</summary>
    public DCRImagePanelView? SiblingPanel { get; set; }

    public event EventHandler? ImagesChanged;

    public DCRImagePanelView()
    {
        InitializeComponent();
        KeyDown   += OnKeyDown;
        Focusable  = true;
        Loaded    += (_, _) => { ZoomSlider.Value = 100; UpdateTabHighlight(); };
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public string GalleryTitle
    {
        get => GalleryTitleText.Text;
        set => GalleryTitleText.Text = value;
    }

    public void LoadImages(IEnumerable<(string FileName, byte[] Data)> items)
    {
        _images.Clear();
        int idx = 0;
        foreach (var (name, data) in items)
            _images.Add(BuildVm(idx++, 0, name, data));
        Refresh();
        SelectImage(0);
    }

    public IReadOnlyList<(string FileName, byte[] Data)> GetImages() =>
        _images.Select(v => (v.FileName, v.FullData)).ToList();

    public void ClearImages() { _images.Clear(); Refresh(); }

    // ── Toolbar handlers ──────────────────────────────────────────────────────

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Title       = "Select images",
            Filter      = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif",
            Multiselect = true
        };
        if (ofd.ShowDialog() != true) return;
        foreach (var path in ofd.FileNames)
        {
            try { AddImageBytes(Path.GetFileName(path), File.ReadAllBytes(path)); }
            catch { /* skip bad files */ }
        }
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e) => PasteFromClipboard();
    private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteSelected();

    private void ExpandButton_Click(object sender, RoutedEventArgs e)
    {
        if (_images.Count == 0) return;
        OpenLightbox(_selectedIndex);
    }

    private void FullscreenSideBySide_Click(object sender, RoutedEventArgs e)
    {
        var sibling = FindSiblingPanel();
        if (sibling is null)
        {
            MessageBox.Show("Cannot find the paired image panel.", "Side-by-Side",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var win = new SideBySideWindow(this, sibling);
        win.Owner = Window.GetWindow(this);
        win.ShowDialog();
    }

    // ── View-mode tabs ────────────────────────────────────────────────────────

    private void TabSingle_Click(object sender, MouseButtonEventArgs e) => SetViewMode(ImageViewMode.Single);
    private void TabGrid2_Click(object sender, MouseButtonEventArgs e)  => SetViewMode(ImageViewMode.Grid2);
    private void TabGrid3_Click(object sender, MouseButtonEventArgs e)  => SetViewMode(ImageViewMode.Grid3);
    private void TabStrip_Click(object sender, MouseButtonEventArgs e)  => SetViewMode(ImageViewMode.Strip);

    private void SetViewMode(ImageViewMode mode)
    {
        _viewMode = mode;
        UpdateTabHighlight();
        RefreshContentArea();
    }

    private void UpdateTabHighlight()
    {
        var inactive   = (Brush)FindResource("BorderStrongBrush");
        var inactiveFg = (Brush)FindResource("MutedTextBrush");

        void Reset(Border b)
        {
            b.Background      = Brushes.Transparent;
            b.BorderBrush     = inactive;
            b.BorderThickness = new Thickness(1);
            foreach (var tb in FindVisualChildren<TextBlock>(b))                    tb.Foreground = inactiveFg;
            foreach (var ic in FindVisualChildren<Wpf.Ui.Controls.SymbolIcon>(b))  ic.Foreground = inactiveFg;
        }

        void Activate(Border b)
        {
            b.Background      = (Brush)FindResource("PrimaryBrush");
            b.BorderThickness = new Thickness(0);
            foreach (var tb in FindVisualChildren<TextBlock>(b))                    tb.Foreground = Brushes.White;
            foreach (var ic in FindVisualChildren<Wpf.Ui.Controls.SymbolIcon>(b))  ic.Foreground = Brushes.White;
        }

        Reset(TabSingle); Reset(TabGrid2); Reset(TabGrid3); Reset(TabStrip);
        switch (_viewMode)
        {
            case ImageViewMode.Single: Activate(TabSingle); break;
            case ImageViewMode.Grid2:  Activate(TabGrid2);  break;
            case ImageViewMode.Grid3:  Activate(TabGrid3);  break;
            case ImageViewMode.Strip:  Activate(TabStrip);  break;
        }
    }

    // ── Zoom ──────────────────────────────────────────────────────────────────

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        _zoomFactor = e.NewValue / 100.0;
        ZoomLabel.Text = $"{(int)e.NewValue}%";
        ApplyZoom();
    }

    private void ZoomIn_Click(object s,  RoutedEventArgs e) => ZoomSlider.Value = Math.Min(ZoomSlider.Value + 25, 300);
    private void ZoomOut_Click(object s, RoutedEventArgs e) => ZoomSlider.Value = Math.Max(ZoomSlider.Value - 25,  20);

    private void ZoomFit_Click(object s, RoutedEventArgs e)
    {
        if (_viewMode == ImageViewMode.Single && _images.Count > 0 && _selectedIndex >= 0)
        {
            var bmp  = _images[_selectedIndex].Thumbnail;
            double pW = MainScrollViewer.ActualWidth  > 0 ? MainScrollViewer.ActualWidth  : ActualWidth;
            double pH = MainScrollViewer.ActualHeight > 0 ? MainScrollViewer.ActualHeight : ActualHeight;
            double fit = Math.Min(pW / bmp.PixelWidth, pH / bmp.PixelHeight) * 100;
            ZoomSlider.Value = Math.Max(20, Math.Min(300, fit));
        }
        else ZoomSlider.Value = 100;
    }

    private void ZoomActual_Click(object s, RoutedEventArgs e) => ZoomSlider.Value = 100;

    private void ApplyZoom()
    {
        switch (_viewMode)
        {
            case ImageViewMode.Single:
                if (MainImage.Source is BitmapImage bmp)
                {
                    MainImage.Width  = bmp.PixelWidth  * _zoomFactor;
                    MainImage.Height = bmp.PixelHeight * _zoomFactor;
                }
                break;
            case ImageViewMode.Grid2:
            case ImageViewMode.Grid3:
                RefreshGridView();
                break;
            case ImageViewMode.Strip:
                RefreshStripView();
                break;
        }
    }

    // ── Thumb strip toggle ────────────────────────────────────────────────────

    private void ToggleThumbStrip_Click(object sender, RoutedEventArgs e)
    {
        _thumbStripVisible = !_thumbStripVisible;
        ThumbStripBorder.Visibility = _thumbStripVisible ? Visibility.Visible   : Visibility.Collapsed;
        ThumbStripRow.Height        = _thumbStripVisible ? new GridLength(76)   : new GridLength(0);
    }

    // ── Multi-select checkbox ─────────────────────────────────────────────────

    private void ThumbCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        int count = _images.Count(i => i.IsSelected);
        SelectionHint.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        SelectionHint.Text       = count > 0 ? $"{count} selected" : string.Empty;
        DeleteButton.IsEnabled   = count > 0 || _selectedIndex >= 0;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void PrevButton_Click(object sender, RoutedEventArgs e) => SelectImage(_selectedIndex - 1);
    private void NextButton_Click(object sender, RoutedEventArgs e) => SelectImage(_selectedIndex + 1);

    private void Thumbnail_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
        {
            if (e.ClickCount >= 2) OpenLightbox(idx);
            else                   SelectImage(idx);
        }
    }

    private void MainImage_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2) OpenLightbox(_selectedIndex);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:  case Key.Up:    SelectImage(_selectedIndex - 1); e.Handled = true; break;
            case Key.Right: case Key.Down:  SelectImage(_selectedIndex + 1); e.Handled = true; break;
            case Key.Delete:                DeleteSelected();                 e.Handled = true; break;
            case Key.V when Keyboard.Modifiers == ModifierKeys.Control:
                PasteFromClipboard(); e.Handled = true; break;
            case Key.Add:      case Key.OemPlus:   ZoomIn_Click(this, e);  e.Handled = true; break;
            case Key.Subtract: case Key.OemMinus:  ZoomOut_Click(this, e); e.Handled = true; break;
        }
    }

    // ── Core helpers ──────────────────────────────────────────────────────────

    private void AddImageBytes(string fileName, byte[] data)
    {
        _images.Add(BuildVm(_images.Count, 0, fileName, data));
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

    private void DeleteSelected()
    {
        var toDelete = _images.Where(i => i.IsSelected).Select(i => i.Index).ToList();
        if (toDelete.Count == 0 && _selectedIndex >= 0) toDelete.Add(_selectedIndex);
        foreach (var idx in toDelete.OrderByDescending(x => x))
            if (idx < _images.Count) _images.RemoveAt(idx);
        Refresh();
        SelectImage(Math.Min(_selectedIndex, _images.Count - 1));
        ImagesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SelectImage(int index)
    {
        if (_images.Count == 0) { _selectedIndex = -1; UpdateMainViewer(); return; }
        _selectedIndex = Math.Max(0, Math.Min(index, _images.Count - 1));
        UpdateMainViewer();
        HighlightThumbnail(_selectedIndex);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void Refresh()
    {
        for (int i = 0; i < _images.Count; i++) _images[i].Index = i;

        ThumbnailList.ItemsSource = null;
        ThumbnailList.ItemsSource = _images;

        CountText.Text = _images.Count.ToString();
        CountBadge.Background = _images.Count > 0
            ? (Brush)FindResource("PrimaryBrush")
            : (Brush)FindResource("MutedTextBrush");

        RefreshContentArea();
        DeleteButton.IsEnabled = _selectedIndex >= 0 && _images.Count > 0;
    }

    private void RefreshContentArea()
    {
        SingleViewPanel.Visibility = Visibility.Collapsed;
        GridViewPanel.Visibility   = Visibility.Collapsed;
        StripViewPanel.Visibility  = Visibility.Collapsed;

        switch (_viewMode)
        {
            case ImageViewMode.Single:
                SingleViewPanel.Visibility = Visibility.Visible;
                UpdateMainViewer();
                break;
            case ImageViewMode.Grid2:
            case ImageViewMode.Grid3:
                GridViewPanel.Visibility = Visibility.Visible;
                RefreshGridView();
                break;
            case ImageViewMode.Strip:
                StripViewPanel.Visibility = Visibility.Visible;
                RefreshStripView();
                break;
        }
    }

    // ── Single viewer ─────────────────────────────────────────────────────────

    private void UpdateMainViewer()
    {
        bool has = _images.Count > 0 && _selectedIndex >= 0;

        EmptyState.Visibility       = has ? Visibility.Collapsed : Visibility.Visible;
        MainScrollViewer.Visibility = has ? Visibility.Visible   : Visibility.Collapsed;
        PrevButton.Visibility       = has && _images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        NextButton.Visibility       = has && _images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        IndexBadge.Visibility       = has && _images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        DeleteButton.IsEnabled      = has;

        if (!has) { MainImage.Source = null; return; }

        var vm = _images[_selectedIndex];
        MainImage.Source = vm.Thumbnail;
        MainImage.Width  = vm.Thumbnail.PixelWidth  * _zoomFactor;
        MainImage.Height = vm.Thumbnail.PixelHeight * _zoomFactor;
        IndexText.Text   = $"{_selectedIndex + 1} / {_images.Count}";
    }

    // ── Grid viewer ───────────────────────────────────────────────────────────

    private void RefreshGridView()
    {
        GridWrap.Children.Clear();
        if (_images.Count == 0) return;

        int    cols  = _viewMode == ImageViewMode.Grid2 ? 2 : 3;
        double avail = ActualWidth > 20 ? ActualWidth - 20 : 300;
        double cell  = Math.Max(80, (avail / cols - 8) * _zoomFactor);

        foreach (var vm in _images)
        {
            var border = new Border
            {
                Width           = cell, Height = cell,
                Margin          = new Thickness(4),
                CornerRadius    = new CornerRadius(8),
                BorderThickness = new Thickness(2),
                BorderBrush     = _selectedIndex == vm.Index
                    ? (Brush)FindResource("PrimaryBrush") : Brushes.Transparent,
                Background   = new SolidColorBrush(Color.FromRgb(0xDD, 0xE5, 0xEF)),
                ClipToBounds = true,
                Cursor       = Cursors.Hand,
                Tag          = vm.Index
            };

            var img = new System.Windows.Controls.Image
            {
                Source  = vm.Thumbnail,
                Stretch = Stretch.UniformToFill
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
            var label = new TextBlock
            {
                Text = (vm.Index + 1).ToString(),
                FontSize = 10, FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0)),
                Padding = new Thickness(4, 2, 4, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Top
            };
            var grid = new Grid();
            grid.Children.Add(img);
            grid.Children.Add(label);
            border.Child = grid;

            border.MouseLeftButtonDown += (s, ev) =>
            {
                int idx = (int)((FrameworkElement)s).Tag;
                if (ev.ClickCount >= 2) OpenLightbox(idx);
                else                    SelectImage(idx);
            };
            GridWrap.Children.Add(border);
        }
    }

    // ── Strip viewer ──────────────────────────────────────────────────────────

    private void RefreshStripView()
    {
        StripStack.Children.Clear();
        if (_images.Count == 0) return;

        double h = Math.Max(80, 180 * _zoomFactor);

        foreach (var vm in _images)
        {
            double aspect = vm.Thumbnail.PixelWidth / (double)vm.Thumbnail.PixelHeight;
            double w = h * aspect;

            var border = new Border
            {
                Width = w, Height = h,
                Margin          = new Thickness(0, 0, 8, 0),
                CornerRadius    = new CornerRadius(6),
                BorderThickness = new Thickness(2),
                BorderBrush     = _selectedIndex == vm.Index
                    ? (Brush)FindResource("PrimaryBrush") : Brushes.Transparent,
                Background   = new SolidColorBrush(Color.FromRgb(0xDD, 0xE5, 0xEF)),
                ClipToBounds = true,
                Cursor       = Cursors.Hand,
                Tag          = vm.Index
            };
            var stripImg = new System.Windows.Controls.Image
            {
                Source  = vm.Thumbnail,
                Stretch = Stretch.UniformToFill
            };
            RenderOptions.SetBitmapScalingMode(stripImg, BitmapScalingMode.HighQuality);
            border.Child = stripImg;
            border.MouseLeftButtonDown += (s, ev) =>
            {
                int idx = (int)((FrameworkElement)s).Tag;
                if (ev.ClickCount >= 2) OpenLightbox(idx);
                else                    SelectImage(idx);
            };
            StripStack.Children.Add(border);
        }
    }

    // ── Thumbnail strip highlight ─────────────────────────────────────────────

    private void HighlightThumbnail(int index)
    {
        for (int i = 0; i < ThumbnailList.Items.Count; i++)
        {
            var cp = ThumbnailList.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
            if (cp is null) continue;
            if (VisualTreeHelper.GetChildrenCount(cp) == 0) continue;
            var root   = VisualTreeHelper.GetChild(cp, 0) as Grid;
            var border = root?.Children.OfType<Border>().FirstOrDefault();
            if (border is null) continue;
            border.BorderBrush = i == index
                ? (Brush)FindResource("PrimaryBrush") : Brushes.Transparent;
        }
    }

    // ── Lightbox ──────────────────────────────────────────────────────────────

    private void OpenLightbox(int startIndex)
    {
        if (_images.Count == 0) return;
        var lb = new ImageLightboxWindow(_images, startIndex);
        lb.Owner = Window.GetWindow(this);
        lb.ShowDialog();
    }

    // ── Sibling panel ─────────────────────────────────────────────────────────

    private DCRImagePanelView? FindSiblingPanel()
    {
        if (SiblingPanel is not null) return SiblingPanel;
        var detail = FindParent<DCRDetailView>(this);
        if (detail is null) return null;
        return ReferenceEquals(this, detail.BeforePanel)
            ? detail.AfterPanel
            : detail.BeforePanel;
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    private static ImageThumbVm BuildVm(int index, int dbId, string fileName, byte[] data) =>
        new()
        {
            Index     = index,
            ImageId   = dbId,
            FileName  = fileName,
            Thumbnail = LoadBitmap(data),
            FullData  = data,
            ThumbSize = 64
        };

    private static BitmapImage LoadBitmap(byte[] data)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.StreamSource  = new MemoryStream(data);
        bmp.CacheOption   = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.None;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    // ── Visual tree helpers ───────────────────────────────────────────────────

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        if (parent is null) yield break;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) yield return typed;
            foreach (var d in FindVisualChildren<T>(child)) yield return d;
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null)
        {
            if (parent is T t) return t;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}
