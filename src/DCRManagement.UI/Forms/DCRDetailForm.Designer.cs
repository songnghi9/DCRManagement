using System.Drawing;
using System.Windows.Forms;

namespace DCRManagement.UI.Forms;

partial class DCRDetailForm
{
    private System.ComponentModel.IContainer components = null;

    // ── Topbar ────────────────────────────────────────────────────────────────
    private Panel _pnlTopbar;
    private Label _lblDCRNumber;
    private Label _lblStatusBadge;
    private Label _lblHint;
    private FlowLayoutPanel _flowActions;
    private Button _btnSave;
    private Button _btnEdit;
    private Button _btnSubmit;
    private Button _btnClose;

    // ── Main grid ─────────────────────────────────────────────────────────────
    private TableLayoutPanel _layoutMain;

    // ── Work area ─────────────────────────────────────────────────────────────
    private Panel _pnlWorkArea;
    private Panel _pnlFormStrip;
    private Panel _pnlReasonRow;
    private TableLayoutPanel _layoutBeforeAfter;

    // Form strip fields
    private Label _lblDCRNumber2;
    private Label _lblCreatedBy;
    private Label _lblCreatedAt;
    private ComboBox _cmbPriority;
    private TextBox _txtTitle;
    private TextBox _txtMachine;
    private DateTimePicker _dtpTargetDate;
    private CheckBox _chkNoTargetDate;

    // Reason/Action
    private RichTextBox _rtbReason;
    private RichTextBox _rtbDescription;

    // Image galleries
    private ImageGalleryControl _galleryBefore;
    private ImageGalleryControl _galleryAfter;

    // ── Side panel ────────────────────────────────────────────────────────────
    private Panel _pnlSide;
    private Panel _pnlSideScroll;

    // Risk assessment
    private ComboBox _cmbLeadTimeRisk;
    private RichTextBox _rtbLeadTimeNote;
    private ComboBox _cmbSafetyRisk;
    private RichTextBox _rtbSafetyNote;
    private ComboBox _cmbComplianceRisk;
    private RichTextBox _rtbComplianceNote;
    private TextBox _txtSaving;
    private ComboBox _cmbSavingCurrency;
    private TextBox _txtCostToChange;
    private ComboBox _cmbCostCurrency;

    // DCR Decision section
    private RichTextBox _rtbImpactAnalysis;
    private ComboBox _cmbConsultant;
    private ComboBox _cmbConsultantDecision;
    private DateTimePicker _dtpDecisionDate;
    private RichTextBox _rtbDecisionComment;

    // Bottom decision bar in side panel
    private Panel _pnlSideBottom;
    private ComboBox _cmbDCRDecision;
    private Button _btnSaveSide;
    private Button _btnSubmitSide;

    // ── Approval log (collapsible) ────────────────────────────────────────────
    private Panel _pnlApprovalLog;
    private Panel _pnlLogHeader;
    private Button _btnToggleLog;
    private DataGridView _gridHistory;

    // ── Attachments tab (keep for compatibility) ──────────────────────────────
    private TabControl _tabRight;
    private TabPage _tabHistory;
    private TabPage _tabAttachments;
    private Panel _pnlAttachmentToolbar;
    private Button _btnAddAttachment;
    private DataGridView _gridAttachments;

    // Backward compat backing field
    private RichTextBox _rtbAffectedParts;

