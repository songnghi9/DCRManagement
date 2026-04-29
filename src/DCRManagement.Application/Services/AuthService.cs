using BCrypt.Net;
using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCRManagement.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Validates credentials and populates SessionContext on success.
    /// Returns failure Result instead of throwing for invalid login.
    /// </summary>
    public async Task<Result<UserDto>> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Result<UserDto>.Failure("Username and password are required.", "EMPTY_CREDENTIALS");

        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", username);
                return Result<UserDto>.Failure("Invalid username or password.", "INVALID_CREDENTIALS");
            }

            if (!user.IsActive)
                return Result<UserDto>.Failure("Your account has been deactivated. Contact administrator.", "ACCOUNT_INACTIVE");

            SessionContext.Instance.Login(user.Id, user.Username, user.FullName, user.Email, user.Role);

            _logger.LogInformation("User {Username} logged in successfully", username);

            return new UserDto(user.Id, user.Username, user.FullName,
                               user.Email, user.Role, user.Department, user.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Username}", username);
            throw;
        }
    }

    public void Logout()
    {
        _logger.LogInformation("User {Username} logged out", SessionContext.Instance.Username);
        SessionContext.Instance.Logout();
    }
}