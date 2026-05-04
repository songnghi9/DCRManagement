# DCRImage — Agent Implementation Spec
> Đây là tài liệu hướng dẫn cho Coding Agent tự động tạo toàn bộ flow lưu trữ hình ảnh DCR vào SQL Server.
> Đọc từ trên xuống, thực hiện theo đúng thứ tự các bước. Mỗi bước có checklist xác nhận trước khi tiếp tục.

---

## 0. Bối cảnh & ràng buộc

| Mục | Giá trị |
|-----|---------|
| Solution | `DCRManagement.sln` |
| Target framework | `net8.0` / `net8.0-windows` (UI) |
| ORM | Entity Framework Core 8 + SQL Server |
| UI | WinForms, MVP pattern |
| Kiến trúc | Clean Architecture: Domain → Infrastructure → Application → UI |
| Quy tắc dependency | Domain không tham chiếu bất kỳ project nào khác |

**Nguồn ảnh được hỗ trợ:**
- Người dùng mở file từ dialog (`OpenFileDialog`)
- Người dùng paste từ clipboard (`Ctrl+V`) — **không có file path**, chỉ có binary

**Lưu trữ:** Binary lưu thẳng vào SQL Server column `VARBINARY(MAX)` — không lưu file lên disk.

---

## 1. Domain layer
**Project:** `src/DCRManagement.Domain`

### 1.1 Tạo enum `ImageCategory`
**File:** `src/DCRManagement.Domain/Enums/ImageCategory.cs`

```csharp
namespace DCRManagement.Domain.Enums;

public enum ImageCategory
{
    Photo      = 0,
    Diagram    = 1,
    Screenshot = 2,
    Other      = 3
}
```

### 1.2 Tạo entity `DCRImage`
**File:** `src/DCRManagement.Domain/Entities/DCRImage.cs`

```csharp
using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Entities;

public class DCRImage : BaseEntity
{
    public int DCRId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] ImageData { get; set; } = [];          // VARBINARY(MAX)
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public ImageCategory Category { get; set; } = ImageCategory.Photo;
    public string? Caption { get; set; }

    // Navigation
    public DCR DCR { get; set; } = null!;
}
```

### 1.3 Thêm navigation property vào entity `DCR`
**File:** `src/DCRManagement.Domain/Entities/DCR.cs`

Thêm dòng sau vào cuối phần navigation properties (sau `Attachments`):
```csharp
public ICollection<DCRImage> Images { get; set; } = [];
```

### 1.4 Tạo interface `IDCRImageRepository`
**File:** `src/DCRManagement.Domain/Interfaces/IDCRImageRepository.cs`

```csharp
using DCRManagement.Domain.Entities;

namespace DCRManagement.Domain.Interfaces;

public interface IDCRImageRepository : IRepository<DCRImage>
{
    /// <summary>Trả về metadata — KHÔNG kèm ImageData để tránh load toàn bộ binary.</summary>
    Task<IEnumerable<DCRImageThumbnailInfo>> GetThumbnailsAsync(int dcrId);

    /// <summary>Load đầy đủ binary — chỉ gọi khi cần hiển thị ảnh.</summary>
    Task<DCRImage?> GetWithDataAsync(int imageId);

    Task<IEnumerable<DCRImage>> GetByDCRAsync(int dcrId);
    Task<int> CountByDCRAsync(int dcrId);
}
```

### 1.5 Tạo record `DCRImageThumbnailInfo`
**File:** `src/DCRManagement.Domain/Entities/DCRImageThumbnailInfo.cs`

```csharp
using DCRManagement.Domain.Enums;

namespace DCRManagement.Domain.Entities;

/// <summary>Projection không chứa ImageData — dùng cho danh sách, tránh load binary không cần thiết.</summary>
public record DCRImageThumbnailInfo(
    int Id,
    int DCRId,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    ImageCategory Category,
    string? Caption,
    DateTime CreatedAt
);
```

