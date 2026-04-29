# Integration Guide: Using ImageGalleryControl in DCRDetailPresenter

## Overview

This guide shows how to integrate the new image gallery functionality into the DCRDetailPresenter to handle image persistence and display.

## Example: DCRDetailPresenter Integration

### 1. Saving Images When Submitting DCR

```csharp
// In DCRDetailPresenter.cs

public class DCRDetailPresenter
{
    private readonly DCRService _dcrService;
    private readonly IDCRDetailView _view;

    public async Task SaveAsync()
    {
        try
        {
            // Get form data
            var dto = new CreateOrUpdateDCRDto
            {
                Title = _view.DCRTitle,
                Description = _view.Description,
                // ... other fields ...
            };

            // Get images from galleries
            var beforeImages = _view.GetBeforeImages();
            var afterImages = _view.GetAfterImages();

            // Save DCR with images
            int dcrId = await _dcrService.SaveAsync(dto);

            // Save images to storage
            if (beforeImages.Count > 0)
            {
                await _dcrService.SaveImagesAsync(dcrId, ImageType.Before, beforeImages);
            }

            if (afterImages.Count > 0)
            {
                await _dcrService.SaveImagesAsync(dcrId, ImageType.After, afterImages);
            }

            MessageBox.Show("DCR saved successfully", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save DCR");
            MessageBox.Show($"Error: {ex.Message}", "Save Failed");
        }
    }
}
```

### 2. Loading Images When Opening DCR

```csharp
public async Task InitViewAsync(int dcrId)
{
    try
    {
        // Load DCR data
        var dcr = await _dcrService.GetByIdAsync(dcrId);

        // Populate form
        _view.DCRTitle = dcr.Title;
        _view.Description = dcr.Description;
        // ... other fields ...

        // Load images
        var images = await _dcrService.GetImagesAsync(dcrId);

        // Add images to galleries
        _view.ClearBeforeImages();
        _view.ClearAfterImages();

        foreach (var image in images.BeforeImages)
        {
            _view.AddBeforeImage(image);
        }

        foreach (var image in images.AfterImages)
        {
            _view.AddAfterImage(image);
        }

        _view.SetMode(DetailMode.View);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load DCR {dcrId}", dcrId);
        MessageBox.Show($"Error loading DCR: {ex.Message}", "Load Failed");
    }
}
```

### 3. Creating New DCR with Initial Images

```csharp
public void InitCreate()
{
    // Clear any existing data
    _view.DCRTitle = string.Empty;
    _view.Description = string.Empty;

    // Clear images
    _view.ClearBeforeImages();
    _view.ClearAfterImages();

    // Optional: Load default/template images if needed
    // var defaultBefore = Image.FromFile("Resources/placeholder-before.png");
    // _view.AddBeforeImage(defaultBefore);

    _view.SetMode(DetailMode.Create);
}
```

### 4. Editing Existing DCR

```csharp
public async Task InitEditAsync(int dcrId)
{
    // Same as InitViewAsync, but set to Edit mode
    try
    {
        var dcr = await _dcrService.GetByIdAsync(dcrId);

        _view.DCRTitle = dcr.Title;
        // ... populate other fields ...

        // Load existing images
        var images = await _dcrService.GetImagesAsync(dcrId);

        _view.ClearBeforeImages();
        _view.ClearAfterImages();

        foreach (var image in images.BeforeImages)
        {
            _view.AddBeforeImage(image);
        }

        foreach (var image in images.AfterImages)
        {
            _view.AddAfterImage(image);
        }

        // Set to Edit mode - galleries will be enabled
        _view.SetMode(DetailMode.Edit);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load DCR for editing {dcrId}", dcrId);
        MessageBox.Show($"Error: {ex.Message}", "Edit Failed");
    }
}
```

### 5. Deleting Images from Saved DCR

```csharp
public async Task DeleteImagesAsync(int dcrId, ImageType imageType, List<int> imageIds)
{
    try
    {
        await _dcrService.DeleteImagesAsync(dcrId, imageType, imageIds);

        // Reload images to refresh gallery
        var images = await _dcrService.GetImagesAsync(dcrId);

        if (imageType == ImageType.Before)
        {
            _view.ClearBeforeImages();
            foreach (var image in images.BeforeImages)
            {
                _view.AddBeforeImage(image);
            }
        }
        else
        {
            _view.ClearAfterImages();
            foreach (var image in images.AfterImages)
            {
                _view.AddAfterImage(image);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to delete images");
        MessageBox.Show("Error deleting images", "Delete Failed");
    }
}
```

## Service Layer Changes

### Image Service Methods Required

```csharp
public interface IDCRService
{
    // Existing methods...

    // Image handling methods
    Task SaveImagesAsync(int dcrId, ImageType imageType, List<Image> images);
    Task<DCRImagesDto> GetImagesAsync(int dcrId);
    Task DeleteImagesAsync(int dcrId, ImageType imageType, List<int> imageIds);
}

// DTOs
public class DCRImagesDto
{
    public List<Image> BeforeImages { get; set; }
    public List<Image> AfterImages { get; set; }
}

public enum ImageType
{
    Before = 0,
    After = 1
}
```

## View Interface Update

```csharp
public interface IDCRDetailView
{
    // Existing properties...

    // New image methods
    List<System.Drawing.Image> GetBeforeImages();
    List<System.Drawing.Image> GetAfterImages();
    void ClearBeforeImages();
    void ClearAfterImages();
    void AddBeforeImage(System.Drawing.Image image);
    void AddAfterImage(System.Drawing.Image image);
}
```

## Image Storage Implementation Example

