using Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public interface IDialogService
{
    void OpenSheet<TContent>(TContent content) where TContent : Control;
}