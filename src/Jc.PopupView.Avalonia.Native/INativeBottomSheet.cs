namespace Jc.PopupView.Avalonia.Native;

public interface INativeBottomSheet
{
    object? DataContext { get; set; }
    bool IsOpen { get; set; }
    bool IsDraggable { get; set; }
}