namespace DCRManagement.Domain.Entities;

public class Attachment : BaseEntity
{
    public int DCRId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty; // GUID-based, prevents collisions
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;

    // Navigation
    public DCR DCR { get; set; } = null!;
}