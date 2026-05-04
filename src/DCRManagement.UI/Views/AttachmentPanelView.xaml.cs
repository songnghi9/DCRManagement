using DCRManagement.Application.DTOs;
using System.Windows.Controls;

namespace DCRManagement.UI.Views;

public partial class AttachmentPanelView : UserControl
{
    public AttachmentPanelView()
    {
        InitializeComponent();
    }

    public void BindAttachments(IEnumerable<AttachmentDto> attachments)
    {
        AttachmentGrid.ItemsSource = attachments;
    }
}
