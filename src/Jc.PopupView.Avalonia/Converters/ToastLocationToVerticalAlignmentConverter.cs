using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Converters;

internal sealed class ToastLocationToVerticalAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ToastLocation toastLocation)
        {
            return toastLocation switch
            {
                ToastLocation.Top => VerticalAlignment.Top,
                ToastLocation.Bottom => VerticalAlignment.Bottom,
                _ => throw new InvalidOperationException($"Invalid toast location: {toastLocation}.")
            };
        }

        throw new InvalidOperationException($"Could not determine toast location.");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}