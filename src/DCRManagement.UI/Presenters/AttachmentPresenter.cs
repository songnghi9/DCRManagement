using DCRManagement.Application.DTOs;
using DCRManagement.Infrastructure.Services;
using DCRManagement.UI.Common;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace DCRManagement.UI.Presenters;

public interface IAttachmentView : IView
{
    void BindAttachments(IEnumerable<AttachmentDto> attachments);
    void UpdateTabBadge(int count);

    event EventHandler AddRequested;
    event EventHandler<int> RemoveRequested;
    event EventHandler<int> OpenRequested;
}

public class AttachmentPresenter
{
    private readonly IAttachmentView _view;
    private readonly AttachmentService _attachmentService;
    private readonly ILogger<AttachmentPresenter> _logger;

    private int _dcrId;
    private List<AttachmentDto> _attachments = [];

    public event EventHandler? AttachmentsChanged;

    public AttachmentPresenter(
        IAttachmentView view,
        AttachmentService attachmentService,
        ILogger<AttachmentPresenter> logger)
    {
        _view = view;
        _attachmentService = attachmentService;
        _logger = logger;

        _view.AddRequested += async (s, e) => await OnAddAsync();
        _view.RemoveRequested += async (s, id) => await OnRemoveAsync(id);
        _view.OpenRequested += (s, id) => OnOpen(id);
    }

    public void Load(int dcrId, IEnumerable<AttachmentDto> existing)
    {
        _dcrId = dcrId;
        _attachments = existing.ToList();
        Render();
    }

    public void UpdateAttachments(IEnumerable<AttachmentDto> attachments)
    {
        _attachments = attachments.ToList();
        Render();
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private async Task OnAddAsync()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select file to attach",
            Filter = "Supported files|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.png;*.jpg;*.jpeg;*.zip" +
                     "|All files|*.*",
            Multiselect = false
        };

        if (ofd.ShowDialog() != DialogResult.OK) return;

        _view.SetBusy(true);
        try
        {
            var attachment = await _attachmentService.SaveAttachmentAsync(
                _dcrId,
                ofd.FileName,
                SessionContext.Instance.UserId);

            if (attachment is null)
            {
                _view.ShowError(
                    "File could not be saved.\n\n" +
                    "Ensure the file type is supported (PDF, Word, Excel, PNG, JPG, ZIP) " +
                    "and the size is under 50 MB.",
                    "Upload Failed");
                return;
            }

            // Map entity → DTO for immediate display (no round-trip DB read)
            var dto = new AttachmentDto(
                attachment.Id,
                attachment.DCRId,
                attachment.FileName,
                attachment.FilePath,
                attachment.FileSizeBytes,
                attachment.ContentType,
                SessionContext.Instance.FullName,
                attachment.CreatedAt,
                ImageType: null,
                DisplayOrder: null);

            _attachments.Add(dto);
            Render();
            AttachmentsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attachment to DCR {DCRId}", _dcrId);
            _view.ShowError("An unexpected error occurred while uploading the file.", "Error");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    private async Task OnRemoveAsync(int attachmentId)
    {
        if (!_view.Confirm("Remove this attachment permanently?", "Confirm Remove"))
            return;

        _view.SetBusy(true);
        try
        {
            await _attachmentService.DeleteAttachmentAsync(attachmentId);
            _attachments.RemoveAll(a => a.Id == attachmentId);
            Render();
            AttachmentsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing attachment {AttachmentId}", attachmentId);
            _view.ShowError("Failed to remove attachment.", "Error");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    private void OnOpen(int attachmentId)
    {
        var att = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (att is null) return;

        try
        {
            if (!File.Exists(att.FilePath))
            {
                _view.ShowError("File not found on disk. It may have been moved or deleted.", "Not Found");
                return;
            }

            // Shell-open — respects user's default application for the file type
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = att.FilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening attachment {AttachmentId}", attachmentId);
            _view.ShowError("Could not open the file.", "Error");
        }
    }

    private void Render()
    {
        _view.BindAttachments(_attachments);
        _view.UpdateTabBadge(_attachments.Count);
    }
}