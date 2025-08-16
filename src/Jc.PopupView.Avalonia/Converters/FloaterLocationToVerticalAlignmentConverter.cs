using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Converters;

internal sealed class FloaterLocationToVerticalAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FloaterLocation floaterLocation)
        {
            return floaterLocation switch
            {
                FloaterLocation.Top => VerticalAlignment.Top,
                FloaterLocation.Bottom => VerticalAlignment.Bottom,
                _ => throw new InvalidOperationException($"Invalid floater location: {floaterLocation}.")
            };
        }
        
        throw new InvalidOperationException($"Could not determine floater location.");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}