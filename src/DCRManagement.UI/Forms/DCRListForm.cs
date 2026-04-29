using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Forms;

/// <summary>
/// DCR list view — implements IDCRListView, delegates all logic to DCRListPresenter.
/// UserControl only handles control events and data binding; zero business logic here.
/// </summary>
public partial class DCRListForm : BaseUserControl, IDCRListView
{
    private readonly DCRListPresenter _presenter;
    private const int DEFAULT_PAGE_SIZE = 20;

    // ─── IDCRListView ─────────────────────────────────────────────────────────
    public string SearchText => _txtSearch.Text.Trim();
    public string SelectedStatus => _cmbStatus.SelectedItem?.ToString() ?? "All";
    public string SelectedPriority => _cmbPriority.SelectedItem?.ToString() ?? "All";
    public int CurrentPage { get; set; } = 1;
    public int PageSize => DEFAULT_PAGE_SIZE;

    public event EventHandler? SearchRequested;
    public event EventHandler? FilterChanged;
    public event EventHandler? CreateRequested;
    public event EventHandler<int>? ViewRequested;
    public event EventHandler<int>? EditRequested;
    public event EventHandler? PageChanged;

    public DCRListForm(DCRService dcrService, ILogger<DCRListPresenter> logger)
    {
        InitializeComponent();
        ApplyStyling();

        _presenter = new DCRListPresenter(this, dcrService, logger);
        _presenter.OpenCreateRequested += (s, e) => CreateRequested?.Invoke(this, EventArgs.Empty);
        _presenter.OpenViewRequested += (s, id) => ViewRequested?.Invoke(this, id);
        _presenter.OpenEditRequested += (s, id) => EditRequested?.Invoke(this, id);

        WireEvents();
    }

    /// <summary>Called by MainForm after navigation.</summary>
    public async Task LoadAsync() => await _presenter.LoadAsync();

    // ─── IDCRListView methods ─────────────────────────────────────────────────

    public void BindDCRs(IEnumerable<DCRDto> dcrs, int totalCount, int totalPages)
    {
        InvokeIfRequired(() =>
        {
            var list = dcrs.ToList();
            _grid.DataSource = list;

            // Colour-code the status column cells
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is DCRDto dcr)
                {
                    var statusCell = row.Cells["colStatus"];
                    statusCell.Style.BackColor = ThemeManager.GetStatusColor(dcr.Status.ToString());
                    statusCell.Style.ForeColor = Color.White;
                    statusCell.Style.Font = ThemeManager.BoldFont;
                }
            }

            _lblTotalCount.Text = $"{totalCount} record{(totalCount != 1 ? "s" : "")} found";

            // Right-align total count label
            _lblTotalCount.Left = _pnlPaging.Width - _lblTotalCount.Width - 12;
        });
    }

    public void SetActionButtonStates(bool canCreate)
    {
        _btnCreate.Visible = canCreate;
    }

    public void UpdatePagingControls(int currentPage, int totalPages)
    {
        InvokeIfRequired(() =>
        {
            _lblPageInfo.Text = $"Page {currentPage} of {totalPages}";
            _btnPrev.Enabled = currentPage > 1;
            _btnNext.Enabled = currentPage < totalPages;
        });
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private void ApplyStyling()
    {
        ThemeManager.StylePrimaryButton(_btnCreate);
        ThemeManager.StyleSecondaryButton(_btnSearch);
        ThemeManager.StyleSecondaryButton(_btnRefresh);
        ThemeManager.StyleSecondaryButton(_btnPrev);
        ThemeManager.StyleSecondaryButton(_btnNext);

        _txtSearch.SetPlaceholder("Search by DCR#, title, creator...");

        // Position Create button flush right
        _pnlToolbar.Resize += (s, e) =>
            _btnCreate.Left = _pnlToolbar.Width - _btnCreate.Width - 4;
    }

    private void WireEvents()
    {
        _btnSearch.Click += (s, e) => SearchRequested?.Invoke(this, EventArgs.Empty);
        _btnRefresh.Click += async (s, e) =>
            await RunAsync(() => _presenter.RefreshAsync(), "refresh");

        _txtSearch.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
                SearchRequested?.Invoke(this, EventArgs.Empty);
        };

        _cmbStatus.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);
        _cmbPriority.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke(this, EventArgs.Empty);

        _btnCreate.Click += (s, e) => CreateRequested?.Invoke(this, EventArgs.Empty);

        _btnPrev.Click += (s, e) =>
        {
            CurrentPage--;
            PageChanged?.Invoke(this, EventArgs.Empty);
        };

        _btnNext.Click += (s, e) =>
        {
            CurrentPage++;
            PageChanged?.Invoke(this, EventArgs.Empty);
        };

        // Handle View / Edit button columns
        _grid.CellClick += OnGridCellClick;

        // Double-click row → View
        _grid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_grid.Rows[e.RowIndex].DataBoundItem is DCRDto dcr)
                ViewRequested?.Invoke(this, dcr.Id);
        };
    }

    private void OnGridCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (_grid.Rows[e.RowIndex].DataBoundItem is not DCRDto dcr) return;

        switch (_grid.Columns[e.ColumnIndex].Name)
        {
            case "colView":
                ViewRequested?.Invoke(this, dcr.Id);
                break;

            case "colEdit":
                // Only editable DCRs show active Edit button
                if (dcr.IsEditable)
                    EditRequested?.Invoke(this, dcr.Id);
                else
                    ShowInfo(
                        $"DCR cannot be edited in '{dcr.StatusDisplay}' status.\nOnly Draft or Rejected DCRs are editable.",
                        "Edit Restricted");
                break;
        }
    }
}