using Avalonia.Collections;
using Avalonia.Media;

namespace Jc.PopupView.Avalonia.Services;

public sealed class PopupOptions
{
    public bool DismissOnBackdropTap { get; set; } = true;
    public bool? ShowBackdrop { get; set; }
    public IBrush? BackdropColor { get; set; }
    public bool? ClickToDismiss { get; set; }
    public TimeSpan? AnimationDuration { get; set; }
    public TimeSpan? Duration { get; set; }
    public double? SnapPoint { get; set; }
    public IReadOnlyList<double>? Detents { get; set; }
    public double? InitialDetent { get; set; }
    public PopupPlacement? Placement { get; set; }

    internal AvaloniaList<double>? ToDetentList()
    {
        return Detents is { Count: > 0 } ? new AvaloniaList<double>(Detents) : null;
    }
}
