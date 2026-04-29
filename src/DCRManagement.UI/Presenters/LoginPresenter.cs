using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Presenters;

public interface ILoginView : IView
{
    string Username { get; }
    string Password { get; }
    event EventHandler LoginRequested;
}

public class LoginPresenter
{
    private readonly ILoginView _view;
    private readonly AuthService _authService;
    private readonly ILogger<LoginPresenter> _logger;

    public event EventHandler? LoginSucceeded;

    public LoginPresenter(
        ILoginView view,
        AuthService authService,
        ILogger<LoginPresenter> logger)
    {
        _view = view;
        _authService = authService;
        _logger = logger;

        _view.LoginRequested += OnLoginRequested;
    }

    private async void OnLoginRequested(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_view.Username))
        {
            _view.ShowError("Please enter your username.", "Validation");
            return;
        }

        if (string.IsNullOrWhiteSpace(_view.Password))
        {
            _view.ShowError("Please enter your password.", "Validation");
            return;
        }

        _view.SetBusy(true);

        try
        {
            var result = await _authService.LoginAsync(_view.Username, _view.Password);

            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Login Failed");
                return;
            }

            _logger.LogInformation("User {Username} logged in via UI", _view.Username);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login UI error for {Username}", _view.Username);
            //_view.ShowError(
            //    "Unable to connect to the database. Please check your connection and try again.",
            //    "Connection Error");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }
}