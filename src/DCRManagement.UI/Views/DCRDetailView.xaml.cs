using DCRManagement.Application.Common;
using DCRManagement.Application.DTOs;
using DCRManagement.Application.Services;
using DCRManagement.Domain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DCRManagement.UI.Views;

public partial class DCRDetailView : UserControl
{
    private readonly DCRService _dcrService;
    private readonly WorkflowService _workflowService;
    private readonly UserService _userService;
    private int? _dcrId;
    private bool _approvalExpanded;
    private bool _isEditing;
    private DCRDto? _currentDcr;

    public event EventHandler? CloseRequested;
    public event EventHandler? DataChanged;

    public DCRDetailView(DCRService dcrService, WorkflowService workflowService, UserService userService)
    {
        InitializeComponent();
        _dcrService = dcrService;
        _workflowService = workflowService;
        _userService = userService;

        // Watch CalendarDatePicker.Date via DependencyPropertyDescriptor
        // so the selected date is shown in the placeholder TextBlock
        var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
            Wpf.Ui.Controls.CalendarDatePicker.DateProperty,
            typeof(Wpf.Ui.Controls.CalendarDatePicker));
        dpd?.AddValueChanged(DecisionDatePicker, (_, _) => UpdateDateDisplay());

        Loaded += (_, _) => UpdateDateDisplay();    }

    private void UpdateDateDisplay()
    {
        if (DecisionDatePicker.Date is DateTime date)
        {
            // Show the selected date — hide placeholder hint, show formatted date
            DecisionDatePlaceholder.Text       = date.ToString("dd/MM/yyyy");
            DecisionDatePlaceholder.Foreground = (System.Windows.Media.Brush)FindResource("TextBrush");
            DecisionDatePlaceholder.FontWeight = FontWeights.Normal;
            DecisionDatePlaceholder.Visibility = Visibility.Visible;
        }
        else
        {
            // No date selected — show muted hint text
            DecisionDatePlaceholder.Text       = "Select date…";
            DecisionDatePlaceholder.Foreground = (System.Windows.Media.Brush)FindResource("MutedTextBrush");
            DecisionDatePlaceholder.Visibility = Visibility.Visible;
        }
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public Task OpenCreateAsync()
    {
        _dcrId = null;
        _currentDcr = null;
        _isEditing = true;

        DcrNumberText.Text = "New DCR";
        StatusBadgeText.Text = "Draft";
        StatusBadge.Background = GetStatusColor("Draft");
        HintText.Text = string.Empty;

        ChangeLogText.Text = string.Empty;
        RaisedByText.Text = SessionContext.Instance.FullName;
        RaisedDateText.Text = DateTime.Now.ToString("dd/MM/yyyy");
        DrawingNoTextBox.Text = string.Empty;
        MachineTextBox.Text = string.Empty;
        ReasonTextBox.Text = string.Empty;
        ActionTextBox.Text = string.Empty;
        SuggestionTextBox.Text = string.Empty;
        DecisionCommentTextBox.Text = string.Empty;
        SavingTextBox.Text = string.Empty;
        CostTextBox.Text = string.Empty;
        LeadTimeMemoBox.Text = string.Empty;
        SafetyMemoBox.Text = string.Empty;
        ComplianceMemoBox.Text = string.Empty;
        ApprovalGrid.ItemsSource = null;

        WorkflowActionsPanel.Children.Clear();
        SetEditable(true);
        return Task.CompletedTask;
    }

    public async Task OpenAsync(int dcrId, bool edit)
    {
        var result = await _dcrService.GetByIdAsync(dcrId);
        if (!result.IsSuccess || result.Value is null)
        {
            MessageBox.Show(
                result.ErrorMessage ?? $"DCR {dcrId} not found.",
                "Load Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _dcrId = dcrId;
        _currentDcr = result.Value;
        Bind(result.Value);
        SetEditable(edit && result.Value.IsEditable);
    }

    // ─── Bind ─────────────────────────────────────────────────────────────────

    private void Bind(DCRDto dto)
    {
        DcrNumberText.Text = dto.DCRNumber;
        HintText.Text = $"Created by: {dto.CreatedBy}  |  {dto.CreatedAt:dd/MM/yyyy}  |  {dto.StatusDisplay}";
        SetStatusBadge(dto.StatusDisplay);

        ChangeLogText.Text = dto.DCRNumber;
        RaisedByText.Text = dto.CreatedBy;
        RaisedDateText.Text = dto.CreatedAt.ToString("dd/MM/yyyy");
        DrawingNoTextBox.Text = dto.AffectedParts ?? string.Empty;
        ReasonTextBox.Text = dto.Reason ?? string.Empty;
        ActionTextBox.Text = dto.Description;
        SuggestionTextBox.Text = dto.ImpactAnalysis ?? string.Empty;

        ApprovalGrid.ItemsSource = dto.History.ToList();

        SetWorkflowButtons(dto);
    }

    // ─── Status badge ─────────────────────────────────────────────────────────

    private void SetStatusBadge(string status)
    {
        StatusBadgeText.Text = status;
        StatusBadge.Background = GetStatusColor(status);
    }

    private static SolidColorBrush GetStatusColor(string status) => status switch
    {
        "Draft"            => new SolidColorBrush(Color.FromRgb(108, 117, 125)),
        "Pending Review"   => new SolidColorBrush(Color.FromRgb(255, 153, 0)),
        "PendingReview"    => new SolidColorBrush(Color.FromRgb(255, 153, 0)),
        "Under Review"     => new SolidColorBrush(Color.FromRgb(36, 99, 166)),
        "UnderReview"      => new SolidColorBrush(Color.FromRgb(36, 99, 166)),
        "Pending Approval" => new SolidColorBrush(Color.FromRgb(136, 0, 255)),
        "PendingApproval"  => new SolidColorBrush(Color.FromRgb(136, 0, 255)),
        "Approved"         => new SolidColorBrush(Color.FromRgb(23, 132, 95)),
        "Rejected"         => new SolidColorBrush(Color.FromRgb(191, 47, 47)),
        "Closed"           => new SolidColorBrush(Color.FromRgb(50, 50, 50)),
        "Cancelled"        => new SolidColorBrush(Color.FromRgb(150, 150, 150)),
        _                  => new SolidColorBrush(Color.FromRgb(108, 117, 125))
    };

    // ─── Editable state ───────────────────────────────────────────────────────

    private void SetEditable(bool editable)
    {
        _isEditing = editable;

        // TextBoxes — skip read-only display fields
        foreach (var tb in FindVisualChildren<TextBox>(this))
            tb.IsReadOnly = !editable;

        // ComboBoxes
        foreach (var cb in FindVisualChildren<ComboBox>(this))
            cb.IsEnabled = editable;

        // DatePickers
        foreach (var dp in FindVisualChildren<DatePicker>(this))
            dp.IsEnabled = editable;

        // Display-only TextBlocks are never editable — nothing to do

        // Save / Edit button visibility
        SaveButton.Visibility = editable ? Visibility.Visible : Visibility.Collapsed;
        SaveSideButton.Visibility = editable ? Visibility.Visible : Visibility.Collapsed;
        EditButton.Visibility = (!editable && SessionContext.Instance.CanCreateDCR)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    // ─── Workflow buttons ─────────────────────────────────────────────────────

    private void SetWorkflowButtons(DCRDto dcr)
    {
        WorkflowActionsPanel.Children.Clear();
        var session = SessionContext.Instance;

        void AddButton(string label, RoutedEventHandler handler, string? styleKey = null)
        {
            var btn = new Button
            {
                Content = label,
                MinHeight = 34,
                Padding = new Thickness(14, 0, 14, 0),
                Margin = new Thickness(0, 0, 8, 0)
            };
            if (styleKey is not null && TryFindResource(styleKey) is Style style)
                btn.Style = style;
            btn.Click += handler;
            WorkflowActionsPanel.Children.Add(btn);
        }

        switch (dcr.Status)
        {
            case DCRStatus.PendingReview when session.CanReview:
                AddButton("Start Review", StartReview_Click, "PrimaryButtonStyle");
                AddButton("Reject", Reject_Click, "DangerButtonStyle");
                break;

            case DCRStatus.UnderReview when session.CanReview:
                AddButton("Send to Approval", SendToApproval_Click, "PrimaryButtonStyle");
                AddButton("Reject", Reject_Click, "DangerButtonStyle");
                break;

            case DCRStatus.PendingApproval when session.CanApprove:
                AddButton("Approve", Approve_Click, "PrimaryButtonStyle");
                AddButton("Reject", Reject_Click, "DangerButtonStyle");
                break;

            case DCRStatus.Approved when session.CanApprove:
                AddButton("Close", Close_Click);
                break;

            case DCRStatus.Draft when session.CanCreateDCR:
                AddButton("Cancel", Cancel_Click, "DangerButtonStyle");
                break;

            case DCRStatus.PendingReview when session.CanCreateDCR:
                AddButton("Cancel", Cancel_Click, "DangerButtonStyle");
                break;
        }
    }

    // ─── Workflow action handlers ─────────────────────────────────────────────

    private async void StartReview_Click(object sender, RoutedEventArgs e)
        => await ExecuteWorkflowActionAsync(ApprovalAction.StartReview, "Start Review");

    private async void SendToApproval_Click(object sender, RoutedEventArgs e)
        => await ExecuteWorkflowActionAsync(ApprovalAction.SendToApproval, "Send to Approval");

    private async void Approve_Click(object sender, RoutedEventArgs e)
        => await ExecuteWorkflowActionAsync(ApprovalAction.Approve, "Approve");

    private async void Reject_Click(object sender, RoutedEventArgs e)
        => await ExecuteWorkflowActionWithCommentAsync(ApprovalAction.Reject, "Reject");

    private async void Close_Click(object sender, RoutedEventArgs e)
        => await ExecuteWorkflowActionAsync(ApprovalAction.Close, "Close");

    private async void Cancel_Click(object sender, RoutedEventArgs e)
        => await ExecuteWorkflowActionWithCommentAsync(ApprovalAction.Cancel, "Cancel");

    private async Task ExecuteWorkflowActionAsync(ApprovalAction action, string actionLabel)
    {
        if (_dcrId is null) return;

        try
        {
            var result = await _workflowService.ExecuteActionAsync(
                _dcrId.Value, action, SessionContext.Instance.UserId);

            if (!result.IsSuccess)
            {
                MessageBox.Show(result.ErrorMessage, actionLabel + " Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataChanged?.Invoke(this, EventArgs.Empty);
            await OpenAsync(_dcrId.Value, false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", actionLabel + " Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExecuteWorkflowActionWithCommentAsync(ApprovalAction action, string actionLabel)
    {
        if (_dcrId is null) return;

        var dialog = new Dialogs.CommentDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var result = await _workflowService.ExecuteActionAsync(
                _dcrId.Value, action, SessionContext.Instance.UserId, dialog.Comment);

            if (!result.IsSuccess)
            {
                MessageBox.Show(result.ErrorMessage, actionLabel + " Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataChanged?.Invoke(this, EventArgs.Empty);
            await OpenAsync(_dcrId.Value, false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", actionLabel + " Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ─── Topbar button handlers ───────────────────────────────────────────────

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
        => await SaveAsync();

    private async void SaveSideButton_Click(object sender, RoutedEventArgs e)
        => await SaveAsync();

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        => await SubmitAsync();

    private async void SubmitSideButton_Click(object sender, RoutedEventArgs e)
        => await SubmitAsync();

    private void EditButton_Click(object sender, RoutedEventArgs e)
        => SetEditable(true);

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => CloseRequested?.Invoke(this, EventArgs.Empty);

    // ─── Save logic ───────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ActionTextBox.Text))
        {
            MessageBox.Show("Action / description is required.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_dcrId.HasValue)
            {
                var result = await _dcrService.UpdateAsync(new UpdateDCRDto(
                    _dcrId.Value,
                    ActionTextBox.Text,          // Title
                    ActionTextBox.Text,          // Description
                    DrawingNoTextBox.Text,        // AffectedParts
                    ReasonTextBox.Text,           // Reason
                    SuggestionTextBox.Text,       // ImpactAnalysis
                    null,                         // Priority
                    null));                       // TargetCompletionDate

                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.ErrorMessage, "Save Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentDcr = result.Value;
                if (result.Value is not null)
                    Bind(result.Value);
            }
            else
            {
                // For create, use DrawingNoTextBox as AffectedParts; ActionTextBox as both Title and Description
                var title = string.IsNullOrWhiteSpace(ActionTextBox.Text)
                    ? "New DCR"
                    : ActionTextBox.Text.Split('\n')[0].Trim();

                var result = await _dcrService.CreateAsync(new CreateDCRDto(
                    title,                        // Title
                    ActionTextBox.Text,           // Description
                    DrawingNoTextBox.Text,         // AffectedParts
                    ReasonTextBox.Text,            // Reason
                    SuggestionTextBox.Text,        // ImpactAnalysis
                    null,                          // Priority
                    null));                        // TargetCompletionDate

                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.ErrorMessage, "Save Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _dcrId = result.Value?.Id;
                _currentDcr = result.Value;
                if (result.Value is not null)
                    Bind(result.Value);
            }

            DataChanged?.Invoke(this, EventArgs.Empty);
            MessageBox.Show("DCR saved successfully.", "Saved",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save DCR: {ex.Message}", "Save Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ─── Submit logic ─────────────────────────────────────────────────────────

    private async Task SubmitAsync()
    {
        if (_dcrId is null)
        {
            MessageBox.Show("Please save the DCR before submitting.", "Submit",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var reviewers = (await _userService.GetReviewersAsync()).ToList();
            var approvers = (await _userService.GetApproversAsync()).ToList();

            var dialog = new Dialogs.SubmitDCRDialog();
            dialog.Owner = Window.GetWindow(this);
            dialog.BindUsers(reviewers, approvers);

            if (dialog.ShowDialog() != true) return;

            if (dialog.ReviewerId is null || dialog.ApproverId is null)
            {
                MessageBox.Show("Please select both a reviewer and an approver.", "Submit",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _dcrService.SubmitAsync(
                new SubmitDCRDto(_dcrId.Value, dialog.ReviewerId.Value, dialog.ApproverId.Value));

            if (!result.IsSuccess)
            {
                MessageBox.Show(result.ErrorMessage, "Submit Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataChanged?.Invoke(this, EventArgs.Empty);
            await OpenAsync(_dcrId.Value, false);
            MessageBox.Show("DCR submitted for review.", "Submitted",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to submit DCR: {ex.Message}", "Submit Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ─── Approval log toggle ──────────────────────────────────────────────────

    private void ToggleApprovalButton_Click(object sender, RoutedEventArgs e)
    {
        _approvalExpanded = !_approvalExpanded;
        ApprovalLogRow.Height = new GridLength(_approvalExpanded ? 260 : 44);
        ToggleApprovalButton.Content = _approvalExpanded ? "Collapse" : "Expand";
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        if (parent is null) yield break;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed)
                yield return typed;

            foreach (var descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }
}
