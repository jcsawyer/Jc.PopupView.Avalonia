using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Jc.PopupView.Avalonia.Controls;
using Jc.PopupView.Avalonia.Exceptions;

namespace Jc.PopupView.Avalonia.Behaviors;

internal sealed class ScrollableDialogDragBehavior : Behavior<Grid>
{
    private Sheet? _sheet;
    private bool _isDragging;
    private bool _hasPendingDrag;
    private int? _activePointerId;
    private ScrollViewer? _dragScrollViewer;
    private bool _scrollWasAtTopOnPointerDown;
    private double _dragStartY;
    private double _dragOriginY;
    private Transitions? _dragTransitions;

    public static readonly StyledProperty<bool> ClickToDismissProperty =
        AvaloniaProperty.Register<ScrollableDialogDragBehavior, bool>(nameof(ClickToDismiss));

    public bool ClickToDismiss
    {
        get => GetValue(ClickToDismissProperty);
        set => SetValue(ClickToDismissProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<ScrollableDialogDragBehavior, TimeSpan>(nameof(AnimationDuration));

    public static readonly StyledProperty<SheetDragStartMode> DragStartModeProperty =
        AvaloniaProperty.Register<ScrollableDialogDragBehavior, SheetDragStartMode>(
            nameof(DragStartMode), defaultValue: SheetDragStartMode.FullSheet);

    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public SheetDragStartMode DragStartMode
    {
        get => GetValue(DragStartModeProperty);
        set => SetValue(DragStartModeProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is { } grid)
        {
            grid.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, true);
            grid.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, true);
            grid.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, true);
            grid.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Tunnel, true);
        }
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        _sheet = AssociatedObject?.FindAncestorOfType<Sheet>();
        if (_sheet is null)
        {
            throw new InvalidDialogDragBehaviorControl();
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var grid = AssociatedObject;
        if (_sheet is null || grid is null || !_sheet.IsOpen || sender is not Control dragHandle)
        {
            return;
        }

        if (!CanStartDrag(dragHandle, e))
        {
            return;
        }

        if (DragStartMode == SheetDragStartMode.TabBarOnly && !IsWithinTabBar(e.Source))
        {
            return;
        }

        _isDragging = false;
        _hasPendingDrag = true;
        _activePointerId = e.Pointer.Id;
        _dragScrollViewer = FindAncestorScrollViewer(e.Source);
        _scrollWasAtTopOnPointerDown = _dragScrollViewer is null || IsScrolledToTop(_dragScrollViewer);
        _dragStartY = GetPointerY(e, grid);

        if (grid.RenderTransform is TranslateTransform translate)
        {
            _dragOriginY = Math.Max(0, translate.Y);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var grid = AssociatedObject;
        if (_sheet is null || grid is null || sender is not Control dragHandle)
        {
            return;
        }

        if (_activePointerId is not null && _activePointerId != e.Pointer.Id)
        {
            return;
        }

        if (!_isDragging)
        {
            if (!_hasPendingDrag || grid.RenderTransform is not TranslateTransform pendingTranslate)
            {
                return;
            }

            if (!IsPointerStillPressed(dragHandle, e))
            {
                ResetTracking();
                return;
            }

            var dragDelta = GetPointerY(e, grid) - _dragStartY;
            if (dragDelta <= 6)
            {
                return;
            }

            if (!_scrollWasAtTopOnPointerDown)
            {
                return;
            }

            if (_dragScrollViewer is not null && !IsScrolledToTop(_dragScrollViewer))
            {
                return;
            }

            _isDragging = true;
            _hasPendingDrag = false;
            _sheet.BeginDrag();
            _dragOriginY = Math.Max(0, pendingTranslate.Y);
            _dragStartY = GetPointerY(e, grid);
            _dragTransitions = pendingTranslate.Transitions;
            pendingTranslate.Transitions = null;
            e.Pointer.Capture(dragHandle);
            e.Handled = true;
        }

        if (grid.RenderTransform is not TranslateTransform translate)
        {
            return;
        }

        var delta = GetPointerY(e, grid) - _dragStartY;
        var offset = Math.Clamp(_dragOriginY + delta, _sheet.GetMinDetentOffset(), _sheet.GetCloseOffset());
        translate.Y = offset;
        _sheet.SetMaskFromOffset(offset);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Control dragHandle)
        {
            return;
        }

        if (_activePointerId != e.Pointer.Id)
        {
            return;
        }

        if (!_isDragging)
        {
            ResetTracking();
            return;
        }

        FinishDrag(dragHandle, e.Pointer);
        e.Handled = true;
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (sender is not Control dragHandle)
        {
            return;
        }

        if (_isDragging)
        {
            FinishDrag(dragHandle, null);
            return;
        }

        ResetTracking();
    }

    private void FinishDrag(Control dragHandle, IPointer? pointer)
    {
        var grid = AssociatedObject;
        if (_sheet is null || grid is null || grid.RenderTransform is not TranslateTransform translate)
        {
            ResetTracking();
            return;
        }

        _sheet.EndDrag();

        _isDragging = false;
        _hasPendingDrag = false;
        _dragScrollViewer = null;
        _activePointerId = null;
        pointer?.Capture(null);

        translate.Transitions = _dragTransitions ??
            [
                new DoubleTransition
                {
                    Property = TranslateTransform.YProperty,
                    Duration = AnimationDuration,
                    Easing = _sheet.Easing,
                },
            ];

        var maxOffset = _sheet.GetCloseOffset();
        var lowestDetent = _sheet.GetLowestDetentOffset();
        var closeThreshold = Math.Min(maxOffset - 12, lowestDetent + Math.Max(82, maxOffset * 0.16));

        if (translate.Y >= closeThreshold)
        {
            _sheet.Close();
            return;
        }

        var snapped = _sheet.GetNearestDetentOffset(translate.Y);
        _sheet.SetDetentIndex(snapped.index);
        translate.Y = snapped.offset;
        _sheet.SetMaskFromOffset(snapped.offset);
        dragHandle.InvalidateVisual();
    }

    private void ResetTracking()
    {
        _isDragging = false;
        _hasPendingDrag = false;
        _dragScrollViewer = null;
        _activePointerId = null;
    }

    private static bool CanStartDrag(Control dragHandle, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            return e.GetCurrentPoint(dragHandle).Properties.IsLeftButtonPressed;
        }

        return true;
    }

    private static bool IsPointerStillPressed(Control dragHandle, PointerEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            return e.GetCurrentPoint(dragHandle).Properties.IsLeftButtonPressed;
        }

        return true;
    }

    private static bool IsScrolledToTop(ScrollViewer scrollViewer)
    {
        return scrollViewer.Offset.Y <= 0.5;
    }

    private static ScrollViewer? FindAncestorScrollViewer(object? source)
    {
        if (source is not Visual visual)
        {
            return null;
        }

        for (Visual? current = visual; current is not null; current = current.GetVisualParent() as Visual)
        {
            if (current is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }
        }

        return null;
    }

    private static double GetPointerY(PointerEventArgs e, Visual reference)
    {
        var topLevel = TopLevel.GetTopLevel(reference);
        return topLevel is not null ? e.GetPosition(topLevel).Y : e.GetPosition(reference).Y;
    }

    private static bool IsTabBar(Control control)
    {
        return control.Name is "PART_InternalSheetPill" or "PART_ExternalSheetPill";
    }

    private bool IsWithinTabBar(object? source)
    {
        if (source is not Visual visual || AssociatedObject is null)
        {
            return false;
        }

        for (Visual? current = visual; current is not null; current = current.GetVisualParent() as Visual)
        {
            if (current is Control control && IsTabBar(control))
            {
                return true;
            }

            if (ReferenceEquals(current, AssociatedObject))
            {
                break;
            }
        }

        return false;
    }
}
