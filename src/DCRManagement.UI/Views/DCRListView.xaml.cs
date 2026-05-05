using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DCRManagement.UI.Views;

public partial class DCRListView : UserControl
{
    private readonly DCRService _dcrService;
    private List<DCRDto> _allDcrs = [];

    public event EventHandler? CreateRequested;
    public event EventHandler<int>? ViewRequested;
    public event EventHandler<int>? EditRequested;

    public DCRListView(DCRService dcrService)
    {
        InitializeComponent();
        _dcrService = dcrService;

        // AlternationCount must be set for row index to work
        DcrGrid.AlternationCount = int.MaxValue;
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        try
        {
            _allDcrs = (await _dcrService.GetAllForCurrentUserAsync())
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load DCR list: {ex.Message}",
                "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (DcrGrid is null) return;

        var search   = SearchTextBox?.Text?.Trim() ?? string.Empty;
        var status   = (StatusComboBox?.SelectedItem   as ComboBoxItem)?.Content?.ToString() ?? "All Status";
        var priority = (PriorityComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Priority";

        DcrGrid.ItemsSource = _allDcrs.Where(d =>
        {
            var matchSearch   = string.IsNullOrWhiteSpace(search)
                || d.DCRNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || d.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                || d.CreatedBy.Contains(search, StringComparison.OrdinalIgnoreCase);
            var matchStatus   = status   == "All Status"   || d.StatusDisplay == status;
            var matchPriority = priority == "All Priority"
                || string.Equals(d.Priority, priority, StringComparison.OrdinalIgnoreCase);
            return matchSearch && matchStatus && matchPriority;
        }).ToList();

        UpdateSelectionBar();
    }

    // ── Selection helpers ─────────────────────────────────────────────────────

    private DCRDto? SelectedDcr => DcrGrid.SelectedItem as DCRDto;

    private IEnumerable<DCRDto> SelectedDcrs =>
        DcrGrid.SelectedItems.OfType<DCRDto>();

    private void UpdateSelectionBar()
    {
        var count = DcrGrid.SelectedItems.Count;
        SelectionBar.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        SelectionCountText.Text = count == 1 ? "1 item selected" : $"{count} items selected";
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilter();

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private void CreateButton_Click(object sender, RoutedEventArgs e)
        => CreateRequested?.Invoke(this, EventArgs.Empty);

    private void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedDcr is not null)
            ViewRequested?.Invoke(this, SelectedDcr.Id);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedDcr is not null)
            EditRequested?.Invoke(this, SelectedDcr.Id);
    }

    private void DcrGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedDcr is not null)
            ViewRequested?.Invoke(this, SelectedDcr.Id);
    }

    private void DcrGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => UpdateSelectionBar();

    // ── Checkbox handlers ─────────────────────────────────────────────────────

    /// <summary>Row checkbox clicked — sync DataGrid selection with checkbox state.</summary>
    private void RowCheckBox_Click(object sender, RoutedEventArgs e)
    {
        // Selection is already bound two-way via IsSelected on DataGridRow.
        // Just update the summary bar.
        UpdateSelectionBar();
    }

    /// <summary>Header checkbox — select or deselect all visible rows.</summary>
    private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox cb) return;

        if (cb.IsChecked == true)
            DcrGrid.SelectAll();
        else
            DcrGrid.UnselectAll();

        UpdateSelectionBar();
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        DcrGrid.SelectAll();
        UpdateSelectionBar();
    }

    private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        DcrGrid.UnselectAll();
        UpdateSelectionBar();
    }
}
