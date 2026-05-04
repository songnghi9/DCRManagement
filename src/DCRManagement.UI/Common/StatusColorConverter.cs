using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DCRManagement.UI.Common;

/// <summary>Converts a DCR status string to a SolidColorBrush for the status badge.</summary>
public sealed class StatusColorConverter : IValueConverter
{
    public static readonly StatusColorConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value?.ToString() ?? string.Empty) switch
        {
            "Draft"            => new SolidColorBrush(Color.FromRgb(100, 116, 139)),  // slate-500
            "Pending Review"   => new SolidColorBrush(Color.FromRgb(217, 119,   6)),  // amber-600
            "Under Review"     => new SolidColorBrush(Color.FromRgb( 37,  99, 235)),  // blue-600
            "Pending Approval" => new SolidColorBrush(Color.FromRgb(124,  58, 237)),  // violet-600
            "Approved"         => new SolidColorBrush(Color.FromRgb(  5, 150, 105)),  // emerald-600
            "Rejected"         => new SolidColorBrush(Color.FromRgb(220,  38,  38)),  // red-600
            "Closed"           => new SolidColorBrush(Color.FromRgb( 30,  41,  59)),  // slate-800
            "Cancelled"        => new SolidColorBrush(Color.FromRgb(148, 163, 184)),  // slate-400
            _                  => new SolidColorBrush(Color.FromRgb(100, 116, 139))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
