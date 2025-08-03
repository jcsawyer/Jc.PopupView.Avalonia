using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Behaviors;

internal sealed class DialogDragBehavior : Behavior<Grid>
{
    public enum DragOrigin
    {
        Bottom,
        //Top,
        //Left,
        //Right,
    };

    private Visual? _contentVisual;
    private Compositor? _compositor;
    private CompositionVisual? _compositionVisual;
    private bool _isDragging;
    private Point _dragStart;
    private Point _lastDrag;
    private Size _clientSize;
    private Rect? _dialogSize;
    private Vector3D? _initialOffset;

    public bool SnapBack { get; set; }
    public int? SnapBackThreshold { get; set; }

    public bool ClickToDismiss { get; set; }

    public static readonly StyledProperty<DragOrigin> OriginProperty =
        AvaloniaProperty.Register<DialogDragBehavior, DragOrigin>(
            nameof(Origin), defaultValue: DragOrigin.Bottom);

    public DragOrigin Origin
    {
        get => GetValue(OriginProperty);
        set => SetValue(OriginProperty, value);
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
            grid.AttachedToVisualTree += GridOnAttachedToVisualTree;
            grid.AddHandler(InputElement.PointerReleasedEvent, GridOnPointerReleased, handledEventsToo: true,
                routes: RoutingStrategies.Tunnel);
            grid.AddHandler(InputElement.PointerPressedEvent, GridOnPointerPressed, handledEventsToo: true,
                routes: RoutingStrategies.Tunnel);
            grid.AddHandler(InputElement.PointerMovedEvent, GridOnPointerMoved, handledEventsToo: true,
                routes: RoutingStrategies.Tunnel);
        }
    }

    private void GridOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject is { } grid)
        {
            _contentVisual = grid;
            _compositionVisual = ElementComposition.GetElementVisual(AssociatedObject);
            _compositor = _compositionVisual?.Compositor;

            if (TopLevel.GetTopLevel(AssociatedObject) is { } topLevel)
            {
                _clientSize = topLevel.ClientSize;
                _dialogSize = AssociatedObject?.GetTransformedBounds()?.Bounds;
                topLevel.SizeChanged += TopLevelOnSizeChanged;
            }
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is { } grid)
        {
            grid.AttachedToVisualTree -= GridOnAttachedToVisualTree;
        }

        if (TopLevel.GetTopLevel(AssociatedObject) is { } topLevel)
        {
            topLevel.SizeChanged -= TopLevelOnSizeChanged;
        }
    }

    protected override void OnLoaded()
    {
        if (_compositionVisual is { } visual)
        {
            _initialOffset = visual.Offset;
            if (AssociatedObject.FindAncestorOfType<IDialog>() is { IsOpen: false } && _dialogSize is not null)
            {
                visual.Offset = new Vector3D(visual.Offset.X,
                    (float)((_compositionVisual.Offset.Y) + _dialogSize.Value.Height),
                    _compositionVisual.Offset.Z);
            }
        }

        base.OnLoaded();
    }

    private void TopLevelOnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _clientSize = e.NewSize;
        _dialogSize = AssociatedObject?.GetTransformedBounds()?.Bounds;
        SnapBackThreshold = _dialogSize?.Height is { } height ? (int)(height * 0.5) : null;
    }

    private void GridOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject.FindAncestorOfType<IDialog>() is not { } dialog || !dialog.IsOpen)
        {
            return;
        }
        if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(AssociatedObject);
            _lastDrag = _dragStart;
            e.Pointer.Capture(AssociatedObject);
        }
    }

    private void GridOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;

        if (ClickToDismiss)
        {
            AnimateOut();
        }

        if (SnapBack && SnapBackThreshold is not null)
        {
            if (_compositionVisual?.Offset.Y >= SnapBackThreshold - _initialOffset?.Y)
            {
                AnimateOut();
                return;
            }

            AnimateIn();
        }
    }

    private void GridOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _contentVisual is null || _compositionVisual is null)
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

        _compositionVisual.Offset = new Vector3D(0, (float)(currentPos.Y + (_initialOffset?.Y ?? 0) - _dragStart.Y), 0);
    }

    internal void AnimateOut()
    {
        if (_compositionVisual is null || _contentVisual is null || _compositor is null)
        {
            return;
        }

        var timeRemaining = _compositionVisual.Offset.Y / (_dialogSize?.Height ?? _clientSize.Height) *
                            AnimationDuration.TotalMilliseconds;
        if (timeRemaining <= 0)
        {
            return;
        }
        

        var animation = _compositor.CreateVector3KeyFrameAnimation();
        animation.Duration = TimeSpan.FromMilliseconds(AnimationDuration.TotalMilliseconds);

        var value = Origin switch
        {
            DragOrigin.Bottom => new Vector3(0, (float)(_dialogSize?.Height + _initialOffset?.Y ?? _clientSize.Height),
                0),
            _ => throw new InvalidOperationException("Invalid drag origin specified."),
        };

        animation.InsertKeyFrame(1f, value, new CubicEaseOut());
        animation.Target = "Offset.Y";

        if (AssociatedObject.FindAncestorOfType<IDialog>() is not { } dialog)
        {
            return;
        }

        DispatcherTimer.RunOnce(() =>
        {
            dialog.IsClosing = false;
            if (dialog.IsOpen)
            {
                dialog.IsOpen = false;
            }
            dialog.UpdatePseudoClasses();
        }, animation.Duration, DispatcherPriority.Render);

        dialog.IsClosing = true;
        _compositionVisual.StartAnimation("Offset", animation);
    }

    internal void AnimateIn()
    {
        if (_compositionVisual is null || _contentVisual is null || _compositor is null)
        {
            return;
        }

        var timeRemaining = _compositionVisual.Offset.Y / (_dialogSize?.Height ?? _clientSize.Height) *
                            AnimationDuration.TotalMilliseconds;
        if (timeRemaining <= 0)
        {
            return;
        }

        var animation = _compositor.CreateVector3KeyFrameAnimation();
        animation.Duration = TimeSpan.FromMilliseconds(timeRemaining);

        var value = Origin switch
        {
            DragOrigin.Bottom => new Vector3(0, (float)(_initialOffset?.Y ?? 0), 0),
            _ => throw new InvalidOperationException("Invalid drag origin specified."),
        };

        animation.InsertKeyFrame(1f, value, new CubicEaseIn());
        animation.Target = "Offset.Y";

        if (AssociatedObject.FindAncestorOfType<IDialog>() is not { } dialog)
        {
            return;
        }

        DispatcherTimer.RunOnce(() =>
        {
            dialog.IsOpening = false;
            if (!dialog.IsOpen)
            {
                dialog.IsOpen = true;
            }
            dialog.UpdatePseudoClasses();
        }, animation.Duration, DispatcherPriority.Render);

        dialog.IsOpening = true;
        _compositionVisual.StartAnimation("Offset", animation);
    }
}