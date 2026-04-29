namespace DCRManagement.UI.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private MenuStrip _menuStrip;
    private ToolStripMenuItem _mnuDCR;
    private ToolStripMenuItem _mnuDCRList;
    private ToolStripMenuItem _mnuDCRCreate;
    private ToolStripMenuItem _mnuAdmin;
    private ToolStripMenuItem _mnuUserManagement;
    private ToolStripMenuItem _mnuAccount;
    private ToolStripMenuItem _mnuSignOut;
    private Panel _pnlSidebar;
    private Panel _pnlContent;
    private Label _lblUserName;
    private Label _lblUserRole;
    private Label _lblSidebarTitle;
    private Button _btnNavDCRList;
    private Button _btnNavCreateDCR;
    private Button _btnNavUsers;
    private StatusStrip _statusStrip;
    private ToolStripStatusLabel _lblStatus;
    private ToolStripStatusLabel _lblStatusRight;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _menuStrip = new MenuStrip();
        _mnuDCR = new ToolStripMenuItem();
        _mnuDCRList = new ToolStripMenuItem();
        _mnuDCRCreate = new ToolStripMenuItem();
        _mnuAdmin = new ToolStripMenuItem();
        _mnuUserManagement = new ToolStripMenuItem();
        _mnuAccount = new ToolStripMenuItem();
        _mnuSignOut = new ToolStripMenuItem();
        _pnlSidebar = new Panel();
        _pnlContent = new Panel();
        _lblSidebarTitle = new Label();
        _lblUserName = new Label();
        _lblUserRole = new Label();
        _btnNavDCRList = new Button();
        _btnNavCreateDCR = new Button();
        _btnNavUsers = new Button();
        _statusStrip = new StatusStrip();
        _lblStatus = new ToolStripStatusLabel();
        _lblStatusRight = new ToolStripStatusLabel();

        SuspendLayout();

        // ── Form ──────────────────────────────────────────────────────────────
        Text = "DCR Management System";
        Size = new Size(1280, 800);
        MinimumSize = new Size(1024, 600);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        BackColor = ThemeManager.BackgroundColor;

        // ── MenuStrip ─────────────────────────────────────────────────────────
        _menuStrip.BackColor = ThemeManager.PrimaryColor;
        _menuStrip.ForeColor = Color.White;
        _menuStrip.Font = ThemeManager.DefaultFont;
        _menuStrip.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());

        _mnuDCR.Text = "DCR";
        _mnuDCR.ForeColor = Color.White;
        _mnuDCRList.Text = "📋  DCR List";
        _mnuDCRCreate.Text = "➕  New DCR";
        _mnuDCR.DropDownItems.AddRange([_mnuDCRList, _mnuDCRCreate]);

        _mnuAdmin.Text = "Admin";
        _mnuAdmin.ForeColor = Color.White;
        _mnuUserManagement.Text = "👥  User Management";
        _mnuAdmin.DropDownItems.Add(_mnuUserManagement);

        _mnuAccount.Text = "Account";
        _mnuAccount.ForeColor = Color.White;
        _mnuSignOut.Text = "🚪  Sign Out";
        _mnuAccount.DropDownItems.Add(_mnuSignOut);

        _menuStrip.Items.AddRange([_mnuDCR, _mnuAdmin, _mnuAccount]);

        // ── Sidebar ───────────────────────────────────────────────────────────
        _pnlSidebar.Dock = DockStyle.Left;
        _pnlSidebar.Width = 200;
        _pnlSidebar.BackColor = Color.FromArgb(30, 30, 45);
        _pnlSidebar.Padding = new Padding(0, 8, 0, 8);

        _lblSidebarTitle.Text = "DCR SYSTEM";
        _lblSidebarTitle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _lblSidebarTitle.ForeColor = Color.FromArgb(150, 170, 210);
        _lblSidebarTitle.Dock = DockStyle.Top;
        _lblSidebarTitle.Height = 40;
        _lblSidebarTitle.TextAlign = ContentAlignment.MiddleCenter;

        _lblUserName.Dock = DockStyle.Bottom;
        _lblUserName.Height = 24;
        _lblUserName.ForeColor = Color.White;
        _lblUserName.Font = ThemeManager.BoldFont;
        _lblUserName.TextAlign = ContentAlignment.MiddleCenter;

        _lblUserRole.Dock = DockStyle.Bottom;
        _lblUserRole.Height = 20;
        _lblUserRole.ForeColor = Color.FromArgb(150, 170, 210);
        _lblUserRole.Font = ThemeManager.SmallFont;
        _lblUserRole.TextAlign = ContentAlignment.MiddleCenter;

        StyleNavButton(_btnNavDCRList, "📋  DCR List");
        StyleNavButton(_btnNavCreateDCR, "➕  New DCR");
        StyleNavButton(_btnNavUsers, "👥  Users");

        _btnNavDCRList.Top = 50;
        _btnNavCreateDCR.Top = 90;
        _btnNavUsers.Top = 130;

        _pnlSidebar.Controls.AddRange([
            _lblSidebarTitle,
            _btnNavDCRList, _btnNavCreateDCR, _btnNavUsers,
            _lblUserRole, _lblUserName
        ]);

        // ── Content Area ──────────────────────────────────────────────────────
        _pnlContent.Dock = DockStyle.Fill;
        _pnlContent.BackColor = ThemeManager.BackgroundColor;
        _pnlContent.Padding = new Padding(16);

        // ── Status Strip ──────────────────────────────────────────────────────
        _statusStrip.BackColor = ThemeManager.PrimaryColor;
        _lblStatus.ForeColor = Color.White;
        _lblStatus.Text = "Ready";
        _lblStatusRight.Spring = true;
        _lblStatusRight.TextAlign = ContentAlignment.MiddleRight;
        _lblStatusRight.ForeColor = Color.FromArgb(200, 220, 255);
        _statusStrip.Items.AddRange([_lblStatus, _lblStatusRight]);

        // ── Assembly ──────────────────────────────────────────────────────────
        Controls.Add(_pnlContent);
        Controls.Add(_pnlSidebar);
        Controls.Add(_statusStrip);
        Controls.Add(_menuStrip);
        MainMenuStrip = _menuStrip;

        ResumeLayout(false);
        PerformLayout();
    }

    private static void StyleNavButton(Button btn, string text)
    {
        btn.Text = text;
        btn.Dock = DockStyle.Top;
        btn.Height = 40;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.ForeColor = Color.FromArgb(200, 210, 230);
        btn.BackColor = Color.Transparent;
        btn.Font = ThemeManager.DefaultFont;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(16, 0, 0, 0);
        btn.Cursor = Cursors.Hand;

        btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 80, 130);
        btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
    }

    // Custom menu color table — makes MenuStrip match primary blue
    private class MenuColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected =>
            Color.FromArgb(0, 90, 158);
        public override Color MenuItemBorder =>
            Color.FromArgb(0, 90, 158);
        public override Color MenuBorder =>
            Color.FromArgb(0, 90, 158);
        public override Color ToolStripDropDownBackground =>
            Color.FromArgb(40, 40, 55);
        public override Color ImageMarginGradientBegin =>
            Color.FromArgb(40, 40, 55);
        public override Color ImageMarginGradientEnd =>
            Color.FromArgb(40, 40, 55);
        public override Color ImageMarginGradientMiddle =>
            Color.FromArgb(40, 40, 55);
    }
}