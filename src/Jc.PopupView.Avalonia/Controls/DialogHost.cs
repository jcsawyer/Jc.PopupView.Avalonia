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

    public static readonly StyledProperty<AvaloniaList<Popup>> PopupsProperty =
        AvaloniaProperty.Register<DialogHost, AvaloniaList<Popup>>(
            nameof(Popups), defaultValue: new AvaloniaList<Popup>());

    public AvaloniaList<Popup> Popups
    {
        get => GetValue(PopupsProperty);
        set => SetValue(PopupsProperty, value);
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

        Popups.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null)
            {
                foreach (var popup in args.NewItems)
                {
                    if (popup is Control control)
                    {
                        _modalLayer?.Children.Add(control);
                    }
                }
            }

            if (args.OldItems is not null)
            {
                foreach (var popup in args.OldItems)
                {
                    if (popup is Control control)
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
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            DialogHostRegistry.Register(topLevel, this);
        }

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
        IDialog dialog,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var id = Guid.NewGuid();

        OverlayEntry? entry = null;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            entry = new OverlayEntry(id, route, dialog, kind, ResolveAnimationDuration(dialog));
            AddEntry(entry);
            _stack.Add(entry);
        });

        var localEntry = entry!;
        var handle = new InMemoryPopupHandle(id, _ => DismissInternalAsync(id, CancellationToken.None));
        localEntry.Handle = handle;

        return handle;
    }

    public async Task<object?> ShowForResultAsync(
        PopupKind kind,
        string route,
        IDialog dialog,
        CancellationToken cancellationToken = default)
    {
        var handle = await ShowAsync(kind, route, dialog, cancellationToken);
        if (dialog.Content is IPopupResultSource resultSource)
        {
            var result = await resultSource.WaitForResultAsync(cancellationToken);
            await handle.DismissAsync(cancellationToken);
            return result;
        }

        return handle;
    }

    public async Task<bool> DismissTopMostAsync(CancellationToken cancellationToken = default)
    {
        OverlayEntry? trackedEntry = null;
        IDialog? dialog = null;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            dialog = GetTopMostOpenDialog();
            trackedEntry = _stack.LastOrDefault(entry => ReferenceEquals(entry.Dialog, dialog));
        });

        if (dialog is null)
        {
            return false;
        }

        if (trackedEntry?.Handle is not null)
        {
            await trackedEntry.Handle.DismissAsync(cancellationToken);
            return true;
        }

        await DismissDialogAsync(dialog, cancellationToken);
        return true;
    }

    public async Task<int> DismissAllAsync(CancellationToken cancellationToken = default)
    {
        List<IPopupHandle> handles = [];
        List<IDialog> untrackedDialogs = [];

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dialogs = GetOpenDialogsInDismissOrder();
            foreach (var dialog in dialogs)
            {
                var trackedEntry = _stack.LastOrDefault(entry => ReferenceEquals(entry.Dialog, dialog));
                if (trackedEntry?.Handle is not null)
                {
                    handles.Add(trackedEntry.Handle);
                }
                else
                {
                    untrackedDialogs.Add(dialog);
                }
            }
        });

        foreach (var handle in handles)
        {
            await handle.DismissAsync(cancellationToken);
        }

        foreach (var dialog in untrackedDialogs)
        {
            await DismissDialogAsync(dialog, cancellationToken);
        }

        return handles.Count + untrackedDialogs.Count;
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

    private async Task DismissDialogAsync(IDialog dialog, CancellationToken cancellationToken)
    {
        TimeSpan animationDuration = TimeSpan.Zero;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            animationDuration = ResolveAnimationDuration(dialog);
            dialog.Close();
        });

        if (animationDuration <= TimeSpan.Zero)
        {
            return;
        }

        try
        {
            await Task.Delay(animationDuration, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static TimeSpan ResolveAnimationDuration(IDialog dialog)
    {
        return dialog switch
        {
            Sheet sheet => sheet.AnimationDuration,
            Toast toast => toast.AnimationDuration,
            Floater floater => floater.AnimationDuration,
            Popup popup => popup.AnimationDuration,
            _ => TimeSpan.FromMilliseconds(220),
        };
    }

    private IDialog? GetTopMostOpenDialog()
    {
        return FindTopMostOpenDialog(_toastLayer) ??
               FindTopMostOpenDialog(_floaterLayer) ??
               FindTopMostOpenDialog(_modalLayer);
    }

    private List<IDialog> GetOpenDialogsInDismissOrder()
    {
        var dialogs = new List<IDialog>();
        AddOpenDialogsInReverseOrder(_toastLayer, dialogs);
        AddOpenDialogsInReverseOrder(_floaterLayer, dialogs);
        AddOpenDialogsInReverseOrder(_modalLayer, dialogs);
        return dialogs;
    }

    private static IDialog? FindTopMostOpenDialog(Panel? layer)
    {
        if (layer is null)
        {
            return null;
        }

        for (var index = layer.Children.Count - 1; index >= 0; index--)
        {
            if (layer.Children[index] is IDialog dialog && dialog.IsOpen)
            {
                return dialog;
            }
        }

        return null;
    }

    private static void AddOpenDialogsInReverseOrder(Panel? layer, List<IDialog> dialogs)
    {
        if (layer is null)
        {
            return;
        }

        for (var index = layer.Children.Count - 1; index >= 0; index--)
        {
            if (layer.Children[index] is IDialog dialog && dialog.IsOpen)
            {
                dialogs.Add(dialog);
            }
        }
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
            case PopupKind.Popup:
                if (entry.Dialog is Popup popup)
                {
                    Popups.Add(popup);
                    Dispatcher.UIThread.Post(() => popup.IsOpen = true, DispatcherPriority.Loaded);
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

        foreach (var child in Popups)
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
