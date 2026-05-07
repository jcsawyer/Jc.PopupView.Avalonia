using Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public sealed class OverlayPopupPresenter : IPopupPresenter
{
    private readonly InMemoryPopupPresenter _fallback = new();

    public Task<IPopupHandle> ShowAsync(
        PopupKind kind,
        string route,
        Control content,
        PopupOptions? options,
        CancellationToken cancellationToken = default)
    {
        var host = PopupOverlayHostLocator.Current;
        return host is null
            ? _fallback.ShowAsync(kind, route, content, options, cancellationToken)
            : host.ShowAsync(kind, route, content, options, cancellationToken);
    }

    public Task<object?> ShowForResultAsync(
        PopupKind kind,
        string route,
        Control content,
        PopupOptions? options,
        CancellationToken cancellationToken = default)
    {
        var host = PopupOverlayHostLocator.Current;
        return host is null
            ? _fallback.ShowForResultAsync(kind, route, content, options, cancellationToken)
            : host.ShowForResultAsync(kind, route, content, options, cancellationToken);
    }

    public Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default)
    {
        var host = PopupOverlayHostLocator.Current;
        return host is null
            ? _fallback.DismissTopMostAsync(cancellationToken)
            : host.DismissTopMostAsync(cancellationToken);
    }
}
