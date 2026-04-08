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
public sealed class Floater : DialogBase
{
    private readonly DispatcherTimer _animationTimer;
    private readonly Stopwatch _animationStopwatch = new();
    private readonly TimeSpan AnimationFramerate = TimeSpan.FromMilliseconds(16);

    private Border? _floaterPart;
    private Rectangle? _maskPart;

    private double _startY;
    private double _endY;
    private bool _isAnimating;

    internal bool DetachOnClose { get; set; }

    public new static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Floater, bool>(
        nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public override bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set
        {
            if (_floaterPart?.RenderTransform is not TranslateTransform transform)
                return;

            CalculateAnimationStartEnd(value);

            IsOpening = value;
            IsClosing = !value;

            _animationStopwatch.Restart();
            UpdatePseudoClasses();
            _animationTimer.Start();
            SetValue(IsOpenProperty, value);
        }
    }

    public new static readonly StyledProperty<bool> ShowBackgroundMaskProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(ShowBackgroundMask), defaultValue: false);

    public override bool ShowBackgroundMask
    {
        get => GetValue(ShowBackgroundMaskProperty);
        set => SetValue(ShowBackgroundMaskProperty, value);
    }

    public static readonly StyledProperty<bool> ClickToDismissProperty = AvaloniaProperty.Register<Floater, bool>(
        nameof(ClickToDismiss), defaultValue: true);

    public override bool ClickToDismiss
    {
        get => GetValue(ClickToDismissProperty);
        set => SetValue(ClickToDismissProperty, value);
    }

    public static readonly StyledProperty<FloaterLocation> LocationProperty =
        AvaloniaProperty.Register<Floater, FloaterLocation>(
            nameof(Location), defaultValue: FloaterLocation.Top);

    public FloaterLocation Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    public static readonly StyledProperty<double> ShadowOffsetXProperty = AvaloniaProperty.Register<Floater, double>(
        nameof(ShadowOffsetX));

    public double ShadowOffsetX
    {
        get => GetValue(ShadowOffsetXProperty);
        set => SetValue(ShadowOffsetXProperty, value);
    }

    public static readonly StyledProperty<double> ShadowOffsetYProperty = AvaloniaProperty.Register<Floater, double>(
        nameof(ShadowOffsetY));

    public double ShadowOffsetY
    {
        get => GetValue(ShadowOffsetYProperty);
        set => SetValue(ShadowOffsetYProperty, value);
    }

    public static readonly StyledProperty<Color> ShadowColorProperty = AvaloniaProperty.Register<Floater, Color>(
        nameof(ShadowColor));

    public Color ShadowColor
    {
        get => GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }

    public Floater()
    {
        _animationTimer = new DispatcherTimer()
        {
            Interval = AnimationFramerate,
        };
    }

    static Floater()
    {
        IsOpenProperty.Changed.AddClassHandler<Floater>((floater, e) =>
        {
            if (e.NewValue is bool isOpen)
            {
                if (floater._floaterPart?.RenderTransform is not TranslateTransform transform)
                    return;

                floater.CalculateAnimationStartEnd(isOpen);

                floater.IsOpening = isOpen;
                floater.IsClosing = !isOpen;

                floater._animationStopwatch.Restart();
                floater.UpdatePseudoClasses();
                floater._animationTimer.Start();
            }
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();

        _floaterPart = e.NameScope.Find<Border>("PART_FloaterContent");
        _maskPart = e.NameScope.Find<Rectangle>("PART_FloaterMask");
        _maskPart?.AddHandler(PointerPressedEvent, (_, _) =>
        {
            // TODO fix this when mask is not visible
            if (ClickOutsideToDismiss)
            {
                IsOpen = false;
            }
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        _animationTimer.Tick += AnimateFrame;
        if (_floaterPart?.RenderTransform is TranslateTransform translateTransform)
        {
            _floaterPart.RenderTransform = translateTransform;
            translateTransform.Y = Location switch
            {
                FloaterLocation.Top => -_floaterPart.GetTransformedBounds()?.Bounds.Height ?? 0,
                FloaterLocation.Bottom => _floaterPart.GetTransformedBounds()?.Bounds.Height ?? 0,
                _ => 0
            };
        }

        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _animationTimer.Tick -= AnimateFrame;
        _animationStopwatch.Reset();
    }

    private void AnimateFrame(object? sender, EventArgs e)
    {
        if (_floaterPart?.RenderTransform is not TranslateTransform transform)
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
                    host.Floaters.Remove(this);
                }
            }

            UpdatePseudoClasses();
        }
    }

    private void CalculateAnimationStartEnd(bool opening)
    {
        if (_floaterPart?.RenderTransform is not TranslateTransform transform)
            return;

        var height = _floaterPart.GetTransformedBounds()?.Bounds.Height ?? 0;

        if (Location == FloaterLocation.Top)
        {
            _startY = opening ? -height : 0;
            _endY = opening ? 0 : -height;
        }
        else // Bottom
        {
            _startY = opening ? height : 0;
            _endY = opening ? 0 : height;
        }

        _isAnimating = opening;
    }
}