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

    public Task<IPopupHandle> ShowToastAsync<TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default)
        where TContent : Control
        => _presenter.ShowAsync(PopupKind.Toast, typeof(TContent).Name, content, options, cancellationToken);

    public Task<IPopupHandle> ShowFloaterAsync<TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default)
        where TContent : Control
        => _presenter.ShowAsync(PopupKind.Floater, typeof(TContent).Name, content, options, cancellationToken);

    public Task<IPopupHandle> ShowSheetAsync<TContent>(TContent content, PopupOptions? options = null, CancellationToken cancellationToken = default)
        where TContent : Control
        => _presenter.ShowAsync(PopupKind.Sheet, typeof(TContent).Name, content, options, cancellationToken);

    public Task<TResult?> ShowToastForResultAsync<TResult, TContent>(
        TContent content,
        PopupOptions? options = null,
        CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource
        => ShowForResultAsync<TResult, TContent>(PopupKind.Toast, content, options, cancellationToken);

    public Task<TResult?> ShowFloaterForResultAsync<TResult, TContent>(
        TContent content,
        PopupOptions? options = null,
        CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource
        => ShowForResultAsync<TResult, TContent>(PopupKind.Floater, content, options, cancellationToken);

    public Task<TResult?> ShowSheetForResultAsync<TResult, TContent>(
        TContent content,
        PopupOptions? options = null,
        CancellationToken cancellationToken = default)
        where TContent : Control, IPopupResultSource
        => ShowForResultAsync<TResult, TContent>(PopupKind.Sheet, content, options, cancellationToken);

    public Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default)
        => _presenter.DismissTopMostAsync(cancellationToken);

    private async Task<TResult?> ShowForResultAsync<TResult, TContent>(
        PopupKind kind,
        TContent content,
        PopupOptions? options,
        CancellationToken cancellationToken)
        where TContent : Control
    {
        var result = await _presenter.ShowForResultAsync(kind, typeof(TContent).Name, content, options, cancellationToken)
            .ConfigureAwait(false);

        if (result is IPopupHandle)
        {
            return default;
        }

        return result is TResult typed ? typed : default;
    }
}
