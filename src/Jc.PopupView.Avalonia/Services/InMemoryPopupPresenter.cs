using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public sealed class InMemoryPopupPresenter : IPopupPresenter
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly List<(PopupKind Kind, string Route, IPopupHandle Handle)> _stack = [];

    public async Task<IPopupHandle> ShowAsync(
        PopupKind kind,
        string route,
        IDialog dialog,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentNullException.ThrowIfNull(dialog);

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

            _ = kind;

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
        IDialog dialog,
        CancellationToken cancellationToken = default)
    {
        var handle = await ShowAsync(kind, route, dialog, cancellationToken).ConfigureAwait(false);
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

    public async Task<int> DismissAllAsync(CancellationToken cancellationToken = default)
    {
        IPopupHandle[] handles;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            handles = _stack
                .Select(entry => entry.Handle)
                .Reverse()
                .ToArray();
        }
        finally
        {
            _gate.Release();
        }

        foreach (var handle in handles)
        {
            await handle.DismissAsync(cancellationToken).ConfigureAwait(false);
        }

        return handles.Length;
    }
}
