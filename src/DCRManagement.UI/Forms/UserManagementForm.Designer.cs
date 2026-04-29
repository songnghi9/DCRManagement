namespace DCRManagement.UI.Forms;

partial class UserManagementForm
{
    private System.ComponentModel.IContainer components = null;

    private Label _lblTitle;
    private Panel _pnlToolbar;
    private TextBox _txtSearch;
    private Button _btnSearch;
    private Button _btnRefresh;
    private ComboBox _cmbRole;
    private Label _lblRole;
    private Button _btnCreate;
    private DataGridView _grid;
    private Label _lblCount;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _lblTitle = new Label();
        _pnlToolbar = new Panel();
        _txtSearch = new TextBox();
        _btnSearch = new Button();
        _btnRefresh = new Button();
        _cmbRole = new ComboBox();
        _lblRole = new Label();
        _btnCreate = new Button();
        _grid = new DataGridView();
        _lblCount = new Label();

        SuspendLayout();

        // ── Section title ─────────────────────────────────────────────────────
        _lblTitle.Text = "User Management";
        _lblTitle.Font = ThemeManager.TitleFont;
        _lblTitle.ForeColor = ThemeManager.TextPrimary;
        _lblTitle.Dock = DockStyle.Top;
        _lblTitle.Height = 44;
        _lblTitle.TextAlign = ContentAlignment.BottomLeft;

        // ── Toolbar ───────────────────────────────────────────────────────────
        _pnlToolbar.Dock = DockStyle.Top;
        _pnlToolbar.Height = 56;
        _pnlToolbar.BackColor = Color.White;
        _pnlToolbar.Padding = new Padding(0, 8, 0, 8);

        _txtSearch.Location = new Point(0, 12);
        _txtSearch.Size = new Size(240, 28);
        _txtSearch.Font = ThemeManager.DefaultFont;
        _txtSearch.BorderStyle = BorderStyle.FixedSingle;

        _btnSearch.Text = "🔍  Search";
        _btnSearch.Location = new Point(248, 11);
        _btnSearch.Size = new Size(90, 30);

        _btnRefresh.Text = "↻  Refresh";
        _btnRefresh.Location = new Point(346, 11);
        _btnRefresh.Size = new Size(90, 30);

        _lblRole.Text = "Role:";
        _lblRole.Location = new Point(460, 15);
        _lblRole.AutoSize = true;
        _lblRole.Font = ThemeManager.DefaultFont;

        _cmbRole.Location = new Point(498, 11);
        _cmbRole.Size = new Size(140, 28);
        _cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbRole.Font = ThemeManager.DefaultFont;
        _cmbRole.Items.AddRange(["All", "Admin", "Engineer", "Reviewer", "Approver", "ReadOnly"]);
        _cmbRole.SelectedIndex = 0;

        _btnCreate.Text = "➕  New User";
        _btnCreate.Size = new Size(120, 32);
        _btnCreate.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _pnlToolbar.Controls.AddRange([
            _txtSearch, _btnSearch, _btnRefresh,
            _lblRole, _cmbRole, _btnCreate
        ]);

        // ── DataGridView ──────────────────────────────────────────────────────
        _grid.Dock = DockStyle.Fill;
        ThemeManager.StyleDataGrid(_grid);
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                Name = "colUsername",
                HeaderText = "Username",
                DataPropertyName = "Username",
                Width = 120
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colFullName",
                HeaderText = "Full Name",
                DataPropertyName = "FullName",
                FillWeight = 30
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colEmail",
                HeaderText = "Email",
                DataPropertyName = "Email",
                FillWeight = 30
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colRole",
                HeaderText = "Role",
                DataPropertyName = "Role",
                Width = 100
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colDept",
                HeaderText = "Department",
                DataPropertyName = "Department",
                Width = 120
            },
            new DataGridViewCheckBoxColumn
            {
                Name = "colActive",
                HeaderText = "Active",
                DataPropertyName = "IsActive",
                Width = 60,
                ReadOnly = true
            },
            new DataGridViewButtonColumn
            {
                Name = "colEdit",
                HeaderText = "",
                Text = "✏ Edit",
                UseColumnTextForButtonValue = true,
                Width = 75
            },
            new DataGridViewButtonColumn
            {
                Name = "colToggle",
                HeaderText = "",
                Text = "Toggle",
                UseColumnTextForButtonValue = true,
                Width = 85
            },
            new DataGridViewButtonColumn
            {
                Name = "colPwd",
                HeaderText = "",
                Text = "🔑 Pwd",
                UseColumnTextForButtonValue = true,
                Width = 75
            }
        );

        // ── Count label ───────────────────────────────────────────────────────
        _lblCount.Dock = DockStyle.Bottom;
        _lblCount.Height = 24;
        _lblCount.Font = ThemeManager.SmallFont;
        _lblCount.ForeColor = ThemeManager.TextSecondary;
        _lblCount.TextAlign = ContentAlignment.MiddleRight;
        _lblCount.BackColor = Color.White;

        Controls.Add(_grid);
        Controls.Add(_lblCount);
        Controls.Add(_pnlToolbar);
        Controls.Add(_lblTitle);

        ResumeLayout(false);
        PerformLayout();
    }
}