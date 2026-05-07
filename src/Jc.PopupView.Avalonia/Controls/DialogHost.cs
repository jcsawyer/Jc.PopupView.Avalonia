using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Jc.PopupView.Avalonia.Exceptions;
using Jc.PopupView.Avalonia.Services;

namespace Jc.PopupView.Avalonia.Controls;

public class DialogHost : TemplatedControl, IPopupOverlayHost
{
    private Grid? _modalLayer;
    private Grid? _floaterLayer;
    private Grid? _toastLayer;
    private readonly List<OverlayEntry> _stack = [];

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

        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            DialogHostRegistry.Register(topLevel, this);
        }

        _modalLayer = e.NameScope.Find<Grid>("PART_ModalLayer");
        _floaterLayer = e.NameScope.Find<Grid>("PART_FloaterLayer");
        _toastLayer = e.NameScope.Find<Grid>("PART_ToastLayer");
        UpdateVisualChildren();

        Sheets.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null)
            {
                foreach (var sheet in args.NewItems)
                {
                    if (sheet is Control control)
                    {
                        _modalLayer?.Children.Add(control);
                    }
                }
            }

            if (args.OldItems is not null)
            {
                foreach (var sheet in args.OldItems)
                {
                    if (sheet is Control control)
                    {
                        _modalLayer?.Children.Remove(control);
                    }
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
                        _toastLayer?.Children.Add(control);
                    }
                }
            }

            if (args.OldItems is not null)
            {
                foreach (var toast in args.OldItems)
                {
                    if (toast is Control control)
                    {
                        _toastLayer?.Children.Remove(control);
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
                        _floaterLayer?.Children.Add(control);
                    }
                }
            }

            if (args.OldItems is not null)
            {
                foreach (var floater in args.OldItems)
                {
                    if (floater is Control control)
                    {
                        _floaterLayer?.Children.Remove(control);
                    }
                }
            }
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PopupOverlayHostLocator.Register(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        PopupOverlayHostLocator.Clear(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (TopLevel.GetTopLevel(this)?.InsetsManager is { } insetsManager && UseSafePadding)
        {
            insetsManager.SafeAreaChanged += InsetsManagerOnSafeAreaChanged;
            SafePadding = insetsManager.SafeAreaPadding;
        }
    }

    public async Task<IPopupHandle> ShowAsync(
        PopupKind kind,
        string route,
        Control content,
        PopupOptions? options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var id = Guid.NewGuid();

        OverlayEntry? entry = null;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            entry = CreateEntry(id, kind, route, content, options);
            AddEntry(entry);
            _stack.Add(entry);
        });

        var localEntry = entry!;
        var handle = new InMemoryPopupHandle(id, _ => DismissInternalAsync(id, CancellationToken.None));
        localEntry.Handle = handle;

        if (kind == PopupKind.Toast && options?.Duration is TimeSpan duration)
        {
            _ = AutoDismissAsync(handle, duration);
        }

        return handle;
    }

    public async Task<object?> ShowForResultAsync(
        PopupKind kind,
        string route,
        Control content,
        PopupOptions? options,
        CancellationToken cancellationToken = default)
    {
        var handle = await ShowAsync(kind, route, content, options, cancellationToken);
        if (content is IPopupResultSource resultSource)
        {
            var result = await resultSource.WaitForResultAsync(cancellationToken);
            await handle.DismissAsync(cancellationToken);
            return result;
        }

        return handle;
    }

    public async Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default)
    {
        IPopupHandle? handle = null;
        await Dispatcher.UIThread.InvokeAsync(() => { handle = _stack.LastOrDefault()?.Handle; });
        if (handle is null)
        {
            return false;
        }

        await handle.DismissAsync(cancellationToken);
        return true;
    }

    private static async Task AutoDismissAsync(IPopupHandle handle, TimeSpan duration)
    {
        await Task.Delay(duration);
        await handle.DismissAsync();
    }

    private async Task DismissInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        OverlayEntry? entry = null;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            entry = _stack.FirstOrDefault(s => s.Id == id);
            entry?.Close();
        });

        if (entry is null)
        {
            return;
        }

        try
        {
            await Task.Delay(entry.AnimationDuration, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }

        await Dispatcher.UIThread.InvokeAsync(() => { _stack.RemoveAll(s => s.Id == id); });
    }

    private OverlayEntry CreateEntry(Guid id, PopupKind kind, string route, Control content, PopupOptions? options)
    {
        return kind switch
        {
            PopupKind.Sheet => CreateSheetEntry(id, route, content, options),
            PopupKind.Toast => CreateToastEntry(id, route, content, options),
            PopupKind.Floater => CreateFloaterEntry(id, route, content, options),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    private OverlayEntry CreateSheetEntry(Guid id, string route, Control content, PopupOptions? options)
    {
        var sheet = new Sheet
        {
            Content = content,
            ClickOutsideToDismiss = options?.DismissOnBackdropTap ?? true,
            DetachOnClose = true,
            Detents = options?.ToDetentList(),
            InitialDetent = options?.InitialDetent,
            SnapPoint = options?.SnapPoint,
        };

        return new OverlayEntry(id, route, sheet, PopupKind.Sheet, sheet.AnimationDuration);
    }

    private OverlayEntry CreateToastEntry(Guid id, string route, Control content, PopupOptions? options)
    {
        var toast = new Toast
        {
            Content = content,
            ClickOutsideToDismiss = options?.DismissOnBackdropTap ?? true,
            DetachOnClose = true,
            Location = options?.Placement == PopupPlacement.Bottom ? ToastLocation.Bottom : ToastLocation.Top,
        };

        return new OverlayEntry(id, route, toast, PopupKind.Toast, toast.AnimationDuration);
    }

    private OverlayEntry CreateFloaterEntry(Guid id, string route, Control content, PopupOptions? options)
    {
        var floater = new Floater
        {
            Content = content,
            ClickOutsideToDismiss = options?.DismissOnBackdropTap ?? true,
            DetachOnClose = true,
            Location = options?.Placement == PopupPlacement.Bottom ? FloaterLocation.Bottom : FloaterLocation.Top,
        };

        return new OverlayEntry(id, route, floater, PopupKind.Floater, floater.AnimationDuration);
    }

    private void AddEntry(OverlayEntry entry)
    {
        switch (entry.Kind)
        {
            case PopupKind.Sheet:
                if (entry.Dialog is Sheet sheet)
                {
                    Sheets.Add(sheet);
                    Dispatcher.UIThread.Post(() => sheet.IsOpen = true, DispatcherPriority.Loaded);
                }
                break;
            case PopupKind.Toast:
                if (entry.Dialog is Toast toast)
                {
                    Toasts.Add(toast);
                    Dispatcher.UIThread.Post(() => toast.IsOpen = true, DispatcherPriority.Loaded);
                }
                break;
            case PopupKind.Floater:
                if (entry.Dialog is Floater floater)
                {
                    Floaters.Add(floater);
                    Dispatcher.UIThread.Post(() => floater.IsOpen = true, DispatcherPriority.Loaded);
                }
                break;
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
        if (_modalLayer is null || _floaterLayer is null || _toastLayer is null)
        {
            return;
        }

        _modalLayer.Children.Clear();
        _floaterLayer.Children.Clear();
        _toastLayer.Children.Clear();

        foreach (var child in Sheets)
        {
            if (child is Control control)
            {
                _modalLayer.Children.Add(control);
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
                _floaterLayer.Children.Add(control);
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
                _toastLayer.Children.Add(control);
            }
            else
            {
                throw new InvalidDialogHostControl();
            }
        }
    }

    private sealed class OverlayEntry
    {
        public OverlayEntry(Guid id, string route, IDialog dialog, PopupKind kind, TimeSpan animationDuration)
        {
            Id = id;
            Route = route;
            Dialog = dialog;
            Kind = kind;
            AnimationDuration = animationDuration;
        }

        public Guid Id { get; }
        public string Route { get; }
        public IDialog Dialog { get; }
        public PopupKind Kind { get; }
        public TimeSpan AnimationDuration { get; }
        public IPopupHandle? Handle { get; set; }

        public void Close() => Dialog.Close();
    }
}
