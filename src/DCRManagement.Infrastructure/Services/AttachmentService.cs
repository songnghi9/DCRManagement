using DCRManagement.Application.Common;
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Drawing.Imaging;

namespace DCRManagement.Infrastructure.Services;

public class AttachmentService : IGalleryImageService
{
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly ILogger<AttachmentService> _logger;
    private readonly string _storageBasePath;
    private readonly IConfiguration _configuration;

    private const long MAX_FILE_SIZE_BYTES = 50 * 1024 * 1024; // 50 MB
    private static readonly string[] ALLOWED_EXTENSIONS =
        [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip"];

    public AttachmentService(
        IRepository<Attachment> attachmentRepository,
        IConfiguration configuration,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _configuration = configuration;
        _logger = logger;
        _storageBasePath = configuration["Storage:AttachmentPath"]
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");

        Directory.CreateDirectory(_storageBasePath);
    }

    // ── Regular file attachments ──────────────────────────────────────────────

    /// <summary>
    /// Saves a file to disk and records metadata in the database.
    /// Returns the created Attachment, or null if validation fails.
    /// </summary>
    public async Task<Attachment?> SaveAttachmentAsync(
        int dcrId, string sourceFilePath, int uploadedById)
    {
        var fileInfo = new FileInfo(sourceFilePath);
        var extension = fileInfo.Extension.ToLowerInvariant();

        if (!ALLOWED_EXTENSIONS.Contains(extension))
        {
            _logger.LogWarning("Rejected file upload: extension {Ext} not allowed", extension);
            return null;
        }

        if (fileInfo.Length > MAX_FILE_SIZE_BYTES)
        {
            _logger.LogWarning("Rejected file upload: size {Size} exceeds limit", fileInfo.Length);
            return null;
        }

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var destPath = Path.Combine(_storageBasePath, storedFileName);

        File.Copy(sourceFilePath, destPath);

        var attachment = new Attachment
        {
            DCRId          = dcrId,
            FileName       = fileInfo.Name,
            StoredFileName = storedFileName,
            FilePath       = destPath,
            FileSizeBytes  = fileInfo.Length,
            ContentType    = GetContentType(extension),
            CreatedById    = uploadedById,
            CreatedAt      = DateTime.UtcNow
            // ImageType and DisplayOrder remain null → regular attachment
        };

        return await _attachmentRepository.AddAsync(attachment);
    }

    public async Task DeleteAttachmentAsync(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment is null) return;

        if (File.Exists(attachment.FilePath))
            File.Delete(attachment.FilePath);

        await _attachmentRepository.DeleteAsync(attachmentId);
    }

    // ── Gallery image methods ─────────────────────────────────────────────────

    /// <summary>
    /// Replaces all gallery images of a given type (Before/After) for a DCR atomically:
    /// deletes old files + DB records, then saves new images in display order.
    /// Call inside the same unit-of-work as DCR save to keep consistency.
    /// </summary>
    public async Task ReplaceGalleryImagesAsync(
        int dcrId,
        IList<System.Drawing.Image> images,
        string imageType,
        int uploadedById,
        int thumbnailWidth  = 140,
        int thumbnailHeight = 140)
    {
        // 1. Delete existing gallery images of this type
        var existing = (await _attachmentRepository.FindAsync(
            a => a.DCRId == dcrId && a.ImageType == imageType))
            .ToList();

        foreach (var old in existing)
        {
            try { if (File.Exists(old.FilePath)) File.Delete(old.FilePath); }
            catch (Exception ex)
            { _logger.LogWarning(ex, "Could not delete old gallery file {Path}", old.FilePath); }

            await _attachmentRepository.DeleteAsync(old.Id);
        }

        // 2. Save new images in order, persisting the user's chosen thumbnail size
        for (int i = 0; i < images.Count; i++)
            await SaveGalleryImageAsync(dcrId, images[i], imageType, i,
                                        thumbnailWidth, thumbnailHeight, uploadedById);

        _logger.LogInformation(
            "Replaced {Count} {Type} images for DCR {DcrId} at {W}x{H}px",
            images.Count, imageType, dcrId, thumbnailWidth, thumbnailHeight);
    }

    /// <summary>
    /// Saves a single in-memory image as PNG to disk and records it in the database.
    /// </summary>
    private async Task<Attachment> SaveGalleryImageAsync(
        int dcrId,
        System.Drawing.Image image,
        string imageType,
        int displayOrder,
        int thumbnailWidth,
        int thumbnailHeight,
        int uploadedById)
    {
        var storedFileName = $"{Guid.NewGuid()}.png";
        var destPath = Path.Combine(_storageBasePath, storedFileName);

        image.Save(destPath, ImageFormat.Png);
        var fileInfo = new FileInfo(destPath);

        var attachment = new Attachment
        {
            DCRId           = dcrId,
            FileName        = $"{imageType}_{displayOrder + 1}.png",
            StoredFileName  = storedFileName,
            FilePath        = destPath,
            FileSizeBytes   = fileInfo.Length,
            ContentType     = "image/png",
            ImageType       = imageType,
            DisplayOrder    = displayOrder,
            ThumbnailWidth  = thumbnailWidth,
            ThumbnailHeight = thumbnailHeight,
            CreatedById     = uploadedById,
            CreatedAt       = DateTime.UtcNow
        };

        return await _attachmentRepository.AddAsync(attachment);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GetContentType(string extension) => extension switch
    {
        ".pdf"           => "application/pdf",
        ".doc" or ".docx" => "application/msword",
        ".xls" or ".xlsx" => "application/vnd.ms-excel",
        ".png"           => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".zip"           => "application/zip",
        _                => "application/octet-stream"
    };
}