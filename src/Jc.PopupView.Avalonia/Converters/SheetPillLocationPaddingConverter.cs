using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Converters;

internal sealed class SheetPullLocationPaddingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SheetPillLocation targetLocation)
        {
            return targetLocation == SheetPillLocation.Internal
                ? new Thickness(20, 0, 20, 20)
                : new Thickness(20);
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}