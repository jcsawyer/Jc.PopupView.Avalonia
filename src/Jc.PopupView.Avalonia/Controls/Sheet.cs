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
    private static readonly TimeSpan AnimationFramerate = TimeSpan.FromMicroseconds(16);
    private Grid? _sheetPart;
    private Rectangle? _maskPart;

    internal bool DetachOnClose { get; set;  }

    public override bool ClickToDismiss
    {
        get => false;
        set => throw new InvalidOperationException($"Cannot set close on click for Sheet. Use {nameof(ClickOutsideToDismiss)} instead.");
    }

    private int AnimationTotalTicks => (int)(AnimationDuration.TotalSeconds / AnimationFramerate.TotalSeconds);

    
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
                if (isOpen)
                {
                    if (sheet.IsOpening)
                    {
                        return;
                    }
                    sheet.IsOpening = true;
                    sheet.IsClosing = false;
                }
                else
                {
                    if (sheet.IsClosing)
                    {
                        return;
                    }
                    sheet.IsOpening = false;
                    sheet.IsClosing = true;
                }

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
        _animationTimer.Tick += AnimateFrame;
        if (_sheetPart?.RenderTransform is TranslateTransform translateTransform)
        {
            _sheetPart.RenderTransform = translateTransform;
            translateTransform.Y = _sheetPart.GetTransformedBounds()?.Bounds.Height ?? 0;
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
        if (_sheetPart?.RenderTransform is not TranslateTransform)
        {
            _animationTimer.Stop();
            return;
        }
        
        var sheetHeight = _sheetPart.GetTransformedBounds()?.Bounds.Height ?? 0;
        if (IsOpen)
        {
            if (_sheetPart.RenderTransform is not TranslateTransform transform)
            {
                _animationTimer.Stop();
                return;
            }

            if (transform.Y > 0)
            {
                transform.Y -= sheetHeight / AnimationTotalTicks;
                if (transform.Y <= 0)
                {
                    transform.Y = 0;
                    _animationTimer.Stop();
                    if (IsOpening)
                    {
                        //DialogManager.OnSheetOpened?.Invoke(this, EventArgs.Empty);
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
        else if (!IsOpen)
        {
            if (_sheetPart.RenderTransform is not TranslateTransform transform)
            {
                _animationTimer.Stop();
                return;
            }

            if (transform.Y < sheetHeight)
            {
                transform.Y += sheetHeight / AnimationTotalTicks;
                if (transform.Y > sheetHeight)
                {
                    transform.Y = sheetHeight;
                    _animationTimer.Stop();
                    if (IsClosing)
                    {
                        //DialogManager.OnSheetClosed?.Invoke(this, EventArgs.Empty);
                        IsClosing = false;
                        UpdatePseudoClasses();
                        if (DetachOnClose)
                        {
                            var host = DialogHost.GetDialogHost();
                            host.Sheets.Remove(this);
                        }
                    }
                }
            }
            else
            {
                _animationTimer.Stop();
            }
        }
    }
    
}