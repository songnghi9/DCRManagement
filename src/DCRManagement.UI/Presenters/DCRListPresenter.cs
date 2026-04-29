using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.Domain.Enums;
using DCRManagement.UI.Common;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace DCRManagement.UI.Presenters;

public interface IDCRListView : IView
{
    string SearchText { get; }
    string SelectedStatus { get; }      // "All" or DCRStatus name
    string SelectedPriority { get; }    // "All" or priority string
    int CurrentPage { get; set; }
    int PageSize { get; }

    void BindDCRs(IEnumerable<DCRDto> dcrs, int totalCount, int totalPages);
    void SetActionButtonStates(bool canCreate);
    void UpdatePagingControls(int currentPage, int totalPages);

    event EventHandler SearchRequested;
    event EventHandler FilterChanged;
    event EventHandler CreateRequested;
    event EventHandler<int> ViewRequested;      // dcrId
    event EventHandler<int> EditRequested;      // dcrId
    event EventHandler PageChanged;
}

public class DCRListPresenter
{
    private readonly IDCRListView _view;
    private readonly DCRService _dcrService;
    private readonly ILogger<DCRListPresenter> _logger;

    private List<DCRDto> _allDcrs = [];
    private List<DCRDto> _filtered = [];

    public DCRListPresenter(
        IDCRListView view,
        DCRService dcrService,
        ILogger<DCRListPresenter> logger)
    {
        _view = view;
        _dcrService = dcrService;
        _logger = logger;

        _view.SearchRequested += async (s, e) => await ApplyFilterAsync();
        _view.FilterChanged += async (s, e) => { _view.CurrentPage = 1; await ApplyFilterAsync(); };
        _view.PageChanged += async (s, e) => await RenderCurrentPageAsync();

        var session = SessionContext.Instance;
        _view.SetActionButtonStates(session.CanCreateDCR);
    }

    /// <summary>Called once when the list form is shown/activated.</summary>
    public async Task LoadAsync()
    {
        _view.SetBusy(true);
        try
        {
            _allDcrs = (await _dcrService.GetAllForCurrentUserAsync()).ToList();
            _view.CurrentPage = 1;
            await ApplyFilterAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading DCR list");
            _view.ShowError("Failed to load DCR list. Please try again.", "Load Error");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    /// <summary>Refreshes data from DB — called after create/edit/workflow action.</summary>
    public async Task RefreshAsync() => await LoadAsync();

    private async Task ApplyFilterAsync()
    {
        // Capture UI state on the UI thread before entering the background thread
        var searchText = _view.SearchText;
        var selectedStatus = _view.SelectedStatus;
        var selectedPriority = _view.SelectedPriority;

        await Task.Run(() =>
        {
            _filtered = _allDcrs
                .Where(d =>
                {
                    var matchSearch = string.IsNullOrWhiteSpace(searchText)
                        || d.DCRNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                        || d.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                        || d.CreatedBy.Contains(searchText, StringComparison.OrdinalIgnoreCase);

                    var matchStatus = selectedStatus == "All"
                        || d.Status.ToString() == selectedStatus;

                    var matchPriority = selectedPriority == "All"
                        || d.Priority == selectedPriority;

                    return matchSearch && matchStatus && matchPriority;
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
        });

        await RenderCurrentPageAsync();
    }

    private Task RenderCurrentPageAsync()
    {
        var paged = PagedResult<DCRDto>.Create(_filtered, _view.CurrentPage, _view.PageSize);

        _view.BindDCRs(paged.Items, _filtered.Count, paged.TotalPages);
        _view.UpdatePagingControls(_view.CurrentPage, paged.TotalPages);

        return Task.CompletedTask;
    }
}