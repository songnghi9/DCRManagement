using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views;

public partial class SideBySideWindow : FluentWindow
{
    private readonly IReadOnlyList<ImageThumbVm> _before;
    private readonly IReadOnlyList<ImageThumbVm> _after;

    private int    _beforeIndex  = 0;
    private int    _afterIndex   = 0;
    private double _zoomFactor   = 1.0;
    private bool   _syncingScroll = false;

    public SideBySideWindow(DCRImagePanelView beforePanel, DCRImagePanelView afterPanel)
    {
        InitializeComponent();

        _before = beforePanel.GetImages()
            .Select((t, i) => new ImageThumbVm
            {
                Index     = i,
                FileName  = t.FileName,
                Thumbnail = LoadBitmap(t.Data),
                FullData  = t.Data
            })
            .ToList();

        _after = afterPanel.GetImages()
            .Select((t, i) => new ImageThumbVm
            {
                Index     = i,
                FileName  = t.FileName,
                Thumbnail = LoadBitmap(t.Data),
                FullData  = t.Data
            })
            .ToList();

        // Set gallery titles from panel
        BeforeTitleText.Text = string.IsNullOrWhiteSpace(beforePanel.GalleryTitle)
            ? "Before" : beforePanel.GalleryTitle;
        AfterTitleText.Text = string.IsNullOrWhiteSpace(afterPanel.GalleryTitle)
            ? "After" : afterPanel.GalleryTitle;

        BeforeThumbStrip.ItemsSource = _before;
        AfterThumbStrip.ItemsSource  = _after;

        KeyDown += OnKeyDown;
        Loaded  += (_, _) =>
        {
            ShowBefore(0);
            ShowAfter(0);
            ZoomSlider.Value = 100;
        };
    }

    // ── Split presets ─────────────────────────────────────────────────────────

    private void Split5050_Click(object sender, RoutedEventArgs e) => SetSplit(1, 1);
    private void Split6040_Click(object sender, RoutedEventArgs e) => SetSplit(3, 2);
    private void Split4060_Click(object sender, RoutedEventArgs e) => SetSplit(2, 3);

    private void SetSplit(double before, double after)
    {
        BeforeCol.Width = new GridLength(before, GridUnitType.Star);
        AfterCol.Width  = new GridLength(after,  GridUnitType.Star);
    }

