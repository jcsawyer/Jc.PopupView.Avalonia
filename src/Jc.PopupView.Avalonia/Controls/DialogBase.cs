using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Jc.PopupView.Avalonia.Controls;

public abstract class DialogBase : TemplatedControl, IDialog
{
    protected bool IsOpening { get; set; }
    protected bool IsClosing { get; set; }

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<DialogBase, bool>(
        nameof(IsOpen));

    public virtual bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<Sheet, TimeSpan>(
            nameof(AnimationDuration), defaultValue: TimeSpan.FromMilliseconds(200));

    public virtual TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }
    
    public static readonly StyledProperty<Easing> EasingProperty = AvaloniaProperty.Register<Toast, Easing>(
        nameof(Easing), defaultValue: new CubicEaseOut());

    public Easing Easing
    {
        get => GetValue(EasingProperty);
        set => SetValue(EasingProperty, value);
    }
    
    public static readonly StyledProperty<bool> CloseOnClickOutsideProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(ClickOutsideToDismiss));

    public virtual bool ClickOutsideToDismiss
    {
        get => GetValue(CloseOnClickOutsideProperty);
        set => SetValue(CloseOnClickOutsideProperty, value);
    }

    public static readonly StyledProperty<bool> CloseOnClickProperty = AvaloniaProperty.Register<DialogBase, bool>(
        nameof(ClickToDismiss));

    public virtual bool ClickToDismiss
    {
        get => GetValue(CloseOnClickProperty);
        set => SetValue(CloseOnClickProperty, value);
    }
    
    public static readonly StyledProperty<bool> ShowBackgroundMaskProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(ShowBackgroundMask), defaultValue: true);

    public virtual bool ShowBackgroundMask
    {
        get => GetValue(ShowBackgroundMaskProperty);
        set => SetValue(ShowBackgroundMaskProperty, value);
    }
    
    public static readonly StyledProperty<IBrush> MaskColorProperty = AvaloniaProperty.Register<Sheet, IBrush>(
        nameof(MaskColor));

    public virtual IBrush MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }
    
    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<Sheet, object?>(
        nameof(Content));
    
    [Content]
    public virtual object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":opening", IsOpening);
        PseudoClasses.Set(":closing", IsClosing);

        if (!IsOpening && !IsClosing)
        {
            PseudoClasses.Set(":open", IsOpen);
            PseudoClasses.Set(":closed", !IsOpen);
        }
        else
        {
            PseudoClasses.Set(":open", false);
            PseudoClasses.Set(":closed", false);
        }
    }

    public virtual void Close()
    {
        if (IsOpen || IsOpening)
        {
            IsOpen = false;
        }
        else
        {
            IsClosing = false;
            IsOpening = false;
            UpdatePseudoClasses();
        }
    }
}