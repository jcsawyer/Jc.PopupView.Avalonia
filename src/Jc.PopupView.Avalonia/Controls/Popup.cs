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

namespace Jc.PopupView.Avalonia.Controls;

[PseudoClasses(":open", ":opening", ":closed", ":closing")]
public sealed class Popup : DialogBase
{
    private readonly DispatcherTimer _animationTimer;
    private readonly Stopwatch _animationStopwatch = new();
    private static readonly TimeSpan AnimationFramerate = TimeSpan.FromMilliseconds(16);

    private Border? _popupPart;
    private Rectangle? _maskPart;

    private double _startY;
    private double _endY;
    private double _startScale;
    private double _endScale;
    private double _startOpacity;
    private double _endOpacity;
    private double _startMaskOpacity;
    private double _endMaskOpacity;
    private bool _isAnimating;

    internal bool DetachOnClose { get; set; }

    public new static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Popup, bool>(
        nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public override bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set
        {
            if (_popupPart is null)
            {
                return;
            }

            PrepareAnimation(value);

            IsOpening = value;
            IsClosing = !value;

            _animationStopwatch.Restart();
            UpdatePseudoClasses();

            if (GetEffectiveAnimationType(value) is PopupAnimationType.None)
            {
                CompleteImmediate(value);
            }
            else
            {
                _animationTimer.Start();
            }

            SetValue(IsOpenProperty, value);
        }
    }

    public static readonly StyledProperty<PopupPosition> PositionProperty =
        AvaloniaProperty.Register<Popup, PopupPosition>(nameof(Position), defaultValue: PopupPosition.Center);

    public PopupPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<Popup, Thickness>(nameof(ContentMargin), defaultValue: new Thickness(18));

    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    public static readonly StyledProperty<PopupAnimationType?> OpenAnimationTypeProperty =
        AvaloniaProperty.Register<Popup, PopupAnimationType?>(nameof(OpenAnimationType));

    public PopupAnimationType? OpenAnimationType
    {
        get => GetValue(OpenAnimationTypeProperty);
        set => SetValue(OpenAnimationTypeProperty, value);
    }

    public static readonly StyledProperty<PopupAnimationType?> CloseAnimationTypeProperty =
        AvaloniaProperty.Register<Popup, PopupAnimationType?>(nameof(CloseAnimationType));

    public PopupAnimationType? CloseAnimationType
    {
        get => GetValue(CloseAnimationTypeProperty);
        set => SetValue(CloseAnimationTypeProperty, value);
    }

