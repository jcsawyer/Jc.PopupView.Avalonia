using Avalonia;
using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;
using Jc.PopupView.Avalonia.Exceptions;

namespace Jc.PopupView.Avalonia;

internal static class DialogHostRegistry
{
    private static readonly Dictionary<Visual, WeakReference<DialogHost>> Hosts = new();

    private static Visual? _active;

    public static void Register(TopLevel topLevel, DialogHost root)
    {
        Hosts[topLevel] = new WeakReference<DialogHost>(root);
        SetActive(topLevel);

        topLevel.AttachedToVisualTree += (_, _) => SetActive(topLevel);
        topLevel.GotFocus += (_, _) => SetActive(topLevel);
    }

    private static void SetActive(Visual visual)
    {
        _active = visual;
    }

    public static DialogHost GetActiveHost()
    {
        if (_active != null &&
            Hosts.TryGetValue(_active, out var weak) &&
            weak.TryGetTarget(out var host))
        {
            return host;
        }

        throw new InvalidDialogHostException();
    }
}