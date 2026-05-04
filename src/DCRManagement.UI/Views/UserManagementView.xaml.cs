using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using System.Windows;
using System.Windows.Controls;

namespace DCRManagement.UI.Views;

public partial class UserManagementView : UserControl
{
    private readonly UserService _userService;
    private List<UserDto> _allUsers = [];

    public UserManagementView(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    public async Task LoadAsync()
    {
        try
        {
            _allUsers = (await _userService.GetAllUsersAsync())
                .OrderBy(u => u.FullName)
                .ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load users: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (UserGrid is null)
            return;

        var search = SearchTextBox?.Text?.Trim() ?? string.Empty;
        var role = (RoleComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Roles";

        UserGrid.ItemsSource = _allUsers.Where(u =>
        {
            var matchesSearch = string.IsNullOrWhiteSpace(search)
                || u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || u.Username.Contains(search, StringComparison.OrdinalIgnoreCase)
                || u.Email.Contains(search, StringComparison.OrdinalIgnoreCase);
            var matchesRole = role == "All Roles" || u.Role.ToString() == role;
            return matchesSearch && matchesRole;
        }).ToList();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private void Filter_Changed(object sender, RoutedEventArgs e)
    {
        ApplyFilter();
    }
}
