using System.Windows;
using Wpf.Ui.Controls;

namespace DCRManagement.UI.Views.Dialogs;

public partial class CommentDialog : FluentWindow
{
    public string Comment => CommentTextBox.Text.Trim();

    public CommentDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
