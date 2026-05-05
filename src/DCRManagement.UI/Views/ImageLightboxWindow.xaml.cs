using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views;

public partial class ImageLightboxWindow : FluentWindow
{
    private readonly IReadOnlyList<ImageThumbVm> _images;
    private int _current;

    public ImageLightboxWindow(IReadOnlyList<ImageThumbVm> images, int startIndex)
    {
        InitializeComponent();
        _images  = images;
        _current = Math.Max(0, Math.Min(startIndex, images.Count - 1));

        ThumbStrip.ItemsSource = _images;
        KeyDown += OnKeyDown;
        Loaded  += (_, _) => ShowImage(_current);
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void PrevBtn_Click(object sender, RoutedEventArgs e) => ShowImage(_current - 1);
    private void NextBtn_Click(object sender, RoutedEventArgs e) => ShowImage(_current + 1);

    private void Thumb_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is int idx)
            ShowImage(idx);
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
        if (_images.Count == 0) return;
        _current = Math.Max(0, Math.Min(index, _images.Count - 1));

        var vm = _images[_current];
        MainImage.Source    = vm.Thumbnail;
        ImageNameText.Text  = vm.FileName;
        ImageIndexText.Text = $"({_current + 1} / {_images.Count})";

        PrevBtn.Visibility = _current > 0                  ? Visibility.Visible : Visibility.Collapsed;
        NextBtn.Visibility = _current < _images.Count - 1  ? Visibility.Visible : Visibility.Collapsed;

        HighlightThumb(_current);
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
