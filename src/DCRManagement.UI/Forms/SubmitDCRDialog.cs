using DCRManagement.Application.DTOs;
using DCRManagement.UI.Common;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Modal dialog shown when engineer submits a DCR.
/// Lets them pick an assigned Reviewer and Approver before submitting.
/// </summary>
public partial class SubmitDCRDialog : Form
{
    private readonly ComboBox _cmbReviewer = new();
    private readonly ComboBox _cmbApprover = new();
    private readonly Button _btnOk = new() { Text = "Submit", DialogResult = DialogResult.OK };
    private readonly Button _btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

    public int SelectedReviewerId =>
        _cmbReviewer.SelectedValue is int id ? id : 0;
    public int SelectedApproverId =>
        _cmbApprover.SelectedValue is int id ? id : 0;

    public SubmitDCRDialog(
        IEnumerable<UserSummaryDto> reviewers,
        IEnumerable<UserSummaryDto> approvers)
    {
        Text = "Submit DCR for Review";
        Size = new Size(400, 260);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        AcceptButton = _btnOk; CancelButton = _btnCancel;
        Font = ThemeManager.DefaultFont;
        BackColor = Color.White;

        var lblReviewer = new Label { Text = "Assign Reviewer:", AutoSize = true, Location = new Point(24, 24) };
        _cmbReviewer.BindUsers(reviewers);
        _cmbReviewer.Location = new Point(24, 44); _cmbReviewer.Size = new Size(340, 28);
        _cmbReviewer.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblApprover = new Label { Text = "Assign Approver:", AutoSize = true, Location = new Point(24, 90) };
        _cmbApprover.BindUsers(approvers);
        _cmbApprover.Location = new Point(24, 110); _cmbApprover.Size = new Size(340, 28);
        _cmbApprover.DropDownStyle = ComboBoxStyle.DropDownList;

        ThemeManager.StylePrimaryButton(_btnOk);
        ThemeManager.StyleSecondaryButton(_btnCancel);
        _btnOk.Location = new Point(180, 170); _btnOk.Size = new Size(90, 34);
        _btnCancel.Location = new Point(278, 170); _btnCancel.Size = new Size(90, 34);

        Controls.AddRange([
            lblReviewer, _cmbReviewer,
            lblApprover, _cmbApprover,
            _btnOk, _btnCancel
        ]);
    }
}