namespace Jc.PopupView.Avalonia.Native.iOS;

public class BottomSheet : INativeBottomSheet, IDisposable
{
    private UISheetPresentationControllerDetent _detent;

    public void Dispose()
    {
        _detent?.Dispose();
    }

    public object? DataContext { get; set; }
    public bool IsOpen { get; set; }
    public bool IsDraggable { get; set; }
}
