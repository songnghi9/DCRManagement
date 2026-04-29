using DCRManagement.UI.Common;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Admin-triggered password reset dialog.
/// Requires current password verification before accepting new password.
/// </summary>
public partial class ResetPasswordDialog : Form
{
    private readonly TextBox _txtCurrent = new() { UseSystemPasswordChar = true };
    private readonly TextBox _txtNew = new() { UseSystemPasswordChar = true };
    private readonly TextBox _txtConfirm = new() { UseSystemPasswordChar = true };
    private readonly Button _btnOk = new() { Text = "Change Password", DialogResult = DialogResult.OK };
    private readonly Button _btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

    public string CurrentPassword => _txtCurrent.Text;
    public string NewPassword => _txtNew.Text;

    public ResetPasswordDialog(string userName)
    {
        Text = $"Change Password — {userName}";
        Size = new Size(400, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        AcceptButton = _btnOk;
        CancelButton = _btnCancel;
        Font = ThemeManager.DefaultFont;
        BackColor = Color.White;

        int y = 20;
        AddField("Current Password", _txtCurrent, ref y);
        AddField("New Password", _txtNew, ref y);
        AddField("Confirm Password", _txtConfirm, ref y);

        ThemeManager.StylePrimaryButton(_btnOk);
        ThemeManager.StyleSecondaryButton(_btnCancel);
        _btnOk.Location = new Point(180, y + 8); _btnOk.Size = new Size(90, 34);
        _btnCancel.Location = new Point(278, y + 8); _btnCancel.Size = new Size(90, 34);

        Controls.AddRange([_btnOk, _btnCancel]);

        _btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(_txtCurrent.Text))
            { Warn("Current password is required."); return; }

            if (_txtNew.Text.Length < 8)
            { Warn("New password must be at least 8 characters."); return; }

            if (_txtNew.Text != _txtConfirm.Text)
            { Warn("New passwords do not match."); return; }

            DialogResult = DialogResult.OK;
        };
    }

    private void AddField(string label, TextBox txt, ref int y)
    {
        Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = ThemeManager.TextSecondary,
            Location = new Point(20, y)
        });
        txt.Location = new Point(20, y + 18);
        txt.Size = new Size(350, 28);
        txt.Font = ThemeManager.DefaultFont;
        txt.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(txt);
        y += 52;
    }

    private void Warn(string msg) =>
        MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}