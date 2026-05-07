using Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public sealed class InMemoryPopupPresenter : IPopupPresenter
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly List<(PopupKind Kind, string Route, IPopupHandle Handle)> _stack = [];

    public async Task<IPopupHandle> ShowAsync(
        PopupKind kind,
        string route,
        Control content,
        PopupOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentNullException.ThrowIfNull(content);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var id = Guid.NewGuid();
            var created = new InMemoryPopupHandle(id, async _ =>
            {
                await _gate.WaitAsync().ConfigureAwait(false);
                try
                {
                    _stack.RemoveAll(s => s.Handle.Id == id);
                }
                finally
                {
                    _gate.Release();
                }
            });

            _stack.Add((kind, route, created));

            if (kind == PopupKind.Toast && options?.Duration is TimeSpan duration)
            {
                _ = AutoDismissAsync(created, duration);
            }

            return created;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<object?> ShowForResultAsync(
        PopupKind kind,
        string route,
        Control content,
        PopupOptions? options,
        CancellationToken cancellationToken = default)
    {
        var handle = await ShowAsync(kind, route, content, options, cancellationToken).ConfigureAwait(false);
        return handle;
    }

    public async Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default)
    {
        IPopupHandle? handle;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            handle = _stack.LastOrDefault().Handle;
        }
        finally
        {
            _gate.Release();
        }

        if (handle is null)
        {
            return false;
        }

        await handle.DismissAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static async Task AutoDismissAsync(IPopupHandle handle, TimeSpan duration)
    {
        await Task.Delay(duration).ConfigureAwait(false);
        await handle.DismissAsync().ConfigureAwait(false);
    }
}
