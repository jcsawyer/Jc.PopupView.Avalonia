using Avalonia;

namespace Jc.PopupView.Avalonia.Native.iOS;

public static class AppBuilderExtensions
{
    public static AppBuilder UsePopupsNative(this AppBuilder builder)
    {
        return builder.AfterSetup(_ =>
        {
            Native.BottomSheetService.Current = new BottomSheetService();
        });
    }
}