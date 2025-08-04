using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Converters;

internal sealed class ToastLocationToSafeAreaConverter : IMultiValueConverter
{
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is Thickness safeArea && values[1] is Label toastLocation)
        {
            return toastLocation.Content switch
            {
                ToastLocation.Top => new Thickness(safeArea.Left, safeArea.Top, safeArea.Right, 0),
                ToastLocation.Bottom => new Thickness(safeArea.Left, 0, safeArea.Right, safeArea.Bottom),
                _ => throw new InvalidOperationException($"Invalid toast location: {toastLocation}.")
            };
        }

        return new Thickness(0);
    }
}