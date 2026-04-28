using DCRManagement.Application.Common;
using DCRManagement.UI.Common;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace DCRManagement.UI.Presenters;

public interface IMainView : IView
{
    void SetUserInfo(string fullName, string role);
    void NavigateTo(Control content);
    event EventHandler LogoutRequested;
    event EventHandler ShowDCRListRequested;
    event EventHandler ShowUserManagementRequested;
}

public class MainPresenter
{
    private readonly IMainView _view;
    private readonly ILogger<MainPresenter> _logger;

    public MainPresenter(IMainView view, ILogger<MainPresenter> logger)
    {
        _view = view;
        _logger = logger;

        _view.LogoutRequested += OnLogoutRequested;
        _view.ShowDCRListRequested += OnShowDCRList;
        _view.ShowUserManagementRequested += OnShowUserManagement;

        var session = SessionContext.Instance;
        _view.SetUserInfo(session.FullName, session.Role.ToString());
    }

    private void OnLogoutRequested(object? sender, EventArgs e)
    {
        if (!_view.Confirm("Are you sure you want to sign out?", "Sign Out"))
            return;

        SessionContext.Instance.Logout();
        _logger.LogInformation("User signed out");
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnShowDCRList(object? sender, EventArgs e) =>
        NavigateRequested?.Invoke(this, "DCRList");

    private void OnShowUserManagement(object? sender, EventArgs e) =>
        NavigateRequested?.Invoke(this, "UserManagement");

    public event EventHandler? LogoutRequested;
    public event EventHandler<string>? NavigateRequested;
}