```csharp
public class ImageStorageService
{
    private readonly string _imagePath;

    public ImageStorageService(IConfiguration config)
    {
        _imagePath = config["ImageStorage:Path"] ?? "App_Data/Images";
    }

    /// <summary>
    /// Save images to disk/cloud storage
    /// </summary>
    public async Task SaveImagesAsync(int dcrId, ImageType imageType, List<Image> images)
    {
        var folderPath = Path.Combine(_imagePath, dcrId.ToString());

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        for (int i = 0; i < images.Count; i++)
        {
            var filename = $"{imageType}_{i:D3}.jpg";
            var filepath = Path.Combine(folderPath, filename);

            // Convert to JPEG for consistent format
            using (var bitmap = new Bitmap(images[i]))
            {
                bitmap.Save(filepath, ImageFormat.Jpeg);
            }
        }
    }

    /// <summary>
    /// Load images from storage
    /// </summary>
    public async Task<DCRImagesDto> LoadImagesAsync(int dcrId)
    {
        var folderPath = Path.Combine(_imagePath, dcrId.ToString());
        var result = new DCRImagesDto { BeforeImages = new(), AfterImages = new() };

        if (!Directory.Exists(folderPath))
            return result;

        // Load Before images
        var beforeFiles = Directory.GetFiles(folderPath, "Before_*.jpg")
            .OrderBy(f => f)
            .ToList();

        foreach (var file in beforeFiles)
        {
            result.BeforeImages.Add(Image.FromFile(file));
        }

        // Load After images
        var afterFiles = Directory.GetFiles(folderPath, "After_*.jpg")
            .OrderBy(f => f)
            .ToList();

        foreach (var file in afterFiles)
        {
            result.AfterImages.Add(Image.FromFile(file));
        }

        return result;
    }

    /// <summary>
    /// Delete images for DCR
    /// </summary>
    public async Task DeleteImagesAsync(int dcrId)
    {
        var folderPath = Path.Combine(_imagePath, dcrId.ToString());

        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
        }
    }
}
```

## Error Handling Patterns

### Safe Image Load with Fallback

```csharp
public async Task LoadImagesWithFallbackAsync(int dcrId)
{
    try
    {
        var images = await _dcrService.GetImagesAsync(dcrId);

        _view.ClearBeforeImages();
        _view.ClearAfterImages();

        // Load with error recovery
        foreach (var image in images.BeforeImages)
        {
            try
            {
                _view.AddBeforeImage(image);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load before image");
                // Continue loading other images
            }
        }

        foreach (var image in images.AfterImages)
        {
            try
            {
                _view.AddAfterImage(image);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load after image");
                // Continue loading other images
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load DCR images");
        MessageBox.Show("Warning: Could not load all images", "Partial Load");
    }
}
```

### Image Validation

```csharp
private bool ValidateImages(List<Image> beforeImages, List<Image> afterImages)
{
    // Validate image count
    const int maxImagesPerType = 20;

    if (beforeImages.Count > maxImagesPerType || afterImages.Count > maxImagesPerType)
    {
        MessageBox.Show($"Maximum {maxImagesPerType} images per type", "Too Many Images");
        return false;
    }

    // Validate image sizes
    const long maxImageSize = 5 * 1024 * 1024; // 5MB

    foreach (var img in beforeImages.Concat(afterImages))
    {
        // Note: System.Drawing.Image doesn't have direct size, 
        // so validate at save time instead
    }

    return true;
}
```

## Database Schema Example

### Images Table

```sql
CREATE TABLE DCRImages (
    ImageId INT PRIMARY KEY IDENTITY(1,1),
    DCRId INT NOT NULL,
    ImageType INT NOT NULL, -- 0=Before, 1=After
    FileName NVARCHAR(255) NOT NULL,
    FileSize INT NOT NULL,
    MimeType NVARCHAR(50) NOT NULL,
    StoragePath NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (DCRId) REFERENCES DCRs(Id) ON DELETE CASCADE
);

CREATE INDEX IX_DCRImages_DCRId ON DCRImages(DCRId);
CREATE INDEX IX_DCRImages_ImageType ON DCRImages(ImageType);
```

## Testing Examples

### Unit Test for Image Gallery

```csharp
[TestClass]
public class ImageGalleryControlTests
{
    private ImageGalleryControl _gallery;

    [TestInitialize]
    public void Setup()
    {
        _gallery = new ImageGalleryControl();
    }

    [TestMethod]
    public void AddImage_Should_RaisImageAddedEvent()
    {
        // Arrange
        var image = new Bitmap(100, 100);
        bool eventFired = false;
        _gallery.ImageAdded += (s, e) => eventFired = true;

        // Act
        _gallery.AddImage(image);

        // Assert
        Assert.IsTrue(eventFired, "ImageAdded event should fire");
        Assert.AreEqual(1, _gallery.GetImages().Count);
    }

    [TestMethod]
    public void ClearImages_Should_RemoveAllImages()
    {
        // Arrange
        _gallery.AddImage(new Bitmap(100, 100));
        _gallery.AddImage(new Bitmap(100, 100));
        Assert.AreEqual(2, _gallery.GetImages().Count);

        // Act
        _gallery.ClearImages();

        // Assert
        Assert.AreEqual(0, _gallery.GetImages().Count);
    }
}
```

## Summary

The new ImageGalleryControl provides a complete solution for handling multiple images in the Before/After sections. The presenter layer should:

1. **On Save:** Extract images via `GetBeforeImages()` / `GetAfterImages()`
2. **On Load:** Populate galleries via `AddBeforeImage()` / `AddAfterImage()`
3. **On Clear:** Use `ClearBeforeImages()` / `ClearAfterImages()`
4. **Track Changes:** Monitor image events to set dirty flag
5. **Error Recovery:** Handle individual image load failures gracefully
