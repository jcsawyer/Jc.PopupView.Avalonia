using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Jc.PopupView.Avalonia.Controls;
using Jc.PopupView.Avalonia.Exceptions;

namespace Jc.PopupView.Avalonia.Behaviors;

internal sealed class DialogDragBehavior : Behavior<Grid>
{
    private IDialog _dialog;
    private bool _isDragging;
    private Point _dragStart;
    private Point _lastDrag;
    private Rect? _dialogSize;
    private int? _snapBackThreshold;
    
    public static readonly StyledProperty<bool> ClickToDismissProperty =
        AvaloniaProperty.Register<DialogDragBehavior, bool>(
            nameof(ClickToDismiss));

    public bool ClickToDismiss
    {
        get => GetValue(ClickToDismissProperty);
        set => SetValue(ClickToDismissProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<DialogDragBehavior, TimeSpan>(
            nameof(AnimationDuration));

    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { } grid)
        {
            grid.AddHandler(InputElement.PointerPressedEvent, GridOnPointerPressed, handledEventsToo: true,
                routes: RoutingStrategies.Tunnel);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (TopLevel.GetTopLevel(AssociatedObject) is { } topLevel)
        {
            topLevel.SizeChanged -= TopLevelOnSizeChanged;
        }
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        if (AssociatedObject.FindAncestorOfType<IDialog>() is not { } dialog)
        {
            throw new InvalidDialogDragBehaviorControl();
        }
        _dialog = dialog;
        
        _dialogSize = AssociatedObject?.GetTransformedBounds()?.Bounds;
        _snapBackThreshold = _dialogSize?.Height is { } height ? (int)(height * 0.3) : null;

        if (TopLevel.GetTopLevel(AssociatedObject) is { } topLevel)
        {
            _dialogSize = AssociatedObject?.GetTransformedBounds()?.Bounds;
            topLevel.SizeChanged += TopLevelOnSizeChanged;
        }
    }

    private void TopLevelOnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _dialogSize = AssociatedObject?.GetTransformedBounds()?.Bounds;
        _snapBackThreshold = _dialogSize?.Height is { } height ? (int)(height * 0.3) : null;
    }

    private void GridOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_dialog.IsOpen)
        {
            return;
        }

        if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(AssociatedObject);
            _lastDrag = _dragStart;
            e.Pointer.Capture(AssociatedObject);
            
            if (AssociatedObject is { } grid)
            {
                grid.AddHandler(InputElement.PointerReleasedEvent, GridOnPointerReleased, handledEventsToo: true,
                    routes: RoutingStrategies.Tunnel);
                grid.AddHandler(InputElement.PointerMovedEvent, GridOnPointerMoved, handledEventsToo: true,
                    routes: RoutingStrategies.Tunnel);
                grid.AddHandler(InputElement.PointerCaptureLostEvent, GridOnPointerCaptureLost, handledEventsToo: true,
                    routes: RoutingStrategies.Tunnel);   
            }
        }
    }

    private void GridOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        PointerReleasedAndLost();
    }

    private void GridOnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        PointerReleasedAndLost();
    }

    private void PointerReleasedAndLost()
    {
        if (AssociatedObject is { } grid)
        {
            grid.RemoveHandler(InputElement.PointerReleasedEvent, GridOnPointerReleased);
            grid.RemoveHandler(InputElement.PointerMovedEvent, GridOnPointerMoved);
            grid.RemoveHandler(InputElement.PointerCaptureLostEvent, GridOnPointerCaptureLost);   
        }
        
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;

        if (ClickToDismiss)
        {
            _dialog.Close();
        }

        if (AssociatedObject?.RenderTransform is not TranslateTransform translate)
        {
            return;
        }

        if (_snapBackThreshold is not null)
        {
            var isOpen = !(translate.Y > _snapBackThreshold);
            if (isOpen)
            {
                _dialog.IsOpen = true;
            }
            else
            {
                _dialog.Close();
            }
        }
    }

    private void GridOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        var currentPos = e.GetPosition(AssociatedObject);
        var deltaY = currentPos.Y - _dragStart.Y;

        if (sender is Grid grid && grid.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault() is
                { } scrollViewer)
        {
            var scrollViewerDelta = currentPos.Y - _lastDrag.Y;
            if (scrollViewer.Offset.Y > 0 || scrollViewerDelta < 0)
            {
                e.Pointer.Capture(null);
                return;
            }
        }

        if (deltaY < 0)
        {
            return;
        }
        
        if (AssociatedObject?.RenderTransform is not TranslateTransform translate)
        {
            return;
        }

        translate.Y += deltaY;
    }
}