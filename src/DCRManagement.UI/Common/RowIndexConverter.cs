using System.Globalization;
using System.Windows.Data;

namespace DCRManagement.UI.Common;

/// <summary>
/// Converts AlternationIndex (0-based) to a 1-based row number string.
/// Used in DataGridRowHeader to display row numbers.
/// </summary>
public sealed class RowIndexConverter : IValueConverter
{
    public static readonly RowIndexConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int i ? (i + 1).ToString() : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
