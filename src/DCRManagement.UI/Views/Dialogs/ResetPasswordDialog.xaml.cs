using System.Windows;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views.Dialogs;

public partial class ResetPasswordDialog : FluentWindow
{
    public string NewPassword => PasswordBox.Password;

    public ResetPasswordDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            System.Windows.MessageBox.Show("Passwords do not match.", "Validation",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            System.Windows.MessageBox.Show("Password cannot be empty.", "Validation",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
