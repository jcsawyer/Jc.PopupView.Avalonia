namespace Jc.PopupView.Avalonia.Services;

public interface IPopupHandle : IAsyncDisposable
{
    Guid Id { get; }
    Task DismissAsync(CancellationToken cancellationToken = default);
}
