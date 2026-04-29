using DCRManagement.Application.DTOs;

namespace DCRManagement.UI.Common;

public static class ControlExtensions
{
    /// <summary>
    /// Hiển thị placeholder text (hint) trong TextBox khi rỗng.
    /// </summary>
    public static void SetPlaceholder(this TextBox textBox, string placeholder)
    {
        textBox.Text = placeholder;
        textBox.ForeColor = Color.Gray;

        textBox.GotFocus += (s, e) =>
        {
            if (textBox.Text == placeholder)
            {
                textBox.Text = string.Empty;
                textBox.ForeColor = SystemColors.WindowText;
            }
        };

        textBox.LostFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = Color.Gray;
            }
        };
    }

    /// <summary>
    /// Trả về true nếu TextBox đang hiển thị placeholder (không có giá trị thực).
    /// </summary>
    public static bool HasPlaceholder(this TextBox textBox, string placeholder) =>
        textBox.Text == placeholder || string.IsNullOrWhiteSpace(textBox.Text);

    /// <summary>
    /// Binds a collection of UserSummaryDto to a ComboBox with DisplayMember and ValueMember.
    /// </summary>
    public static void BindUsers(this ComboBox comboBox, IEnumerable<UserSummaryDto> users)
    {
        comboBox.DataSource = users.ToList();
        comboBox.DisplayMember = nameof(UserSummaryDto.FullName);
        comboBox.ValueMember = nameof(UserSummaryDto.Id);
    }
}
