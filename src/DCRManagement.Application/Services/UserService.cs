using BCrypt.Net;
using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCRManagement.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            if (await _userRepository.ExistsAsync(u => u.Username == dto.Username))
                return Result<UserDto>.Failure($"Username '{dto.Username}' is already taken.", "DUPLICATE_USERNAME");

            if (await _userRepository.ExistsAsync(u => u.Email == dto.Email))
                return Result<UserDto>.Failure($"Email '{dto.Email}' is already registered.", "DUPLICATE_EMAIL");

            var user = new User
            {
                Username = dto.Username.Trim().ToLower(),
                PasswordHash = BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Role = dto.Role,
                Department = dto.Department?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = SessionContext.Instance.UserId
            };

            await _userRepository.AddAsync(user);

            _logger.LogInformation("Created user {Username} with role {Role}", user.Username, user.Role);

            return new UserDto(user.Id, user.Username, user.FullName, user.Email,
                               user.Role, user.Department, user.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", dto.Username);
            throw;
        }
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user is null)
                return Result.Failure($"User {dto.Id} not found.", "NOT_FOUND");

            // Prevent Admin from deactivating their own account
            if (dto.Id == SessionContext.Instance.UserId && !dto.IsActive)
                return Result.Failure("You cannot deactivate your own account.", "SELF_DEACTIVATION");

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim().ToLower();
            user.Role = dto.Role;
            user.Department = dto.Department?.Trim();
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedById = SessionContext.Instance.UserId;

            await _userRepository.UpdateAsync(user);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", dto.Id);
            throw;
        }
    }

    public async Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                return Result.Failure("User not found.", "NOT_FOUND");

            if (!BCrypt.Verify(currentPassword, user.PasswordHash))
                return Result.Failure("Current password is incorrect.", "WRONG_PASSWORD");

            if (newPassword.Length < 8)
                return Result.Failure("New password must be at least 8 characters.", "WEAK_PASSWORD");

            user.PasswordHash = BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Returns users eligible to be assigned as reviewers for dropdown population.
    /// </summary>
    public async Task<IEnumerable<UserSummaryDto>> GetReviewersAsync()
    {
        var users = await _userRepository.GetByRoleAsync(UserRole.Reviewer);
        return users.Select(u => new UserSummaryDto(u.Id, u.FullName, u.Email, u.Role));
    }

    /// <summary>
    /// Returns users eligible to be assigned as approvers for dropdown population.
    /// </summary>
    public async Task<IEnumerable<UserSummaryDto>> GetApproversAsync()
    {
        var users = await _userRepository.GetByRoleAsync(UserRole.Approver);
        return users.Select(u => new UserSummaryDto(u.Id, u.FullName, u.Email, u.Role));
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto(
            u.Id, u.Username, u.FullName, u.Email, u.Role, u.Department, u.IsActive));
    }
}