    // ─────────────────────────────────────────────────────────────────────────
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // InitializeComponent
    // ─────────────────────────────────────────────────────────────────────────
    private void InitializeComponent()
    {
        _pnlTopbar           = new Panel();
        _lblDCRNumber        = new Label();
        _lblStatusBadge      = new Label();
        _lblHint             = new Label();
        _flowActions         = new FlowLayoutPanel();
        _btnSave             = new Button();
        _btnEdit             = new Button();
        _btnSubmit           = new Button();
        _btnClose            = new Button();
        _layoutMain          = new TableLayoutPanel();
        _pnlWorkArea         = new Panel();
        _pnlFormStrip        = new Panel();
        _pnlReasonRow        = new Panel();
        _layoutBeforeAfter   = new TableLayoutPanel();
        _lblDCRNumber2       = new Label();
        _lblCreatedBy        = new Label();
        _lblCreatedAt        = new Label();
        _cmbPriority         = new ComboBox();
        _txtTitle            = new TextBox();
        _txtMachine          = new TextBox();
        _dtpTargetDate       = new DateTimePicker();
        _chkNoTargetDate     = new CheckBox();
        _rtbReason           = new RichTextBox();
        _rtbDescription      = new RichTextBox();
        _rtbAffectedParts    = new RichTextBox();
        _galleryBefore       = new ImageGalleryControl();
        _galleryAfter        = new ImageGalleryControl();
        _pnlSide             = new Panel();
        _pnlSideScroll       = new Panel();
        _cmbLeadTimeRisk     = new ComboBox();
        _rtbLeadTimeNote     = new RichTextBox();
        _cmbSafetyRisk       = new ComboBox();
        _rtbSafetyNote       = new RichTextBox();
        _cmbComplianceRisk   = new ComboBox();
        _rtbComplianceNote   = new RichTextBox();
        _txtSaving           = new TextBox();
        _cmbSavingCurrency   = new ComboBox();
        _txtCostToChange     = new TextBox();
        _cmbCostCurrency     = new ComboBox();
        _rtbImpactAnalysis   = new RichTextBox();
        _cmbConsultant       = new ComboBox();
        _cmbConsultantDecision = new ComboBox();
        _dtpDecisionDate     = new DateTimePicker();
        _rtbDecisionComment  = new RichTextBox();
        _pnlSideBottom       = new Panel();
        _cmbDCRDecision      = new ComboBox();
        _btnSaveSide         = new Button();
        _btnSubmitSide       = new Button();
        _pnlApprovalLog      = new Panel();
        _pnlLogHeader        = new Panel();
        _btnToggleLog        = new Button();
        _gridHistory         = new DataGridView();
        _tabRight            = new TabControl();
        _tabHistory          = new TabPage();
        _tabAttachments      = new TabPage();
        _pnlAttachmentToolbar = new Panel();
        _btnAddAttachment    = new Button();
        _gridAttachments     = new DataGridView();

        ((System.ComponentModel.ISupportInitialize)_gridHistory).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_gridAttachments).BeginInit();
        SuspendLayout();

        // ── Form ──────────────────────────────────────────────────────────────
        Text        = "DCR Detail";
        Size        = new Size(1600, 950);
        MinimumSize = new Size(1200, 800);
        BackColor   = Color.FromArgb(238, 242, 246);
        Font        = new Font("Segoe UI", 9.5F);

