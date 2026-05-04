using DCRManagement.Application.Services;
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
        _services = services;

        // Allow Enter key on the password box
        PasswordBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) _ = LoginAsync(); };
    }

    // Called by the WPF-UI PasswordBox PasswordChanged event
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.PasswordBox pb)
            _rawPassword = pb.Password;
    }

    // Show/hide password toggle — WPF-UI PasswordBox has built-in reveal button
    // The CheckBox in XAML is kept for UX but we just toggle the reveal button visibility
    private void ShowPasswordCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // WPF-UI PasswordBox handles reveal internally via its built-in eye button
        // No code needed — the CheckBox is wired to nothing (kept for future use)
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e) => await LoginAsync();

    private async Task LoginAsync()
    {
        ErrorInfoBar.IsOpen = false;
        LoginButton.IsEnabled = false;
        LoginButton.Content = "Signing in…";

        try
        {
            var result = await _authService.LoginAsync(UsernameTextBox.Text.Trim(), _rawPassword);
            if (!result.IsSuccess)
            {
                ErrorInfoBar.Message = result.ErrorMessage ?? "Login failed.";
                ErrorInfoBar.IsOpen = true;
                return;
            }

            _services.GetRequiredService<MainWindow>().Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorInfoBar.Message = $"Unable to sign in: {ex.Message}";
            ErrorInfoBar.IsOpen = true;
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Content = "Sign in";
        }
    }
}
