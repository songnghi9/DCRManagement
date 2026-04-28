using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DCRManagement.Infrastructure.Services;

public class AttachmentService
{
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly ILogger<AttachmentService> _logger;
    private readonly string _storageBasePath;

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

    private readonly IConfiguration _configuration;

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

        // Use GUID filename on disk to prevent path traversal & collisions
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var destPath = Path.Combine(_storageBasePath, storedFileName);

        File.Copy(sourceFilePath, destPath);

        var attachment = new Attachment
        {
            DCRId = dcrId,
            FileName = fileInfo.Name,
            StoredFileName = storedFileName,
            FilePath = destPath,
            FileSizeBytes = fileInfo.Length,
            ContentType = GetContentType(extension),
            CreatedById = uploadedById,
            CreatedAt = DateTime.UtcNow
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

    private static string GetContentType(string extension) => extension switch
    {
        ".pdf" => "application/pdf",
        ".doc" or ".docx" => "application/msword",
        ".xls" or ".xlsx" => "application/vnd.ms-excel",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".zip" => "application/zip",
        _ => "application/octet-stream"
    };
}