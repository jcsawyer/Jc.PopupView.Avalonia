using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Jc.PopupView.Avalonia.Behaviors;

namespace Jc.PopupView.Avalonia.Controls;

[PseudoClasses(":open", ":closed")]
public sealed class Sheet : TemplatedControl, IDialog
{
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<Sheet, TimeSpan>(
            nameof(AnimationDuration) , defaultValue: TimeSpan.FromSeconds(0.5));

    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(IsOpen), coerce: CoerceIsOpen);

    private static bool CoerceIsOpen(AvaloniaObject sheetObject, bool value)
    {
        if (sheetObject is Sheet sheet &&
            sheet.GetVisualDescendants().OfType<Grid>().FirstOrDefault(x => x.Name == "PART_Sheet") is { } sheetContent)
        {
            if (value)
            {
                ((IDialog)sheet).IsOpening = true;
                ((IDialog)sheet).IsClosing = false;
                if (Interaction.GetBehaviors(sheetContent).OfType<DialogDragBehavior>().FirstOrDefault() is
                    { } dragBehavior)
                {
                    dragBehavior.AnimateIn();
                }

                sheet.UpdatePseudoClasses();
            }
            else
            {
                ((IDialog)sheet).IsOpening = false;
                ((IDialog)sheet).IsClosing = true;
                if (Interaction.GetBehaviors(sheetContent).OfType<DialogDragBehavior>().FirstOrDefault() is
                    { } dragBehavior)
                {
                    dragBehavior.AnimateOut();
                }

                sheet.UpdatePseudoClasses();
            }
        }

        return value;
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    bool IDialog.IsOpening { get; set; }

    bool IDialog.IsClosing { get; set; }

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

    public static readonly StyledProperty<IBrush> MaskColorProperty = AvaloniaProperty.Register<Sheet, IBrush>(
        nameof(MaskColor));

    public IBrush MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }

    public static readonly StyledProperty<bool> CloseOnClickOutsideProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(CloseOnClickOutside));

    public bool CloseOnClickOutside
    {
        get => GetValue(CloseOnClickOutsideProperty);
        set => SetValue(CloseOnClickOutsideProperty, value);
    }

    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<Sheet, object?>(
        nameof(Content));

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UpdatePseudoClasses();
        base.OnAttachedToVisualTree(e);
    }

    public void UpdatePseudoClasses()
    {
        var opening = ((IDialog)this).IsOpening;
        var closing = ((IDialog)this).IsClosing;
        PseudoClasses.Set(":opening", opening);
        PseudoClasses.Set(":closing", closing);

        if (!opening && !closing)
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
}