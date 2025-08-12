using Android.Content;
using Avalonia;

namespace Jc.PopupView.Avalonia.Native.Android;

public static class AppBuilderExtensions
{
    public static AppBuilder UsePopupsNative(this AppBuilder builder, Context context)
    {
        return builder.AfterSetup(_ =>
        {
            Native.BottomSheetService.Current = new BottomSheetService(context);
        });
    }
}