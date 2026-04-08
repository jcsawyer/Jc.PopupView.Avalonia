using Android.Content;
using Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Native.Android;

public class BottomSheetService : IBottomSheetService
{
    private Context _context;
    public event EventHandler? Opened;
    public event EventHandler? Closed;

    public BottomSheetService(Context context)
    {
        _context = context;
    }

    public void Initialize(Activity activity)
    {
        _context = activity;
    }

    public void ShowBottomSheet(Control control, object? dataContext = null)
    {
        control.DataContext = dataContext;
        var controller = new BottomSheetController(_context, control);
        controller.Opened += OnOpened;
        controller.Closed += OnClosed;
        controller.Show();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Closed?.Invoke(this, EventArgs.Empty);
        Closed -= OnClosed;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Opened?.Invoke(this, EventArgs.Empty);
        Opened -= OnOpened;
    }

    public void Dispose()
    {
    }

}