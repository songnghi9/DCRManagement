namespace DCRManagement.Domain.Enums;

public enum ApprovalAction
{
    Submit = 0,      // Draft → PendingReview
    StartReview = 1, // PendingReview → UnderReview
    SendToApproval = 2, // UnderReview → PendingApproval
    Approve = 3,     // PendingApproval → Approved
    Reject = 4,      // Any → Rejected
    Close = 5,       // Approved → Closed
    Cancel = 6       // Any (non-terminal) → Cancelled
}