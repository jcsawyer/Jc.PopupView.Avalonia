namespace Jc.PopupView.Avalonia.Services;

internal sealed class InMemoryPopupHandle : IPopupHandle
{
    private readonly Func<CancellationToken, Task> _dismiss;
    private int _dismissed;

    public InMemoryPopupHandle(Guid id, Func<CancellationToken, Task> dismiss)
    {
        Id = id;
        _dismiss = dismiss;
    }

    public Guid Id { get; }

    public async Task DismissAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _dismissed, 1) == 1)
        {
            return;
        }

        await _dismiss(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DismissAsync().ConfigureAwait(false);
    }
}
