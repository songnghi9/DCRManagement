using DCRManagement.Domain.Enums;

namespace DCRManagement.Application.Common;

/// <summary>
/// Holds the currently authenticated user for the lifetime of the application.
/// Singleton — set once at login, cleared at logout.
/// UI and Services read from this instead of passing userId everywhere.
/// </summary>
public class SessionContext
{
    private static SessionContext? _instance;
    private static readonly object _lock = new();

    public int UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsAuthenticated { get; private set; }

    private SessionContext() { }

    public static SessionContext Instance
    {
        get
        {
            if (_instance is null)
                lock (_lock)
                    _instance ??= new SessionContext();
            return _instance;
        }
    }

    public void Login(int userId, string username, string fullName, string email, UserRole role)
    {
        UserId = userId;
        Username = username;
        FullName = fullName;
        Email = email;
        Role = role;
        IsAuthenticated = true;
    }

    public void Logout()
    {
        UserId = 0;
        Username = string.Empty;
        FullName = string.Empty;
        Email = string.Empty;
        IsAuthenticated = false;
    }

    // Role-check helpers used by UI to show/hide controls
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsEngineer => Role == UserRole.Engineer;
    public bool IsReviewer => Role == UserRole.Reviewer;
    public bool IsApprover => Role == UserRole.Approver;
    public bool CanCreateDCR => Role is UserRole.Admin or UserRole.Engineer;
    public bool CanReview => Role is UserRole.Admin or UserRole.Reviewer;
    public bool CanApprove => Role is UserRole.Admin or UserRole.Approver;
}