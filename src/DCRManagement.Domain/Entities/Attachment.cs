namespace DCRManagement.Domain.Entities;

public class Attachment : BaseEntity
{
    public int DCRId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty; // GUID-based, prevents collisions
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Distinguishes gallery images from regular file attachments.
    /// "Before" | "After" = gallery image; null = regular file attachment.
    /// </summary>
    public string? ImageType { get; set; }

    /// <summary>
    /// Display order within the Before/After gallery (0-based).
    /// Null for regular file attachments.
    /// </summary>
    public int? DisplayOrder { get; set; }

    // Navigation
    public DCR DCR { get; set; } = null!;
}