**✅ Checklist Domain:**
- [ ] `ImageCategory.cs` tạo xong
- [ ] `DCRImage.cs` tạo xong, kế thừa `BaseEntity`
- [ ] `DCR.cs` có property `Images`
- [ ] `IDCRImageRepository.cs` tạo xong
- [ ] `DCRImageThumbnailInfo.cs` tạo xong
- [ ] Project `DCRManagement.Domain` build thành công (không tham chiếu project nào khác)

---

## 2. Infrastructure layer
**Project:** `src/DCRManagement.Infrastructure`

### 2.1 Tạo EF Core configuration
**File:** `src/DCRManagement.Infrastructure/Persistence/Configurations/DCRImageConfiguration.cs`

```csharp
using DCRManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCRManagement.Infrastructure.Persistence.Configurations;

public class DCRImageConfiguration : IEntityTypeConfiguration<DCRImage>
{
    public void Configure(EntityTypeBuilder<DCRImage> builder)
    {
        builder.ToTable("DCRImages");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.FileName)
               .IsRequired()
               .HasMaxLength(260);

        builder.Property(i => i.ContentType)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.Caption)
               .HasMaxLength(500);

        builder.Property(i => i.Category)
               .HasConversion<string>();

        // Lưu binary trực tiếp vào SQL — không giới hạn kích thước
        builder.Property(i => i.ImageData)
               .IsRequired()
               .HasColumnType("VARBINARY(MAX)");

        // Index để query nhanh theo DCR
        builder.HasIndex(i => i.DCRId);

        builder.HasOne(i => i.DCR)
               .WithMany(d => d.Images)
               .HasForeignKey(i => i.DCRId)
               .OnDelete(DeleteBehavior.Cascade);  // xóa DCR → xóa luôn ảnh
    }
}
```

### 2.2 Thêm `DbSet` vào `AppDbContext`
**File:** `src/DCRManagement.Infrastructure/Persistence/AppDbContext.cs`

Thêm dòng sau vào class `AppDbContext` (sau `DbSet<Attachment>`):
```csharp
public DbSet<DCRImage> DCRImages { get; set; }
```

Không cần sửa `OnModelCreating` — `ApplyConfigurationsFromAssembly` sẽ tự load `DCRImageConfiguration`.

### 2.3 Tạo repository
**File:** `src/DCRManagement.Infrastructure/Persistence/Repositories/DCRImageRepository.cs`

```csharp
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DCRManagement.Infrastructure.Persistence.Repositories;

public class DCRImageRepository : GenericRepository<DCRImage>, IDCRImageRepository
{
    public DCRImageRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Dùng projection để KHÔNG load cột ImageData — tối ưu hiệu năng.
    /// EF Core chỉ SELECT các cột được chỉ định.
    /// </summary>
    public async Task<IEnumerable<DCRImageThumbnailInfo>> GetThumbnailsAsync(int dcrId) =>
        await _dbSet
            .Where(i => i.DCRId == dcrId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new DCRImageThumbnailInfo(
                i.Id, i.DCRId, i.FileName, i.FileSizeBytes,
                i.ContentType, i.Category, i.Caption, i.CreatedAt))
            .ToListAsync();

    /// <summary>Load đầy đủ binary — chỉ gọi khi người dùng bấm xem ảnh.</summary>
    public async Task<DCRImage?> GetWithDataAsync(int imageId) =>
        await _dbSet.FirstOrDefaultAsync(i => i.Id == imageId);

    public async Task<IEnumerable<DCRImage>> GetByDCRAsync(int dcrId) =>
        await _dbSet
            .Where(i => i.DCRId == dcrId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

    public async Task<int> CountByDCRAsync(int dcrId) =>
        await _dbSet.CountAsync(i => i.DCRId == dcrId);
}
```

### 2.4 Đăng ký DI
**File:** `src/DCRManagement.Infrastructure/DependencyInjection.cs`

Thêm dòng sau vào method `AddInfrastructure` (sau dòng `AttachmentService`):
```csharp
services.AddScoped<IDCRImageRepository, DCRImageRepository>();
```

### 2.5 Tạo EF Core Migration
Chạy lệnh sau từ thư mục gốc solution:
```bash
dotnet ef migrations add AddDCRImages \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI
```

Xác nhận file migration được tạo có column:
```csharp
ImageData = table.Column<byte[]>(type: "VARBINARY(MAX)", nullable: false)
```

