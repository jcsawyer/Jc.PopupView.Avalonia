using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public interface IDialogService
{
    void OpenSheet<TContent>(TContent content, Action<Sheet>? configure = null) where TContent : Control;
    void CloseSheet<TContent>(TContent content) where TContent : Control;
    void OpenToast<TContent>(TContent content, Action<Toast>? configure = null) where TContent : Control;
    void CloseToast<TContent>(TContent content) where TContent : Control;
    void OpenFloater<TContent>(TContent content, Action<Floater>? configure = null) where TContent : Control;
    void CloseFloater<TContent>(TContent content) where TContent : Control;

    Task<IPopupHandle> ShowToastAsync<TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default) where TContent : Control;
    Task<IPopupHandle> ShowFloaterAsync<TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default) where TContent : Control;
    Task<IPopupHandle> ShowSheetAsync<TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default) where TContent : Control;
    Task<TResult?> ShowToastForResultAsync<TResult, TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource;
    Task<TResult?> ShowFloaterForResultAsync<TResult, TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource;
    Task<TResult?> ShowSheetForResultAsync<TResult, TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource;
    Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default);
}
