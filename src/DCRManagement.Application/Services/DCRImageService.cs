using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCRManagement.Application.Services;

public class DCRImageService
{
    private readonly IDCRImageRepository _imageRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DCRImageService> _logger;

    // ── Limits ────────────────────────────────────────────────────────────────
    private const long MAX_SIZE_BYTES     = 10 * 1024 * 1024; // 10 MB per image
    private const int  MAX_IMAGES_PER_DCR = 20;               // total across both galleries

    private static readonly HashSet<string> ALLOWED_CONTENT_TYPES =
    [
        "image/jpeg",
        "image/png",
        "image/bmp",
        "image/tiff"
    ];

    // Magic bytes — validate actual file content, not just the declared ContentType
    private static readonly Dictionary<string, byte[]> MAGIC_BYTES = new()
    {
        ["image/jpeg"] = [0xFF, 0xD8, 0xFF],
        ["image/png"]  = [0x89, 0x50, 0x4E, 0x47],
        ["image/bmp"]  = [0x42, 0x4D],
        ["image/tiff"] = [0x49, 0x49]
    };

    public DCRImageService(
        IDCRImageRepository imageRepository,
        IUserRepository userRepository,
        ILogger<DCRImageService> logger)
    {
        _imageRepository = imageRepository;
        _userRepository  = userRepository;
        _logger          = logger;
    }

    // ── Single upload ─────────────────────────────────────────────────────────

    /// <summary>
    /// Validates and saves a single image.
    /// Works for both file-dialog uploads and clipboard pastes (both arrive as byte[]).
    /// </summary>
    public async Task<Result<DCRImageDto>> UploadAsync(UploadImageDto dto)
    {
        try
        {
            var validation = await ValidateAsync(dto);
            if (!validation.IsSuccess)
                return Result<DCRImageDto>.Failure(validation.ErrorMessage!, validation.ErrorCode);

            var userId = SessionContext.Instance.UserId;

            var image = new DCRImage
            {
                DCRId           = dto.DCRId,
                FileName        = dto.FileName.Trim(),
                ImageData       = dto.Data,
                FileSizeBytes   = dto.Data.Length,
                ContentType     = dto.ContentType,
                Category        = dto.Category,
                Caption         = dto.Caption?.Trim(),
                DisplayOrder    = dto.DisplayOrder,
                ThumbnailWidth  = dto.ThumbnailWidth,
                ThumbnailHeight = dto.ThumbnailHeight,
                GalleryType     = dto.GalleryType,
                CreatedById     = userId,
                CreatedAt       = DateTime.UtcNow
            };

            await _imageRepository.AddAsync(image);

            _logger.LogInformation(
                "Uploaded {FileName} ({Size} bytes) to DCR {DCRId} gallery={Gallery} order={Order}",
                image.FileName, image.FileSizeBytes, image.DCRId, image.GalleryType, image.DisplayOrder);

            var uploader = await _userRepository.GetByIdAsync(userId);
            return MapToDto(image, uploader?.FullName ?? "Unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to DCR {DCRId}", dto.DCRId);
            throw;
        }
    }

    // ── Batch replace (used on Save DCR) ─────────────────────────────────────

    /// <summary>
    /// Atomically replaces all images in a gallery:
    ///   1. Bulk-delete existing rows (ExecuteDeleteAsync — no entity load)
    ///   2. Insert new rows in DisplayOrder sequence
    ///
    /// ThumbnailWidth/Height and DisplayOrder from each UploadImageDto are persisted,
    /// so the gallery is restored pixel-perfect on next load.
    /// </summary>
    public async Task<Result> SaveGalleryAsync(SaveGalleryDto dto)
    {
        try
        {
            // Validate all images before touching the DB
            foreach (var img in dto.Images)
            {
                var v = ValidateImageData(img);
                if (!v.IsSuccess)
                    return Result.Failure(
                        $"Image '{img.FileName}': {v.ErrorMessage}", v.ErrorCode);
            }

            // Delete existing gallery images (bulk, no round-trip per row)
            await _imageRepository.DeleteByGalleryAsync(dto.DCRId, dto.GalleryType);

            // Insert new images in order
            var userId = SessionContext.Instance.UserId;
            for (int i = 0; i < dto.Images.Count; i++)
            {
                var src = dto.Images[i];
                var image = new DCRImage
                {
                    DCRId           = dto.DCRId,
                    FileName        = src.FileName.Trim(),
                    ImageData       = src.Data,
                    FileSizeBytes   = src.Data.Length,
                    ContentType     = src.ContentType,
                    Category        = src.Category,
                    Caption         = src.Caption?.Trim(),
                    DisplayOrder    = i,                    // use loop index — authoritative order
                    ThumbnailWidth  = src.ThumbnailWidth,
                    ThumbnailHeight = src.ThumbnailHeight,
                    GalleryType     = dto.GalleryType,
                    CreatedById     = userId,
                    CreatedAt       = DateTime.UtcNow
                };
                await _imageRepository.AddAsync(image);
            }

            _logger.LogInformation(
                "Saved {Count} images to DCR {DCRId} gallery={Gallery}",
                dto.Images.Count, dto.DCRId, dto.GalleryType);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving gallery {Gallery} for DCR {DCRId}",
                dto.GalleryType, dto.DCRId);
            throw;
        }
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns thumbnail metadata for a gallery — NO binary data.
    /// Results are sorted by DisplayOrder so the UI renders in the saved order.
    /// </summary>
    public async Task<IEnumerable<DCRImageThumbnailDto>> GetThumbnailsAsync(
        int dcrId, string galleryType)
    {
        var rows = await _imageRepository.GetThumbnailsAsync(dcrId, galleryType);
        return rows.Select(r => new DCRImageThumbnailDto(
            r.Id, r.DCRId, r.FileName, r.FileSizeBytes,
            r.ContentType, r.Category, r.Caption,
            r.DisplayOrder, r.ThumbnailWidth, r.ThumbnailHeight,
            r.GalleryType, r.CreatedAt));
    }

