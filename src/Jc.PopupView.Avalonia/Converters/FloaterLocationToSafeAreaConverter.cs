using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Converters;

internal sealed class FloaterLocationToSafeAreaConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is Thickness safeArea && values[1] is Label floaterLocation && values[2] is Thickness margin)
        {
            return floaterLocation.Content switch
            {
                FloaterLocation.Top => new Thickness(safeArea.Left + margin.Left, safeArea.Top + margin.Top, safeArea.Right + margin.Right, margin.Bottom),
                FloaterLocation.Bottom => new Thickness(safeArea.Left + margin.Left, margin.Top, safeArea.Right + margin.Right, safeArea.Bottom + margin.Bottom),
                _ => throw new InvalidOperationException($"Invalid floater location: {floaterLocation}.")
            };
        }

        return new Thickness(0);
    }
}