**✅ Checklist Infrastructure:**
- [ ] `DCRImageConfiguration.cs` tạo xong, có `VARBINARY(MAX)`
- [ ] `AppDbContext` có `DbSet<DCRImage>`
- [ ] `DCRImageRepository.cs` tạo xong
- [ ] `GetThumbnailsAsync` dùng projection, **không select** `ImageData`
- [ ] DI đăng ký `IDCRImageRepository`
- [ ] Migration tạo thành công, schema có column `ImageData VARBINARY(MAX)`

---

## 3. Application layer
**Project:** `src/DCRManagement.Application`

### 3.1 Tạo DTOs
**File:** `src/DCRManagement.Application/DTOs/DCRImageDto.cs`

```csharp
using DCRManagement.Domain.Enums;

namespace DCRManagement.Application.DTOs;

/// <summary>DTO đầy đủ — dùng khi trả về sau khi upload.</summary>
public record DCRImageDto(
    int Id,
    int DCRId,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    ImageCategory Category,
    string? Caption,
    string UploadedBy,
    DateTime UploadedAt
)
{
    public string FileSizeDisplay => FileSizeBytes switch
    {
        < 1024             => $"{FileSizeBytes} B",
        < 1024 * 1024      => $"{FileSizeBytes / 1024.0:F1} KB",
        _                  => $"{FileSizeBytes / (1024.0 * 1024):F1} MB"
    };
}

/// <summary>DTO thumbnail — không kèm binary, dùng cho danh sách.</summary>
public record DCRImageThumbnailDto(
    int Id,
    int DCRId,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    ImageCategory Category,
    string? Caption,
    DateTime UploadedAt
);

/// <summary>
/// DTO upload — nhận byte[] trực tiếp.
/// Hoạt động với cả hai nguồn: OpenFileDialog và Clipboard paste.
/// FileName tự sinh nếu từ clipboard: "Paste_yyyyMMdd_HHmmss.png"
/// </summary>
public record UploadImageDto(
    int DCRId,
    string FileName,
    byte[] Data,
    string ContentType,
    ImageCategory Category,
    string? Caption
);
```

### 3.2 Tạo `DCRImageService`
**File:** `src/DCRManagement.Application/Services/DCRImageService.cs`

