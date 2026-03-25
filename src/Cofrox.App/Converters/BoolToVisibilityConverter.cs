using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Cofrox.App.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var invert = string.Equals(parameter?.ToString(), "Invert", StringComparison.OrdinalIgnoreCase);
        var boolValue = value switch
        {
            bool boolean => boolean,
            int intValue => intValue > 0,
            long longValue => longValue > 0,
            string stringValue => !string.IsNullOrWhiteSpace(stringValue),
            _ when value is not null => true,
            _ => false,
        };
        return invert ^ boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility visibility && visibility == Visibility.Visible;
}
