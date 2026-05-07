using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Jc.PopupView.Avalonia.Services;

namespace Jc.PopupView.Avalonia.Sample.Views.Toasts;

public partial class Toast2Result : UserControl, IPopupResultSource
{
    private readonly TaskCompletionSource<object?> _result = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Toast2Result()
    {
        InitializeComponent();
    }

    public Task<object?> WaitForResultAsync(CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return _result.Task;
        }

        cancellationToken.Register(() => _result.TrySetCanceled(cancellationToken));
        return _result.Task;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _result.TrySetResult(null);
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        _result.TrySetResult("Log in");
    }

    private void OnSignUpClick(object? sender, RoutedEventArgs e)
    {
        _result.TrySetResult("Sign up");
    }
}
