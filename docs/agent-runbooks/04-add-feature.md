# Runbook 04 — Quy trình thêm tính năng mới

> Agent dùng runbook này khi cần implement một feature mới theo đúng kiến trúc của project.

## Ví dụ: Thêm tính năng "Export DCR to PDF"

---

## Quy trình chuẩn (theo Clean Architecture)

```
Domain       → (nếu cần entity/interface mới)
Infrastructure → (implement interface, EF config)
Application  → (service logic, DTO mới)
UI           → (form, presenter, view interface)
Tests        → (unit tests cho service mới)
```

---

## Bước 1 — Domain layer (nếu cần)

Chỉ thêm vào Domain khi cần **entity mới** hoặc **interface mới**.

```csharp
// src/DCRManagement.Domain/Interfaces/IExportService.cs
namespace DCRManagement.Domain.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportDCRToPdfAsync(int dcrId);
}
```

## Bước 2 — Application layer (DTO + Service)

### Thêm DTO nếu cần

```csharp
// src/DCRManagement.Application/DTOs/ExportDto.cs
namespace DCRManagement.Application.DTOs;

public record ExportResultDto(byte[] FileBytes, string FileName, string ContentType);
```

### Viết Service

```csharp
// src/DCRManagement.Application/Services/ExportService.cs
using DCRManagement.Application.Common;
using DCRManagement.Domain.Interfaces;

namespace DCRManagement.Application.Services;

public class ExportService
{
    private readonly IDCRRepository _dcrRepository;
    private readonly IExportService _exportProvider; // injected from Infrastructure
    private readonly ILogger<ExportService> _logger;

    public async Task<Result<ExportResultDto>> ExportToPdfAsync(int dcrId)
    {
        var dcr = await _dcrRepository.GetWithFullDetailsAsync(dcrId);
        if (dcr is null)
            return Result<ExportResultDto>.Failure("DCR not found", "NOT_FOUND");

        var bytes = await _exportProvider.ExportDCRToPdfAsync(dcrId);
        var fileName = $"{dcr.DCRNumber}_{DateTime.Now:yyyyMMdd}.pdf";

        return new ExportResultDto(bytes, fileName, "application/pdf");
    }
}
```

### Đăng ký trong DependencyInjection.cs

```csharp
// src/DCRManagement.Application/DependencyInjection.cs
services.AddScoped<ExportService>();
```

## Bước 3 — Infrastructure layer (implement interface)

```csharp
// src/DCRManagement.Infrastructure/Services/PdfExportService.cs
using DCRManagement.Domain.Interfaces;

namespace DCRManagement.Infrastructure.Services;

public class PdfExportService : IExportService
{
    public async Task<byte[]> ExportDCRToPdfAsync(int dcrId)
    {
        // Implementation với thư viện PDF (QuestPDF, iTextSharp, v.v.)
        throw new NotImplementedException();
    }
}
```

Đăng ký trong `Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<IExportService, PdfExportService>();
```

## Bước 4 — UI layer (Presenter + Form)

### Thêm vào IDCRDetailView interface

```csharp
// DCRDetailPresenter.cs
public interface IDCRDetailView : IView
{
    // ... existing ...
    event EventHandler ExportRequested; // THÊM MỚI
}
```

### Xử lý trong DCRDetailPresenter

```csharp
// Trong constructor:
_view.ExportRequested += async (s, e) => await OnExportAsync();

// Handler mới:
private async Task OnExportAsync()
{
    if (_currentDcr is null) return;

    _view.SetBusy(true);
    try
    {
        var result = await _exportService.ExportToPdfAsync(_currentDcr.Id);
        if (!result.IsSuccess)
        {
            _view.ShowError(result.ErrorMessage!, "Export Failed");
            return;
        }

        // Lưu file
        using var dialog = new SaveFileDialog
        {
            FileName = result.Value!.FileName,
            Filter = "PDF files|*.pdf"
        };
        if (dialog.ShowDialog() == DialogResult.OK)
            await File.WriteAllBytesAsync(dialog.FileName, result.Value.FileBytes);
    }
    finally
    {
        _view.SetBusy(false);
    }
}
```

### Thêm nút Export vào Form

```csharp
// DCRDetailForm.cs
private Button _btnExport;

// Trong WireEvents():
_btnExport.Click += (s, e) => ExportRequested?.Invoke(this, EventArgs.Empty);

// Implement interface:
public event EventHandler? ExportRequested;
```

## Bước 5 — Viết Tests

```csharp
// tests/DCRManagement.Tests/ExportServiceTests.cs
public class ExportServiceTests
{
    [Fact]
    public async Task ExportToPdf_DCRNotFound_ReturnsFailure()
    {
        var repoMock = new Mock<IDCRRepository>();
        repoMock.Setup(r => r.GetWithFullDetailsAsync(999)).ReturnsAsync((DCR?)null);

        var service = new ExportService(repoMock.Object, Mock.Of<IExportService>(),
            Mock.Of<ILogger<ExportService>>());

        var result = await service.ExportToPdfAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
    }
}
```

## Checklist khi thêm feature

- [ ] Interface định nghĩa ở Domain (nếu cần external dependency)
- [ ] Service logic ở Application, dùng Result<T>
- [ ] Infrastructure implement interface
- [ ] DI đăng ký ở cả Application và Infrastructure `DependencyInjection.cs`
- [ ] Presenter nhận event từ View, gọi Service
- [ ] View chỉ raise event, không có logic
- [ ] Unit test cho Service (mock repository và infrastructure)
- [ ] Build pass
- [ ] Tests pass

## Nguyên tắc không được vi phạm

❌ **Không** đặt business logic trong Form/Designer  
❌ **Không** inject DbContext trực tiếp vào Application layer  
❌ **Không** throw exception cho expected failures (dùng `Result<T>`)  
❌ **Không** gọi UI controls trực tiếp từ background thread (dùng `InvokeIfRequired`)  
✅ **Luôn** viết test trước hoặc ngay sau khi implement