```csharp
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

    // Giới hạn
    private const long MAX_SIZE_BYTES    = 10 * 1024 * 1024; // 10 MB
    private const int  MAX_IMAGES_PER_DCR = 20;

    // Content type được phép
    private static readonly HashSet<string> ALLOWED_CONTENT_TYPES =
    [
        "image/jpeg",
        "image/png",
        "image/bmp",
        "image/tiff"
    ];

    // Magic bytes để xác thực file thật sự là ảnh
    // Quan trọng: ngăn chặn file độc hại được đổi extension
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

    /// <summary>
    /// Upload một ảnh vào DCR.
    /// Nhận byte[] — hoạt động với cả file path lẫn clipboard paste.
    /// </summary>
    public async Task<Result<DCRImageDto>> UploadAsync(UploadImageDto dto)
    {
        try
        {
            // 1. Kiểm tra giới hạn số lượng
            var count = await _imageRepository.CountByDCRAsync(dto.DCRId);
            if (count >= MAX_IMAGES_PER_DCR)
                return Result<DCRImageDto>.Failure(
                    $"DCR đã đạt giới hạn {MAX_IMAGES_PER_DCR} ảnh.",
                    "MAX_IMAGES_REACHED");

            // 2. Kiểm tra kích thước
            if (dto.Data.Length == 0)
                return Result<DCRImageDto>.Failure("Dữ liệu ảnh rỗng.", "EMPTY_DATA");

            if (dto.Data.Length > MAX_SIZE_BYTES)
                return Result<DCRImageDto>.Failure(
                    $"Ảnh vượt giới hạn 10 MB ({dto.Data.Length / 1024.0 / 1024:F1} MB).",
                    "FILE_TOO_LARGE");

            // 3. Kiểm tra content type
            if (!ALLOWED_CONTENT_TYPES.Contains(dto.ContentType))
                return Result<DCRImageDto>.Failure(
                    "Chỉ chấp nhận JPEG, PNG, BMP, TIFF.",
                    "INVALID_CONTENT_TYPE");

            // 4. Xác thực magic bytes — không tin tưởng extension hay ContentType từ client
            if (!VerifyMagicBytes(dto.Data, dto.ContentType))
                return Result<DCRImageDto>.Failure(
                    "Nội dung file không khớp với định dạng khai báo.",
                    "MAGIC_BYTES_MISMATCH");

            var currentUserId = SessionContext.Instance.UserId;

            var image = new DCRImage
            {
                DCRId         = dto.DCRId,
                FileName      = dto.FileName.Trim(),
                ImageData     = dto.Data,
                FileSizeBytes = dto.Data.Length,
                ContentType   = dto.ContentType,
                Category      = dto.Category,
                Caption       = dto.Caption?.Trim(),
                CreatedById   = currentUserId,
                CreatedAt     = DateTime.UtcNow
            };

            await _imageRepository.AddAsync(image);

            _logger.LogInformation(
                "Uploaded image {FileName} ({Size} bytes) to DCR {DCRId} by user {UserId}",
                image.FileName, image.FileSizeBytes, image.DCRId, currentUserId);

            var uploader = await _userRepository.GetByIdAsync(currentUserId);
            return MapToDto(image, uploader?.FullName ?? "Unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to DCR {DCRId}", dto.DCRId);
            throw;
        }
    }

    /// <summary>Lấy danh sách metadata ảnh — không kèm binary.</summary>
    public async Task<IEnumerable<DCRImageThumbnailDto>> GetThumbnailsAsync(int dcrId)
    {
        var thumbnails = await _imageRepository.GetThumbnailsAsync(dcrId);
        return thumbnails.Select(t => new DCRImageThumbnailDto(
            t.Id, t.DCRId, t.FileName, t.FileSizeBytes,
            t.ContentType, t.Category, t.Caption, t.CreatedAt));
    }

    /// <summary>
    /// Lấy binary của một ảnh — chỉ gọi khi người dùng bấm xem.
    /// Trả về byte[] để UI tự render vào PictureBox.
    /// </summary>
    public async Task<Result<byte[]>> GetImageDataAsync(int imageId)
    {
        var image = await _imageRepository.GetWithDataAsync(imageId);
        if (image is null)
            return Result<byte[]>.Failure($"Không tìm thấy ảnh ID {imageId}.", "NOT_FOUND");
        return image.ImageData;
    }

    public async Task<Result> DeleteAsync(int imageId)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image is null)
                return Result.Failure($"Không tìm thấy ảnh ID {imageId}.", "NOT_FOUND");

            // Chỉ người upload hoặc Admin mới được xóa
            var session = SessionContext.Instance;
            if (image.CreatedById != session.UserId && !session.IsAdmin)
                return Result.Failure("Bạn không có quyền xóa ảnh này.", "UNAUTHORIZED");

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

    // ─── Private helpers ──────────────────────────────────────────────────────

    private static bool VerifyMagicBytes(byte[] data, string contentType)
    {
        if (!MAGIC_BYTES.TryGetValue(contentType, out var magic)) return false;
        if (data.Length < magic.Length) return false;
        return data.Take(magic.Length).SequenceEqual(magic);
    }

    private static DCRImageDto MapToDto(DCRImage image, string uploaderName) =>
        new(image.Id, image.DCRId, image.FileName, image.FileSizeBytes,
            image.ContentType, image.Category, image.Caption,
            uploaderName, image.CreatedAt);
}
```

### 3.3 Đăng ký DI
**File:** `src/DCRManagement.Application/DependencyInjection.cs`

Thêm dòng sau vào `AddApplication`:
```csharp
services.AddScoped<DCRImageService>();
```

**✅ Checklist Application:**
- [ ] `DCRImageDto.cs`, `DCRImageThumbnailDto`, `UploadImageDto` tạo xong
- [ ] `DCRImageService.cs` tạo xong
- [ ] `UploadAsync` validate: số lượng → kích thước → content type → magic bytes
- [ ] `GetThumbnailsAsync` KHÔNG trả về `byte[]`
- [ ] `GetImageDataAsync` chỉ trả về `byte[]` khi được gọi tường minh
- [ ] DI đăng ký `DCRImageService`

