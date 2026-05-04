using DCRManagement.Application.Common;
using DCRManagement.Application.Services;
using DCRManagement.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Wpf.Ui.Controls;

namespace DCRManagement.UI;

public partial class MainWindow : FluentWindow
{
    private readonly IServiceProvider _services;
    private readonly AuthService _authService;
    private bool _sidebarExpanded = true;

    public MainWindow(IServiceProvider services, AuthService authService)
    {
        InitializeComponent();
        _services = services;
        _authService = authService;
        SetUserInfo();
        Loaded += async (_, _) => await ShowDcrListAsync();
    }

    private void SetUserInfo()
    {
        var session = SessionContext.Instance;
        UserNameText.Text = session.FullName;
        UserRoleText.Text = session.Role.ToString();
        CreateDcrButton.IsEnabled = session.CanCreateDCR;
        UsersButton.Visibility = session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task ShowDcrListAsync()
    {
        PageTitleText.Text = "DCR Requests";
        PageSubtitleText.Text = "Search, review, create, and approve document change requests";

        var view = _services.GetRequiredService<DCRListView>();
        view.CreateRequested += async (_, _) => await ShowCreateDcrAsync();
        view.ViewRequested += async (_, id) => await ShowDcrDetailAsync(id, false);
        view.EditRequested += async (_, id) => await ShowDcrDetailAsync(id, true);
        ContentHost.Content = view;
        await view.LoadAsync();
    }

    private async Task ShowCreateDcrAsync()
    {
        PageTitleText.Text = "New DCR";
        PageSubtitleText.Text = "Create a document change request with before and after image space";

        var view = _services.GetRequiredService<DCRDetailView>();
        view.CloseRequested += async (_, _) => await ShowDcrListAsync();
        ContentHost.Content = view;
        await view.OpenCreateAsync();
    }

    private async Task ShowDcrDetailAsync(int dcrId, bool edit)
    {
        PageTitleText.Text = edit ? "Edit DCR" : "DCR Detail";
        PageSubtitleText.Text = "Review details, risk assessment, images, approvals, and comments";

        var view = _services.GetRequiredService<DCRDetailView>();
        view.CloseRequested += async (_, _) => await ShowDcrListAsync();
        ContentHost.Content = view;
        await view.OpenAsync(dcrId, edit);
    }

    private async Task ShowUsersAsync()
    {
        PageTitleText.Text = "Users";
        PageSubtitleText.Text = "Manage DCR accounts, roles, departments, and active status";

        var view = _services.GetRequiredService<UserManagementView>();
        ContentHost.Content = view;
        await view.LoadAsync();
    }

    private async void DcrListButton_Click(object sender, RoutedEventArgs e) => await ShowDcrListAsync();

    private async void CreateDcrButton_Click(object sender, RoutedEventArgs e) => await ShowCreateDcrAsync();

    private async void UsersButton_Click(object sender, RoutedEventArgs e) => await ShowUsersAsync();

    private void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        _authService.Logout();
        _services.GetRequiredService<LoginWindow>().Show();
        Close();
    }

    private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
    {
        _sidebarExpanded = !_sidebarExpanded;
        SidebarColumn.Width = new GridLength(_sidebarExpanded ? 272 : 64);
        SidebarContentColumn.Width = new GridLength(_sidebarExpanded ? 1 : 0, GridUnitType.Star);
        SidebarContentPanel.Visibility = _sidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
    }
}
