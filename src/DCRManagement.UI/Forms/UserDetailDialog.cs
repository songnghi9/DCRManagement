using DCRManagement.Application.DTOs;
using DCRManagement.Domain.Enums;
using DCRManagement.UI.Common;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Modal dialog for creating or editing a user.
/// Pass null for <paramref name="existing"/> to open in Create mode.
/// </summary>
public partial class UserDetailDialog : Form
{
    private readonly bool _isCreate;

    private readonly TextBox _txtUsername = new();
    private readonly TextBox _txtFullName = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtDepartment = new();
    private readonly ComboBox _cmbRole = new();
    private readonly TextBox _txtPassword = new();
    private readonly TextBox _txtConfirm = new();
    private readonly CheckBox _chkActive = new() { Text = "Active", Checked = true };
    private readonly Button _btnOk = new() { Text = "Save", DialogResult = DialogResult.OK };
    private readonly Button _btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

    public UserDetailDialog(UserDto? existing)
    {
        _isCreate = existing is null;

        Text = _isCreate ? "New User" : $"Edit User — {existing!.FullName}";
        Size = new Size(440, _isCreate ? 520 : 440);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        AcceptButton = _btnOk;
        CancelButton = _btnCancel;
        Font = ThemeManager.DefaultFont;
        BackColor = Color.White;
        Padding = new Padding(20);

        BuildLayout(existing);

        _btnOk.Click += OnOkClick;
    }

    // ─── Public ───────────────────────────────────────────────────────────────

    public CreateUserDto GetCreateDto() => new(
        _txtUsername.Text.Trim(),
        _txtPassword.Text,
        _txtFullName.Text.Trim(),
        _txtEmail.Text.Trim(),
        (UserRole)_cmbRole.SelectedIndex,
        string.IsNullOrWhiteSpace(_txtDepartment.Text) ? null : _txtDepartment.Text.Trim());

    public UpdateUserDto GetUpdateDto(int id) => new(
        id,
        _txtFullName.Text.Trim(),
        _txtEmail.Text.Trim(),
        (UserRole)_cmbRole.SelectedIndex,
        string.IsNullOrWhiteSpace(_txtDepartment.Text) ? null : _txtDepartment.Text.Trim(),
        _chkActive.Checked);

    // ─── Private ──────────────────────────────────────────────────────────────

    private void BuildLayout(UserDto? existing)
    {
        int y = 20;

        if (_isCreate)
        {
            AddField("Username *", _txtUsername, ref y);
            _txtUsername.MaxLength = 50;

            AddField("Password *", _txtPassword, ref y);
            _txtPassword.UseSystemPasswordChar = true;

            AddField("Confirm Password *", _txtConfirm, ref y);
            _txtConfirm.UseSystemPasswordChar = true;
        }

        AddField("Full Name *", _txtFullName, ref y);
        AddField("Email *", _txtEmail, ref y);
        AddField("Department", _txtDepartment, ref y);

        // Role
        var lblRole = FieldLabel("Role *", y);
        _cmbRole.Location = new Point(20, y + 18);
        _cmbRole.Size = new Size(390, 28);
        _cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbRole.Items.AddRange(["Admin", "Engineer", "Reviewer", "Approver", "ReadOnly"]);
        _cmbRole.SelectedIndex = existing is not null ? (int)existing.Role : 1;
        y += 54;

        if (!_isCreate)
        {
            _chkActive.Location = new Point(20, y);
            _chkActive.Checked = existing!.IsActive;
            y += 30;
        }

        ThemeManager.StylePrimaryButton(_btnOk);
        ThemeManager.StyleSecondaryButton(_btnCancel);
        _btnOk.Location = new Point(220, y + 10); _btnOk.Size = new Size(90, 34);
        _btnCancel.Location = new Point(318, y + 10); _btnCancel.Size = new Size(90, 34);

        // Pre-fill existing values
        if (existing is not null)
        {
            _txtFullName.Text = existing.FullName;
            _txtEmail.Text = existing.Email;
            _txtDepartment.Text = existing.Department ?? string.Empty;
        }

        var allControls = new List<Control>
        {
            lblRole, _cmbRole, _btnOk, _btnCancel
        };
        if (!_isCreate) allControls.Add(_chkActive);

        Controls.AddRange(allControls.ToArray());
    }

    private void AddField(string label, TextBox txt, ref int y)
    {
        Controls.Add(FieldLabel(label, y));
        txt.Location = new Point(20, y + 18);
        txt.Size = new Size(390, 28);
        txt.Font = ThemeManager.DefaultFont;
        txt.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(txt);
        y += 54;
    }

    private static Label FieldLabel(string text, int y) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
        ForeColor = ThemeManager.TextSecondary,
        AutoSize = true,
        Location = new Point(20, y)
    };

    private void OnOkClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtFullName.Text))
        { Warn("Full name is required."); return; }

        if (string.IsNullOrWhiteSpace(_txtEmail.Text) || !_txtEmail.Text.Contains('@'))
        { Warn("A valid email address is required."); return; }

        if (_isCreate)
        {
            if (string.IsNullOrWhiteSpace(_txtUsername.Text))
            { Warn("Username is required."); return; }

            if (_txtPassword.Text.Length < 8)
            { Warn("Password must be at least 8 characters."); return; }

            if (_txtPassword.Text != _txtConfirm.Text)
            { Warn("Passwords do not match."); return; }
        }

        DialogResult = DialogResult.OK;
    }

    private void Warn(string msg) =>
        MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}