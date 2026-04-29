using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Forms;

public partial class MainForm : BaseForm, IMainView
{
    private readonly MainPresenter _presenter;
    private Control? _currentContent;

    // ─── IMainView ────────────────────────────────────────────────────────────
    public event EventHandler? LogoutRequested;
    public event EventHandler? ShowDCRListRequested;
    public event EventHandler? ShowUserManagementRequested;

    public void SetUserInfo(string fullName, string role)
    {
        _lblUserName.Text = fullName;
        _lblUserRole.Text = role;
        _lblStatusRight.Text = $"Logged in as: {fullName}  |  {DateTime.Now:dd/MM/yyyy}";

        // Hide admin features for non-admin users
        var session = Application.Common.SessionContext.Instance;
        _mnuAdmin.Visible = session.IsAdmin;
        _btnNavUsers.Visible = session.IsAdmin;
        _mnuDCRCreate.Enabled = session.CanCreateDCR;
        _btnNavCreateDCR.Enabled = session.CanCreateDCR;
    }

    public void NavigateTo(Control content)
    {
        _pnlContent.SuspendLayout();

        // Dispose previous content if it's a Form/UserControl we own
        _currentContent?.Dispose();

        content.Dock = DockStyle.Fill;
        _pnlContent.Controls.Clear();
        _pnlContent.Controls.Add(content);
        _currentContent = content;

        _pnlContent.ResumeLayout();
    }

    public MainForm(ILogger<MainPresenter> logger)
    {
        InitializeComponent();

        _presenter = new MainPresenter(this, logger);
        _presenter.LogoutRequested += OnLogoutRequested;
        _presenter.NavigateRequested += OnNavigateRequested;

        WireEvents();

        // Default view: DCR List
        _ = ShowDCRListAsync();
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private void WireEvents()
    {
        _mnuSignOut.Click += (s, e) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        _mnuDCRList.Click += (s, e) => ShowDCRListRequested?.Invoke(this, EventArgs.Empty);
        _mnuUserManagement.Click += (s, e) => ShowUserManagementRequested?.Invoke(this, EventArgs.Empty);

        _btnNavDCRList.Click += (s, e) => ShowDCRListRequested?.Invoke(this, EventArgs.Empty);
        _btnNavCreateDCR.Click += (s, e) => OpenCreateDCR();
        _btnNavUsers.Click += (s, e) => ShowUserManagementRequested?.Invoke(this, EventArgs.Empty);
        _mnuDCRCreate.Click += (s, e) => OpenCreateDCR();
    }

    private void OnLogoutRequested(object? sender, EventArgs e)
    {
        var loginForm = Program.ServiceProvider.GetRequiredService<LoginForm>();
        loginForm.Show();
        Close();
    }

    private async void OnNavigateRequested(object? sender, string destination)
    {
        switch (destination)
        {
            case "DCRList":
                await ShowDCRListAsync();
                break;

            case "UserManagement":
                // Phase 6
                ShowInfo("User Management coming in Phase 6.", "Coming Soon");
                break;
        }
    }

    private void OpenCreateDCR()
    {
        var detailForm = Program.ServiceProvider.GetRequiredService<DCRDetailForm>();
        detailForm.CloseRequested += async (s, e) => await ShowDCRListAsync();

        NavigateTo(detailForm);
        detailForm.OpenCreate();
    }

    private async Task ShowDCRListAsync()
    {
        var listForm = Program.ServiceProvider.GetRequiredService<DCRListForm>();

        listForm.CreateRequested += (s, e) => OpenCreateDCR();
        listForm.ViewRequested += async (s, id) => await OpenViewDCRAsync(id);
        listForm.EditRequested += async (s, id) => await OpenEditDCRAsync(id);

        NavigateTo(listForm);
        await listForm.LoadAsync();
    }

    private async Task OpenViewDCRAsync(int dcrId)
    {
        var detailForm = Program.ServiceProvider.GetRequiredService<DCRDetailForm>();
        detailForm.CloseRequested += async (s, e) => await ShowDCRListAsync();

        NavigateTo(detailForm);
        await detailForm.OpenViewAsync(dcrId);
    }

    private async Task OpenEditDCRAsync(int dcrId)
    {
        var detailForm = Program.ServiceProvider.GetRequiredService<DCRDetailForm>();
        detailForm.CloseRequested += async (s, e) => await ShowDCRListAsync();

        NavigateTo(detailForm);
        await detailForm.OpenEditAsync(dcrId);
    }
}
