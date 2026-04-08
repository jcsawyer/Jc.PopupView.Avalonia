namespace Jc.PopupView.Avalonia.Native.Android;

public static class ActivityExtensions
{
    public static void UsePopupsNative(this Activity activity)
    {
        Native.BottomSheetService.Current = new BottomSheetService(activity);
    }
}