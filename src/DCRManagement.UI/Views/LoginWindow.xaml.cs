using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views;

public partial class LoginWindow : FluentWindow
{
    private readonly AuthService _authService;
    private readonly IServiceProvider _services;
    private string _rawPassword = string.Empty;

    public LoginWindow(AuthService authService, IServiceProvider services)
    {
        InitializeComponent();
        _authService = authService;
        _services    = services;

        PasswordBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) _ = LoginAsync(); };
        UsernameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) PasswordBox.Focus(); };

        // Load saved credentials on startup
        Loaded += (_, _) => LoadSavedCredentials();
    }

    // ── Saved credentials ─────────────────────────────────────────────────────

    private void LoadSavedCredentials()
    {
        var saved = CredentialStore.Load();
        if (saved is null)
        {
            SavedAccountsPanel.Visibility = Visibility.Collapsed;
            return;
        }

        // Show the saved account banner
        SavedUsernameText.Text = saved.Value.Username;
        SavedAccountsPanel.Visibility = Visibility.Visible;

        // Pre-fill fields and check "Remember me"
        UsernameTextBox.Text = saved.Value.Username;
        PasswordBox.Password = saved.Value.Password;
        _rawPassword = saved.Value.Password;
        RememberMeCheckBox.IsChecked = true;
    }

    /// <summary>"Use" button — fills fields from saved credentials.</summary>
    private void UseSavedButton_Click(object sender, RoutedEventArgs e)
    {
        var saved = CredentialStore.Load();
        if (saved is null) return;

        UsernameTextBox.Text = saved.Value.Username;
        PasswordBox.Password = saved.Value.Password;
        _rawPassword = saved.Value.Password;
        RememberMeCheckBox.IsChecked = true;

        // Move focus to Sign in button for quick keyboard confirm
        LoginButton.Focus();
    }

    /// <summary>"Forget" (delete) button — removes saved credentials.</summary>
    private void ForgetButton_Click(object sender, RoutedEventArgs e)
    {
        CredentialStore.Clear();
        SavedAccountsPanel.Visibility = Visibility.Collapsed;
        RememberMeCheckBox.IsChecked = false;

        // Clear fields only if they matched the saved account
        UsernameTextBox.Text = string.Empty;
        PasswordBox.Password = string.Empty;
        _rawPassword = string.Empty;
    }

    // ── Password box ──────────────────────────────────────────────────────────

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.PasswordBox pb)
            _rawPassword = pb.Password;
    }

    private void ShowPasswordCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // WPF-UI PasswordBox has a built-in reveal eye button — no extra code needed
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    private async void LoginButton_Click(object sender, RoutedEventArgs e) => await LoginAsync();

    private async Task LoginAsync()
    {
        var username = UsernameTextBox.Text.Trim();
        var password = _rawPassword;

        if (string.IsNullOrWhiteSpace(username))
        {
            ErrorInfoBar.Message = "Please enter your username.";
            ErrorInfoBar.IsOpen  = true;
            UsernameTextBox.Focus();
            return;
        }

        ErrorInfoBar.IsOpen   = false;
        LoginButton.IsEnabled = false;
        LoginButton.Content   = "Signing in…";

        try
        {
            var result = await _authService.LoginAsync(username, password);

            if (!result.IsSuccess)
            {
                ErrorInfoBar.Message = result.ErrorMessage ?? "Login failed.";
                ErrorInfoBar.IsOpen  = true;
                return;
            }

            // ── Save or clear credentials based on "Remember me" ──────────────
            if (RememberMeCheckBox.IsChecked == true)
            {
                CredentialStore.Save(username, password);
            }
            else
            {
                // User unchecked "Remember me" — remove any previously saved creds
                CredentialStore.Clear();
            }

            _services.GetRequiredService<MainWindow>().Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorInfoBar.Message = $"Unable to sign in: {ex.Message}";
            ErrorInfoBar.IsOpen  = true;
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Content   = "Sign in";
        }
    }
}
