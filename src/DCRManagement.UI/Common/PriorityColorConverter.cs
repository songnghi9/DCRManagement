using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DCRManagement.UI.Common;

/// <summary>Converts a DCR priority string to a SolidColorBrush for the priority badge.</summary>
public sealed class PriorityColorConverter : IValueConverter
{
    public static readonly PriorityColorConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value?.ToString() ?? string.Empty) switch
        {
            "Low"      => new SolidColorBrush(Color.FromRgb( 71, 85, 105)),  // slate-600
            "Medium"   => new SolidColorBrush(Color.FromRgb(  2,132,199)),  // sky-600
            "High"     => new SolidColorBrush(Color.FromRgb(234, 88,  12)),  // orange-600
            "Critical" => new SolidColorBrush(Color.FromRgb(220, 38,  38)),  // red-600
            _          => new SolidColorBrush(Color.FromRgb(148,163,184)),  // slate-400
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
