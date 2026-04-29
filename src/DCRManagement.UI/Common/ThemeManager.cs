namespace DCRManagement.UI.Common;

public static class ThemeManager
{
    // ── Colors ────────────────────────────────────────────────────────────────
    public static Color PrimaryColor => Color.FromArgb(0, 120, 212);
    public static Color BackgroundColor => Color.FromArgb(245, 246, 250);
    public static Color SurfaceColor => Color.White;
    public static Color BorderColor => Color.FromArgb(220, 223, 230);
    public static Color TextPrimary => Color.FromArgb(30, 30, 30);
    public static Color TextSecondary => Color.FromArgb(100, 100, 110);
    public static Color SuccessColor => Color.FromArgb(16, 124, 16);
    public static Color DangerColor => Color.FromArgb(196, 43, 28);

    // ── Fonts ─────────────────────────────────────────────────────────────────
    public static Font DefaultFont => new("Segoe UI", 9.5f);
    public static Font BoldFont => new("Segoe UI", 9.5f, FontStyle.Bold);
    public static Font SmallFont => new("Segoe UI", 8.5f);
    public static Font TitleFont => new("Segoe UI", 13f, FontStyle.Bold);

    // ── Button styles ─────────────────────────────────────────────────────────
    public static void StylePrimaryButton(Button btn)
    {
        btn.BackColor = PrimaryColor;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = BoldFont;
        btn.Cursor = Cursors.Hand;
    }

    public static void StyleSecondaryButton(Button btn)
    {
        btn.BackColor = Color.White;
        btn.ForeColor = PrimaryColor;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = PrimaryColor;
        btn.FlatAppearance.BorderSize = 1;
        btn.Font = DefaultFont;
        btn.Cursor = Cursors.Hand;
    }

    public static void StyleSuccessButton(Button btn)
    {
        btn.BackColor = SuccessColor;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = BoldFont;
        btn.Cursor = Cursors.Hand;
    }

    public static void StyleDangerButton(Button btn)
    {
        btn.BackColor = DangerColor;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = BoldFont;
        btn.Cursor = Cursors.Hand;
    }

    // ── DataGridView ──────────────────────────────────────────────────────────
    public static void StyleDataGrid(DataGridView grid)
    {
        grid.BackgroundColor = SurfaceColor;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = BorderColor;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = BoldFont;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 36;

        grid.DefaultCellStyle.Font = DefaultFont;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 228, 255);
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.RowTemplate.Height = 32;

        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252);
    }

    // ── Status badge colors ───────────────────────────────────────────────────
    public static Color GetStatusColor(string status) => status switch
    {
        "Draft" => Color.FromArgb(108, 117, 125),
        "PendingReview" => Color.FromArgb(255, 153, 0),
        "UnderReview" => Color.FromArgb(0, 120, 212),
        "PendingApproval" => Color.FromArgb(136, 0, 255),
        "Approved" => Color.FromArgb(16, 124, 16),
        "Rejected" => Color.FromArgb(196, 43, 28),
        "Closed" => Color.FromArgb(50, 50, 50),
        "Cancelled" => Color.FromArgb(150, 150, 150),
        _ => Color.Gray
    };
}