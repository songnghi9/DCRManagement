using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Forms;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace DCRManagement.UI.Presenters;

public interface IUserManagementView : IView
{
    string SearchText { get; }
    string SelectedRole { get; }    // "All" or role name

    void BindUsers(IEnumerable<UserDto> users, int totalCount);
    void SetSelectedUser(UserDto? user);

    event EventHandler SearchRequested;
    event EventHandler FilterChanged;
    event EventHandler CreateRequested;
    event EventHandler<int> EditRequested;
    event EventHandler<int> ToggleActiveRequested;
    event EventHandler<int> ResetPasswordRequested;
    event EventHandler RefreshRequested;
}

public class UserManagementPresenter
{
    private readonly IUserManagementView _view;
    private readonly UserService _userService;
    private readonly ILogger<UserManagementPresenter> _logger;

    private List<UserDto> _allUsers = [];

    public UserManagementPresenter(
        IUserManagementView view,
        UserService userService,
        ILogger<UserManagementPresenter> logger)
    {
        _view = view;
        _userService = userService;
        _logger = logger;

        _view.SearchRequested += async (s, e) => ApplyFilter();
        _view.FilterChanged += async (s, e) => ApplyFilter();
        _view.RefreshRequested += async (s, e) => await LoadAsync();
        _view.CreateRequested += OnCreateRequested;
        _view.EditRequested += async (s, id) => await OnEditAsync(id);
        _view.ToggleActiveRequested += async (s, id) => await OnToggleActiveAsync(id);
        _view.ResetPasswordRequested += async (s, id) => await OnResetPasswordAsync(id);
    }

    public async Task LoadAsync()
    {
        _view.SetBusy(true);
        try
        {
            _allUsers = (await _userService.GetAllUsersAsync()).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            _view.ShowError("Failed to load users.", "Load Error");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    public event EventHandler? OpenCreateRequested;
    public event EventHandler<UserDto>? OpenEditRequested;

    // ─── Private ──────────────────────────────────────────────────────────────

    private void ApplyFilter()
    {
        var filtered = _allUsers.Where(u =>
        {
            var matchSearch = string.IsNullOrWhiteSpace(_view.SearchText)
                || u.Username.Contains(_view.SearchText, StringComparison.OrdinalIgnoreCase)
                || u.FullName.Contains(_view.SearchText, StringComparison.OrdinalIgnoreCase)
                || u.Email.Contains(_view.SearchText, StringComparison.OrdinalIgnoreCase);

            var matchRole = _view.SelectedRole == "All"
                || u.Role.ToString() == _view.SelectedRole;

            return matchSearch && matchRole;
        }).ToList();

        _view.BindUsers(filtered, filtered.Count);
    }

    private void OnCreateRequested(object? sender, EventArgs e) =>
        OpenCreateRequested?.Invoke(this, EventArgs.Empty);

    private async Task OnEditAsync(int userId)
    {
        var user = _allUsers.FirstOrDefault(u => u.Id == userId);
        if (user is not null)
            OpenEditRequested?.Invoke(this, user);
    }

    private async Task OnToggleActiveAsync(int userId)
    {
        var user = _allUsers.FirstOrDefault(u => u.Id == userId);
        if (user is null) return;

        var action = user.IsActive ? "deactivate" : "activate";
        if (!_view.Confirm($"Are you sure you want to {action} user '{user.FullName}'?",
                           "Confirm"))
            return;

        _view.SetBusy(true);
        try
        {
            var result = await _userService.UpdateUserAsync(new UpdateUserDto(
                user.Id, user.FullName, user.Email,
                user.Role, user.Department, !user.IsActive));

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Action Failed");
                return;
            }

            await LoadAsync();
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    private async Task OnResetPasswordAsync(int userId)
    {
        var user = _allUsers.FirstOrDefault(u => u.Id == userId);
        if (user is null) return;

        using var dlg = new ResetPasswordDialog(user.FullName);
        if (dlg.ShowDialog() != DialogResult.OK) return;

        _view.SetBusy(true);
        try
        {
            var result = await _userService.ChangePasswordAsync(
                userId,
                dlg.CurrentPassword,
                dlg.NewPassword);

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Reset Failed");
                return;
            }

            _view.ShowInfo("Password changed successfully.", "Done");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }
}