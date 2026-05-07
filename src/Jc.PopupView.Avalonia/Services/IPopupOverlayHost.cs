using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Services;

public interface IPopupOverlayHost
{
    Task<IPopupHandle> ShowAsync(
        PopupKind kind,
        string route,
        IDialog dialog,
        CancellationToken cancellationToken = default);

    Task<object?> ShowForResultAsync(
        PopupKind kind,
        string route,
        IDialog dialog,
        CancellationToken cancellationToken = default);

    Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default);
}
