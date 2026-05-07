using System.Diagnostics;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Animation;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Jc.PopupView.Avalonia.Controls;

[PseudoClasses(":open", ":opening", ":closed", ":closing")]
public class Sheet : DialogBase
{
    private readonly DispatcherTimer _animationTimer;
    private readonly Stopwatch _animationStopwatch = new();
    private static readonly TimeSpan AnimationFramerate = TimeSpan.FromMilliseconds(16); // ~60fps

    private Grid? _sheetPart;
    private Rectangle? _maskPart;

    private double[] _detentOffsets = [0];
    private int _currentDetentIndex;
    private double _closeOffset = 120;
    private double _dragMaskStartOffset;
    private Transitions? _maskTransitionsBeforeDrag;

    private double _startY;
    private double _endY;
    private bool _isAnimating;

    public new static readonly StyledProperty<TimeSpan> AnimationDurationProperty = AvaloniaProperty.Register<Sheet, TimeSpan>(
        nameof(AnimationDuration), defaultValue: TimeSpan.FromMilliseconds(500));

    public new TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<double>?> DetentsProperty =
        AvaloniaProperty.Register<Sheet, AvaloniaList<double>?>(nameof(Detents));

    public AvaloniaList<double>? Detents
    {
        get => GetValue(DetentsProperty);
        set => SetValue(DetentsProperty, value);
    }

    public static readonly StyledProperty<double?> InitialDetentProperty =
        AvaloniaProperty.Register<Sheet, double?>(nameof(InitialDetent));

    public double? InitialDetent
    {
        get => GetValue(InitialDetentProperty);
        set => SetValue(InitialDetentProperty, value);
    }

    public static readonly StyledProperty<double?> SnapPointProperty =
        AvaloniaProperty.Register<Sheet, double?>(nameof(SnapPoint));

    public double? SnapPoint
    {
        get => GetValue(SnapPointProperty);
        set => SetValue(SnapPointProperty, value);
    }

    internal bool DetachOnClose { get; set; }

    public override bool ClickToDismiss
    {
        get => false;
        set => throw new InvalidOperationException($"Cannot set close on click for Sheet. Use {nameof(ClickOutsideToDismiss)} instead.");
    }

