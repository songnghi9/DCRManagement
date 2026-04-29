namespace DCRManagement.UI.Forms;

partial class DCRDetailForm
{
    private System.ComponentModel.IContainer components = null;

    // Header/action controls used by code-behind
    private Panel _pnlHeader;
    private Label _lblHeaderTitle;
    private Label _lblDCRNumber;
    private Label _lblStatusBadge;
    private Label _lblCreatedBy;
    private Label _lblCreatedAt;
    private Panel _pnlActions;
    private FlowLayoutPanel _flowActions;
    private Button _btnSave;
    private Button _btnEdit;
    private Button _btnSubmit;
    private Button _btnClose;

    // Bound input controls used by presenter
    private SplitContainer _splitMain;
    private TextBox _txtTitle;
    private RichTextBox _rtbDescription;
    private RichTextBox _rtbAffectedParts;
    private RichTextBox _rtbReason;
    private RichTextBox _rtbImpactAnalysis;
    private ComboBox _cmbPriority;
    private DateTimePicker _dtpTargetDate;
    private CheckBox _chkNoTargetDate;

    // Existing history/attachment controls used by presenter
    private TabControl _tabRight;
    private TabPage _tabHistory;
    private DataGridView _gridHistory;
    private TabPage _tabAttachments;
    private Panel _pnlAttachmentToolbar;
    private Button _btnAddAttachment;
    private DataGridView _gridAttachments;

    // DCR detail layout controls
    private TableLayoutPanel _layoutRoot;
    private TableLayoutPanel _layoutMeta;
    private TableLayoutPanel _layoutContent;
    private TableLayoutPanel _layoutMainArea;
    private TableLayoutPanel _layoutReasonAction;
    private TableLayoutPanel _layoutBeforeAfter;
    private TableLayoutPanel _layoutRisk;
    private Label _lblTitleCaption;
    private Label _lblNumberCaption;
    private Label _lblCategoryCaption;
    private Label _lblRaisedByCaption;
    private Label _lblRaisedDateCaption;
    private Label _lblDrawingCaption;
    private Label _lblMachineCaption;
    private Label _lblTargetCaption;
    private Label _lblReasonCaption;
    private Label _lblActionCaption;
    private Label _lblBeforeCaption;
    private Label _lblAfterCaption;
    private Label _lblRiskTitle;
    private Label _lblLeadTime;
    private Label _lblSafety;
    private Label _lblCompliance;
    private Label _lblImpactCaption;
    private Label _lblSuggestionCaption;
    private Label _lblDecisionCaption;
    private Panel _pnlBeforeImage;
    private Panel _pnlAfterImage;
    private PictureBox _picBefore;
    private PictureBox _picAfter;
    private Button _btnInsertBefore;
    private Button _btnClearBefore;
    private Button _btnInsertAfter;
    private Button _btnClearAfter;
    private ComboBox _cmbLeadTimeRisk;
    private ComboBox _cmbSafetyRisk;
    private ComboBox _cmbComplianceRisk;
    private TextBox _txtSuggestion;
    private TextBox _txtDecision;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _pnlHeader = new Panel();
        _lblHeaderTitle = new Label();
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
        _txtTitle = new TextBox();
        _rtbDescription = new RichTextBox();
        _rtbAffectedParts = new RichTextBox();
        _rtbReason = new RichTextBox();
        _rtbImpactAnalysis = new RichTextBox();
        _cmbPriority = new ComboBox();
        _dtpTargetDate = new DateTimePicker();
        _chkNoTargetDate = new CheckBox();
        _tabRight = new TabControl();
        _tabHistory = new TabPage();
        _gridHistory = new DataGridView();
        _tabAttachments = new TabPage();
        _pnlAttachmentToolbar = new Panel();
        _btnAddAttachment = new Button();
        _gridAttachments = new DataGridView();
        _layoutRoot = new TableLayoutPanel();
        _layoutMeta = new TableLayoutPanel();
        _layoutContent = new TableLayoutPanel();
        _layoutMainArea = new TableLayoutPanel();
        _layoutReasonAction = new TableLayoutPanel();
        _layoutBeforeAfter = new TableLayoutPanel();
        _layoutRisk = new TableLayoutPanel();
        _lblTitleCaption = new Label();
        _lblNumberCaption = new Label();
        _lblCategoryCaption = new Label();
        _lblRaisedByCaption = new Label();
        _lblRaisedDateCaption = new Label();
        _lblDrawingCaption = new Label();
        _lblMachineCaption = new Label();
        _lblTargetCaption = new Label();
        _lblReasonCaption = new Label();
        _lblActionCaption = new Label();
        _lblBeforeCaption = new Label();
        _lblAfterCaption = new Label();
        _lblRiskTitle = new Label();
        _lblLeadTime = new Label();
        _lblSafety = new Label();
        _lblCompliance = new Label();
        _lblImpactCaption = new Label();
        _lblSuggestionCaption = new Label();
        _lblDecisionCaption = new Label();
        _pnlBeforeImage = new Panel();
        _pnlAfterImage = new Panel();
        _picBefore = new PictureBox();
        _picAfter = new PictureBox();
        _btnInsertBefore = new Button();
        _btnClearBefore = new Button();
        _btnInsertAfter = new Button();
        _btnClearAfter = new Button();
        _cmbLeadTimeRisk = new ComboBox();
        _cmbSafetyRisk = new ComboBox();
        _cmbComplianceRisk = new ComboBox();
        _txtSuggestion = new TextBox();
        _txtDecision = new TextBox();

