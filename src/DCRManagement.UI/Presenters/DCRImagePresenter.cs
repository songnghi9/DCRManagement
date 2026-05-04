using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.Domain.Enums;
using DCRManagement.UI.Common;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Presenters;

// ── View contract ─────────────────────────────────────────────────────────────

public interface IDCRImageView : IView
{
    /// <summary>Bind thumbnail metadata list — no binary data.</summary>
    void BindThumbnails(IEnumerable<DCRImageThumbnailDto> thumbnails);

    /// <summary>Display a full-size image in a preview window.</summary>
    void DisplayImage(byte[] data, string contentType, string fileName);

    /// <summary>Remove a single thumbnail card from the UI without reloading all.</summary>
    void RemoveThumbnail(int imageId);

    event EventHandler  UploadFromFileRequested;
    event EventHandler  PasteFromClipboardRequested;
    event EventHandler<int> ViewImageRequested;
    event EventHandler<int> DeleteImageRequested;
}

// ── Presenter ─────────────────────────────────────────────────────────────────

public class DCRImagePresenter
{
    private readonly IDCRImageView _view;
    private readonly DCRImageService _imageService;
    private readonly ILogger<DCRImagePresenter> _logger;

    private int    _currentDcrId;
    private string _galleryType = "Before";

    // Current thumbnail size — kept in sync with the gallery size spinners
    private int _thumbW = 140;
    private int _thumbH = 140;

    public event EventHandler? DataChanged;

    public DCRImagePresenter(
        IDCRImageView view,
        DCRImageService imageService,
        ILogger<DCRImagePresenter> logger)
    {
        _view         = view;
        _imageService = imageService;
        _logger       = logger;

        _view.UploadFromFileRequested     += async (s, e) => await OnUploadFromFileAsync();
        _view.PasteFromClipboardRequested += async (s, e) => await OnPasteFromClipboardAsync();
        _view.ViewImageRequested          += async (s, id) => await OnViewImageAsync(id);
        _view.DeleteImageRequested        += async (s, id) => await OnDeleteAsync(id);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task LoadAsync(int dcrId, string galleryType)
    {
        _currentDcrId = dcrId;
        _galleryType  = galleryType;

        _view.SetBusy(true);
        try
        {
            var thumbnails = await _imageService.GetThumbnailsAsync(dcrId, galleryType);
            _view.BindThumbnails(thumbnails);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    /// <summary>
    /// Called by the gallery size spinners when the user changes W or H.
    /// Stored so new uploads use the current size.
    /// </summary>
    public void SetThumbnailSize(int w, int h)
    {
        _thumbW = w;
        _thumbH = h;
    }

    // ── Upload from file ──────────────────────────────────────────────────────

    private async Task OnUploadFromFileAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title       = "Select images",
            Filter      = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff",
            Multiselect = true
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        _view.SetBusy(true);
        try
        {
            var thumbnails = (await _imageService.GetThumbnailsAsync(_currentDcrId, _galleryType))
                             .ToList();
            int nextOrder = thumbnails.Count;

            foreach (var path in dialog.FileNames)
            {
                var data        = await File.ReadAllBytesAsync(path);
                var fileInfo    = new FileInfo(path);
                var contentType = ExtToContentType(fileInfo.Extension);

                var dto = new UploadImageDto(
                    DCRId:          _currentDcrId,
                    FileName:       fileInfo.Name,
                    Data:           data,
                    ContentType:    contentType,
                    Category:       ImageCategory.Photo,
                    Caption:        null,
                    DisplayOrder:   nextOrder++,
                    ThumbnailWidth: _thumbW,
                    ThumbnailHeight: _thumbH,
                    GalleryType:    _galleryType);

                var result = await _imageService.UploadAsync(dto);
                if (!result.IsSuccess)
                {
                    _view.ShowError(result.ErrorMessage!, "Upload failed");
                    break;
                }
            }

            await LoadAsync(_currentDcrId, _galleryType);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Paste from clipboard ──────────────────────────────────────────────────

    private async Task OnPasteFromClipboardAsync()
    {
        if (!Clipboard.ContainsImage())
        {
            _view.ShowInfo("Clipboard does not contain an image.", "Paste");
            return;
        }

        using var clipImage = Clipboard.GetImage();
        if (clipImage is null) return;

        // Convert System.Drawing.Image → PNG byte[]
        byte[] data;
        using (var ms = new MemoryStream())
        {
            clipImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            data = ms.ToArray();
        }

        var thumbnails = (await _imageService.GetThumbnailsAsync(_currentDcrId, _galleryType))
                         .ToList();

        var dto = new UploadImageDto(
            DCRId:          _currentDcrId,
            FileName:       $"Paste_{DateTime.Now:yyyyMMdd_HHmmss}.png",
            Data:           data,
            ContentType:    "image/png",
            Category:       ImageCategory.Screenshot,
            Caption:        null,
            DisplayOrder:   thumbnails.Count,
            ThumbnailWidth: _thumbW,
            ThumbnailHeight: _thumbH,
            GalleryType:    _galleryType);

        _view.SetBusy(true);
        try
        {
            var result = await _imageService.UploadAsync(dto);
            if (!result.IsSuccess)
            {
                _view.ShowError(result.ErrorMessage!, "Paste failed");
                return;
            }

            await LoadAsync(_currentDcrId, _galleryType);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── View full-size ────────────────────────────────────────────────────────

    private async Task OnViewImageAsync(int imageId)
    {
        _view.SetBusy(true);
        try
        {
            var result = await _imageService.GetImageDataAsync(imageId);
            if (!result.IsSuccess) { _view.ShowError(result.ErrorMessage!, "Error"); return; }

            var thumbnails = await _imageService.GetThumbnailsAsync(_currentDcrId, _galleryType);
            var info = thumbnails.FirstOrDefault(t => t.Id == imageId);

            _view.DisplayImage(result.Value!, info?.ContentType ?? "image/png", info?.FileName ?? "");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    private async Task OnDeleteAsync(int imageId)
    {
        if (!_view.Confirm("Delete this image?", "Confirm")) return;

        _view.SetBusy(true);
        try
        {
            var result = await _imageService.DeleteAsync(imageId);
            if (!result.IsSuccess) { _view.ShowError(result.ErrorMessage!, "Delete failed"); return; }

            _view.RemoveThumbnail(imageId);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static string ExtToContentType(string ext) =>
        ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".bmp"            => "image/bmp",
            ".tif" or ".tiff" => "image/tiff",
            _                 => "image/png"
        };
}
