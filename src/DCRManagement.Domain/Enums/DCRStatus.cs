namespace DCRManagement.Domain.Enums;

public enum DCRStatus
{
    Draft = 0,
    PendingReview = 1,
    UnderReview = 2,
    PendingApproval = 3,
    Approved = 4,
    Rejected = 5,
    Closed = 6,
    Cancelled = 7
}