    public new static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public override bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set
        {
            if (_sheetPart?.RenderTransform is not TranslateTransform transform)
                return;

            var sheetHeight = GetSheetHeight();
            _startY = transform.Y;
            _endY = value ? GetCurrentDetentOffset() : GetCloseOffset();
            _isAnimating = value;

            IsOpening = value;
            IsClosing = !value;

            _animationStopwatch.Restart();
            UpdatePseudoClasses();
            _animationTimer.Start();
            SetValue(IsOpenProperty, value);
        }
    }

    public static readonly StyledProperty<SheetPillLocation> PillLocationProperty =
        AvaloniaProperty.Register<Sheet, SheetPillLocation>(
            nameof(PillLocation), defaultValue: SheetPillLocation.Internal);

    public SheetPillLocation PillLocation
    {
        get => GetValue(PillLocationProperty);
        set => SetValue(PillLocationProperty, value);
    }

    public static readonly StyledProperty<IBrush> PillColorProperty = AvaloniaProperty.Register<Sheet, IBrush>(
        nameof(PillColor));

    public static readonly StyledProperty<SheetScrollDirection> ScrollDirectionProperty = AvaloniaProperty.Register<Sheet, SheetScrollDirection>(
        nameof(ScrollDirection), defaultValue: SheetScrollDirection.Vertical);

    public static readonly StyledProperty<SheetDragStartMode> DragStartModeProperty =
        AvaloniaProperty.Register<Sheet, SheetDragStartMode>(
            nameof(DragStartMode), defaultValue: SheetDragStartMode.FullSheet);

    public SheetScrollDirection ScrollDirection
    {
        get => GetValue(ScrollDirectionProperty);
        set => SetValue(ScrollDirectionProperty, value);
    }

    public SheetDragStartMode DragStartMode
    {
        get => GetValue(DragStartModeProperty);
        set => SetValue(DragStartModeProperty, value);
    }

    public IBrush PillColor
    {
        get => GetValue(PillColorProperty);
        set => SetValue(PillColorProperty, value);
    }

    public Sheet()
    {
        _animationTimer = new DispatcherTimer()
        {
            Interval = AnimationFramerate,
        };
    }
    
    static Sheet()
    {
        IsOpenProperty.Changed.AddClassHandler<Sheet>((sheet, e) =>
        {
            if (e.NewValue is bool isOpen)
            {
                if (sheet._sheetPart?.RenderTransform is not TranslateTransform transform)
                    return;

                var sheetHeight = sheet.GetSheetHeight();
                sheet._startY = transform.Y;
                sheet._endY = isOpen ? sheet.GetCurrentDetentOffset() : sheet.GetCloseOffset();
                sheet._isAnimating = isOpen;

                sheet.IsOpening = isOpen;
                sheet.IsClosing = !isOpen;

                sheet._animationStopwatch.Restart();
                sheet.UpdatePseudoClasses();
                sheet._animationTimer.Start();
            }
        });

        DetentsProperty.Changed.AddClassHandler<Sheet>((sheet, _) => sheet.RecalculateDetents());
        InitialDetentProperty.Changed.AddClassHandler<Sheet>((sheet, _) => sheet.RecalculateDetents());
        SnapPointProperty.Changed.AddClassHandler<Sheet>((sheet, _) => sheet.RecalculateDetents());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();

        _sheetPart = e.NameScope.Find<Grid>("PART_Sheet");
        _maskPart = e.NameScope.Find<Rectangle>("PART_SheetMask");

        _maskPart?.AddHandler(PointerPressedEvent, (_, args) =>
        {
            if (ClickOutsideToDismiss)
            {
                IsOpen = false;
                args.Handled = true;
            }
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _animationTimer.Tick += AnimateFrame;
        RecalculateDetents();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _animationTimer.Tick -= AnimateFrame;
        _animationStopwatch.Reset();
    }

    private void AnimateFrame(object? sender, EventArgs e)
    {
        if (_sheetPart?.RenderTransform is not TranslateTransform transform)
        {
            _animationTimer.Stop();
            _animationStopwatch.Stop();
            return;
        }

        var progress = _animationStopwatch.Elapsed.TotalMilliseconds / AnimationDuration.TotalMilliseconds;
        progress = Math.Clamp(progress, 0, 1);
        var easedProgress = Easing.Ease(progress);

        transform.Y = _startY + (_endY - _startY) * easedProgress;

        if (progress >= 1)
        {
            _animationTimer.Stop();
            _animationStopwatch.Stop();

            transform.Y = _endY;

            if (_isAnimating)
            {
                IsOpening = false;
            }
            else
            {
                IsClosing = false;

                if (DetachOnClose)
                {
                    var host = DialogHostRegistry.GetActiveHost();
                    host.Sheets.Remove(this);
                }
            }

            UpdatePseudoClasses();
        }
    }

    internal double GetMinDetentOffset() => _detentOffsets.Length == 0 ? 0 : _detentOffsets[0];

    internal double GetLowestDetentOffset() => _detentOffsets.Length == 0 ? 0 : _detentOffsets[^1];

    internal double GetCloseOffset() => _closeOffset;

    internal void SetDetentIndex(int index)
    {
        _currentDetentIndex = Math.Clamp(index, 0, Math.Max(0, _detentOffsets.Length - 1));
    }

    internal (int index, double offset) GetNearestDetentOffset(double y)
    {
        if (_detentOffsets.Length == 0)
        {
            return (0, 0);
        }

        var nearestIndex = 0;
        var nearestDistance = Math.Abs(_detentOffsets[0] - y);
        for (var i = 1; i < _detentOffsets.Length; i++)
        {
            var distance = Math.Abs(_detentOffsets[i] - y);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return (nearestIndex, _detentOffsets[nearestIndex]);
    }

    internal void SetMaskFromOffset(double offset)
    {
        if (_maskPart is not null)
        {
            var startOffset = Math.Clamp(_dragMaskStartOffset, 0, Math.Max(0, _closeOffset - 1));
            var range = Math.Max(1, _closeOffset - startOffset);
            var normalized = Math.Clamp((offset - startOffset) / range, 0, 1);
            _maskPart.Opacity = 1 - normalized;
        }
    }

    internal void BeginDrag()
    {
        if (_animationTimer.IsEnabled)
        {
            _animationTimer.Stop();
            _animationStopwatch.Stop();
        }

        if (_sheetPart?.RenderTransform is TranslateTransform transform)
        {
            _dragMaskStartOffset = Math.Max(0, transform.Y);
        }
        else
        {
            _dragMaskStartOffset = GetCurrentDetentOffset();
        }

        if (_maskPart is not null)
        {
            _maskTransitionsBeforeDrag = _maskPart.Transitions;
            _maskPart.Transitions = null;
        }

        IsOpening = false;
        IsClosing = false;
        UpdatePseudoClasses();
    }

    internal void EndDrag()
    {
        if (_maskPart is not null)
        {
            _maskPart.Transitions = _maskTransitionsBeforeDrag;
        }
    }

    private double GetCurrentDetentOffset()
    {
        if (_detentOffsets.Length == 0)
        {
            return 0;
        }

        return _detentOffsets[Math.Clamp(_currentDetentIndex, 0, _detentOffsets.Length - 1)];
    }

    private double GetSheetHeight()
    {
        var height = _sheetPart?.Bounds.Height ?? 0;
        return height > 0 ? height : _sheetPart?.GetTransformedBounds()?.Bounds.Height ?? 0;
    }

    private void RecalculateDetents()
    {
        if (_sheetPart is null)
        {
            return;
        }

        var hostHeight = (VisualRoot as TopLevel)?.ClientSize.Height ?? Bounds.Height;
        if (hostHeight < 1)
        {
            hostHeight = 800;
        }

        hostHeight = Math.Max(240, hostHeight);
        var configuredMaxHeight = ResolveHeight(SnapPoint, hostHeight, hostHeight * 0.82);

        var detentHeights = new List<double>();
        if (Detents is { Count: > 0 })
        {
            foreach (var detent in Detents)
            {
                var detentHeight = ResolveHeight(detent, hostHeight, configuredMaxHeight);
                detentHeights.Add(Math.Clamp(detentHeight, 120, hostHeight));
            }
        }

        if (detentHeights.Count == 0)
        {
            detentHeights.Add(Math.Clamp(configuredMaxHeight, 120, hostHeight));
        }

        detentHeights = detentHeights.Distinct().OrderByDescending(h => h).ToList();
        var maxHeight = Math.Clamp(Math.Max(configuredMaxHeight, detentHeights.Max()), 120, hostHeight);

        _detentOffsets = detentHeights
            .Select(height => Math.Max(0, maxHeight - Math.Min(height, maxHeight)))
            .Distinct()
            .OrderBy(offset => offset)
            .ToArray();

        var initialOffset = InitialDetent is double initial
            ? Math.Max(0, maxHeight - Math.Min(ResolveHeight(initial, hostHeight, detentHeights[0]), maxHeight))
            : _detentOffsets[0];

        var initialDetentIndex = 0;
        var smallestDistance = double.MaxValue;
        for (var i = 0; i < _detentOffsets.Length; i++)
        {
            var distance = Math.Abs(_detentOffsets[i] - initialOffset);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                initialDetentIndex = i;
            }
        }

        _currentDetentIndex = initialDetentIndex;
        _closeOffset = maxHeight + 36;

        _sheetPart.MaxHeight = maxHeight;
        _sheetPart.Height = maxHeight;

        if (_sheetPart.RenderTransform is TranslateTransform transform)
        {
            transform.Y = IsOpen ? GetCurrentDetentOffset() : _closeOffset;
        }
    }

    private static double ResolveHeight(double? value, double hostHeight, double fallback)
    {
        if (!value.HasValue || value.Value <= 0)
        {
            return fallback;
        }

        return value.Value <= 1 ? hostHeight * value.Value : value.Value;
    }
}