        ((System.ComponentModel.ISupportInitialize)_splitMain).BeginInit();
        _splitMain.Panel1.SuspendLayout();
        _splitMain.Panel2.SuspendLayout();
        _splitMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridHistory).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_gridAttachments).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_picBefore).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_picAfter).BeginInit();
        SuspendLayout();

        // Form
        Text = "DCR Detail";
        Size = new Size(1500, 900);
        MinimumSize = new Size(1100, 820);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 10F);

        // Header
        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Height = 54;
        _pnlHeader.BackColor = Color.FromArgb(30, 63, 102);
        _pnlHeader.Padding = new Padding(12, 8, 12, 8);

        _lblHeaderTitle.Text = "Document Change Request Detail";
        _lblHeaderTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        _lblHeaderTitle.ForeColor = Color.White;
        _lblHeaderTitle.AutoSize = true;
        _lblHeaderTitle.Location = new Point(12, 8);

        _lblDCRNumber.Text = "(new)";

        _lblStatusBadge.Text = "Draft";
        _lblStatusBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblStatusBadge.ForeColor = Color.White;
        _lblStatusBadge.BackColor = Color.FromArgb(93, 110, 128);
        _lblStatusBadge.Size = new Size(130, 24);
        _lblStatusBadge.Location = new Point(300, 13);
        _lblStatusBadge.TextAlign = ContentAlignment.MiddleCenter;

        _lblCreatedBy.Text = "Raised by:";

        _lblCreatedAt.Text = "Raised date:";

        _pnlHeader.Controls.AddRange(new Control[] { _lblHeaderTitle, _lblStatusBadge });

        // Actions
        _pnlActions.Dock = DockStyle.Top;
        _pnlActions.Height = 46;
        _pnlActions.BackColor = Color.White;
        _pnlActions.Padding = new Padding(10, 6, 10, 6);

        _flowActions.Dock = DockStyle.Fill;
        _flowActions.FlowDirection = FlowDirection.LeftToRight;
        _flowActions.WrapContents = false;

        _btnSave.Text = "Save";
        _btnEdit.Text = "Edit";
        _btnSubmit.Text = "Submit to DCR";
        _btnClose.Text = "Close";
        foreach (var button in new[] { _btnSave, _btnEdit, _btnSubmit, _btnClose })
        {
            button.Size = new Size(130, 32);
            button.Margin = new Padding(0, 0, 8, 0);
        }

        _flowActions.Controls.AddRange(new Control[] { _btnSave, _btnEdit, _btnSubmit, _btnClose });
        _pnlActions.Controls.Add(_flowActions);

        // Root split: sample-like detail layout + history/attachment pane
        _splitMain.Dock = DockStyle.Fill;
        _splitMain.Orientation = Orientation.Horizontal;
        _splitMain.Panel1MinSize = 430;
        _splitMain.Panel2MinSize = 150;
        _splitMain.SplitterWidth = 6;

        // Main content: left request area + right risk assessment
        _layoutContent.Dock = DockStyle.Fill;
        _layoutContent.Padding = new Padding(12);
        _layoutContent.ColumnCount = 2;
        _layoutContent.RowCount = 1;
        _layoutContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 78F));
        _layoutContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
        _layoutContent.BackColor = Color.FromArgb(245, 247, 250);

        BuildMainArea();
        BuildRiskArea();
        _layoutContent.Controls.Add(_layoutMainArea, 0, 0);
        _layoutContent.Controls.Add(_layoutRisk, 1, 0);
        _splitMain.Panel1.Controls.Add(_layoutContent);

        // Bottom tabs mirror the sample approval/comment block while keeping existing data grids
        _tabRight.Dock = DockStyle.Fill;
        _tabRight.Font = new Font("Segoe UI", 9F);
        _tabHistory.Text = "Approval / Comments";
        _tabHistory.BackColor = Color.White;
        BuildHistoryGrid();
        _tabHistory.Controls.Add(_gridHistory);

        _tabAttachments.Text = "Attachments";
        _tabAttachments.BackColor = Color.White;
        _pnlAttachmentToolbar.Dock = DockStyle.Top;
        _pnlAttachmentToolbar.Height = 42;
        _pnlAttachmentToolbar.Padding = new Padding(8, 6, 8, 6);
        _pnlAttachmentToolbar.BackColor = Color.White;
        _btnAddAttachment.Text = "Add File";
        _btnAddAttachment.Size = new Size(100, 28);
        _btnAddAttachment.Location = new Point(8, 7);
        _pnlAttachmentToolbar.Controls.Add(_btnAddAttachment);
        BuildAttachmentGrid();
        _tabAttachments.Controls.Add(_gridAttachments);
        _tabAttachments.Controls.Add(_pnlAttachmentToolbar);

        _tabRight.TabPages.AddRange(new TabPage[] { _tabHistory, _tabAttachments });
        _splitMain.Panel2.Controls.Add(_tabRight);

        Controls.Add(_splitMain);
        Controls.Add(_pnlActions);
        Controls.Add(_pnlHeader);

        _splitMain.Panel1.ResumeLayout(false);
        _splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_splitMain).EndInit();
        _splitMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_gridHistory).EndInit();
        ((System.ComponentModel.ISupportInitialize)_gridAttachments).EndInit();
        ((System.ComponentModel.ISupportInitialize)_picBefore).EndInit();
        ((System.ComponentModel.ISupportInitialize)_picAfter).EndInit();
        ResumeLayout(false);
    }

    private void BuildMainArea()
    {
        _layoutMainArea.Dock = DockStyle.Fill;
        _layoutMainArea.BackColor = Color.White;
        _layoutMainArea.Padding = new Padding(10);
        _layoutMainArea.ColumnCount = 1;
        _layoutMainArea.RowCount = 3;
        _layoutMainArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
        _layoutMainArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
        _layoutMainArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        BuildMetadataArea();
        BuildReasonActionArea();
        BuildBeforeAfterArea();

        _layoutMainArea.Controls.Add(_layoutMeta, 0, 0);
        _layoutMainArea.Controls.Add(_layoutReasonAction, 0, 1);
        _layoutMainArea.Controls.Add(_layoutBeforeAfter, 0, 2);
    }

    private void BuildMetadataArea()
    {
        _layoutMeta.Dock = DockStyle.Fill;
        _layoutMeta.ColumnCount = 8;
        _layoutMeta.RowCount = 3;
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        _layoutMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
        _layoutMeta.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _layoutMeta.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _layoutMeta.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

        ConfigureCaption(_lblNumberCaption, "Change Log No.");
        ConfigureCaption(_lblCategoryCaption, "Category");
        ConfigureCaption(_lblRaisedByCaption, "Raised by");
        ConfigureCaption(_lblRaisedDateCaption, "Raised date");
        ConfigureCaption(_lblTitleCaption, "Title");
        ConfigureCaption(_lblDrawingCaption, "Drawing No.");
        ConfigureCaption(_lblMachineCaption, "Machine Model");
        ConfigureCaption(_lblTargetCaption, "Target date");

        ConfigureInputLabel(_lblDCRNumber);
        ConfigureInputLabel(_lblCreatedBy);
        ConfigureInputLabel(_lblCreatedAt);

        _txtTitle.Dock = DockStyle.Fill;
        _txtTitle.MaxLength = 300;
        _txtTitle.BorderStyle = BorderStyle.FixedSingle;

        _cmbPriority.Dock = DockStyle.Fill;
        _cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbPriority.Items.AddRange(new object[] { "", "Low", "Medium", "High", "Critical" });
        _cmbPriority.SelectedIndex = 0;

        _dtpTargetDate.Dock = DockStyle.Fill;
        _dtpTargetDate.Format = DateTimePickerFormat.Short;
        _dtpTargetDate.MinDate = DateTime.Today;

        _chkNoTargetDate.Text = "No date";
        _chkNoTargetDate.Dock = DockStyle.Fill;
        _chkNoTargetDate.TextAlign = ContentAlignment.MiddleLeft;

        _rtbAffectedParts.Dock = DockStyle.Fill;
        _rtbAffectedParts.BorderStyle = BorderStyle.FixedSingle;

        _layoutMeta.Controls.Add(_lblNumberCaption, 0, 0);
        _layoutMeta.Controls.Add(_lblDCRNumber, 1, 0);
        _layoutMeta.Controls.Add(_lblCategoryCaption, 2, 0);
        _layoutMeta.Controls.Add(_cmbPriority, 3, 0);
        _layoutMeta.Controls.Add(_lblDrawingCaption, 4, 0);
        _layoutMeta.Controls.Add(_rtbAffectedParts, 5, 0);
        _layoutMeta.SetRowSpan(_rtbAffectedParts, 2);
        _layoutMeta.Controls.Add(_lblMachineCaption, 6, 0);
        _layoutMeta.Controls.Add(BuildReadOnlyText(""), 7, 0);

        _layoutMeta.Controls.Add(_lblRaisedByCaption, 0, 1);
        _layoutMeta.Controls.Add(_lblCreatedBy, 1, 1);
        _layoutMeta.Controls.Add(_lblRaisedDateCaption, 2, 1);
        _layoutMeta.Controls.Add(_lblCreatedAt, 3, 1);
        _layoutMeta.Controls.Add(_lblTargetCaption, 6, 1);
        _layoutMeta.Controls.Add(_dtpTargetDate, 7, 1);

        _layoutMeta.Controls.Add(_lblTitleCaption, 0, 2);
        _layoutMeta.Controls.Add(_txtTitle, 1, 2);
        _layoutMeta.SetColumnSpan(_txtTitle, 5);
        _layoutMeta.Controls.Add(_chkNoTargetDate, 6, 2);
    }

    private void BuildReasonActionArea()
    {
        _layoutReasonAction.Dock = DockStyle.Fill;
        _layoutReasonAction.ColumnCount = 2;
        _layoutReasonAction.RowCount = 2;
        _layoutReasonAction.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _layoutReasonAction.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _layoutReasonAction.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        _layoutReasonAction.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        ConfigureSectionCaption(_lblReasonCaption, "Reason for change request:");
        ConfigureSectionCaption(_lblActionCaption, "Action:");
        ConfigureMemo(_rtbReason);
        ConfigureMemo(_rtbDescription);

        _layoutReasonAction.Controls.Add(_lblReasonCaption, 0, 0);
        _layoutReasonAction.Controls.Add(_lblActionCaption, 1, 0);
        _layoutReasonAction.Controls.Add(_rtbReason, 0, 1);
        _layoutReasonAction.Controls.Add(_rtbDescription, 1, 1);
    }

    private void BuildBeforeAfterArea()
    {
        _layoutBeforeAfter.Dock = DockStyle.Fill;
        _layoutBeforeAfter.ColumnCount = 2;
        _layoutBeforeAfter.RowCount = 2;
        _layoutBeforeAfter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _layoutBeforeAfter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _layoutBeforeAfter.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _layoutBeforeAfter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        ConfigureSectionCaption(_lblBeforeCaption, "Before");
        ConfigureSectionCaption(_lblAfterCaption, "After");

        // Build Before panel with image and buttons
        _pnlBeforeImage.Dock = DockStyle.Fill;
        _pnlBeforeImage.BorderStyle = BorderStyle.FixedSingle;
        _pnlBeforeImage.BackColor = Color.FromArgb(250, 250, 250);
        _pnlBeforeImage.Margin = new Padding(3);

        ConfigurePicturePlaceholder(_picBefore);
        _picBefore.AllowDrop = true;

        _btnInsertBefore.Text = "📁  Insert Image";
        _btnInsertBefore.Size = new Size(120, 28);
        _btnInsertBefore.Dock = DockStyle.Top;
        _btnInsertBefore.Margin = new Padding(4);
        ThemeManager.StyleSecondaryButton(_btnInsertBefore);

        _btnClearBefore.Text = "🗑  Clear";
        _btnClearBefore.Size = new Size(80, 28);
        _btnClearBefore.Dock = DockStyle.Top;
        _btnClearBefore.Margin = new Padding(4);
        ThemeManager.StyleSecondaryButton(_btnClearBefore);

        _pnlBeforeImage.Controls.Add(_picBefore);
        _pnlBeforeImage.Controls.Add(_btnClearBefore);
        _pnlBeforeImage.Controls.Add(_btnInsertBefore);

        // Build After panel with image and buttons
        _pnlAfterImage.Dock = DockStyle.Fill;
        _pnlAfterImage.BorderStyle = BorderStyle.FixedSingle;
        _pnlAfterImage.BackColor = Color.FromArgb(250, 250, 250);
        _pnlAfterImage.Margin = new Padding(3);

        ConfigurePicturePlaceholder(_picAfter);
        _picAfter.AllowDrop = true;

        _btnInsertAfter.Text = "📁  Insert Image";
        _btnInsertAfter.Size = new Size(120, 28);
        _btnInsertAfter.Dock = DockStyle.Top;
        _btnInsertAfter.Margin = new Padding(4);
        ThemeManager.StyleSecondaryButton(_btnInsertAfter);

        _btnClearAfter.Text = "🗑  Clear";
        _btnClearAfter.Size = new Size(80, 28);
        _btnClearAfter.Dock = DockStyle.Top;
        _btnClearAfter.Margin = new Padding(4);
        ThemeManager.StyleSecondaryButton(_btnClearAfter);

        _pnlAfterImage.Controls.Add(_picAfter);
        _pnlAfterImage.Controls.Add(_btnClearAfter);
        _pnlAfterImage.Controls.Add(_btnInsertAfter);

        _layoutBeforeAfter.Controls.Add(_lblBeforeCaption, 0, 0);
        _layoutBeforeAfter.Controls.Add(_lblAfterCaption, 1, 0);
        _layoutBeforeAfter.Controls.Add(_pnlBeforeImage, 0, 1);
        _layoutBeforeAfter.Controls.Add(_pnlAfterImage, 1, 1);
    }

    private void BuildRiskArea()
    {
        _layoutRisk.Dock = DockStyle.Fill;
        _layoutRisk.BackColor = Color.White;
        _layoutRisk.Padding = new Padding(10);
        _layoutRisk.ColumnCount = 2;
        _layoutRisk.RowCount = 12;
        _layoutRisk.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
        _layoutRisk.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _layoutRisk.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        _lblRiskTitle.Text = "Risk Assessment";
        _lblRiskTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _lblRiskTitle.TextAlign = ContentAlignment.MiddleCenter;
        _lblRiskTitle.Dock = DockStyle.Fill;

        ConfigureCaption(_lblLeadTime, "1. Negative impact to leadtime?");
        ConfigureCaption(_lblSafety, "2. Negative impact to safety?");
        ConfigureCaption(_lblCompliance, "3. Break compliance of standards?");
        ConfigureCaption(_lblImpactCaption, "Impact analysis");
        ConfigureCaption(_lblSuggestionCaption, "Suggestion");
        ConfigureCaption(_lblDecisionCaption, "DCR Decision");

        ConfigureYesNoCombo(_cmbLeadTimeRisk);
        ConfigureYesNoCombo(_cmbSafetyRisk);
        ConfigureYesNoCombo(_cmbComplianceRisk);
        ConfigureMemo(_rtbImpactAnalysis);

        _txtSuggestion.Dock = DockStyle.Fill;
        _txtSuggestion.BorderStyle = BorderStyle.FixedSingle;
        _txtSuggestion.Text = "to raise DCR";
        _txtDecision.Dock = DockStyle.Fill;
        _txtDecision.BorderStyle = BorderStyle.FixedSingle;

        _layoutRisk.Controls.Add(_lblRiskTitle, 0, 0);
        _layoutRisk.SetColumnSpan(_lblRiskTitle, 2);
        _layoutRisk.Controls.Add(_lblLeadTime, 0, 1);
        _layoutRisk.Controls.Add(_cmbLeadTimeRisk, 1, 1);
        _layoutRisk.Controls.Add(BuildReadOnlyMemo(), 0, 2);
        _layoutRisk.SetColumnSpan(_layoutRisk.GetControlFromPosition(0, 2), 2);
        _layoutRisk.Controls.Add(_lblSafety, 0, 3);
        _layoutRisk.Controls.Add(_cmbSafetyRisk, 1, 3);
        _layoutRisk.Controls.Add(BuildReadOnlyMemo(), 0, 4);
        _layoutRisk.SetColumnSpan(_layoutRisk.GetControlFromPosition(0, 4), 2);
        _layoutRisk.Controls.Add(_lblCompliance, 0, 5);
        _layoutRisk.Controls.Add(_cmbComplianceRisk, 1, 5);
        _layoutRisk.Controls.Add(BuildReadOnlyMemo(), 0, 6);
        _layoutRisk.SetColumnSpan(_layoutRisk.GetControlFromPosition(0, 6), 2);
        _layoutRisk.Controls.Add(_lblImpactCaption, 0, 7);
        _layoutRisk.SetColumnSpan(_lblImpactCaption, 2);
        _layoutRisk.Controls.Add(_rtbImpactAnalysis, 0, 8);
        _layoutRisk.SetColumnSpan(_rtbImpactAnalysis, 2);
        _layoutRisk.Controls.Add(_lblSuggestionCaption, 0, 9);
        _layoutRisk.Controls.Add(_txtSuggestion, 1, 9);
        _layoutRisk.Controls.Add(_lblDecisionCaption, 0, 10);
        _layoutRisk.Controls.Add(_txtDecision, 1, 10);
    }

    private void BuildHistoryGrid()
    {
        _gridHistory.Dock = DockStyle.Fill;
        ThemeManager.StyleDataGrid(_gridHistory);
        _gridHistory.AutoGenerateColumns = false;
        _gridHistory.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Date",
                DataPropertyName = "ActionDate",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yy HH:mm" }
            },
            new DataGridViewTextBoxColumn { HeaderText = "Role/User", DataPropertyName = "ActorName", Width = 170 },
            new DataGridViewTextBoxColumn { HeaderText = "Action", DataPropertyName = "ActionDisplay", Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "ToStatus", Width = 140 },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Comment",
                DataPropertyName = "Comment",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
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
            new DataGridViewTextBoxColumn { HeaderText = "File Name", DataPropertyName = "FileName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
            new DataGridViewTextBoxColumn { HeaderText = "Size", DataPropertyName = "FileSizeDisplay", Width = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Uploaded By", DataPropertyName = "UploadedBy", Width = 140 },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Date",
                DataPropertyName = "UploadedAt",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            },
            new DataGridViewButtonColumn
            {
                HeaderText = "",
                Text = "Remove",
                UseColumnTextForButtonValue = true,
                Width = 90
            }
        );
    }

    private static void ConfigureCaption(Label label, string text)
    {
        label.Text = text;
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleRight;
        label.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        label.ForeColor = Color.FromArgb(45, 55, 72);
        label.Margin = new Padding(3);
    }

    private static void ConfigureSectionCaption(Label label, string text)
    {
        label.Text = text;
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label.ForeColor = Color.FromArgb(35, 45, 60);
        label.Margin = new Padding(3, 4, 3, 2);
    }

    private static void ConfigureInputLabel(Label label)
    {
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.BorderStyle = BorderStyle.FixedSingle;
        label.BackColor = Color.White;
        label.ForeColor = Color.FromArgb(30, 40, 55);
        label.Padding = new Padding(4, 0, 4, 0);
        label.Margin = new Padding(3);
    }

    private static void ConfigureMemo(RichTextBox memo)
    {
        memo.Dock = DockStyle.Fill;
        memo.BorderStyle = BorderStyle.FixedSingle;
        memo.Font = new Font("Segoe UI", 10F);
        memo.ScrollBars = RichTextBoxScrollBars.Vertical;
        memo.Margin = new Padding(3);
    }

    private static void ConfigurePicturePlaceholder(PictureBox pictureBox)
    {
        pictureBox.Dock = DockStyle.Fill;
        pictureBox.BorderStyle = BorderStyle.None;
        pictureBox.BackColor = Color.FromArgb(250, 250, 250);
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox.Margin = new Padding(0);
    }

    private static void ConfigureYesNoCombo(ComboBox comboBox)
    {
        comboBox.Dock = DockStyle.Fill;
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(new object[] { "", "Yes", "No" });
        comboBox.SelectedIndex = 0;
        comboBox.Margin = new Padding(3);
    }

    private static TextBox BuildReadOnlyText(string text)
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = true,
            Text = text,
            Margin = new Padding(3)
        };
    }

    private static RichTextBox BuildReadOnlyMemo()
    {
        return new RichTextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            ReadOnly = true,
            BackColor = Color.White,
            Margin = new Padding(3),
            ScrollBars = RichTextBoxScrollBars.Vertical
        };
    }
}
