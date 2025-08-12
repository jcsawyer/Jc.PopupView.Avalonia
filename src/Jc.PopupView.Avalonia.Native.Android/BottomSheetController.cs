using Android.Content;
using Android.Views;
using Avalonia.Android;
using Avalonia.Controls;
using Google.Android.Material.BottomSheet;

namespace Jc.PopupView.Avalonia.Native.Android;

public class BottomSheetController : BottomSheetDialog
{
    public event EventHandler Opened;
    public event EventHandler Closed;
    public event EventHandler<int> StateChanged;
    
    private readonly Control _control;
    private readonly AvaloniaView _rootView;
    
    public BottomSheetController(Context context, Control control) : base(context)
    {
        _control = control;
        
        var rootView = new AvaloniaView(context);
        rootView.Content = _control;
        _rootView = rootView;
        
        SetContentView(_rootView);
        SetOnShowListener(new DialogInterfaceOnShowListener(() => Opened?.Invoke(this, EventArgs.Empty)));

        SetOnDismissListener(new DialogInterfaceOnDismissListener(() => Closed?.Invoke(this, EventArgs.Empty)));

        var bottomSheet = FindViewById<FrameLayout>(
            Resource.Id.design_bottom_sheet
        );

        if (bottomSheet != null)
        {
            var behavior = BottomSheetBehavior.From(bottomSheet);
            behavior.AddBottomSheetCallback(new MyBottomSheetCallback((s) => StateChanged?.Invoke(this, s)));
        }
    }
    
    class MyBottomSheetCallback : BottomSheetBehavior.BottomSheetCallback
    {
        private readonly Action<int>? _stateChanged;
        public MyBottomSheetCallback(Action<int>? stateChanged)
        {
            _stateChanged = stateChanged;
        }

        public override void OnStateChanged(View bottomSheet, int newState)
        {
            _stateChanged?.Invoke(newState);
        }

        public override void OnSlide(View bottomSheet, float slideOffset) { }
    }

    class DialogInterfaceOnShowListener : Java.Lang.Object, IDialogInterfaceOnShowListener
    {
        private readonly Action _action;
        public DialogInterfaceOnShowListener(Action action) => _action = action;
        public void OnShow(IDialogInterface dialog) => _action();
    }

    class DialogInterfaceOnDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
    {
        private readonly Action _action;
        public DialogInterfaceOnDismissListener(Action action) => _action = action;
        public void OnDismiss(IDialogInterface dialog) => _action();
    }
}