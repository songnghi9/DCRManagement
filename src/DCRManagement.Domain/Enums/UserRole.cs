namespace DCRManagement.Domain.Enums;

public enum UserRole
{
    Admin = 0,
    Engineer = 1,      // Creates DCRs
    Reviewer = 2,      // Reviews before approval
    Approver = 3,      // Final approval authority
    ReadOnly = 4
}