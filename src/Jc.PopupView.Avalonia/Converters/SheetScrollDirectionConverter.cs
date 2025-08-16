using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Converters;

public class SheetScrollDirectionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SheetScrollDirection direction && Enum.TryParse<SheetScrollDirection>(parameter as string, true, out var paramDirection))
        {
            if (direction == SheetScrollDirection.Both || direction == paramDirection)
            {
                return ScrollBarVisibility.Auto;
            }
        }

        return ScrollBarVisibility.Disabled;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}