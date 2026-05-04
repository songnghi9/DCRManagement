using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Entities;

/// <summary>
/// Projection record — does NOT contain ImageData binary.
/// Used for listing/binding so EF Core only SELECTs the columns we need.
/// </summary>
public record DCRImageThumbnailInfo(
    int Id,
    int DCRId,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    ImageCategory Category,
    string? Caption,
    int DisplayOrder,
    int ThumbnailWidth,
    int ThumbnailHeight,
    string GalleryType,
    DateTime CreatedAt
);