---

## 4. UI layer (WinForms)
**Project:** `src/DCRManagement.UI`

### 4.1 Tạo interface `IDCRImageView`
**File:** `src/DCRManagement.UI/Presenters/DCRImagePresenter.cs`

```csharp
using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.Domain.Enums;
using DCRManagement.UI.Common;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Presenters;

public interface IDCRImageView : IView
{
    void BindThumbnails(IEnumerable<DCRImageThumbnailDto> thumbnails);
    void DisplayImage(byte[] data, string contentType, string fileName);
    void RemoveThumbnail(int imageId);

    event EventHandler UploadFromFileRequested;
    event EventHandler PasteFromClipboardRequested;   // Ctrl+V hoặc nút Paste
    event EventHandler<int> ViewImageRequested;        // double-click thumbnail
    event EventHandler<int> DeleteImageRequested;
}

public class DCRImagePresenter
{
    private readonly IDCRImageView _view;
    private readonly DCRImageService _imageService;
    private readonly ILogger<DCRImagePresenter> _logger;
    private int _currentDcrId;

    public event EventHandler? DataChanged;

    public DCRImagePresenter(
        IDCRImageView view,
        DCRImageService imageService,
        ILogger<DCRImagePresenter> logger)
    {
        _view         = view;
        _imageService = imageService;
        _logger       = logger;

        _view.UploadFromFileRequested      += async (s, e) => await OnUploadFromFileAsync();
        _view.PasteFromClipboardRequested  += async (s, e) => await OnPasteFromClipboardAsync();
        _view.ViewImageRequested           += async (s, id) => await OnViewImageAsync(id);
        _view.DeleteImageRequested         += async (s, id) => await OnDeleteAsync(id);
    }

    public async Task LoadAsync(int dcrId)
    {
        _currentDcrId = dcrId;
        _view.SetBusy(true);
        try
        {
            var thumbnails = await _imageService.GetThumbnailsAsync(dcrId);
            _view.BindThumbnails(thumbnails);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Upload từ file ────────────────────────────────────────────────────────
    private async Task OnUploadFromFileAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title  = "Chọn hình ảnh",
            Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff",
            Multiselect = true
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        _view.SetBusy(true);
        try
        {
            foreach (var path in dialog.FileNames)
            {
                var data        = await File.ReadAllBytesAsync(path);
                var fileInfo    = new FileInfo(path);
                var contentType = GetContentTypeFromExtension(fileInfo.Extension);

                var dto = new UploadImageDto(
                    DCRId:       _currentDcrId,
                    FileName:    fileInfo.Name,
                    Data:        data,
                    ContentType: contentType,
                    Category:    ImageCategory.Photo,
                    Caption:     null);

                var result = await _imageService.UploadAsync(dto);
                if (!result.IsSuccess)
                {
                    _view.ShowError(result.ErrorMessage!, "Upload thất bại");
                    break;
                }
            }

            // Reload thumbnails sau khi upload
            await LoadAsync(_currentDcrId);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Paste từ clipboard — không có file path ───────────────────────────────
    private async Task OnPasteFromClipboardAsync()
    {
        if (!Clipboard.ContainsImage())
        {
            _view.ShowInfo("Clipboard không có hình ảnh.", "Paste");
            return;
        }

        using var clipImage = Clipboard.GetImage();
        if (clipImage is null) return;

        // Convert System.Drawing.Image → byte[] PNG
        byte[] data;
        using (var ms = new MemoryStream())
        {
            clipImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            data = ms.ToArray();
        }

        // Đặt tên tự sinh — không cần path
        var fileName = $"Paste_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        var dto = new UploadImageDto(
            DCRId:       _currentDcrId,
            FileName:    fileName,
            Data:        data,
            ContentType: "image/png",
            Category:    ImageCategory.Screenshot,  // paste thường là screenshot
            Caption:     null);

        _view.SetBusy(true);
        try
        {
            var result = await _imageService.UploadAsync(dto);
            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Paste thất bại");
                return;
            }

            await LoadAsync(_currentDcrId);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Xem ảnh fullsize ──────────────────────────────────────────────────────
    private async Task OnViewImageAsync(int imageId)
    {
        _view.SetBusy(true);
        try
        {
            var result = await _imageService.GetImageDataAsync(imageId);
            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Lỗi");
                return;
            }

            // Lấy thumbnail info để hiển thị tên file
            var thumbnails = await _imageService.GetThumbnailsAsync(_currentDcrId);
            var info = thumbnails.FirstOrDefault(t => t.Id == imageId);

            _view.DisplayImage(result.Value!, info?.ContentType ?? "image/png", info?.FileName ?? "");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Xóa ảnh ──────────────────────────────────────────────────────────────
    private async Task OnDeleteAsync(int imageId)
    {
        if (!_view.Confirm("Xóa ảnh này?", "Xác nhận")) return;

        _view.SetBusy(true);
        try
        {
            var result = await _imageService.DeleteAsync(imageId);
            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Xóa thất bại");
                return;
            }

            _view.RemoveThumbnail(imageId);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static string GetContentTypeFromExtension(string ext) =>
        ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".bmp"            => "image/bmp",
            ".tif" or ".tiff" => "image/tiff",
            _                 => "image/png"
        };
}
```

