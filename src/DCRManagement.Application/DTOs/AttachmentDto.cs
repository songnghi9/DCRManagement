namespace DCRManagement.Application.DTOs;

public record AttachmentDto(
    int Id,
    int DCRId,
    string FileName,
    string FilePath,
    long FileSizeBytes,
    string ContentType,
    string UploadedBy,
    DateTime UploadedAt,
    string? ImageType,       // "Before" | "After" | null (regular file attachment)
    int? DisplayOrder,       // 0-based order within gallery; null for regular attachments
    int? ThumbnailWidth  = null,   // saved thumbnail width in px; null for regular attachments
    int? ThumbnailHeight = null    // saved thumbnail height in px; null for regular attachments
)
{
    public string FileSizeDisplay => FileSizeBytes switch
    {
        < 1024        => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        _             => $"{FileSizeBytes / (1024.0 * 1024):F1} MB"
    };

    /// <summary>True when this attachment is a Before/After gallery image.</summary>
    public bool IsGalleryImage => ImageType is "Before" or "After";
}