    public Popup()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = AnimationFramerate,
        };
    }

    static Popup()
    {
        IsOpenProperty.Changed.AddClassHandler<Popup>((popup, e) =>
        {
            if (popup._popupPart is null || e.NewValue is not bool isOpen)
            {
                return;
            }

            popup.PrepareAnimation(isOpen);
            popup.IsOpening = isOpen;
            popup.IsClosing = !isOpen;

            popup._animationStopwatch.Restart();
            popup.UpdatePseudoClasses();

            if (popup.GetEffectiveAnimationType(isOpen) is PopupAnimationType.None)
            {
                popup.CompleteImmediate(isOpen);
            }
            else
            {
                popup._animationTimer.Start();
            }
        });

        PositionProperty.Changed.AddClassHandler<Popup>((popup, _) =>
        {
            if (popup._popupPart is null)
            {
                return;
            }

            popup.ApplyClosedState();
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();

        _popupPart = e.NameScope.Find<Border>("PART_PopupContent");
        _maskPart = e.NameScope.Find<Rectangle>("PART_PopupMask");

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
        ApplyClosedState();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _animationTimer.Tick -= AnimateFrame;
        _animationStopwatch.Reset();
    }

    private void AnimateFrame(object? sender, EventArgs e)
    {
        if (_popupPart is null)
        {
            _animationTimer.Stop();
            _animationStopwatch.Stop();
            return;
        }

        var progress = _animationStopwatch.Elapsed.TotalMilliseconds / AnimationDuration.TotalMilliseconds;
        progress = Math.Clamp(progress, 0, 1);
        var easedProgress = Easing.Ease(progress);

        ApplyFrame(easedProgress);

        if (progress < 1)
        {
            return;
        }

        _animationTimer.Stop();
        _animationStopwatch.Stop();
        ApplyFrame(1);

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
                host.Popups.Remove(this);
            }
        }

        UpdatePseudoClasses();
    }

    private void PrepareAnimation(bool opening)
    {
        if (_popupPart is null)
        {
            return;
        }

        var type = GetEffectiveAnimationType(opening);
        var closedY = Position == PopupPosition.Bottom ? 40 : 0;
        const double scaledClosedValue = 0.88;

        _isAnimating = opening;

        _startY = opening ? closedY : 0;
        _endY = opening ? 0 : closedY;
        _startScale = opening ? scaledClosedValue : 1;
        _endScale = opening ? 1 : scaledClosedValue;
        _startOpacity = opening ? 0 : 1;
        _endOpacity = opening ? 1 : 0;
        _startMaskOpacity = opening ? 0 : 1;
        _endMaskOpacity = opening ? 1 : 0;

        switch (type)
        {
            case PopupAnimationType.Slide:
                _startScale = 1;
                _endScale = 1;
                break;
            case PopupAnimationType.Scale:
                _startY = 0;
                _endY = 0;
                break;
            case PopupAnimationType.Fade:
                _startY = 0;
                _endY = 0;
                _startScale = 1;
                _endScale = 1;
                break;
            case PopupAnimationType.None:
                break;
        }
    }

    private void CompleteImmediate(bool opening)
    {
        if (_popupPart is null)
        {
            return;
        }

        ApplyFrame(1);
        IsOpening = false;
        IsClosing = false;

        if (!opening && DetachOnClose)
        {
            var host = DialogHostRegistry.GetActiveHost();
            host.Popups.Remove(this);
        }

        UpdatePseudoClasses();
    }

    private void ApplyFrame(double easedProgress)
    {
        if (_popupPart is null)
        {
            return;
        }

        if (_popupPart.RenderTransform is not TransformGroup group || group.Children.Count < 2)
        {
            return;
        }

        if (group.Children[0] is not ScaleTransform scaleTransform ||
            group.Children[1] is not TranslateTransform translateTransform)
        {
            return;
        }

        translateTransform.Y = _startY + (_endY - _startY) * easedProgress;
        var scale = _startScale + (_endScale - _startScale) * easedProgress;
        scaleTransform.ScaleX = scale;
        scaleTransform.ScaleY = scale;
        _popupPart.Opacity = _startOpacity + (_endOpacity - _startOpacity) * easedProgress;
        if (_maskPart is not null)
        {
            _maskPart.Opacity = _startMaskOpacity + (_endMaskOpacity - _startMaskOpacity) * easedProgress;
        }
    }

    private void ApplyClosedState()
    {
        if (_popupPart?.RenderTransform is not TransformGroup group || group.Children.Count < 2)
        {
            return;
        }

        if (group.Children[0] is not ScaleTransform scaleTransform ||
            group.Children[1] is not TranslateTransform translateTransform)
        {
            return;
        }

        if (Position == PopupPosition.Bottom)
        {
            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY = 1;
            translateTransform.Y = IsOpen ? 0 : 40;
            _popupPart.Opacity = IsOpen ? 1 : 0;
            if (_maskPart is not null)
            {
                _maskPart.Opacity = IsOpen ? 1 : 0;
            }
            return;
        }

        scaleTransform.ScaleX = IsOpen ? 1 : 0.92;
        scaleTransform.ScaleY = IsOpen ? 1 : 0.92;
        translateTransform.Y = 0;
        _popupPart.Opacity = IsOpen ? 1 : 0;
        if (_maskPart is not null)
        {
            _maskPart.Opacity = IsOpen ? 1 : 0;
        }
    }

    private PopupAnimationType GetEffectiveAnimationType(bool opening)
    {
        var configured = opening ? OpenAnimationType : CloseAnimationType;
        if (configured.HasValue)
        {
            return configured.Value;
        }

        return Position == PopupPosition.Bottom ? PopupAnimationType.Slide : PopupAnimationType.Scale;
    }
}
