namespace DCRManagement.Application.Common;

/// <summary>
/// Abstracts gallery image persistence so Application layer stays independent of Infrastructure.
/// Implemented by AttachmentService in the Infrastructure layer.
/// </summary>
public interface IGalleryImageService
{
    /// <summary>
    /// Replaces all gallery images of a given type (Before/After) for a DCR.
    /// Deletes old files + DB records, then saves new images in display order.
    /// Pass an empty list to clear all images of that type.
    /// thumbnailWidth/thumbnailHeight are persisted so the gallery restores
    /// the exact size the user set via the size spinners.
    /// </summary>
    Task ReplaceGalleryImagesAsync(
        int dcrId,
        IList<System.Drawing.Image> images,
        string imageType,
        int uploadedById,
        int thumbnailWidth  = 140,
        int thumbnailHeight = 140);
}
