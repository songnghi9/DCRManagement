using DCRManagement.Application.DTOs;
using DCRManagement.Domain.Enums;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views.Dialogs;

public partial class UserDetailDialog : FluentWindow
{
    public UserDetailDialog()
    {
        InitializeComponent();
    }

    public void Bind(UserDto user)
    {
        FullNameTextBox.Text   = user.FullName;
        EmailTextBox.Text      = user.Email;
        DepartmentTextBox.Text = user.Department ?? string.Empty;
        IsActiveCheckBox.IsChecked = user.IsActive;

        foreach (var item in RoleComboBox.Items.OfType<ComboBoxItem>())
        {
            if (item.Content?.ToString() == user.Role.ToString())
            {
                RoleComboBox.SelectedItem = item;
                break;
            }
        }
    }

    public (string FullName, string Email, UserRole Role, string? Department, bool IsActive) GetUserValues()
    {
        var roleText = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                       ?? UserRole.Engineer.ToString();
        return (
            FullNameTextBox.Text.Trim(),
            EmailTextBox.Text.Trim(),
            Enum.Parse<UserRole>(roleText),
            string.IsNullOrWhiteSpace(DepartmentTextBox.Text) ? null : DepartmentTextBox.Text.Trim(),
            IsActiveCheckBox.IsChecked == true);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
