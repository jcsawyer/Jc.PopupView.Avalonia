using Avalonia.Controls;
using Avalonia.iOS;

namespace Jc.PopupView.Avalonia.Native.iOS;

public class BottomSheetController : UIViewController
{
    public event EventHandler Opened;
    public event EventHandler Closed;

    private readonly Control _control;
    private readonly AvaloniaView _rootView;

    public BottomSheetController(Control control)
    {
        _control = control;

        var rootView = new AvaloniaView();
        rootView.Content = _control;
        _rootView = rootView;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.White;

        _rootView.Frame = View.Bounds;
        _rootView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
        View.AddSubview(_rootView);
    }
    
    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);
        Opened?.Invoke(this, EventArgs.Empty);
    }

    public override void ViewDidDisappear(bool animated)
    {
        base.ViewDidDisappear(animated);
        Closed?.Invoke(this, EventArgs.Empty);
    }
}