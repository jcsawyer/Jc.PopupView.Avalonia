using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Jc.PopupView.Avalonia.Controls;

[PseudoClasses(":open", ":opening", ":closed", ":closing")]
public sealed class Sheet : TemplatedControl, IDialog
{
    private readonly DispatcherTimer _animationTimer;
    private static readonly TimeSpan AnimationFramerate = TimeSpan.FromMicroseconds(16);
    private Grid? _sheetPart;
    private Rectangle? _maskPart;
    private bool _isOpening;
    private bool _isClosing;

    internal bool DetachOnClose { get; set; }
    
    private int AnimationTotalTicks => (int)(AnimationDuration.TotalSeconds / AnimationFramerate.TotalSeconds);

    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<Sheet, TimeSpan>(
            nameof(AnimationDuration), defaultValue: TimeSpan.FromSeconds(0.2));

    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(IsOpen));

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set
        {
            if (value)
            {
                _isOpening = true;
                _isClosing = false;
            }
            else
            {
                _isOpening = false;
                _isClosing = true;
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

    public static readonly StyledProperty<IBrush> MaskColorProperty = AvaloniaProperty.Register<Sheet, IBrush>(
        nameof(MaskColor));

    public IBrush MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }

    public static readonly StyledProperty<bool> CloseOnClickOutsideProperty = AvaloniaProperty.Register<Sheet, bool>(
        nameof(ClickOutsideToDismiss));

    public bool ClickOutsideToDismiss
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
                    sheet._isOpening = true;
                    sheet._isClosing = false;
                }
                else
                {
                    sheet._isOpening = false;
                    sheet._isClosing = true;
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
        if (e.NameScope.Find<Rectangle>("PART_SheetMask") is { } sheetContent)
        {
            sheetContent.AddHandler(PointerPressedEvent, (_, _) =>
            {
                if (ClickOutsideToDismiss)
                {
                    IsOpen = false;
                }
            });
        }
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
                    if (_isOpening)
                    {
                        //DialogManager.OnSheetOpened?.Invoke(this, EventArgs.Empty);
                        _isOpening = false;
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
                    if (_isClosing)
                    {
                        //DialogManager.OnSheetClosed?.Invoke(this, EventArgs.Empty);
                        _isClosing = false;
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

    public void UpdatePseudoClasses()
    {
        var opening = _isOpening;
        var closing = _isClosing;
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

    public void Close()
    {
        if (IsOpen || _isOpening)
        {
            IsOpen = false;
        }
        else
        {
            _isClosing = false;
            _isOpening = false;
            UpdatePseudoClasses();
        }
    }
}