    // ── Zoom ──────────────────────────────────────────────────────────────────

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        _zoomFactor = e.NewValue / 100.0;
        ZoomLabel.Text = $"{(int)e.NewValue}%";
        ApplyZoom();
    }

    private void ZoomIn_Click(object sender,  RoutedEventArgs e) =>
        ZoomSlider.Value = Math.Min(ZoomSlider.Value + 25, 400);

    private void ZoomOut_Click(object sender, RoutedEventArgs e) =>
        ZoomSlider.Value = Math.Max(ZoomSlider.Value - 25, 10);

    private void ZoomFit_Click(object sender, RoutedEventArgs e)
    {
        // Fit based on Before image if available
        if (_before.Count > 0 && _beforeIndex < _before.Count)
        {
            var bmp = _before[_beforeIndex].Thumbnail;
            double pW = BeforeScroll.ActualWidth  > 0 ? BeforeScroll.ActualWidth  : 600;
            double pH = BeforeScroll.ActualHeight > 0 ? BeforeScroll.ActualHeight : 500;
            double fit = Math.Min(pW / bmp.PixelWidth, pH / bmp.PixelHeight) * 100;
            ZoomSlider.Value = Math.Max(10, Math.Min(400, fit));
        }
        else ZoomSlider.Value = 100;
    }

    private void ZoomActual_Click(object sender, RoutedEventArgs e) => ZoomSlider.Value = 100;

    private void ApplyZoom()
    {
        SetImageSize(BeforeImage);
        SetImageSize(AfterImage);
    }

    private void SetImageSize(System.Windows.Controls.Image img)
    {
        if (img.Source is BitmapImage bmp)
        {
            img.Width  = bmp.PixelWidth  * _zoomFactor;
            img.Height = bmp.PixelHeight * _zoomFactor;
        }
    }

    // ── Sync scroll ───────────────────────────────────────────────────────────

    private void BeforeScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_syncingScroll || SyncScrollCheck.IsChecked != true) return;
        _syncingScroll = true;
        AfterScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
        AfterScroll.ScrollToVerticalOffset(e.VerticalOffset);
        _syncingScroll = false;
    }

    private void AfterScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_syncingScroll || SyncScrollCheck.IsChecked != true) return;
        _syncingScroll = true;
        BeforeScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
        BeforeScroll.ScrollToVerticalOffset(e.VerticalOffset);
        _syncingScroll = false;
    }

    // ── Before navigation ─────────────────────────────────────────────────────

    private void BeforePrev_Click(object sender, RoutedEventArgs e) => ShowBefore(_beforeIndex - 1);
    private void BeforeNext_Click(object sender, RoutedEventArgs e) => ShowBefore(_beforeIndex + 1);

    private void BeforeThumb_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
            ShowBefore(idx);
    }

    private void ShowBefore(int index)
    {
        if (_before.Count == 0)
        {
            BeforeImage.Source = null;
            BeforeIndexText.Text = string.Empty;
            BeforePrev.Visibility = Visibility.Collapsed;
            BeforeNext.Visibility = Visibility.Collapsed;
            return;
        }

        _beforeIndex = Math.Max(0, Math.Min(index, _before.Count - 1));
        var vm = _before[_beforeIndex];
        BeforeImage.Source = vm.Thumbnail;
        SetImageSize(BeforeImage);

        BeforeIndexText.Text  = $"({_beforeIndex + 1} / {_before.Count})";
        BeforePrev.Visibility = _beforeIndex > 0                  ? Visibility.Visible : Visibility.Collapsed;
        BeforeNext.Visibility = _beforeIndex < _before.Count - 1  ? Visibility.Visible : Visibility.Collapsed;

        HighlightThumb(BeforeThumbStrip, _beforeIndex);
    }

    // ── After navigation ──────────────────────────────────────────────────────

    private void AfterPrev_Click(object sender, RoutedEventArgs e) => ShowAfter(_afterIndex - 1);
    private void AfterNext_Click(object sender, RoutedEventArgs e) => ShowAfter(_afterIndex + 1);

    private void AfterThumb_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
            ShowAfter(idx);
    }

    private void ShowAfter(int index)
    {
        if (_after.Count == 0)
        {
            AfterImage.Source = null;
            AfterIndexText.Text = string.Empty;
            AfterPrev.Visibility = Visibility.Collapsed;
            AfterNext.Visibility = Visibility.Collapsed;
            return;
        }

        _afterIndex = Math.Max(0, Math.Min(index, _after.Count - 1));
        var vm = _after[_afterIndex];
        AfterImage.Source = vm.Thumbnail;
        SetImageSize(AfterImage);

        AfterIndexText.Text  = $"({_afterIndex + 1} / {_after.Count})";
        AfterPrev.Visibility = _afterIndex > 0                 ? Visibility.Visible : Visibility.Collapsed;
        AfterNext.Visibility = _afterIndex < _after.Count - 1  ? Visibility.Visible : Visibility.Collapsed;

        HighlightThumb(AfterThumbStrip, _afterIndex);
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                ShowBefore(_beforeIndex - 1);
                ShowAfter(_afterIndex - 1);
                e.Handled = true;
                break;
            case Key.Right:
                ShowBefore(_beforeIndex + 1);
                ShowAfter(_afterIndex + 1);
                e.Handled = true;
                break;
            case Key.Add:
            case Key.OemPlus:
                ZoomIn_Click(this, e);
                e.Handled = true;
                break;
            case Key.Subtract:
            case Key.OemMinus:
                ZoomOut_Click(this, e);
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }

    // ── Thumbnail highlight ───────────────────────────────────────────────────

    private static void HighlightThumb(ItemsControl strip, int activeIndex)
    {
        for (int i = 0; i < strip.Items.Count; i++)
        {
            var cp = strip.ItemContainerGenerator.ContainerFromIndex(i)
                     as System.Windows.Controls.ContentPresenter;
            if (cp is null) continue;
            if (System.Windows.Media.VisualTreeHelper.GetChildrenCount(cp) == 0) continue;
            var border = System.Windows.Media.VisualTreeHelper.GetChild(cp, 0)
                         as System.Windows.Controls.Border;
            if (border is null) continue;
            border.BorderBrush = i == activeIndex
                ? System.Windows.Media.Brushes.DodgerBlue
                : System.Windows.Media.Brushes.Transparent;
        }
    }

    // ── Bitmap loader ─────────────────────────────────────────────────────────

    private static BitmapImage LoadBitmap(byte[] data)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.StreamSource  = new System.IO.MemoryStream(data);
        bmp.CacheOption   = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.None;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}
