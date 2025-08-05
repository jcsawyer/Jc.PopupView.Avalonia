using Avalonia.Controls;
using Avalonia.Threading;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public sealed class DialogService : IDialogService
{
    public void OpenSheet<TContent>(TContent content, Action<Sheet>? configure = null) where TContent : Control
    {
        var dialogHost = DialogHost.GetDialogHost();
        var sheet = new Sheet();
        configure?.Invoke(sheet);
        sheet.Content = content;
        sheet.DetachOnClose = true;
        sheet.Loaded += (_, _) =>
        {
            // Delay to next layout cycle to ensure the control is fully loaded
            Dispatcher.UIThread.Post(() => { sheet.IsOpen = true; }, DispatcherPriority.Loaded);
        };
        dialogHost.Sheets.Add(sheet);
    }

    public void CloseSheet<TContent>(TContent content) where TContent : Control
    {
        var dialogHost = DialogHost.GetDialogHost();
        var sheet = dialogHost.Sheets.FirstOrDefault(s => Equals(s.Content, content));
        sheet?.Close();
    }

    public void OpenToast<TContent>(TContent content, Action<Toast>? configure = null) where TContent : Control
    {
        var dialogHost = DialogHost.GetDialogHost();
        var toast = new Toast();
        configure?.Invoke(toast);

        toast.Content = content;
        toast.DetachOnClose = true;

        toast.Loaded += (_, _) =>
        {
            // Delay to next layout cycle to ensure the control is fully loaded
            Dispatcher.UIThread.Post(() => { toast.IsOpen = true; }, DispatcherPriority.Loaded);
        };
        dialogHost.Toasts.Add(toast);
    }

    public void CloseToast<TContent>(TContent content) where TContent : Control
    {
        var dialogHost = DialogHost.GetDialogHost();
        var toast = dialogHost.Toasts.FirstOrDefault(t => Equals(t.Content, content));
        toast?.Close();
    }
}