using Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Native;

public interface IBottomSheetService : IDisposable
{
    event EventHandler Opened;
    event EventHandler Closed;
    
    void ShowBottomSheet(Control control, object? dataContext = null);
}