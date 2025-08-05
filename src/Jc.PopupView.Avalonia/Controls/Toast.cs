using System.Diagnostics;
using Avalonia;
using Avalonia.Animation.Easings;
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
public class Toast : DialogBase
{
    private readonly DispatcherTimer _animationTimer;
    private readonly Stopwatch _animationStopwatch = new();
    private readonly TimeSpan AnimationFramerate = TimeSpan.FromMicroseconds(16);
    
    private Border? _toastPart;
    private Rectangle? _maskPart;
    
    private double _startY;
    private double _endY;
    private bool _isAnimating;

    internal bool DetachOnClose { get; set; }

    public new static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public override bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set
        {
            if (_toastPart?.RenderTransform is not TranslateTransform transform)
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

    public new static readonly StyledProperty<bool> ClickToDismissProperty = AvaloniaProperty.Register<Toast, bool>(
        nameof(ClickToDismiss), defaultValue: true);

    public override bool ClickToDismiss
    {
        get => GetValue(ClickToDismissProperty);
        set => SetValue(ClickToDismissProperty, value);
    }

    public static readonly StyledProperty<ToastLocation> LocationProperty =
        AvaloniaProperty.Register<Toast, ToastLocation>(
            nameof(Location), defaultValue: ToastLocation.Top);

    public ToastLocation Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    public Toast()
    {
        _animationTimer = new DispatcherTimer()
        {
            Interval = AnimationFramerate,
        };
    }

    static Toast()
    {
        IsOpenProperty.Changed.AddClassHandler<Toast>((toast, e) =>
        {
            if (e.NewValue is bool isOpen)
            {
                if (toast._toastPart?.RenderTransform is not TranslateTransform transform)
                    return;

                toast.CalculateAnimationStartEnd(isOpen);
                
                toast.IsOpening = isOpen;
                toast.IsClosing = !isOpen;

                toast._animationStopwatch.Restart();
                toast.UpdatePseudoClasses();
                toast._animationTimer.Start();
            }
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();

        _toastPart = e.NameScope.Find<Border>("PART_ToastContent");
        _maskPart = e.NameScope.Find<Rectangle>("PART_ToastMask");
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
        if (_toastPart?.RenderTransform is TranslateTransform translateTransform)
        {
            _toastPart.RenderTransform = translateTransform;
            translateTransform.Y = Location switch
            {
                ToastLocation.Top => -_toastPart.GetTransformedBounds()?.Bounds.Height ?? 0,
                ToastLocation.Bottom => _toastPart.GetTransformedBounds()?.Bounds.Height ?? 0,
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
        if (_toastPart?.RenderTransform is not TranslateTransform transform)
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
                    host.Toasts.Remove(this);
                }
            }

            UpdatePseudoClasses();
        }
    }
    
    private void CalculateAnimationStartEnd(bool opening)
    {
        if (_toastPart?.RenderTransform is not TranslateTransform transform)
            return;

        var height = _toastPart.GetTransformedBounds()?.Bounds.Height ?? 0;

        if (Location == ToastLocation.Top)
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
    
    private static double EaseOutCubic(double t)
    {
        return 1 - Math.Pow(1 - t, 3);
    }
}