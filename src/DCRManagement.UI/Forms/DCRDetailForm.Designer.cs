using static System.Net.Mime.MediaTypeNames;

namespace DCRManagement.UI.Forms;

partial class DCRDetailForm
{
    private System.ComponentModel.IContainer components = null;

    // ── Header bar ────────────────────────────────────────────────────────────
    private Panel _pnlHeader;
    private Label _lblDCRNumber;
    private Label _lblStatusBadge;
    private Label _lblCreatedBy;
    private Label _lblCreatedAt;

    // ── Workflow action buttons ───────────────────────────────────────────────
    private Panel _pnlActions;
    private FlowLayoutPanel _flowActions;
    private Button _btnSave;
    private Button _btnEdit;
    private Button _btnSubmit;
    private Button _btnClose;

    // ── Main split layout ─────────────────────────────────────────────────────
    private SplitContainer _splitMain;

    // Left pane — fields
    private Panel _pnlFields;
    private Label _lblTitle;
    private TextBox _txtTitle;
    private Label _lblDescription;
    private RichTextBox _rtbDescription;
    private Label _lblAffectedParts;
    private RichTextBox _rtbAffectedParts;
    private Label _lblReason;
    private RichTextBox _rtbReason;
    private Label _lblImpactAnalysis;
    private RichTextBox _rtbImpactAnalysis;
    private Label _lblPriority;
    private ComboBox _cmbPriority;
    private Label _lblTargetDate;
    private DateTimePicker _dtpTargetDate;
    private CheckBox _chkNoTargetDate;

    // Right pane — tabs (history + attachments)
    private TabControl _tabRight;
    private TabPage _tabHistory;
    private DataGridView _gridHistory;
    private TabPage _tabAttachments;
    private Panel _pnlAttachmentToolbar;
    private Button _btnAddAttachment;
    private DataGridView _gridAttachments;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _pnlHeader = new Panel();
        _lblDCRNumber = new Label();
        _lblStatusBadge = new Label();
        _lblCreatedBy = new Label();
        _lblCreatedAt = new Label();
        _pnlActions = new Panel();
        _flowActions = new FlowLayoutPanel();
        _btnSave = new Button();
        _btnEdit = new Button();
        _btnSubmit = new Button();
        _btnClose = new Button();
        _splitMain = new SplitContainer();
        _pnlFields = new Panel();
        _lblTitle = new Label();
        _txtTitle = new TextBox();
        _lblDescription = new Label();
        _rtbDescription = new RichTextBox();
        _lblAffectedParts = new Label();
        _rtbAffectedParts = new RichTextBox();
        _lblReason = new Label();
        _rtbReason = new RichTextBox();
        _lblImpactAnalysis = new Label();
        _rtbImpactAnalysis = new RichTextBox();
        _lblPriority = new Label();
        _cmbPriority = new ComboBox();
        _lblTargetDate = new Label();
        _dtpTargetDate = new DateTimePicker();
        _chkNoTargetDate = new CheckBox();
        _tabRight = new TabControl();
        _tabHistory = new TabPage();
        _gridHistory = new DataGridView();
        _tabAttachments = new TabPage();
        _pnlAttachmentToolbar = new Panel();
        _btnAddAttachment = new Button();
        _gridAttachments = new DataGridView();

        SuspendLayout();

        // ── Form ──────────────────────────────────────────────────────────────
        Text = "DCR Detail";
        Size = new Size(1100, 780);
        MinimumSize = new Size(900, 600);
        BackColor = ThemeManager.BackgroundColor;

        // ── Header bar ────────────────────────────────────────────────────────
        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Height = 64;
        _pnlHeader.BackColor = ThemeManager.PrimaryColor;
        _pnlHeader.Padding = new Padding(16, 0, 16, 0);

        _lblDCRNumber.Text = "DCR-0000-0000";
        _lblDCRNumber.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
        _lblDCRNumber.ForeColor = Color.White;
        _lblDCRNumber.AutoSize = true;
        _lblDCRNumber.Location = new Point(16, 12);

        _lblStatusBadge.Text = "Draft";
        _lblStatusBadge.Font = ThemeManager.BoldFont;
        _lblStatusBadge.ForeColor = Color.White;
        _lblStatusBadge.AutoSize = false;
        _lblStatusBadge.Size = new Size(130, 24);
        _lblStatusBadge.Location = new Point(220, 18);
        _lblStatusBadge.TextAlign = ContentAlignment.MiddleCenter;

        _lblCreatedBy.AutoSize = true;
        _lblCreatedBy.Font = ThemeManager.SmallFont;
        _lblCreatedBy.ForeColor = Color.FromArgb(200, 220, 255);
        _lblCreatedBy.Location = new Point(16, 44);

