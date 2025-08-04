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
public class Toast : DialogBase
{
    private readonly DispatcherTimer _animationTimer;
    private readonly TimeSpan AnimationFramerate = TimeSpan.FromMicroseconds(16);
    private Border? _toastPart;
    private Rectangle? _maskPart;

    private int AnimationTotalTicks => (int)(AnimationDuration.TotalSeconds / AnimationFramerate.TotalSeconds);


    internal bool DetachOnClose { get; set; }

    public new static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public override bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set
        {
            if (value)
            {
                if (IsOpening)
                {
                    return;
                }

                IsOpening = true;
                IsClosing = false;
            }
            else
            {
                if (IsClosing)
                {
                    return;
                }

                IsOpening = false;
                IsClosing = true;
            }

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
                if (isOpen)
                {
                    if (toast.IsOpening)
                    {
                        return;
                    }

                    toast.IsOpening = true;
                    toast.IsClosing = false;
                }
                else
                {
                    if (toast.IsClosing)
                    {
                        return;
                    }

                    toast.IsOpening = false;
                    toast.IsClosing = true;
                }

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
        var test = _toastPart.GetTransformedBounds()?.Bounds.Height ?? 0;
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
        _animationTimer.Tick -= AnimateFrame;
        base.OnUnloaded(e);
    }

    private void AnimateFrame(object? sender, EventArgs e)
    {
        if (_toastPart?.RenderTransform is not TranslateTransform)
        {
            _animationTimer.Stop();
            return;
        }

        var toastHeight = _toastPart.GetTransformedBounds()?.Bounds.Height ?? 0;
        if (IsOpen)
        {
            if (_toastPart.RenderTransform is not TranslateTransform transform)
            {
                _animationTimer.Stop();
                return;
            }

            if (Location == ToastLocation.Bottom)
            {
                if (transform.Y > 0)
                {
                    transform.Y -= toastHeight / AnimationTotalTicks;
                    if (transform.Y <= 0)
                    {
                        transform.Y = 0;
                        _animationTimer.Stop();
                        if (IsOpening)
                        {
                            //DialogManager.OnToastOpened?.Invoke(this, EventArgs.Empty);
                            IsOpening = false;
                            UpdatePseudoClasses();
                        }
                    }
                }
                else
                {
                    _animationTimer.Stop();
                }
            }
            else if (Location == ToastLocation.Top)
            {
                if (transform.Y < 0)
                {
                    transform.Y += toastHeight / AnimationTotalTicks;
                    if (transform.Y >= 0)
                    {
                        transform.Y = 0;
                        _animationTimer.Stop();
                        if (IsOpening)
                        {
                            //DialogManager.OnToastOpened?.Invoke(this, EventArgs.Empty);
                            IsOpening = false;
                            UpdatePseudoClasses();
                        }
                    }
                }
                else
                {
                    _animationTimer.Stop();
                }
            }
            else
            {
                _animationTimer.Stop();
            }
        }
        else if (!IsOpen)
        {
            if (_toastPart.RenderTransform is not TranslateTransform transform)
            {
                _animationTimer.Stop();
                return;
            }

            if (Location == ToastLocation.Bottom)
            {
                if (transform.Y < toastHeight)
                {
                    transform.Y += toastHeight / AnimationTotalTicks;
                    if (transform.Y > toastHeight)
                    {
                        transform.Y = toastHeight;
                        _animationTimer.Stop();
                        if (IsClosing)
                        {
                            //DialogManager.OnToastClosed?.Invoke(this, EventArgs.Empty);
                            IsClosing = false;
                            UpdatePseudoClasses();
                            if (DetachOnClose)
                            {
                                var host = DialogHost.GetDialogHost();
                                host.Toasts.Remove(this);
                            }
                        }
                    }
                }
                else
                {
                    _animationTimer.Stop();
                }
            }
            else if (Location == ToastLocation.Top)
            {
                if (transform.Y > -toastHeight)
                {
                    transform.Y -= toastHeight / AnimationTotalTicks;
                    if (transform.Y < -toastHeight)
                    {
                        transform.Y = -toastHeight;
                        _animationTimer.Stop();
                        if (IsClosing)
                        {
                            //DialogManager.OnToastClosed?.Invoke(this, EventArgs.Empty);
                            IsClosing = false;
                            UpdatePseudoClasses();
                            if (DetachOnClose)
                            {
                                var host = DialogHost.GetDialogHost();
                                host.Toasts.Remove(this);
                            }
                        }
                    }
                }
                else
                {
                    _animationTimer.Stop();
                }
            }
            else
            {
                _animationTimer.Stop();
            }
        }
    }
}