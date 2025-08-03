using System.Globalization;
using Avalonia.Data.Converters;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Converters;

internal sealed class SheetPillLocationVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SheetPillLocation targetLocation &&
            Enum.TryParse<SheetPillLocation>(parameter?.ToString(), out var actualLocation))
        {
            return targetLocation == actualLocation;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}