        // ── Topbar ────────────────────────────────────────────────────────────
        _pnlTopbar.Dock      = DockStyle.Top;
        _pnlTopbar.Height    = 68;
        _pnlTopbar.BackColor = Color.FromArgb(248, 250, 252);
        _pnlTopbar.Padding   = new Padding(16, 0, 16, 0);
        _pnlTopbar.Paint    += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, _pnlTopbar.Height - 1, _pnlTopbar.Width, _pnlTopbar.Height - 1);
        };

        _lblDCRNumber.AutoSize  = true;
        _lblDCRNumber.Text      = "DCR-NEW";
        _lblDCRNumber.Font      = new Font("Segoe UI", 22F, FontStyle.Bold);
        _lblDCRNumber.ForeColor = Color.FromArgb(24, 77, 132);
        _lblDCRNumber.Location  = new Point(16, 10);

        _lblStatusBadge.AutoSize  = false;
        _lblStatusBadge.Size      = new Size(100, 26);
        _lblStatusBadge.Text      = "Draft";
        _lblStatusBadge.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblStatusBadge.ForeColor = Color.White;
        _lblStatusBadge.BackColor = Color.FromArgb(108, 117, 125);
        _lblStatusBadge.TextAlign = ContentAlignment.MiddleCenter;
        _lblStatusBadge.Location  = new Point(210, 21);

        _lblHint.AutoSize  = true;
        _lblHint.Text      = "";
        _lblHint.Font      = new Font("Segoe UI", 9F);
        _lblHint.ForeColor = Color.FromArgb(91, 103, 120);
        _lblHint.Location  = new Point(326, 26);

        _flowActions.AutoSize      = true;
        _flowActions.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
        _flowActions.FlowDirection = FlowDirection.LeftToRight;
        _flowActions.WrapContents  = false;
        _flowActions.Anchor        = AnchorStyles.Right | AnchorStyles.Top;
        _flowActions.Location      = new Point(1200, 17);

        _btnSave.Text   = "Save Draft";
        _btnEdit.Text   = "Edit";
        _btnSubmit.Text = "Submit";
        _btnClose.Text  = "Close";
        foreach (var btn in new[] { _btnSave, _btnEdit, _btnSubmit, _btnClose })
        {
            btn.Height    = 34;
            btn.AutoSize  = true;
            btn.Padding   = new Padding(12, 0, 12, 0);
            btn.Margin    = new Padding(0, 0, 8, 0);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Cursor    = Cursors.Hand;
        }
        _flowActions.Controls.AddRange(new Control[] { _btnSave, _btnEdit, _btnSubmit, _btnClose });

        _pnlTopbar.Controls.AddRange(new Control[] { _lblDCRNumber, _lblStatusBadge, _lblHint, _flowActions });
        _pnlTopbar.Resize += (s, e) =>
        {
            _flowActions.Location = new Point(
                _pnlTopbar.Width - _flowActions.Width - 16,
                (_pnlTopbar.Height - _flowActions.Height) / 2);
        };

        // ── Approval log (Dock=Bottom, collapsible) ───────────────────────────
        _pnlApprovalLog.Dock      = DockStyle.Bottom;
        _pnlApprovalLog.Height    = 44;
        _pnlApprovalLog.BackColor = Color.White;
        _pnlApprovalLog.Paint    += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, 0, _pnlApprovalLog.Width, 0);
        };

        _pnlLogHeader.Dock      = DockStyle.Top;
        _pnlLogHeader.Height    = 44;
        _pnlLogHeader.BackColor = Color.White;

        var lblLogTitle = new Label
        {
            Text      = "Approval / Comments",
            Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(23, 32, 42),
            AutoSize  = true,
            Location  = new Point(16, 12)
        };

        _btnToggleLog.Text      = "▼ Expand";
        _btnToggleLog.AutoSize  = true;
        _btnToggleLog.FlatStyle = FlatStyle.Flat;
        _btnToggleLog.FlatAppearance.BorderSize = 0;
        _btnToggleLog.BackColor = Color.Transparent;
        _btnToggleLog.ForeColor = Color.FromArgb(36, 99, 166);
        _btnToggleLog.Font      = new Font("Segoe UI", 9F);
        _btnToggleLog.Cursor    = Cursors.Hand;
        _btnToggleLog.Anchor    = AnchorStyles.Right | AnchorStyles.Top;
        _btnToggleLog.Location  = new Point(900, 12);
        _pnlLogHeader.Resize   += (s, e) =>
            _btnToggleLog.Location = new Point(_pnlLogHeader.Width - _btnToggleLog.Width - 16, 12);

        _pnlLogHeader.Controls.AddRange(new Control[] { lblLogTitle, _btnToggleLog });

        BuildHistoryGrid();
        _gridHistory.Dock    = DockStyle.Fill;
        _gridHistory.Visible = false;

        _pnlApprovalLog.Controls.Add(_gridHistory);
        _pnlApprovalLog.Controls.Add(_pnlLogHeader);

        // ── Main 2-column layout ──────────────────────────────────────────────
        _layoutMain.Dock        = DockStyle.Fill;
        _layoutMain.ColumnCount = 2;
        _layoutMain.RowCount    = 1;
        _layoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
        _layoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layoutMain.BackColor   = Color.FromArgb(215, 221, 230);
        _layoutMain.Padding     = new Padding(0);
        _layoutMain.Margin      = new Padding(0);

        BuildWorkArea();
        _layoutMain.Controls.Add(_pnlWorkArea, 0, 0);

        BuildSidePanel();
        _layoutMain.Controls.Add(_pnlSide, 1, 0);

        // ── Hidden backing controls ───────────────────────────────────────────
        _rtbAffectedParts.Visible = false;
        _rtbAffectedParts.Size    = new Size(1, 1);

        // ── Tab controls (kept for SetAttachments compat) ─────────────────────
        _tabRight.Visible    = false;
        _tabRight.Size       = new Size(1, 1);
        _tabHistory.Text     = "Approval / Comments";
        _tabAttachments.Text = "Attachments";
        _pnlAttachmentToolbar.Dock      = DockStyle.Top;
        _pnlAttachmentToolbar.Height    = 42;
        _pnlAttachmentToolbar.BackColor = Color.White;
        _btnAddAttachment.Text     = "Add File";
        _btnAddAttachment.Size     = new Size(100, 28);
        _btnAddAttachment.Location = new Point(8, 7);
        _pnlAttachmentToolbar.Controls.Add(_btnAddAttachment);
        BuildAttachmentGrid();
        _tabAttachments.Controls.Add(_gridAttachments);
        _tabAttachments.Controls.Add(_pnlAttachmentToolbar);
        _tabRight.TabPages.AddRange(new TabPage[] { _tabHistory, _tabAttachments });

        // ── Assemble form ─────────────────────────────────────────────────────
        Controls.Add(_layoutMain);
        Controls.Add(_pnlApprovalLog);
        Controls.Add(_pnlTopbar);
        Controls.Add(_rtbAffectedParts);
        Controls.Add(_tabRight);

        ((System.ComponentModel.ISupportInitialize)_gridHistory).EndInit();
        ((System.ComponentModel.ISupportInitialize)_gridAttachments).EndInit();
        ResumeLayout(false);
    }
    // MARKER_END_INIT

    // ─────────────────────────────────────────────────────────────────────────
    // BuildWorkArea — left column: FormStrip + ReasonRow + BeforeAfter
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildWorkArea()
    {
        _pnlWorkArea.Dock      = DockStyle.Fill;
        _pnlWorkArea.BackColor = Color.White;
        _pnlWorkArea.Margin    = new Padding(0);

        // ── Form strip (6 equal columns, 52px tall) ───────────────────────────
        _pnlFormStrip.Dock      = DockStyle.Top;
        _pnlFormStrip.Height    = 52;
        _pnlFormStrip.BackColor = Color.FromArgb(252, 253, 255);
        _pnlFormStrip.Padding   = new Padding(10, 6, 10, 6);
        _pnlFormStrip.Paint    += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, _pnlFormStrip.Height - 1, _pnlFormStrip.Width, _pnlFormStrip.Height - 1);
        };

        var stripLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 6,
            RowCount    = 2,
            BackColor   = Color.Transparent,
            Padding     = new Padding(0),
            Margin      = new Padding(0)
        };
        for (int i = 0; i < 6; i++)
            stripLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 6F));
        stripLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
        stripLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Row 0: captions
        stripLayout.Controls.Add(ConfigureFieldLabel(new Label(), "Change log no."), 0, 0);
        stripLayout.Controls.Add(ConfigureFieldLabel(new Label(), "Category"),       1, 0);
        stripLayout.Controls.Add(ConfigureFieldLabel(new Label(), "Drawing no."),    2, 0);
        stripLayout.Controls.Add(ConfigureFieldLabel(new Label(), "Machine Model"),  3, 0);
        stripLayout.Controls.Add(ConfigureFieldLabel(new Label(), "Raised by"),      4, 0);
        stripLayout.Controls.Add(ConfigureFieldLabel(new Label(), "Raised date"),    5, 0);

        // Row 1: values
        ConfigureReadOnlyValue(_lblDCRNumber2);
        _lblDCRNumber2.Text = "(new)";
        stripLayout.Controls.Add(_lblDCRNumber2, 0, 1);

        _cmbPriority.Dock         = DockStyle.Fill;
        _cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbPriority.Items.AddRange(new object[] { "", "Low", "Medium", "High", "Critical" });
        _cmbPriority.SelectedIndex = 0;
        _cmbPriority.Margin       = new Padding(2);
        ConfigureInput(_cmbPriority);
        stripLayout.Controls.Add(_cmbPriority, 1, 1);

        _txtTitle.Dock       = DockStyle.Fill;
        _txtTitle.MaxLength  = 300;
        _txtTitle.Margin     = new Padding(2);
        ConfigureInput(_txtTitle);
        stripLayout.Controls.Add(_txtTitle, 2, 1);

        _txtMachine.Dock    = DockStyle.Fill;
        _txtMachine.Margin  = new Padding(2);
        ConfigureInput(_txtMachine);
        stripLayout.Controls.Add(_txtMachine, 3, 1);

        ConfigureReadOnlyValue(_lblCreatedBy);
        stripLayout.Controls.Add(_lblCreatedBy, 4, 1);

        ConfigureReadOnlyValue(_lblCreatedAt);
        stripLayout.Controls.Add(_lblCreatedAt, 5, 1);

        _pnlFormStrip.Controls.Add(stripLayout);

        // ── Reason row (2 cols, 130px tall) ──────────────────────────────────
        _pnlReasonRow.Dock      = DockStyle.Top;
        _pnlReasonRow.Height    = 130;
        _pnlReasonRow.BackColor = Color.White;
        _pnlReasonRow.Padding   = new Padding(10, 6, 10, 6);
        _pnlReasonRow.Paint    += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, _pnlReasonRow.Height - 1, _pnlReasonRow.Width, _pnlReasonRow.Height - 1);
        };

        var reasonLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 2,
            BackColor   = Color.Transparent,
            Padding     = new Padding(0),
            Margin      = new Padding(0)
        };
        reasonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        reasonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        reasonLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        reasonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        reasonLayout.Controls.Add(BuildSectionTitle("Reason for change request"), 0, 0);
        reasonLayout.Controls.Add(BuildSectionTitle("Action"), 1, 0);

        ConfigureRiskMemo(_rtbReason);
        _rtbReason.Margin = new Padding(2);
        reasonLayout.Controls.Add(_rtbReason, 0, 1);

        ConfigureRiskMemo(_rtbDescription);
        _rtbDescription.Margin = new Padding(2);
        reasonLayout.Controls.Add(_rtbDescription, 1, 1);

        _pnlReasonRow.Controls.Add(reasonLayout);

        // ── Before/After image stage (fills remaining space) ──────────────────
        _layoutBeforeAfter.Dock        = DockStyle.Fill;
        _layoutBeforeAfter.ColumnCount = 2;
        _layoutBeforeAfter.RowCount    = 1;
        _layoutBeforeAfter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _layoutBeforeAfter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _layoutBeforeAfter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layoutBeforeAfter.BackColor   = Color.FromArgb(215, 221, 230);
        _layoutBeforeAfter.Padding     = new Padding(0);
        _layoutBeforeAfter.Margin      = new Padding(0);

        var pnlBefore = BuildGalleryPanel("Before", _galleryBefore);
        var pnlAfter  = BuildGalleryPanel("After",  _galleryAfter);
        _layoutBeforeAfter.Controls.Add(pnlBefore, 0, 0);
        _layoutBeforeAfter.Controls.Add(pnlAfter,  1, 0);

        _pnlWorkArea.Controls.Add(_layoutBeforeAfter);
        _pnlWorkArea.Controls.Add(_pnlReasonRow);
        _pnlWorkArea.Controls.Add(_pnlFormStrip);
    }

    private Panel BuildGalleryPanel(string title, ImageGalleryControl gallery)
    {
        var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0) };

        var head = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 42,
            BackColor = Color.White,
            Padding   = new Padding(10, 0, 10, 0)
        };
        head.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, head.Height - 1, head.Width, head.Height - 1);
        };

        var lbl = new Label
        {
            Text      = title,
            Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(23, 32, 42),
            AutoSize  = true,
            Location  = new Point(10, 10)
        };
        head.Controls.Add(lbl);

        gallery.Dock        = DockStyle.Fill;
        gallery.BackColor   = Color.FromArgb(244, 247, 251);
        gallery.BorderStyle = BorderStyle.None;

        pnl.Controls.Add(gallery);
        pnl.Controls.Add(head);
        return pnl;
    }
    // MARKER_END_WORKAREA

    // ─────────────────────────────────────────────────────────────────────────
    // BuildSidePanel — right column: Risk Assessment + DCR Decision + bottom bar
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildSidePanel()
    {
        _pnlSide.Dock      = DockStyle.Fill;
        _pnlSide.BackColor = Color.FromArgb(251, 252, 254);
        _pnlSide.Margin    = new Padding(0);
        _pnlSide.Paint    += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, 0, 0, _pnlSide.Height);
        };

        // ── Bottom bar (fixed, Dock=Bottom) ───────────────────────────────────
        _pnlSideBottom.Dock      = DockStyle.Bottom;
        _pnlSideBottom.Height    = 110;
        _pnlSideBottom.BackColor = Color.White;
        _pnlSideBottom.Padding   = new Padding(12, 8, 12, 8);
        _pnlSideBottom.Paint    += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, 0, _pnlSideBottom.Width, 0);
        };

        var lblDecLabel = ConfigureFieldLabel(new Label(), "DCR Decision");
        lblDecLabel.Location = new Point(12, 10);
        lblDecLabel.AutoSize = true;

        _cmbDCRDecision.Dock         = DockStyle.None;
        _cmbDCRDecision.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbDCRDecision.Items.AddRange(new object[] { "", "Yes — Raise DCR", "No — Reject" });
        _cmbDCRDecision.SelectedIndex = 0;
        _cmbDCRDecision.Size          = new Size(270, 28);
        _cmbDCRDecision.Location      = new Point(12, 28);
        ConfigureInput(_cmbDCRDecision);

        _btnSaveSide.Text     = "Save";
        _btnSaveSide.Dock     = DockStyle.None;
        _btnSaveSide.Size     = new Size(130, 32);
        _btnSaveSide.Location = new Point(12, 64);
        _btnSaveSide.FlatStyle = FlatStyle.Flat;
        _btnSaveSide.Cursor   = Cursors.Hand;

        _btnSubmitSide.Text     = "Submit to DCR";
        _btnSubmitSide.Dock     = DockStyle.None;
        _btnSubmitSide.Size     = new Size(150, 32);
        _btnSubmitSide.Location = new Point(152, 64);
        _btnSubmitSide.FlatStyle = FlatStyle.Flat;
        _btnSubmitSide.Cursor   = Cursors.Hand;

        _pnlSideBottom.Controls.AddRange(new Control[] { lblDecLabel, _cmbDCRDecision, _btnSaveSide, _btnSubmitSide });

        // ── Scrollable content ────────────────────────────────────────────────
        _pnlSideScroll.Dock        = DockStyle.Fill;
        _pnlSideScroll.AutoScroll  = true;
        _pnlSideScroll.BackColor   = Color.FromArgb(251, 252, 254);
        _pnlSideScroll.Padding     = new Padding(0);

        // Section: Risk Assessment
        var riskSection = BuildRiskSection();

        // Section: DCR Decision
        var decisionSection = BuildDecisionSection();

        // Stack sections (add in reverse order for Dock=Top stacking)
        _pnlSideScroll.Controls.Add(decisionSection);
        _pnlSideScroll.Controls.Add(riskSection);

        _pnlSide.Controls.Add(_pnlSideScroll);
        _pnlSide.Controls.Add(_pnlSideBottom);
    }

    private Panel BuildRiskSection()
    {
        var pnl = new Panel
        {
            Dock      = DockStyle.Top,
            AutoSize  = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Padding   = new Padding(12, 10, 12, 10)
        };
        pnl.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
        };

        int y = 10;
        const int rowH = 28, memoH = 72, gap = 6;

        // Title
        var title = BuildSectionTitle("Risk Assessment");
        title.Location = new Point(0, y);
        title.AutoSize = true;
        pnl.Controls.Add(title);
        y += 28;

        // 1. Leadtime
        pnl.Controls.Add(BuildRiskRow("1. Leadtime impact", _cmbLeadTimeRisk, ref y));
        ConfigureRiskMemo(_rtbLeadTimeNote);
        _rtbLeadTimeNote.Location = new Point(0, y);
        _rtbLeadTimeNote.Size     = new Size(294, memoH);
        pnl.Controls.Add(_rtbLeadTimeNote);
        y += memoH + gap;

        // 2. Safety
        pnl.Controls.Add(BuildRiskRow("2. Safety impact", _cmbSafetyRisk, ref y));
        ConfigureRiskMemo(_rtbSafetyNote);
        _rtbSafetyNote.Location = new Point(0, y);
        _rtbSafetyNote.Size     = new Size(294, memoH);
        pnl.Controls.Add(_rtbSafetyNote);
        y += memoH + gap;

        // 3. Compliance
        pnl.Controls.Add(BuildRiskRow("3. Compliance break", _cmbComplianceRisk, ref y));
        ConfigureRiskMemo(_rtbComplianceNote);
        _rtbComplianceNote.Location = new Point(0, y);
        _rtbComplianceNote.Size     = new Size(294, memoH);
        pnl.Controls.Add(_rtbComplianceNote);
        y += memoH + gap;

        // Metrics: Saving
        var lblSaving = ConfigureFieldLabel(new Label(), "Saving");
        lblSaving.Location = new Point(0, y);
        lblSaving.AutoSize = true;
        pnl.Controls.Add(lblSaving);
        y += 18;
        _txtSaving.Location = new Point(0, y);
        _txtSaving.Size     = new Size(180, 28);
        ConfigureInput(_txtSaving);
        _cmbSavingCurrency.Location     = new Point(186, y);
        _cmbSavingCurrency.Size         = new Size(108, 28);
        _cmbSavingCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbSavingCurrency.Items.AddRange(new object[] { "Mvnd", "USD", "VND", "AUD" });
        _cmbSavingCurrency.SelectedIndex = 0;
        ConfigureInput(_cmbSavingCurrency);
        pnl.Controls.AddRange(new Control[] { _txtSaving, _cmbSavingCurrency });
        y += rowH + gap;

        // Metrics: Cost
        var lblCost = ConfigureFieldLabel(new Label(), "Cost to change");
        lblCost.Location = new Point(0, y);
        lblCost.AutoSize = true;
        pnl.Controls.Add(lblCost);
        y += 18;
        _txtCostToChange.Location = new Point(0, y);
        _txtCostToChange.Size     = new Size(180, 28);
        ConfigureInput(_txtCostToChange);
        _cmbCostCurrency.Location     = new Point(186, y);
        _cmbCostCurrency.Size         = new Size(108, 28);
        _cmbCostCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCostCurrency.Items.AddRange(new object[] { "Mvnd", "USD", "VND", "AUD" });
        _cmbCostCurrency.SelectedIndex = 0;
        ConfigureInput(_cmbCostCurrency);
        pnl.Controls.AddRange(new Control[] { _txtCostToChange, _cmbCostCurrency });
        y += rowH + 10;

        pnl.Height = y;
        return pnl;
    }

    private Panel BuildRiskRow(string labelText, ComboBox combo, ref int y)
    {
        var row = new Panel
        {
            Location  = new Point(0, y),
            Size      = new Size(294, 28),
            BackColor = Color.Transparent
        };
        var lbl = ConfigureFieldLabel(new Label(), labelText);
        lbl.Location = new Point(0, 5);
        lbl.AutoSize = true;

        combo.Size         = new Size(64, 26);
        combo.Location     = new Point(226, 1);
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        if (combo.Items.Count == 0)
            combo.Items.AddRange(new object[] { "", "Yes", "No" });
        combo.SelectedIndex = 0;
        ConfigureInput(combo);

        row.Controls.AddRange(new Control[] { lbl, combo });
        y += 30;
        return row;
    }

    private Panel BuildDecisionSection()
    {
        var pnl = new Panel
        {
            Dock      = DockStyle.Top,
            AutoSize  = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Padding   = new Padding(12, 10, 12, 10)
        };
        pnl.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 221, 230));
            e.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
        };

        int y = 10;
        const int memoH = 72, rowH = 28, gap = 6;

        var title = BuildSectionTitle("DCR Decision");
        title.Location = new Point(0, y);
        title.AutoSize = true;
        pnl.Controls.Add(title);
        y += 28;

        // Suggestion
        var lblSuggestion = ConfigureFieldLabel(new Label(), "Suggestion to raise DCR");
        lblSuggestion.Location = new Point(0, y);
        lblSuggestion.AutoSize = true;
        pnl.Controls.Add(lblSuggestion);
        y += 18;
        ConfigureRiskMemo(_rtbImpactAnalysis);
        _rtbImpactAnalysis.Location = new Point(0, y);
        _rtbImpactAnalysis.Size     = new Size(294, memoH);
        pnl.Controls.Add(_rtbImpactAnalysis);
        y += memoH + gap;

        // Consultant
        var lblConsultant = ConfigureFieldLabel(new Label(), "Select Consultant");
        lblConsultant.Location = new Point(0, y);
        lblConsultant.AutoSize = true;
        pnl.Controls.Add(lblConsultant);
        y += 18;
        _cmbConsultant.Location     = new Point(0, y);
        _cmbConsultant.Size         = new Size(294, 28);
        _cmbConsultant.DropDownStyle = ComboBoxStyle.DropDownList;
        ConfigureInput(_cmbConsultant);
        pnl.Controls.Add(_cmbConsultant);
        y += rowH + gap;

        // Consultant decision + Date (2-col row)
        var lblConsDecision = ConfigureFieldLabel(new Label(), "Consultant decision");
        lblConsDecision.Location = new Point(0, y);
        lblConsDecision.AutoSize = true;
        var lblDecDate = ConfigureFieldLabel(new Label(), "Date");
        lblDecDate.Location = new Point(152, y);
        lblDecDate.AutoSize = true;
        pnl.Controls.AddRange(new Control[] { lblConsDecision, lblDecDate });
        y += 18;
        _cmbConsultantDecision.Location     = new Point(0, y);
        _cmbConsultantDecision.Size         = new Size(144, 28);
        _cmbConsultantDecision.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbConsultantDecision.Items.AddRange(new object[] { "", "Approve", "Reject", "Pending" });
        _cmbConsultantDecision.SelectedIndex = 0;
        ConfigureInput(_cmbConsultantDecision);
        _dtpDecisionDate.Location = new Point(152, y);
        _dtpDecisionDate.Size     = new Size(142, 28);
        _dtpDecisionDate.Format   = DateTimePickerFormat.Short;
        pnl.Controls.AddRange(new Control[] { _cmbConsultantDecision, _dtpDecisionDate });
        y += rowH + gap;

        // Comment
        var lblComment = ConfigureFieldLabel(new Label(), "Comment");
        lblComment.Location = new Point(0, y);
        lblComment.AutoSize = true;
        pnl.Controls.Add(lblComment);
        y += 18;
        ConfigureRiskMemo(_rtbDecisionComment);
        _rtbDecisionComment.Location = new Point(0, y);
        _rtbDecisionComment.Size     = new Size(294, memoH);
        pnl.Controls.Add(_rtbDecisionComment);
        y += memoH + 10;

        pnl.Height = y;
        return pnl;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Grid builders
    // Grid builders
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildHistoryGrid()
    {
        _gridHistory.Dock = DockStyle.Fill;
        ThemeManager.StyleDataGrid(_gridHistory);
        _gridHistory.AutoGenerateColumns = false;
        _gridHistory.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                HeaderText       = "Date",
                DataPropertyName = "ActionDate",
                Width            = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yy HH:mm" }
            },
            new DataGridViewTextBoxColumn { HeaderText = "Role/User", DataPropertyName = "ActorName",     Width = 170 },
            new DataGridViewTextBoxColumn { HeaderText = "Action",    DataPropertyName = "ActionDisplay", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = "Status",    DataPropertyName = "ToStatus",      Width = 140 },
            new DataGridViewTextBoxColumn
            {
                HeaderText       = "Comment",
                DataPropertyName = "Comment",
                AutoSizeMode     = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True }
            }
        );
        _gridHistory.RowTemplate.Height = 36;
    }

    private void BuildAttachmentGrid()
    {
        _gridAttachments.Dock = DockStyle.Fill;
        ThemeManager.StyleDataGrid(_gridAttachments);
        _gridAttachments.AutoGenerateColumns = false;
        _gridAttachments.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "File Name",    DataPropertyName = "FileName",        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
            new DataGridViewTextBoxColumn { HeaderText = "Size",         DataPropertyName = "FileSizeDisplay", Width = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Uploaded By",  DataPropertyName = "UploadedBy",      Width = 140 },
            new DataGridViewTextBoxColumn
            {
                HeaderText       = "Date",
                DataPropertyName = "UploadedAt",
                Width            = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            },
            new DataGridViewButtonColumn
            {
                HeaderText               = "",
                Text                     = "Remove",
                UseColumnTextForButtonValue = true,
                Width                    = 90
            }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper factory methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>12px bold muted label for field captions.</summary>
    private static Label ConfigureFieldLabel(Label lbl, string text)
    {
        lbl.Text      = text;
        lbl.Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lbl.ForeColor = Color.FromArgb(91, 103, 120);
        lbl.AutoSize  = true;
        lbl.Margin    = new Padding(0);
        return lbl;
    }

    /// <summary>Read-only value label with border and white background.</summary>
    private static void ConfigureReadOnlyValue(Label lbl)
    {
        lbl.Dock        = DockStyle.Fill;
        lbl.BorderStyle = BorderStyle.FixedSingle;
        lbl.BackColor   = Color.White;
        lbl.ForeColor   = Color.FromArgb(23, 32, 42);
        lbl.Padding     = new Padding(4, 0, 4, 0);
        lbl.TextAlign   = ContentAlignment.MiddleLeft;
        lbl.Margin      = new Padding(2);
    }

    /// <summary>Standard input styling: light field background + border.</summary>
    private static void ConfigureInput(Control ctrl)
    {
        ctrl.BackColor = Color.FromArgb(247, 249, 252);
        if (ctrl is TextBox tb)   tb.BorderStyle   = BorderStyle.FixedSingle;
        if (ctrl is ComboBox cmb) cmb.FlatStyle    = FlatStyle.Flat;
    }

    /// <summary>72px editable RichTextBox for risk memos.</summary>
    private static void ConfigureRiskMemo(RichTextBox rtb)
    {
        rtb.Height      = 72;
        rtb.BorderStyle = BorderStyle.FixedSingle;
        rtb.BackColor   = Color.FromArgb(247, 249, 252);
        rtb.Font        = new Font("Segoe UI", 9F);
        rtb.ScrollBars  = RichTextBoxScrollBars.Vertical;
        rtb.Margin      = new Padding(0, 2, 0, 2);
    }

    /// <summary>Returns a 14px bold section title label.</summary>
    private static Label BuildSectionTitle(string text)
    {
        return new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(23, 32, 42),
            AutoSize  = true,
            Margin    = new Padding(0, 0, 0, 4)
        };
    }
}

