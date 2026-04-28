namespace DCRManagement.Application.DTOs;

public record AttachmentDto(
    int Id,
    int DCRId,
    string FileName,
    string FilePath,
    long FileSizeBytes,
    string ContentType,
    string UploadedBy,
    DateTime UploadedAt
)
{
    public string FileSizeDisplay => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        _ => $"{FileSizeBytes / (1024.0 * 1024):F1} MB"
    };
}