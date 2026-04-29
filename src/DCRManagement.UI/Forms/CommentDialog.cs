using DCRManagement.UI.Common;

namespace DCRManagement.UI.Forms;

/// <summary>
/// Reusable modal dialog for collecting a text comment.
/// Used by workflow actions (Reject, Cancel require mandatory comment;
/// Approve/Close/etc allow optional comment).
/// </summary>
public partial class CommentDialog : Form
{
    private readonly RichTextBox _rtbComment = new();
    private readonly Button _btnOk = new() { Text = "OK", DialogResult = DialogResult.OK };
    private readonly Button _btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };
    private readonly bool _required;

    public string? Comment =>
        string.IsNullOrWhiteSpace(_rtbComment.Text) ? null : _rtbComment.Text.Trim();

    public CommentDialog(string prompt, bool required)
    {
        _required = required;

        Text = required ? "Comment Required" : "Add Comment (Optional)";
        Size = new Size(460, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        AcceptButton = _btnOk; CancelButton = _btnCancel;
        Font = ThemeManager.DefaultFont;
        BackColor = Color.White;

        var lblPrompt = new Label
        {
            Text = prompt,
            AutoSize = true,
            Location = new Point(16, 16),
            Font = ThemeManager.DefaultFont
        };

        _rtbComment.Location = new Point(16, 40);
        _rtbComment.Size = new Size(416, 140);
        _rtbComment.BorderStyle = BorderStyle.FixedSingle;
        _rtbComment.Font = ThemeManager.DefaultFont;
        _rtbComment.ScrollBars = RichTextBoxScrollBars.Vertical;

        ThemeManager.StylePrimaryButton(_btnOk);
        ThemeManager.StyleSecondaryButton(_btnCancel);
        _btnOk.Location = new Point(248, 196); _btnOk.Size = new Size(90, 34);
        _btnCancel.Location = new Point(344, 196); _btnCancel.Size = new Size(90, 34);

        Controls.AddRange([lblPrompt, _rtbComment, _btnOk, _btnCancel]);

        // Guard: prevent OK if comment is required but empty
        _btnOk.Click += (s, e) =>
        {
            if (_required && string.IsNullOrWhiteSpace(_rtbComment.Text))
            {
                MessageBox.Show("A comment is required for this action.", "Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        };
    }
}