### 4.2 Tạo `DCRImagePanel` (UserControl)
**File:** `src/DCRManagement.UI/Forms/DCRImagePanel.cs`

```csharp
using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Panel hiển thị danh sách ảnh dạng thumbnail grid.
/// Hỗ trợ upload từ file và paste từ clipboard (Ctrl+V).
/// </summary>
public class DCRImagePanel : UserControl, IDCRImageView
{
    private readonly DCRImagePresenter _presenter;
    private FlowLayoutPanel _flowThumbnails = null!;
    private Button _btnUpload = null!;
    private Button _btnPaste  = null!;
    private Label  _lblCount  = null!;

    // Lưu map imageId → panel để xóa nhanh
    private readonly Dictionary<int, Panel> _thumbnailPanels = new();

    // ── IDCRImageView events ──────────────────────────────────────────────────
    public event EventHandler? UploadFromFileRequested;
    public event EventHandler? PasteFromClipboardRequested;
    public event EventHandler<int>? ViewImageRequested;
    public event EventHandler<int>? DeleteImageRequested;

    public DCRImagePanel(DCRImageService imageService, ILogger<DCRImagePresenter> logger)
    {
        _presenter = new DCRImagePresenter(this, imageService, logger);
        _presenter.DataChanged += (s, e) => { /* parent form xử lý nếu cần */ };

        InitLayout();
        WireEvents();
    }

    public async Task LoadAsync(int dcrId) =>
        await _presenter.LoadAsync(dcrId);

    // ── IDCRImageView implementation ──────────────────────────────────────────

    public void BindThumbnails(IEnumerable<DCRImageThumbnailDto> thumbnails)
    {
        InvokeIfRequired(() =>
        {
            _flowThumbnails.Controls.Clear();
            _thumbnailPanels.Clear();

            foreach (var t in thumbnails)
                _flowThumbnails.Controls.Add(CreateThumbnailPanel(t));

            _lblCount.Text = $"{thumbnails.Count()} ảnh";
        });
    }

    public void DisplayImage(byte[] data, string contentType, string fileName)
    {
        InvokeIfRequired(() =>
        {
            using var ms = new MemoryStream(data);
            var img = System.Drawing.Image.FromStream(ms);

            var preview = new Form
            {
                Text        = fileName,
                Size        = new Size(900, 700),
                StartPosition = FormStartPosition.CenterParent
            };

            var picBox = new PictureBox
            {
                Dock        = DockStyle.Fill,
                Image       = img,
                SizeMode    = PictureBoxSizeMode.Zoom,
                BackColor   = Color.Black
            };

            preview.Controls.Add(picBox);
            preview.ShowDialog();
        });
    }

    public void RemoveThumbnail(int imageId)
    {
        InvokeIfRequired(() =>
        {
            if (_thumbnailPanels.TryGetValue(imageId, out var panel))
            {
                _flowThumbnails.Controls.Remove(panel);
                _thumbnailPanels.Remove(imageId);
                _lblCount.Text = $"{_thumbnailPanels.Count} ảnh";
            }
        });
    }

    // IView helpers — delegate lên BaseForm nếu cần, hoặc implement trực tiếp
    public void ShowError(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    public void ShowInfo(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    public bool Confirm(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    public void SetBusy(bool isBusy) =>
        InvokeIfRequired(() => Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default);

    // ── Layout ────────────────────────────────────────────────────────────────

    private void InitLayout()
    {
        Dock = DockStyle.Fill;
        KeyPreview = true;   // nhận Ctrl+V trước control con

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 6, 0, 6) };

        _btnUpload = new Button { Text = "📁  Chọn file", Size = new Size(110, 30), Location = new Point(0, 7) };
        _btnPaste  = new Button { Text = "📋  Paste (Ctrl+V)", Size = new Size(130, 30), Location = new Point(118, 7) };
        _lblCount  = new Label  { Text = "0 ảnh", AutoSize = true, Location = new Point(260, 13) };

        toolbar.Controls.AddRange([_btnUpload, _btnPaste, _lblCount]);

        _flowThumbnails = new FlowLayoutPanel
        {
            Dock        = DockStyle.Fill,
            AutoScroll  = true,
            Padding     = new Padding(4)
        };

        Controls.Add(_flowThumbnails);
        Controls.Add(toolbar);
    }

    private void WireEvents()
    {
        _btnUpload.Click += (s, e) => UploadFromFileRequested?.Invoke(this, EventArgs.Empty);
        _btnPaste.Click  += (s, e) => PasteFromClipboardRequested?.Invoke(this, EventArgs.Empty);

        // Ctrl+V bắt ở mức panel
        KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.V)
                PasteFromClipboardRequested?.Invoke(this, EventArgs.Empty);
        };
    }

    private Panel CreateThumbnailPanel(DCRImageThumbnailDto thumbnail)
    {
        var pnl = new Panel { Size = new Size(130, 150), Margin = new Padding(6) };
        _thumbnailPanels[thumbnail.Id] = pnl;

        // Placeholder icon — ảnh thật load khi double-click
        var pic = new PictureBox
        {
            Size     = new Size(120, 100),
            Location = new Point(5, 5),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor   = Color.FromArgb(240, 240, 240),
            Cursor      = Cursors.Hand,
            Tag         = thumbnail.Id
        };
        pic.DoubleClick += (s, e) => ViewImageRequested?.Invoke(this, thumbnail.Id);

        var lblName = new Label
        {
            Text      = thumbnail.FileName.Length > 16
                            ? thumbnail.FileName[..13] + "..."
                            : thumbnail.FileName,
            Location  = new Point(5, 108),
            Size      = new Size(120, 18),
            Font      = new Font("Segoe UI", 8f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var btnDel = new Button
        {
            Text     = "✕",
            Size     = new Size(22, 22),
            Location = new Point(103, 5),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Red,
            Tag      = thumbnail.Id
        };
        btnDel.FlatAppearance.BorderSize = 0;
        btnDel.Click += (s, e) => DeleteImageRequested?.Invoke(this, thumbnail.Id);

        pnl.Controls.AddRange([pic, lblName, btnDel]);
        return pnl;
    }

    private void InvokeIfRequired(Action action)
    {
        if (InvokeRequired) Invoke(action);
        else action();
    }
}
```