    /// <summary>
    /// Loads the binary for a single image.
    /// Only call when the user explicitly requests to view/display an image.
    /// </summary>
    public async Task<Result<byte[]>> GetImageDataAsync(int imageId)
    {
        var image = await _imageRepository.GetWithDataAsync(imageId);
        if (image is null)
            return Result<byte[]>.Failure($"Image {imageId} not found.", "NOT_FOUND");
        return image.ImageData;
    }

    /// <summary>
    /// Loads binary for all images in a gallery at once.
    /// Used when opening a DCR to populate the ImageGalleryControl.
    /// Returns items sorted by DisplayOrder.
    /// </summary>
    public async Task<IEnumerable<(DCRImageThumbnailDto Meta, byte[] Data)>> LoadGalleryAsync(
        int dcrId, string galleryType)
    {
        var images = await _imageRepository.GetByGalleryAsync(dcrId, galleryType);
        return images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => (
                Meta: new DCRImageThumbnailDto(
                    i.Id, i.DCRId, i.FileName, i.FileSizeBytes,
                    i.ContentType, i.Category, i.Caption,
                    i.DisplayOrder, i.ThumbnailWidth, i.ThumbnailHeight,
                    i.GalleryType, i.CreatedAt),
                Data: i.ImageData));
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task<Result> DeleteAsync(int imageId)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image is null)
                return Result.Failure($"Image {imageId} not found.", "NOT_FOUND");

            var session = SessionContext.Instance;
            if (image.CreatedById != session.UserId && !session.IsAdmin)
                return Result.Failure("You do not have permission to delete this image.", "UNAUTHORIZED");

            await _imageRepository.DeleteAsync(imageId);
            _logger.LogInformation("Deleted image {ImageId} from DCR {DCRId}", imageId, image.DCRId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImageId}", imageId);
            throw;
        }
    }

    // ── Validation ────────────────────────────────────────────────────────────

    private async Task<Result> ValidateAsync(UploadImageDto dto)
    {
        var count = await _imageRepository.CountByDCRAsync(dto.DCRId);
        if (count >= MAX_IMAGES_PER_DCR)
            return Result.Failure(
                $"DCR has reached the limit of {MAX_IMAGES_PER_DCR} images.", "MAX_IMAGES_REACHED");

        return ValidateImageData(dto);
    }

    private static Result ValidateImageData(UploadImageDto dto)
    {
        if (dto.Data.Length == 0)
            return Result.Failure("Image data is empty.", "EMPTY_DATA");

        if (dto.Data.Length > MAX_SIZE_BYTES)
            return Result.Failure(
                $"Image exceeds 10 MB limit ({dto.Data.Length / 1024.0 / 1024:F1} MB).",
                "FILE_TOO_LARGE");

        if (!ALLOWED_CONTENT_TYPES.Contains(dto.ContentType))
            return Result.Failure(
                "Only JPEG, PNG, BMP, TIFF are accepted.", "INVALID_CONTENT_TYPE");

        if (!VerifyMagicBytes(dto.Data, dto.ContentType))
            return Result.Failure(
                "File content does not match the declared format.", "MAGIC_BYTES_MISMATCH");

        return Result.Success();
    }

    private static bool VerifyMagicBytes(byte[] data, string contentType)
    {
        if (!MAGIC_BYTES.TryGetValue(contentType, out var magic)) return false;
        if (data.Length < magic.Length) return false;
        return data.Take(magic.Length).SequenceEqual(magic);
    }

    private static DCRImageDto MapToDto(DCRImage img, string uploaderName) =>
        new(img.Id, img.DCRId, img.FileName, img.FileSizeBytes,
            img.ContentType, img.Category, img.Caption,
            img.DisplayOrder, img.ThumbnailWidth, img.ThumbnailHeight,
            img.GalleryType, uploaderName, img.CreatedAt);
}
