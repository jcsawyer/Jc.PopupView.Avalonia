namespace Jc.PopupView.Avalonia.Services;

public static class PopupOverlayHostLocator
{
    private static readonly object Gate = new();
    private static WeakReference<IPopupOverlayHost>? _current;

    public static IPopupOverlayHost? Current
    {
        get
        {
            lock (Gate)
            {
                if (_current is not null && _current.TryGetTarget(out var target))
                {
                    return target;
                }

                return null;
            }
        }
    }

    public static void Register(IPopupOverlayHost host)
    {
        lock (Gate)
        {
            _current = new WeakReference<IPopupOverlayHost>(host);
        }
    }

    public static void Clear(IPopupOverlayHost host)
    {
        lock (Gate)
        {
            if (_current is not null && _current.TryGetTarget(out var target) && ReferenceEquals(target, host))
            {
                _current = null;
            }
        }
    }
}
