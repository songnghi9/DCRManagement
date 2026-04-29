using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.UI.Common;
using DCRManagement.UI.Presenters;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI.Forms;

public partial class UserManagementForm : BaseUserControl, IUserManagementView
{
    private readonly UserManagementPresenter _presenter;

    // ─── IUserManagementView ──────────────────────────────────────────────────
    public string SearchText => _txtSearch.Text.Trim();
    public string SelectedRole => _cmbRole.SelectedItem?.ToString() ?? "All";

    public event EventHandler? SearchRequested;
    public event EventHandler? FilterChanged;
    public event EventHandler? CreateRequested;
    public event EventHandler<int>? EditRequested;
    public event EventHandler<int>? ToggleActiveRequested;
    public event EventHandler<int>? ResetPasswordRequested;
    public event EventHandler? RefreshRequested;

    public UserManagementForm(
        UserService userService,
        ILogger<UserManagementPresenter> logger)
    {
        InitializeComponent();
        ApplyStyling();

        _presenter = new UserManagementPresenter(this, userService, logger);
        _presenter.OpenCreateRequested += OnOpenCreate;
        _presenter.OpenEditRequested += OnOpenEdit;

        WireEvents();
    }

    public async Task LoadAsync() => await _presenter.LoadAsync();

    // ─── IUserManagementView ──────────────────────────────────────────────────

    public void BindUsers(IEnumerable<UserDto> users, int totalCount)
    {
        InvokeIfRequired(() =>
        {
            var list = users.ToList();
            _grid.DataSource = list;

            // Grey out inactive rows
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is UserDto u && !u.IsActive)
                {
                    row.DefaultCellStyle.ForeColor = Color.Gray;
                    row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                }
            }

            // Dynamic Toggle button text per row
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is UserDto u)
                {
                    var cell = row.Cells["colToggle"] as DataGridViewButtonCell;
                    if (cell is not null)
                        cell.Value = u.IsActive ? "🚫 Deactivate" : "✅ Activate";
                }
            }

            _lblCount.Text = $"{totalCount} user{(totalCount != 1 ? "s" : "")}  ";
        });
    }

    public void SetSelectedUser(UserDto? user) { /* detail panel unused — dialog-based */ }

    // ─── Private ──────────────────────────────────────────────────────────────

    private void ApplyStyling()
    {
        ThemeManager.StylePrimaryButton(_btnCreate);
        ThemeManager.StyleSecondaryButton(_btnSearch);
        ThemeManager.StyleSecondaryButton(_btnRefresh);

        _txtSearch.SetPlaceholder("Search by name, username, email...");

        _pnlToolbar.Resize += (s, e) =>
            _btnCreate.Left = _pnlToolbar.Width - _btnCreate.Width - 4;
    }

    private void WireEvents()
    {
        _btnSearch.Click += (s, e) => SearchRequested?.Invoke(this, EventArgs.Empty);
        _btnRefresh.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

        _txtSearch.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
                SearchRequested?.Invoke(this, EventArgs.Empty);
        };

        _cmbRole.SelectedIndexChanged += (s, e) =>
            FilterChanged?.Invoke(this, EventArgs.Empty);

        _btnCreate.Click += (s, e) => CreateRequested?.Invoke(this, EventArgs.Empty);

        _grid.CellClick += OnGridCellClick;
    }

    private void OnGridCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (_grid.Rows[e.RowIndex].DataBoundItem is not UserDto user) return;

        switch (_grid.Columns[e.ColumnIndex].Name)
        {
            case "colEdit":
                EditRequested?.Invoke(this, user.Id);
                break;
            case "colToggle":
                ToggleActiveRequested?.Invoke(this, user.Id);
                break;
            case "colPwd":
                ResetPasswordRequested?.Invoke(this, user.Id);
                break;
        }
    }

    private void OnOpenCreate(object? sender, EventArgs e)
    {
        using var dlg = new UserDetailDialog(null);
        if (dlg.ShowDialog() != DialogResult.OK) return;

        _ = RunAsync(async () =>
        {
            var svc = Program.ServiceProvider
                .GetRequiredService<UserService>();
            var result = await svc.CreateUserAsync(dlg.GetCreateDto());

            if (!result.IsSuccess)
                ShowError(result.ErrorMessage!, "Create Failed");
            else
            {
                ShowInfo($"User '{result.Value!.Username}' created successfully.", "Created");
                RefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }, "create user");
    }

    private void OnOpenEdit(object? sender, UserDto user)
    {
        using var dlg = new UserDetailDialog(user);
        if (dlg.ShowDialog() != DialogResult.OK) return;

        _ = RunAsync(async () =>
        {
            var svc = Program.ServiceProvider
                .GetRequiredService<UserService>();
            var result = await svc.UpdateUserAsync(dlg.GetUpdateDto(user.Id));

            if (!result.IsSuccess)
                ShowError(result.ErrorMessage!, "Update Failed");
            else
                RefreshRequested?.Invoke(this, EventArgs.Empty);
        }, "update user");
    }
}