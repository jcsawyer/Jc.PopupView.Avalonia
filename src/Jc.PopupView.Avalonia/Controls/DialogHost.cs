using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Jc.PopupView.Avalonia.Exceptions;
using Jc.PopupView.Avalonia.Services;

namespace Jc.PopupView.Avalonia.Controls;

public class DialogHost : TemplatedControl
{
    private static Window? _host;
    
    private Grid? _dialogHost;

    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<DialogHost, object?>(
        nameof(Content));

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<Sheet>> SheetsProperty =
        AvaloniaProperty.Register<DialogHost, AvaloniaList<Sheet>>(
            nameof(Sheets), defaultValue: new AvaloniaList<Sheet>());

    public AvaloniaList<Sheet> Sheets
    {
        get => GetValue(SheetsProperty);
        set => SetValue(SheetsProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<Toast>> ToastsProperty = AvaloniaProperty.Register<DialogHost, AvaloniaList<Toast>>(
        nameof(Toasts), defaultValue: new AvaloniaList<Toast>());

    public AvaloniaList<Toast> Toasts
    {
        get => GetValue(ToastsProperty);
        set => SetValue(ToastsProperty, value);
    }

    public static readonly StyledProperty<AvaloniaList<Floater>> FloatersProperty = AvaloniaProperty.Register<DialogHost, AvaloniaList<Floater>>(
        nameof(Floaters), defaultValue: new AvaloniaList<Floater>());

    public AvaloniaList<Floater> Floaters
    {
        get => GetValue(FloatersProperty);
        set => SetValue(FloatersProperty, value);
    }
    
    public static readonly StyledProperty<bool> UseSafePaddingProperty = AvaloniaProperty.Register<DialogHost, bool>(
        nameof(UseSafePadding), defaultValue: true);

    public bool UseSafePadding
    {
        get => GetValue(UseSafePaddingProperty);
        set => SetValue(UseSafePaddingProperty, value);
    }

    public static readonly StyledProperty<Thickness> SafePaddingProperty =
        AvaloniaProperty.Register<DialogHost, Thickness>(
            nameof(SafePadding));

    public Thickness SafePadding
    {
        get => GetValue(SafePaddingProperty);
        set => SetValue(SafePaddingProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        DialogHostRegistry.Register(TopLevel.GetTopLevel(this), this);
        
        _dialogHost = e.NameScope.Find<Grid>("PART_DialogHost");
        UpdateVisualChildren();

        Sheets.CollectionChanged += (sender, args) =>
        {
            if (args.NewItems is not null)
                foreach (var sheet in args.NewItems)
                {
                    if (sheet is Control control)
                    {
                        _dialogHost.Children.Add(control);
                    }
                }

            if (args.OldItems is not null)
                foreach (var sheet in args.OldItems)
                {
                    if (sheet is Control control)
                    {
                        _dialogHost.Children.Remove(control);
                    }
                }
        };

        Toasts.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null)
            {
                foreach (var toast in args.NewItems)
                {
                    if (toast is Control control)
                    {
                        _dialogHost.Children.Add(control);
                    }
                }
            }

            if (args.OldItems is not null)
            {
                foreach (var toast in args.OldItems)
                {
                    if (toast is Control control)
                    {
                        _dialogHost.Children.Remove(control);
                    }
                }
            }
        };
        
        Floaters.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null)
            {
                foreach (var floater in args.NewItems)
                {
                    if (floater is Control control)
                    {
                        _dialogHost.Children.Add(control);
                    }
                }
            }

            if (args.OldItems is not null)
            {
                foreach (var floater in args.OldItems)
                {
                    if (floater is Control control)
                    {
                        _dialogHost.Children.Remove(control);
                    }
                }
            }
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (TopLevel.GetTopLevel(_dialogHost)?.InsetsManager is { } insetsManager)
        {
            if (UseSafePadding)
            {
                insetsManager.SafeAreaChanged += InsetsManagerOnSafeAreaChanged;
                SafePadding = insetsManager.SafeAreaPadding;
            }
        }
    }

    private void InsetsManagerOnSafeAreaChanged(object? sender, SafeAreaChangedArgs e)
    {
        if (UseSafePadding)
        {
            SafePadding = e.SafeAreaPadding;
        }
    }

    private void UpdateVisualChildren()
    {
        if (_dialogHost is null)
        {
            return;
        }

        _dialogHost.Children.Clear();
        foreach (var child in Sheets)
        {
            if (child is Control control)
            {
                _dialogHost.Children.Add(control);
            }
            else
            {
                throw new InvalidDialogHostControl();
            }
        }
        foreach (var child in Toasts)
        {
            if (child is Control control)
            {
                _dialogHost.Children.Add(control);
            }
            else
            {
                throw new InvalidDialogHostControl();
            }
        }

        foreach (var child in Floaters)
        {
            if (child is Control control)
            {
                _dialogHost.Children.Add(control);
            }
            else
            {
                throw new InvalidDialogHostControl();
            }
        }
    }
}