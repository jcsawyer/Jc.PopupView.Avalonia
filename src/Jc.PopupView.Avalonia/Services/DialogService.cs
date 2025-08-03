using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public sealed class DialogService : IDialogService
{
    public void OpenSheet<TContent>(TContent content) where TContent : Control
    {
        var dialogHost = DialogHost.GetDialogHost();
        var sheet = new Sheet { Content = content, DetachOnClose = true, };
        sheet.AttachedToVisualTree += (_, _) =>
        {
            sheet.IsOpen = true;
        };
        dialogHost.Sheets.Add(sheet);
    }
}