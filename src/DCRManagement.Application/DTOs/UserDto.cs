using DCRManagement.Domain.Enums;

namespace DCRManagement.Application.DTOs;

public record UserDto(
    int Id,
    string Username,
    string FullName,
    string Email,
    UserRole Role,
    string? Department,
    bool IsActive
);

// Lightweight variant for dropdowns (Reviewer / Approver picker)
public record UserSummaryDto(int Id, string FullName, string Email, UserRole Role);

public record CreateUserDto(
    string Username,
    string Password,
    string FullName,
    string Email,
    UserRole Role,
    string? Department
);

public record UpdateUserDto(
    int Id,
    string FullName,
    string Email,
    UserRole Role,
    string? Department,
    bool IsActive
);