### 4.3 Tích hợp `DCRImagePanel` vào `DCRDetailForm`

Trong `DCRDetailForm.Designer.cs`, thêm tab mới vào `_tabRight`:
```csharp
// Thêm sau _tabAttachments
private TabPage _tabImages;
private DCRImagePanel _imagePanel;
```

Trong `InitializeComponent()`:
```csharp
_tabImages = new TabPage { Text = "🖼️  Hình ảnh" };
_imagePanel = new DCRImagePanel(/* inject từ DI */);
_tabImages.Controls.Add(_imagePanel);
_tabRight.TabPages.Add(_tabImages);
```

Trong `DCRDetailForm.cs`, sau khi load DCR (method `LoadDcrAsync`), gọi:
```csharp
await _imagePanel.LoadAsync(_currentDcr.Id);
```

**✅ Checklist UI:**
- [ ] `IDCRImageView` interface định nghĩa đủ events
- [ ] `DCRImagePresenter` xử lý cả `OnUploadFromFileAsync` và `OnPasteFromClipboardAsync`
- [ ] `OnPasteFromClipboardAsync` dùng `Clipboard.GetImage()`, tự sinh `FileName`, không dùng path
- [ ] `DCRImagePanel` có `KeyPreview = true`, bắt `Ctrl+V`
- [ ] `DisplayImage` mở preview form với `PictureBoxSizeMode.Zoom`
- [ ] `DCRImagePanel` được tích hợp vào `DCRDetailForm` tab Images

