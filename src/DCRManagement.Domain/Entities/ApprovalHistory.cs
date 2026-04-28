using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Entities;

public class ApprovalHistory : BaseEntity
{
    public int DCRId { get; set; }
    public int ActorId { get; set; }           // Who performed the action
    public ApprovalAction Action { get; set; }
    public DCRStatus FromStatus { get; set; }
    public DCRStatus ToStatus { get; set; }
    public string? Comment { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public DCR DCR { get; set; } = null!;
    public User Actor { get; set; } = null!;
}