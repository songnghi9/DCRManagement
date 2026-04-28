using DCRManagement.Domain.Enums;
using System.Net.Mail;

namespace DCRManagement.Domain.Entities;

public class DCR : BaseEntity
{
    public string DCRNumber { get; set; } = string.Empty; // e.g. DCR-2024-0001
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AffectedParts { get; set; }
    public string? Reason { get; set; }
    public string? ImpactAnalysis { get; set; }
    public DCRStatus Status { get; set; } = DCRStatus.Draft;
    public string? Priority { get; set; }            // Low / Medium / High / Critical
    public DateTime? TargetCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }

    // Assigned reviewer & approver (set when submitted)
    public int? AssignedReviewerId { get; set; }
    public int? AssignedApproverId { get; set; }

    // Navigation
    public User? AssignedReviewer { get; set; }
    public User? AssignedApprover { get; set; }
    public ICollection<ApprovalHistory> ApprovalHistories { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
}