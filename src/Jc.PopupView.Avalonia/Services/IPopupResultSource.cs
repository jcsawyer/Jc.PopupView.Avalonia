namespace Jc.PopupView.Avalonia.Services;

public interface IPopupResultSource
{
    Task<object?> WaitForResultAsync(CancellationToken cancellationToken = default);
}
