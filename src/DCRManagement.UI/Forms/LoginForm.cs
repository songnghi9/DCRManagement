using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Forms;

public partial class LoginForm : BaseForm, ILoginView
{
    private readonly LoginPresenter _presenter;

    // ─── ILoginView ───────────────────────────────────────────────────────────
    public string Username => _txtUsername.Text.Trim();
    public string Password => _txtPassword.Text;
    public event EventHandler? LoginRequested;

    public LoginForm(AuthService authService, ILogger<LoginPresenter> logger)
    {
        InitializeComponent();
        ApplyStyling();

        _presenter = new LoginPresenter(this, authService, logger);
        _presenter.LoginSucceeded += OnLoginSucceeded;

        WireEvents();
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private void ApplyStyling()
    {
        ThemeManager.StylePrimaryButton(_btnLogin);
        _txtUsername.SetPlaceholder("Enter your username");
    }

    private void WireEvents()
    {
        _btnLogin.Click += (s, e) => LoginRequested?.Invoke(this, EventArgs.Empty);

        // Enter key in password field triggers login
        _txtPassword.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
                LoginRequested?.Invoke(this, EventArgs.Empty);
        };

        _chkShowPassword.CheckedChanged += (s, e) =>
            _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        // Resolve MainForm from DI — ensures all its dependencies are injected
        var mainForm = Program.ServiceProvider.GetRequiredService<MainForm>();
        mainForm.Show();
        Hide();
    }

    public override void SetBusy(bool isBusy)
    {
        base.SetBusy(isBusy);
        _lblLoading.Visible = isBusy;
        _btnLogin.Text = isBusy ? "Signing in..." : "SIGN IN";
    }
}