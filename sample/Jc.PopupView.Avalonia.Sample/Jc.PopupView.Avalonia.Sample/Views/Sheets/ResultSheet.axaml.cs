using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Jc.PopupView.Avalonia.Services;

namespace Jc.PopupView.Avalonia.Sample.Views.Sheets;

public partial class ResultSheet : UserControl, IPopupResultSource
{
    private readonly TaskCompletionSource<object?> _result = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ResultSheet()
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

    private async void OnNestedClick(object? sender, RoutedEventArgs e)
    {
        await new DialogService().ShowSheetAsync(new TextBlock { Text = "Nested sheet!" }, sheet => sheet.Detents = [0.36]);
    }

    private void OnImportantClick(object? sender, RoutedEventArgs e)
    {
        _result.TrySetResult("important");
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        _result.TrySetResult(null);
    }
}
