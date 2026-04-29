namespace DCRManagement.UI.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    // ─── Controls ─────────────────────────────────────────────────────────────
    private Panel _pnlBackground;
    private Panel _pnlCard;
    private Panel _pnlHeader;
    private Label _lblAppTitle;
    private Label _lblAppSubtitle;
    private Label _lblUsername;
    private TextBox _txtUsername;
    private Label _lblPassword;
    private TextBox _txtPassword;
    private CheckBox _chkShowPassword;
    private Button _btnLogin;
    private Label _lblVersion;
    private PictureBox _pbxLogo;
    private Label _lblLoading;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _pnlBackground = new Panel();
        _pnlCard = new Panel();
        _pnlHeader = new Panel();
        _lblAppTitle = new Label();
        _lblAppSubtitle = new Label();
        _lblUsername = new Label();
        _txtUsername = new TextBox();
        _lblPassword = new Label();
        _txtPassword = new TextBox();
        _chkShowPassword = new CheckBox();
        _btnLogin = new Button();
        _lblVersion = new Label();
        _pbxLogo = new PictureBox();
        _lblLoading = new Label();

        // ── Form ──────────────────────────────────────────────────────────────
        Text = "DCR Management System — Login";
        Size = new Size(440, 560);
        MinimumSize = new Size(440, 560);
        MaximumSize = new Size(440, 560);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ThemeManager.PrimaryColor;

        // ── Background Panel ──────────────────────────────────────────────────
        _pnlBackground.Dock = DockStyle.Fill;
        _pnlBackground.BackColor = ThemeManager.PrimaryColor;

        // ── Card Panel ────────────────────────────────────────────────────────
        _pnlCard.Size = new Size(360, 420);
        _pnlCard.Location = new Point(40, 80);
        _pnlCard.BackColor = Color.White;
        _pnlCard.Padding = new Padding(32, 24, 32, 24);

        // Card subtle shadow via BorderStyle
        _pnlCard.Paint += (s, e) =>
        {
            ControlPaint.DrawBorder(e.Graphics, _pnlCard.ClientRectangle,
                ThemeManager.BorderColor, ButtonBorderStyle.Solid);
        };

        // ── Header Panel (blue strip at top of card) ──────────────────────────
        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Height = 80;
        _pnlHeader.BackColor = ThemeManager.PrimaryColor;
        _pnlHeader.Padding = new Padding(24, 16, 24, 0);

        // ── App Title ─────────────────────────────────────────────────────────
        _lblAppTitle.Text = "DCR Management";
        _lblAppTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        _lblAppTitle.ForeColor = Color.White;
        _lblAppTitle.AutoSize = true;
        _lblAppTitle.Location = new Point(24, 16);

        // ── App Subtitle ──────────────────────────────────────────────────────
        _lblAppSubtitle.Text = "Design Change Request System";
        _lblAppSubtitle.Font = ThemeManager.SmallFont;
        _lblAppSubtitle.ForeColor = Color.FromArgb(200, 220, 255);
        _lblAppSubtitle.AutoSize = true;
        _lblAppSubtitle.Location = new Point(25, 44);

        // ── Username Label ────────────────────────────────────────────────────
        _lblUsername.Text = "USERNAME";
        _lblUsername.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        _lblUsername.ForeColor = ThemeManager.TextSecondary;
        _lblUsername.AutoSize = true;
        _lblUsername.Location = new Point(32, 104);

        // ── Username TextBox ──────────────────────────────────────────────────
        _txtUsername.Location = new Point(32, 122);
        _txtUsername.Size = new Size(296, 28);
        _txtUsername.Font = ThemeManager.DefaultFont;
        _txtUsername.BorderStyle = BorderStyle.FixedSingle;
        _txtUsername.MaxLength = 50;
        _txtUsername.TabIndex = 0;

        // ── Password Label ────────────────────────────────────────────────────
        _lblPassword.Text = "PASSWORD";
        _lblPassword.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        _lblPassword.ForeColor = ThemeManager.TextSecondary;
        _lblPassword.AutoSize = true;
        _lblPassword.Location = new Point(32, 168);

        // ── Password TextBox ──────────────────────────────────────────────────
        _txtPassword.Location = new Point(32, 186);
        _txtPassword.Size = new Size(296, 28);
        _txtPassword.Font = ThemeManager.DefaultFont;
        _txtPassword.BorderStyle = BorderStyle.FixedSingle;
        _txtPassword.UseSystemPasswordChar = true;
        _txtPassword.MaxLength = 128;
        _txtPassword.TabIndex = 1;

        // ── Show Password CheckBox ────────────────────────────────────────────
        _chkShowPassword.Text = "Show password";
        _chkShowPassword.Font = ThemeManager.SmallFont;
        _chkShowPassword.ForeColor = ThemeManager.TextSecondary;
        _chkShowPassword.Location = new Point(32, 220);
        _chkShowPassword.AutoSize = true;
        _chkShowPassword.TabIndex = 2;

        // ── Login Button ──────────────────────────────────────────────────────
        _btnLogin.Text = "SIGN IN";
        _btnLogin.Location = new Point(32, 260);
        _btnLogin.Size = new Size(296, 40);
        _btnLogin.TabIndex = 3;

        // ── Loading Label ─────────────────────────────────────────────────────
        _lblLoading.Text = "Signing in...";
        _lblLoading.Font = ThemeManager.SmallFont;
        _lblLoading.ForeColor = ThemeManager.TextSecondary;
        _lblLoading.AutoSize = true;
        _lblLoading.Location = new Point(140, 308);
        _lblLoading.Visible = false;

        // ── Version Label ─────────────────────────────────────────────────────
        _lblVersion.Text = "v1.0.0 — © 2024 Your Company";
        _lblVersion.Font = ThemeManager.SmallFont;
        _lblVersion.ForeColor = Color.FromArgb(180, 200, 240);
        _lblVersion.AutoSize = true;
        _lblVersion.Location = new Point(105, 528);

        // ── Assembly ──────────────────────────────────────────────────────────
        _pnlHeader.Controls.AddRange([_lblAppTitle, _lblAppSubtitle]);
        _pnlCard.Controls.AddRange([
            _pnlHeader,
            _lblUsername, _txtUsername,
            _lblPassword, _txtPassword,
            _chkShowPassword,
            _btnLogin,
            _lblLoading
        ]);
        _pnlBackground.Controls.AddRange([_pnlCard, _lblVersion]);
        Controls.Add(_pnlBackground);

        AcceptButton = _btnLogin;
    }
}