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

        UsernameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) PasswordBox.Focus(); };
        PasswordBox.KeyDown     += (s, e) => { if (e.Key == Key.Enter) _ = LoginAsync(); };

        Loaded += (_, _) => RefreshSavedAccountsList();
    }

    // ── Saved accounts list ───────────────────────────────────────────────────

    /// <summary>Reload the saved accounts panel from CredentialStore.</summary>
    private void RefreshSavedAccountsList()
    {
        var accounts = CredentialStore.LoadAll();

        if (accounts.Count == 0)
        {
            SavedAccountsSection.Visibility = Visibility.Collapsed;
            return;
        }

        // Build view models with avatar initial
        SavedAccountsList.ItemsSource = accounts
            .Select(c => new AccountViewModel(c.Username, c.Password))
            .ToList();

        SavedAccountsSection.Visibility = Visibility.Visible;
    }

    /// <summary>"Use" button on an account row — fills fields.</summary>
    private void UseAccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.Tag is not AccountViewModel vm) return;

        UsernameTextBox.Text = vm.Username;
        PasswordBox.Password = vm.Password;
        _rawPassword         = vm.Password;
        RememberMeCheckBox.IsChecked = true;

        LoginButton.Focus();
    }

    /// <summary>Remove (×) button on an account row.</summary>
    private void RemoveAccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.Tag is not AccountViewModel vm) return;

        CredentialStore.Remove(vm.Username);
        RefreshSavedAccountsList();

        // Clear fields if they matched the removed account
        if (UsernameTextBox.Text.Equals(vm.Username, StringComparison.OrdinalIgnoreCase))
        {
            UsernameTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            _rawPassword         = string.Empty;
            RememberMeCheckBox.IsChecked = false;
        }
    }

    /// <summary>"Clear all" button — removes every saved account.</summary>
    private void ClearAllAccountsButton_Click(object sender, RoutedEventArgs e)
    {
        CredentialStore.ClearAll();
        RefreshSavedAccountsList();
        RememberMeCheckBox.IsChecked = false;
    }

    // ── Password box ──────────────────────────────────────────────────────────

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.PasswordBox pb)
            _rawPassword = pb.Password;
    }

    private void ShowPasswordCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // WPF-UI PasswordBox has a built-in reveal eye button
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

            // Save or remove based on "Remember me"
            if (RememberMeCheckBox.IsChecked == true)
                CredentialStore.Save(username, password);
            else
                CredentialStore.Remove(username);

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

// ── View model for saved account row ─────────────────────────────────────────

/// <summary>Wraps a SavedCredential for display in the accounts list.</summary>
internal sealed class AccountViewModel(string username, string password)
{
    public string Username { get; } = username;
    public string Password { get; } = password;

    /// <summary>First letter of username, uppercased — shown in avatar circle.</summary>
    public string Initial => string.IsNullOrEmpty(Username)
        ? "?"
        : Username[0].ToString().ToUpperInvariant();
}
