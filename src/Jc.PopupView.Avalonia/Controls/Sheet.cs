using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
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

    private double _startY;
    private double _endY;
    private bool _isAnimating;

    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty = AvaloniaProperty.Register<Sheet, TimeSpan>(
        nameof(AnimationDuration), defaultValue: TimeSpan.FromMilliseconds(500));

    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
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

            var sheetHeight = _sheetPart.GetTransformedBounds()?.Bounds.Height ?? 0;
            _startY = transform.Y;
            _endY = value ? 0 : sheetHeight;
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

                var sheetHeight = sheet._sheetPart.GetTransformedBounds()?.Bounds.Height ?? 0;
                sheet._startY = transform.Y;
                sheet._endY = isOpen ? 0 : sheetHeight;
                sheet._isAnimating = isOpen;

                sheet.IsOpening = isOpen;
                sheet.IsClosing = !isOpen;

                sheet._animationStopwatch.Restart();
                sheet.UpdatePseudoClasses();
                sheet._animationTimer.Start();
            }
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();

        _sheetPart = e.NameScope.Find<Grid>("PART_Sheet");
        _maskPart = e.NameScope.Find<Rectangle>("PART_SheetMask");

        _maskPart?.AddHandler(PointerPressedEvent, (_, _) =>
        {
            if (ClickOutsideToDismiss)
            {
                IsOpen = false;
            }
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _animationTimer.Tick += AnimateFrame;

        if (_sheetPart?.RenderTransform is TranslateTransform transform)
        {
            transform.Y = _sheetPart.GetTransformedBounds()?.Bounds.Height ?? 0;
        }
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
                    var host = DialogHost.GetDialogHost();
                    host.Sheets.Remove(this);
                }
            }

            UpdatePseudoClasses();
        }
    }
}