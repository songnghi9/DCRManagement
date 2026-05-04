using DCRManagement.Domain.Entities;

namespace DCRManagement.Domain.Interfaces;

public interface IDCRImageRepository : IRepository<DCRImage>
{
    /// <summary>
    /// Returns metadata projection — does NOT include ImageData binary.
    /// Use this for binding the gallery list to avoid loading all blobs.
    /// </summary>
    Task<IEnumerable<DCRImageThumbnailInfo>> GetThumbnailsAsync(int dcrId, string galleryType);

    /// <summary>
    /// Loads the full record including ImageData binary.
    /// Only call when the user explicitly requests to view/display an image.
    /// </summary>
    Task<DCRImage?> GetWithDataAsync(int imageId);

    Task<IEnumerable<DCRImage>> GetByDCRAsync(int dcrId);
    Task<IEnumerable<DCRImage>> GetByGalleryAsync(int dcrId, string galleryType);
    Task<int> CountByDCRAsync(int dcrId);
    Task DeleteByGalleryAsync(int dcrId, string galleryType);
}
