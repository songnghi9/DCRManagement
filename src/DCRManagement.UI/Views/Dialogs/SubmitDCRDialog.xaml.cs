using DCRManagement.Application.DTOs;
using System.Windows;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views.Dialogs;

public partial class SubmitDCRDialog : FluentWindow
{
    public int? ReviewerId => ReviewerComboBox.SelectedValue as int?;
    public int? ApproverId => ApproverComboBox.SelectedValue as int?;

    public SubmitDCRDialog()
    {
        InitializeComponent();
    }

    public void BindUsers(IEnumerable<UserSummaryDto> reviewers, IEnumerable<UserSummaryDto> approvers)
    {
        ReviewerComboBox.ItemsSource = reviewers.ToList();
        ApproverComboBox.ItemsSource = approvers.ToList();
        if (ReviewerComboBox.Items.Count > 0) ReviewerComboBox.SelectedIndex = 0;
        if (ApproverComboBox.Items.Count > 0) ApproverComboBox.SelectedIndex = 0;
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
