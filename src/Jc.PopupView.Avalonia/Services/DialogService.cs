using Avalonia.Controls;
using Avalonia.Threading;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public sealed class DialogService : IDialogService
{
    private readonly IPopupPresenter _presenter;

    public DialogService()
        : this(new OverlayPopupPresenter())
    {
    }

    internal DialogService(IPopupPresenter presenter)
    {
        _presenter = presenter;
    }

    public void OpenSheet<TContent>(TContent content, Action<Sheet>? configure = null) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var sheet = new Sheet();
        configure?.Invoke(sheet);
        sheet.Content = content;
        sheet.DetachOnClose = true;
        sheet.Loaded += (_, _) =>
        {
            Dispatcher.UIThread.Post(() => { sheet.IsOpen = true; }, DispatcherPriority.Loaded);
        };
        dialogHost.Sheets.Add(sheet);
    }

    public void CloseSheet<TContent>(TContent content) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var sheet = dialogHost.Sheets.FirstOrDefault(s => Equals(s.Content, content));
        sheet?.Close();
    }

    public void OpenToast<TContent>(TContent content, Action<Toast>? configure = null) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var toast = new Toast();
        configure?.Invoke(toast);

        toast.Content = content;
        toast.DetachOnClose = true;

        toast.Loaded += (_, _) =>
        {
            Dispatcher.UIThread.Post(() => { toast.IsOpen = true; }, DispatcherPriority.Loaded);
        };
        dialogHost.Toasts.Add(toast);
    }

    public void CloseToast<TContent>(TContent content) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var toast = dialogHost.Toasts.FirstOrDefault(t => Equals(t.Content, content));
        toast?.Close();
    }

    public void OpenFloater<TContent>(TContent content, Action<Floater>? configure = null) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var floater = new Floater();
        configure?.Invoke(floater);

        floater.Content = content;
        floater.DetachOnClose = true;

        floater.Loaded += (_, _) =>
        {
            Dispatcher.UIThread.Post(() => { floater.IsOpen = true; }, DispatcherPriority.Loaded);
        };
        dialogHost.Floaters.Add(floater);
    }

    public void CloseFloater<TContent>(TContent content) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var floater = dialogHost.Floaters.FirstOrDefault(f => Equals(f.Content, content));
        floater?.Close();
    }

    public void OpenPopup<TContent>(TContent content, Action<Popup>? configure = null) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var popup = new Popup();
        configure?.Invoke(popup);

        popup.Content = content;
        popup.DetachOnClose = true;

        popup.Loaded += (_, _) =>
        {
            Dispatcher.UIThread.Post(() => { popup.IsOpen = true; }, DispatcherPriority.Loaded);
        };
        dialogHost.Popups.Add(popup);
    }

    public void ClosePopup<TContent>(TContent content) where TContent : Control
    {
        var dialogHost = DialogHostRegistry.GetActiveHost();
        var popup = dialogHost.Popups.FirstOrDefault(p => Equals(p.Content, content));
        popup?.Close();
    }

    public Task<IPopupHandle> ShowToastAsync<TContent>(TContent content, Action<Toast>? configure = null, CancellationToken cancellationToken = default)
        where TContent : Control
    {
        var toast = new Toast
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(toast);
        return _presenter.ShowAsync(PopupKind.Toast, typeof(TContent).Name, toast, cancellationToken);
    }

    public Task<IPopupHandle> ShowFloaterAsync<TContent>(TContent content, Action<Floater>? configure = null, CancellationToken cancellationToken = default)
        where TContent : Control
    {
        var floater = new Floater
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(floater);
        return _presenter.ShowAsync(PopupKind.Floater, typeof(TContent).Name, floater, cancellationToken);
    }

    public Task<IPopupHandle> ShowSheetAsync<TContent>(TContent content, Action<Sheet>? configure = null, CancellationToken cancellationToken = default)
        where TContent : Control
    {
        var sheet = new Sheet
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(sheet);
        return _presenter.ShowAsync(PopupKind.Sheet, typeof(TContent).Name, sheet, cancellationToken);
    }

    public Task<IPopupHandle> ShowPopupAsync<TContent>(TContent content, Action<Popup>? configure = null, CancellationToken cancellationToken = default)
        where TContent : Control
    {
        var popup = new Popup
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(popup);
        return _presenter.ShowAsync(PopupKind.Popup, typeof(TContent).Name, popup, cancellationToken);
    }

    public Task<TResult?> ShowToastForResultAsync<TResult, TContent>(
        TContent content,
        Action<Toast>? configure = null,
        CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource
    {
        var toast = new Toast
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(toast);
        return ShowForResultAsync<TResult>(PopupKind.Toast, typeof(TContent).Name, toast, cancellationToken);
    }

    public Task<TResult?> ShowFloaterForResultAsync<TResult, TContent>(
        TContent content,
        Action<Floater>? configure = null,
        CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource
    {
        var floater = new Floater
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(floater);
        return ShowForResultAsync<TResult>(PopupKind.Floater, typeof(TContent).Name, floater, cancellationToken);
    }

    public Task<TResult?> ShowSheetForResultAsync<TResult, TContent>(
        TContent content,
        Action<Sheet>? configure = null,
        CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource
    {
        var sheet = new Sheet
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(sheet);
        return ShowForResultAsync<TResult>(PopupKind.Sheet, typeof(TContent).Name, sheet, cancellationToken);
    }

    public Task<TResult?> ShowPopupForResultAsync<TResult, TContent>(
        TContent content,
        Action<Popup>? configure = null,
        CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource
    {
        var popup = new Popup
        {
            Content = content,
            DetachOnClose = true,
        };
        configure?.Invoke(popup);
        return ShowForResultAsync<TResult>(PopupKind.Popup, typeof(TContent).Name, popup, cancellationToken);
    }

    public Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default)
        => _presenter.DismissTopMostAsync(cancellationToken);

    public Task<int> DismissAllAsync(CancellationToken cancellationToken = default)
        => _presenter.DismissAllAsync(cancellationToken);

    private async Task<TResult?> ShowForResultAsync<TResult>(
        PopupKind kind,
        string route,
        IDialog dialog,
        CancellationToken cancellationToken)
    {
        var result = await _presenter.ShowForResultAsync(kind, route, dialog, cancellationToken)
            .ConfigureAwait(false);

        if (result is IPopupHandle)
        {
            return default;
        }

        return result is TResult typed ? typed : default;
    }
}
