using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public interface IDialogService
{
    void OpenSheet<TContent>(TContent content, Action<Sheet>? configure = null) where TContent : Control;
    void CloseSheet<TContent>(TContent content) where TContent : Control;
    void OpenToast<TContent>(TContent content, Action<Toast>? configure = null) where TContent : Control;
    void CloseToast<TContent>(TContent content) where TContent : Control;
}