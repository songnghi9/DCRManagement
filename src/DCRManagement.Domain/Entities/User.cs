using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<DCR> CreatedDCRs { get; set; } = [];
    public ICollection<ApprovalHistory> ApprovalHistories { get; set; } = [];
}