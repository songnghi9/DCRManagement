using DCRManagement.Domain.Enums;

namespace DCRManagement.Application.DTOs;

public record ApprovalHistoryDto(
    int Id,
    int DCRId,
    string ActorName,
    string ActorRole,
    ApprovalAction Action,
    DCRStatus FromStatus,
    DCRStatus ToStatus,
    string? Comment,
    DateTime ActionDate
)
{
    public string ActionDisplay => Action switch
    {
        ApprovalAction.Submit => "Submitted for Review",
        ApprovalAction.StartReview => "Review Started",
        ApprovalAction.SendToApproval => "Sent for Approval",
        ApprovalAction.Approve => "Approved",
        ApprovalAction.Reject => "Rejected",
        ApprovalAction.Close => "Closed",
        ApprovalAction.Cancel => "Cancelled",
        _ => Action.ToString()
    };
}