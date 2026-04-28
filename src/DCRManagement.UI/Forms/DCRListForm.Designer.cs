using System.Windows.Forms;

namespace DCRManagement.UI.Forms;

partial class DCRListForm
{
    private System.ComponentModel.IContainer components = null;

    // ── Toolbar row ───────────────────────────────────────────────────────────
    private Panel _pnlToolbar;
    private TextBox _txtSearch;
    private Button _btnSearch;
    private Button _btnRefresh;
    private ComboBox _cmbStatus;
    private ComboBox _cmbPriority;
    private Button _btnCreate;
    private Label _lblStatus;
    private Label _lblPriority;

    // ── Grid ──────────────────────────────────────────────────────────────────
    private DataGridView _grid;

    // ── Paging row ────────────────────────────────────────────────────────────
    private Panel _pnlPaging;
    private Button _btnPrev;
    private Button _btnNext;
    private Label _lblPageInfo;
    private Label _lblTotalCount;

    // ── Section header ────────────────────────────────────────────────────────
    private Label _lblTitle;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _pnlToolbar = new Panel();
        _txtSearch = new TextBox();
        _btnSearch = new Button();
        _btnRefresh = new Button();
        _cmbStatus = new ComboBox();
        _cmbPriority = new ComboBox();
        _btnCreate = new Button();
        _lblStatus = new Label();
        _lblPriority = new Label();
        _grid = new DataGridView();
        _pnlPaging = new Panel();
        _btnPrev = new Button();
        _btnNext = new Button();
        _lblPageInfo = new Label();
        _lblTotalCount = new Label();
        _lblTitle = new Label();

        SuspendLayout();

        // ── Section Title ─────────────────────────────────────────────────────
        _lblTitle.Text = "Design Change Requests";
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

        // Search box
        _txtSearch.Location = new Point(0, 12);
        _txtSearch.Size = new Size(260, 28);
        _txtSearch.Font = ThemeManager.DefaultFont;
        _txtSearch.BorderStyle = BorderStyle.FixedSingle;

        _btnSearch.Text = "🔍  Search";
        _btnSearch.Location = new Point(268, 11);
        _btnSearch.Size = new Size(90, 30);

        _btnRefresh.Text = "↻  Refresh";
        _btnRefresh.Location = new Point(366, 11);
        _btnRefresh.Size = new Size(90, 30);

        // Status filter
        _lblStatus.Text = "Status:";
        _lblStatus.Location = new Point(480, 15);
        _lblStatus.AutoSize = true;
        _lblStatus.Font = ThemeManager.DefaultFont;

        _cmbStatus.Location = new Point(528, 11);
        _cmbStatus.Size = new Size(150, 28);
        _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbStatus.Font = ThemeManager.DefaultFont;
        _cmbStatus.Items.AddRange([
            "All", "Draft", "PendingReview", "UnderReview",
            "PendingApproval", "Approved", "Rejected", "Closed", "Cancelled"
        ]);
        _cmbStatus.SelectedIndex = 0;

        // Priority filter
        _lblPriority.Text = "Priority:";
        _lblPriority.Location = new Point(694, 15);
        _lblPriority.AutoSize = true;
        _lblPriority.Font = ThemeManager.DefaultFont;

        _cmbPriority.Location = new Point(742, 11);
        _cmbPriority.Size = new Size(110, 28);
        _cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbPriority.Font = ThemeManager.DefaultFont;
        _cmbPriority.Items.AddRange(["All", "Low", "Medium", "High", "Critical"]);
        _cmbPriority.SelectedIndex = 0;

        // Create button (right-aligned)
        _btnCreate.Text = "➕  New DCR";
        _btnCreate.Size = new Size(120, 32);
        _btnCreate.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _pnlToolbar.Controls.AddRange([
            _txtSearch, _btnSearch, _btnRefresh,
            _lblStatus, _cmbStatus,
            _lblPriority, _cmbPriority,
            _btnCreate
        ]);

        // ── DataGridView ──────────────────────────────────────────────────────
        _grid.Dock = DockStyle.Fill;
        ThemeManager.StyleDataGrid(_grid);

        // Column definitions — bound in code-behind via BindDCRs()
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "colNumber", HeaderText = "DCR #", DataPropertyName = "DCRNumber", Width = 120, FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Title", DataPropertyName = "Title", FillWeight = 35 },
            new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Status", DataPropertyName = "StatusDisplay", Width = 130, FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "colPriority", HeaderText = "Priority", DataPropertyName = "Priority", Width = 90, FillWeight = 8 },
            new DataGridViewTextBoxColumn { Name = "colCreator", HeaderText = "Created By", DataPropertyName = "CreatedBy", Width = 140, FillWeight = 12 },
            new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "Created",
                DataPropertyName = "CreatedAt",
                Width = 110,
                FillWeight = 10,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            },
            new DataGridViewButtonColumn { Name = "colView", HeaderText = "", Text = "View", UseColumnTextForButtonValue = true, Width = 65, FillWeight = 5 },
            new DataGridViewButtonColumn { Name = "colEdit", HeaderText = "", Text = "Edit", UseColumnTextForButtonValue = true, Width = 65, FillWeight = 5 }
        );

        // ── Paging Panel ──────────────────────────────────────────────────────
        _pnlPaging.Dock = DockStyle.Bottom;
        _pnlPaging.Height = 44;
        _pnlPaging.BackColor = Color.White;
        _pnlPaging.Padding = new Padding(0, 6, 0, 6);

        _btnPrev.Text = "◀  Prev";
        _btnPrev.Location = new Point(0, 8);
        _btnPrev.Size = new Size(80, 28);

        _lblPageInfo.Location = new Point(90, 12);
        _lblPageInfo.AutoSize = true;
        _lblPageInfo.Font = ThemeManager.DefaultFont;
        _lblPageInfo.ForeColor = ThemeManager.TextSecondary;

        _btnNext.Text = "Next  ▶";
        _btnNext.Location = new Point(200, 8);
        _btnNext.Size = new Size(80, 28);

        _lblTotalCount.AutoSize = true;
        _lblTotalCount.Font = ThemeManager.SmallFont;
        _lblTotalCount.ForeColor = ThemeManager.TextSecondary;
        _lblTotalCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _pnlPaging.Controls.AddRange([_btnPrev, _lblPageInfo, _btnNext, _lblTotalCount]);

        // ── Root Assembly ─────────────────────────────────────────────────────
        Controls.Add(_grid);
        Controls.Add(_pnlPaging);
        Controls.Add(_pnlToolbar);
        Controls.Add(_lblTitle);

        ResumeLayout(false);
        PerformLayout();
    }
}