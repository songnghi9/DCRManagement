using DCRManagement.Domain.Enums;

namespace DCRManagement.Application.DTOs;

/// <summary>Full DTO returned after upload — no binary data.</summary>
public record DCRImageDto(
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
    string UploadedBy,
    DateTime UploadedAt
)
{
    public string FileSizeDisplay => FileSizeBytes switch
    {
        < 1024        => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        _             => $"{FileSizeBytes / (1024.0 * 1024):F1} MB"
    };
}

/// <summary>
/// Thumbnail DTO — no binary, used for gallery list binding.
/// Includes layout fields so the gallery can restore exact size/position.
/// </summary>
public record DCRImageThumbnailDto(
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
    DateTime UploadedAt
);

/// <summary>
/// Upload DTO — accepts raw byte[] so it works for both:
///   • OpenFileDialog  → File.ReadAllBytes(path)
///   • Clipboard paste → MemoryStream from Clipboard.GetImage()
/// FileName is auto-generated for clipboard pastes: "Paste_yyyyMMdd_HHmmss.png"
/// </summary>
public record UploadImageDto(
    int DCRId,
    string FileName,
    byte[] Data,
    string ContentType,
    ImageCategory Category,
    string? Caption,
    int DisplayOrder,
    int ThumbnailWidth,
    int ThumbnailHeight,
    string GalleryType       // "Before" | "After"
);

/// <summary>
/// Batch save DTO — replaces all images in a gallery in one call.
/// Preserves order and thumbnail size for every image.
/// </summary>
public record SaveGalleryDto(
    int DCRId,
    string GalleryType,
    IList<UploadImageDto> Images
);
