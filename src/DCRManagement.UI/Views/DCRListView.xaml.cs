using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using System.Windows;
using System.Windows.Controls;
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
    }

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
            MessageBox.Show($"Failed to load DCR list: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (DcrGrid is null)
            return;

        var search = SearchTextBox?.Text?.Trim() ?? string.Empty;
        var status = (StatusComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Status";
        var priority = (PriorityComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Priority";

        var rows = _allDcrs.Where(d =>
        {
            var matchesSearch = string.IsNullOrWhiteSpace(search)
                || d.DCRNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                || d.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                || d.CreatedBy.Contains(search, StringComparison.OrdinalIgnoreCase);

            var matchesStatus = status == "All Status" || d.StatusDisplay == status;
            var matchesPriority = priority == "All Priority" || string.Equals(d.Priority, priority, StringComparison.OrdinalIgnoreCase);
            return matchesSearch && matchesStatus && matchesPriority;
        }).ToList();

        DcrGrid.ItemsSource = rows;
    }

    private DCRDto? SelectedDcr => DcrGrid.SelectedItem as DCRDto;

    private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilter();

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private void CreateButton_Click(object sender, RoutedEventArgs e) => CreateRequested?.Invoke(this, EventArgs.Empty);

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
}
