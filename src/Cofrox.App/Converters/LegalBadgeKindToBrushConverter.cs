using Cofrox.App.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Cofrox.App.Converters;

public sealed class LegalBadgeKindToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var key = value is LegalBadgeKind badgeKind
            ? badgeKind switch
            {
                LegalBadgeKind.Permissive => "LegalBadgePermissiveBrush",
                LegalBadgeKind.Lgpl => "LegalBadgeLgplBrush",
                LegalBadgeKind.Gpl => "LegalBadgeGplBrush",
                LegalBadgeKind.Agpl => "LegalBadgeAgplBrush",
                _ => "LegalBadgeCommunityBrush",
            }
            : "LegalBadgeCommunityBrush";

        return Microsoft.UI.Xaml.Application.Current.Resources[key] as Brush ?? new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
