using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Entities;

public class DCRImage : BaseEntity
{
    public int DCRId { get; set; }

    /// <summary>Original file name or auto-generated name for clipboard pastes.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Raw image binary stored directly in SQL — VARBINARY(MAX).</summary>
    public byte[] ImageData { get; set; } = [];

    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public ImageCategory Category { get; set; } = ImageCategory.Photo;
    public string? Caption { get; set; }

    // ── Layout preservation ───────────────────────────────────────────────────
    /// <summary>
    /// Display order within the Before/After gallery (0-based).
    /// Preserved across save/load so drag-reorder is persisted.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Thumbnail width in pixels as set by the user via the size spinner.
    /// Restored on load so the gallery looks identical after reopening.
    /// </summary>
    public int ThumbnailWidth { get; set; } = 140;

    /// <summary>
    /// Thumbnail height in pixels as set by the user via the size spinner.
    /// </summary>
    public int ThumbnailHeight { get; set; } = 140;

    /// <summary>"Before" | "After" — which gallery this image belongs to.</summary>
    public string GalleryType { get; set; } = "Before";

    // Navigation
    public DCR DCR { get; set; } = null!;
}
