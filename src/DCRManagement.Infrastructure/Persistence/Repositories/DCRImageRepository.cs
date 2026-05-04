using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DCRManagement.Infrastructure.Persistence.Repositories;

public class DCRImageRepository : GenericRepository<DCRImage>, IDCRImageRepository
{
    public DCRImageRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Projection query — EF Core only SELECTs the listed columns.
    /// ImageData (VARBINARY MAX) is intentionally excluded to keep the query fast.
    /// Results are sorted by DisplayOrder so the gallery renders in the saved order.
    /// </summary>
    public async Task<IEnumerable<DCRImageThumbnailInfo>> GetThumbnailsAsync(
        int dcrId, string galleryType) =>
        await _dbSet
            .Where(i => i.DCRId == dcrId && i.GalleryType == galleryType)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new DCRImageThumbnailInfo(
                i.Id, i.DCRId, i.FileName, i.FileSizeBytes,
                i.ContentType, i.Category, i.Caption,
                i.DisplayOrder, i.ThumbnailWidth, i.ThumbnailHeight,
                i.GalleryType, i.CreatedAt))
            .ToListAsync();

    /// <summary>
    /// Loads the full record including ImageData binary.
    /// Only call when the user explicitly requests to view/display an image.
    /// </summary>
    public async Task<DCRImage?> GetWithDataAsync(int imageId) =>
        await _dbSet.FirstOrDefaultAsync(i => i.Id == imageId);

    public async Task<IEnumerable<DCRImage>> GetByDCRAsync(int dcrId) =>
        await _dbSet
            .Where(i => i.DCRId == dcrId)
            .OrderBy(i => i.GalleryType)
            .ThenBy(i => i.DisplayOrder)
            .ToListAsync();

    public async Task<IEnumerable<DCRImage>> GetByGalleryAsync(int dcrId, string galleryType) =>
        await _dbSet
            .Where(i => i.DCRId == dcrId && i.GalleryType == galleryType)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();

    public async Task<int> CountByDCRAsync(int dcrId) =>
        await _dbSet.CountAsync(i => i.DCRId == dcrId);

    /// <summary>
    /// Bulk-deletes all images of a given gallery type for a DCR.
    /// Used by ReplaceGalleryAsync before inserting the new set.
    /// ExecuteDeleteAsync avoids loading entities into memory.
    /// </summary>
    public async Task DeleteByGalleryAsync(int dcrId, string galleryType) =>
        await _dbSet
            .Where(i => i.DCRId == dcrId && i.GalleryType == galleryType)
            .ExecuteDeleteAsync();
}
