using DCRManagement.Domain.Enums;

namespace DCRManagement.Application.DTOs;

public record DCRDto(
    int Id,
    string DCRNumber,
    string Title,
    string Description,
    string? AffectedParts,
    string? Reason,
    string? ImpactAnalysis,
    DCRStatus Status,
    string? Priority,
    DateTime? TargetCompletionDate,
    DateTime? ActualCompletionDate,
    string CreatedBy,
    DateTime CreatedAt,
    string? AssignedReviewer,
    string? AssignedApprover,
    IEnumerable<ApprovalHistoryDto> History,
    IEnumerable<AttachmentDto> Attachments
)
{
    public string StatusDisplay => Status switch
    {
        DCRStatus.Draft => "Draft",
        DCRStatus.PendingReview => "Pending Review",
        DCRStatus.UnderReview => "Under Review",
        DCRStatus.PendingApproval => "Pending Approval",
        DCRStatus.Approved => "Approved",
        DCRStatus.Rejected => "Rejected",
        DCRStatus.Closed => "Closed",
        DCRStatus.Cancelled => "Cancelled",
        _ => Status.ToString()
    };

    public bool IsEditable => Status is DCRStatus.Draft or DCRStatus.Rejected;
    public bool IsClosed => Status is DCRStatus.Closed or DCRStatus.Cancelled;
}

public record CreateDCRDto(
    string Title,
    string Description,
    string? AffectedParts,
    string? Reason,
    string? ImpactAnalysis,
    string? Priority,
    DateTime? TargetCompletionDate,
    IList<System.Drawing.Image>? BeforeImages = null,
    IList<System.Drawing.Image>? AfterImages  = null,
    int BeforeThumbnailWidth  = 140,
    int BeforeThumbnailHeight = 140,
    int AfterThumbnailWidth   = 140,
    int AfterThumbnailHeight  = 140
);

public record UpdateDCRDto(
    int Id,
    string Title,
    string Description,
    string? AffectedParts,
    string? Reason,
    string? ImpactAnalysis,
    string? Priority,
    DateTime? TargetCompletionDate,
    IList<System.Drawing.Image>? BeforeImages = null,
    IList<System.Drawing.Image>? AfterImages  = null,
    int BeforeThumbnailWidth  = 140,
    int BeforeThumbnailHeight = 140,
    int AfterThumbnailWidth   = 140,
    int AfterThumbnailHeight  = 140
);

public record SubmitDCRDto(
    int DCRId,
    int AssignedReviewerId,
    int AssignedApproverId
);