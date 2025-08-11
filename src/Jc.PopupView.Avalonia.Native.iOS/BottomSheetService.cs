using Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Native.iOS;

public class BottomSheetService : IBottomSheetService
{
    public event EventHandler? Opened;
    public event EventHandler? Closed;

    public void ShowBottomSheet(Control control, object? dataContext = null)
    {
        control.DataContext = dataContext;
        
        var controller = new BottomSheetController(control);
        controller.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

        if (controller.SheetPresentationController is { } sheet)
        {
            sheet.Detents = [UISheetPresentationControllerDetent.CreateLargeDetent()];
            sheet.SelectedDetentIdentifier = UISheetPresentationControllerDetentIdentifier.Large;
            sheet.PrefersGrabberVisible = true;
        }

        controller.Opened += OnOpened;
        controller.Closed += OnClosed;

        UIApplication.SharedApplication.KeyWindow?.RootViewController?.PresentViewController(controller, true, null);
    }

    public void Dispose()
    {
    }
    
    private void OnOpened(object? sender, EventArgs e)
    {
        Opened?.Invoke(this, EventArgs.Empty);
        Opened -= OnOpened;
    }
    
    private void OnClosed(object? sender, EventArgs e)
    {
        Closed?.Invoke(this, EventArgs.Empty);
        Closed -= OnClosed;
    }
}