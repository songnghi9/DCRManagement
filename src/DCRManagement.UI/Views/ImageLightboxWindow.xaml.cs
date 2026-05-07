using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views;

public partial class ImageLightboxWindow : FluentWindow
{
    private readonly IReadOnlyList<ImageTileVm> _primary;
    private readonly IReadOnlyList<ImageTileVm>? _secondary;  // sibling gallery (Before/After)
    private int  _current;
    private bool _sideBySide;

    public ImageLightboxWindow(
        IReadOnlyList<ImageTileVm> primary,
        int startIndex,
        IReadOnlyList<ImageTileVm>? secondary)
    {
        InitializeComponent();
        _primary   = primary;
        _secondary = secondary;
        _current   = Math.Max(0, Math.Min(startIndex, primary.Count - 1));

        // Show side-by-side button only when sibling gallery has images
        if (_secondary is { Count: > 0 })
            SideBySideButton.Visibility = Visibility.Visible;

        ThumbStrip.ItemsSource = _primary;
        KeyDown += OnKeyDown;
        Loaded  += (_, _) => ShowImage(_current);
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void PrevBtn_Click(object sender, RoutedEventArgs e) => ShowImage(_current - 1);
    private void NextBtn_Click(object sender, RoutedEventArgs e) => ShowImage(_current + 1);

    private void Thumb_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
            ShowImage(idx);
    }

    private void SideBySideButton_Click(object sender, RoutedEventArgs e)
    {
        _sideBySide = !_sideBySide;
        SideBySideButton.Content = _sideBySide ? "Single View" : "Side by Side";

        SingleView.Visibility     = _sideBySide ? Visibility.Collapsed : Visibility.Visible;
        SideBySideView.Visibility = _sideBySide ? Visibility.Visible   : Visibility.Collapsed;

        if (_sideBySide) ShowSideBySide(_current);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
            case Key.Up:
                ShowImage(_current - 1);
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Down:
                ShowImage(_current + 1);
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }

    // ── Display ───────────────────────────────────────────────────────────────

    private void ShowImage(int index)
    {
        if (_primary.Count == 0) return;
        _current = Math.Max(0, Math.Min(index, _primary.Count - 1));

        var vm = _primary[_current];
        ImageNameText.Text  = vm.FileName;
        ImageIndexText.Text = $"({_current + 1} / {_primary.Count})";

        if (_sideBySide)
        {
            ShowSideBySide(_current);
        }
        else
        {
            MainImage.Source = vm.Thumbnail;
            PrevBtn.Visibility = _current > 0                   ? Visibility.Visible : Visibility.Collapsed;
            NextBtn.Visibility = _current < _primary.Count - 1  ? Visibility.Visible : Visibility.Collapsed;
        }

        HighlightThumb(_current);
    }

    private void ShowSideBySide(int index)
    {
        // Before = primary gallery at index
        BeforeImage.Source = _primary.Count > 0
            ? _primary[Math.Min(index, _primary.Count - 1)].Thumbnail
            : null;

        // After = secondary gallery at same index (or last available)
        AfterImage.Source = _secondary is { Count: > 0 }
            ? _secondary[Math.Min(index, _secondary.Count - 1)].Thumbnail
            : null;
    }

    private void HighlightThumb(int index)
    {
        for (int i = 0; i < ThumbStrip.Items.Count; i++)
        {
            var cp = ThumbStrip.ItemContainerGenerator.ContainerFromIndex(i)
                     as System.Windows.Controls.ContentPresenter;
            if (cp is null) continue;
            if (System.Windows.Media.VisualTreeHelper.GetChildrenCount(cp) == 0) continue;
            var border = System.Windows.Media.VisualTreeHelper.GetChild(cp, 0)
                         as System.Windows.Controls.Border;
            if (border is null) continue;
            border.BorderBrush = i == index
                ? (System.Windows.Media.Brush)FindResource("PrimaryBrush")
                : System.Windows.Media.Brushes.Transparent;
        }
    }
}