---

## 5. Kiểm tra sau khi hoàn thành

### 5.1 Build verification
```bash
dotnet build DCRManagement.sln
```
Kết quả mong đợi: **0 errors**

### 5.2 Test cases thủ công

| # | Scenario | Kết quả mong đợi |
|---|----------|------------------|
| 1 | Mở file JPEG hợp lệ < 10 MB | Upload thành công, thumbnail xuất hiện |
| 2 | Paste screenshot từ clipboard | Upload thành công, FileName = `Paste_*.png` |
| 3 | Upload file > 10 MB | Hiển thị lỗi `FILE_TOO_LARGE` |
| 4 | Upload file .exe đổi thành .jpg | Hiển thị lỗi `MAGIC_BYTES_MISMATCH` |
| 5 | Upload 21 ảnh | Ảnh thứ 21 hiển thị lỗi `MAX_IMAGES_REACHED` |
| 6 | Double-click thumbnail | Preview form mở, hiển thị ảnh fullsize |
| 7 | Xóa ảnh không phải của mình (non-admin) | Hiển thị lỗi `UNAUTHORIZED` |
| 8 | Ctrl+V khi clipboard không có ảnh | Hiển thị thông báo "Clipboard không có hình ảnh" |

### 5.3 Kiểm tra query SQL
Mở SQL Server Profiler và xác nhận:
- `GetThumbnailsAsync` **KHÔNG** có `SELECT ImageData` trong câu query
- `GetWithDataAsync` **CÓ** `SELECT ImageData` chỉ khi người dùng double-click

---

## 6. Phụ lục — Tổng hợp file cần tạo/sửa

| Action | File |
|--------|------|
| ✨ Tạo mới | `Domain/Enums/ImageCategory.cs` |
| ✨ Tạo mới | `Domain/Entities/DCRImage.cs` |
| ✨ Tạo mới | `Domain/Entities/DCRImageThumbnailInfo.cs` |
| ✨ Tạo mới | `Domain/Interfaces/IDCRImageRepository.cs` |
| ✏️ Sửa | `Domain/Entities/DCR.cs` — thêm `Images` nav property |
| ✨ Tạo mới | `Infrastructure/Persistence/Configurations/DCRImageConfiguration.cs` |
| ✨ Tạo mới | `Infrastructure/Persistence/Repositories/DCRImageRepository.cs` |
| ✏️ Sửa | `Infrastructure/Persistence/AppDbContext.cs` — thêm `DbSet<DCRImage>` |
| ✏️ Sửa | `Infrastructure/DependencyInjection.cs` — đăng ký repository |
| 🗄️ Chạy | EF Migration `AddDCRImages` |
| ✨ Tạo mới | `Application/DTOs/DCRImageDto.cs` |
| ✨ Tạo mới | `Application/Services/DCRImageService.cs` |
| ✏️ Sửa | `Application/DependencyInjection.cs` — đăng ký service |
| ✨ Tạo mới | `UI/Presenters/DCRImagePresenter.cs` |
| ✨ Tạo mới | `UI/Forms/DCRImagePanel.cs` |
| ✏️ Sửa | `UI/Forms/DCRDetailForm.cs` — tích hợp panel |
| ✏️ Sửa | `UI/Forms/DCRDetailForm.Designer.cs` — thêm tab Images |