        _lblCreatedAt.AutoSize = true;
        _lblCreatedAt.Font = ThemeManager.SmallFont;
        _lblCreatedAt.ForeColor = Color.FromArgb(200, 220, 255);
        _lblCreatedAt.Location = new Point(200, 44);

        _pnlHeader.Controls.AddRange([
            _lblDCRNumber, _lblStatusBadge, _lblCreatedBy, _lblCreatedAt
        ]);

        // ── Actions bar ───────────────────────────────────────────────────────
        _pnlActions.Dock = DockStyle.Top;
        _pnlActions.Height = 52;
        _pnlActions.BackColor = Color.White;
        _pnlActions.Padding = new Padding(12, 8, 12, 8);

        _flowActions.Dock = DockStyle.Fill;
        _flowActions.FlowDirection = FlowDirection.LeftToRight;
        _flowActions.WrapContents = false;
        _flowActions.AutoSize = true;
        _flowActions.Padding = new Padding(0);

        _btnSave.Text = "💾  Save";
        _btnEdit.Text = "✏️  Edit";
        _btnSubmit.Text = "📤  Submit for Review";
        _btnClose.Text = "✕  Close";

        foreach (var btn in new[] { _btnSave, _btnEdit, _btnSubmit, _btnClose })
        {
            btn.Size = new Size(150, 34);
            btn.Margin = new Padding(0, 0, 8, 0);
        }

        _flowActions.Controls.AddRange([_btnSave, _btnEdit, _btnSubmit, _btnClose]);
        _pnlActions.Controls.Add(_flowActions);

        // ── SplitContainer (left: fields, right: tabs) ────────────────────────
        _splitMain.Dock = DockStyle.Fill;
        _splitMain.Orientation = Orientation.Vertical;
        _splitMain.SplitterDistance = 580;
        _splitMain.Panel1MinSize = 420;
        _splitMain.Panel2MinSize = 300;
        _splitMain.BackColor = ThemeManager.BackgroundColor;

        // ── Left pane: scrollable fields ──────────────────────────────────────
        _pnlFields.Dock = DockStyle.Fill;
        _pnlFields.AutoScroll = true;
        _pnlFields.Padding = new Padding(16, 12, 16, 12);
        _pnlFields.BackColor = Color.White;

        int y = 12;
        LayoutField(_lblTitle, "TITLE *", y); y += 20;
        _txtTitle.Location = new Point(0, y); _txtTitle.Dock = DockStyle.None;
        _txtTitle.Size = new Size(530, 28);
        _txtTitle.MaxLength = 300;
        _txtTitle.Font = ThemeManager.DefaultFont;
        _txtTitle.BorderStyle = BorderStyle.FixedSingle;
        y += 36;

        LayoutField(_lblDescription, "DESCRIPTION *", y); y += 20;
        LayoutRtb(_rtbDescription, ref y, 100);

        LayoutField(_lblAffectedParts, "AFFECTED PARTS", y); y += 20;
        LayoutRtb(_rtbAffectedParts, ref y, 80);

        LayoutField(_lblReason, "REASON / JUSTIFICATION", y); y += 20;
        LayoutRtb(_rtbReason, ref y, 80);

        LayoutField(_lblImpactAnalysis, "IMPACT ANALYSIS", y); y += 20;
        LayoutRtb(_rtbImpactAnalysis, ref y, 80);

        LayoutField(_lblPriority, "PRIORITY", y); y += 20;
        _cmbPriority.Location = new Point(0, y);
        _cmbPriority.Size = new Size(160, 28);
        _cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbPriority.Font = ThemeManager.DefaultFont;
        _cmbPriority.Items.AddRange(["", "Low", "Medium", "High", "Critical"]);
        _cmbPriority.SelectedIndex = 0;
        y += 36;

        LayoutField(_lblTargetDate, "TARGET COMPLETION DATE", y); y += 20;
        _dtpTargetDate.Location = new Point(0, y);
        _dtpTargetDate.Size = new Size(200, 28);
        _dtpTargetDate.Font = ThemeManager.DefaultFont;
        _dtpTargetDate.Format = DateTimePickerFormat.Short;
        _dtpTargetDate.MinDate = DateTime.Today;

        _chkNoTargetDate.Text = "No target date";
        _chkNoTargetDate.Location = new Point(210, y + 3);
        _chkNoTargetDate.AutoSize = true;
        _chkNoTargetDate.Font = ThemeManager.DefaultFont;

        _pnlFields.Controls.AddRange([
            _lblTitle, _txtTitle,
            _lblDescription, _rtbDescri