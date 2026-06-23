using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Jc.PopupView.Avalonia.Controls;
using Jc.PopupView.Avalonia.Sample.Views.Popups;
using Jc.PopupView.Avalonia.Sample.Views.Sheets;
using Jc.PopupView.Avalonia.Sample.Views.Toasts;
using Jc.PopupView.Avalonia.Services;
using ReactiveUI;

namespace Jc.PopupView.Avalonia.Sample.ViewModels;

public class MainViewModel : ViewModelBase
{
    private bool _isSheet1Open;

    public bool IsSheet1Open
    {
        get => _isSheet1Open;
        set => this.RaiseAndSetIfChanged(ref _isSheet1Open, value);
    }

    public ICommand OpenSheet1Command { get; }

    private bool _isSheet2Open;

    public bool IsSheet2Open
    {
        get => _isSheet2Open;
        set => this.RaiseAndSetIfChanged(ref _isSheet2Open, value);
    }

    public ICommand OpenSheet2Command { get; }

    private bool _isSheetTabBarOnlyOpen;

    public bool IsSheetTabBarOnlyOpen
    {
        get => _isSheetTabBarOnlyOpen;
        set => this.RaiseAndSetIfChanged(ref _isSheetTabBarOnlyOpen, value);
    }

    public ICommand OpenSheetTabBarOnlyCommand { get; }

    public ICommand OpenSheet3Command { get; }

    public ICommand OpenSheet4Command { get; }
    public ICommand OpenSheetResultCommand { get; }

    public ICommand OpenNativeSheet1Command { get; }

    private string? _lastSheetResult;
    public string? LastSheetResult
    {
        get => _lastSheetResult;
        set => this.RaiseAndSetIfChanged(ref _lastSheetResult, value);
    }

    private bool _isToast1Open;

    public bool IsToast1Open
    {
        get => _isToast1Open;
        set => this.RaiseAndSetIfChanged(ref _isToast1Open, value);
    }

    public ICommand OpenToast1Command { get; }

    private bool _isToast2Open;

    public bool IsToast2Open
    {
        get => _isToast2Open;
        set => this.RaiseAndSetIfChanged(ref _isToast2Open, value);
    }

    public ICommand OpenToast2Command { get; }

    private bool _isToast3Open;

    public bool IsToast3Open
    {
        get => _isToast3Open;
        set => this.RaiseAndSetIfChanged(ref _isToast3Open, value);
    }

    public ICommand OpenToast3Command { get; }

    public ICommand OpenToast4Command { get; }
    public ICommand OpenToast5Command { get; }

    private string? _lastToastResult;
    public string? LastToastResult
    {
        get => _lastToastResult;
        set => this.RaiseAndSetIfChanged(ref _lastToastResult, value);
    }
    
    private bool _isFloater1Open;
    public bool IsFloater1Open
    {
        get => _isFloater1Open;
        set => this.RaiseAndSetIfChanged(ref _isFloater1Open, value);
    }
    public ICommand OpenFloater1Command { get; }
    
    private bool _isFloater2Open;
    public bool IsFloater2Open
    {
        get => _isFloater2Open;
        set => this.RaiseAndSetIfChanged(ref _isFloater2Open, value);
    }
    public ICommand OpenFloater2Command { get; }
    
    private bool _isFloater3Open;
    public bool IsFloater3Open
    {
        get => _isFloater3Open;
        set => this.RaiseAndSetIfChanged(ref _isFloater3Open, value);
    }
    public ICommand OpenFloater3Command { get; }
    public ICommand OpenFloater4Command { get; }
    public ICommand OpenPopup1Command { get; }
    public ICommand OpenPopup2Command { get; }
    public ICommand OpenPopup3Command { get; }
    public ICommand DismissTopMostDemoCommand { get; }
    public ICommand DismissAllDemoCommand { get; }

    private string? _lastFloaterResult;
    public string? LastFloaterResult
    {
        get => _lastFloaterResult;
        set => this.RaiseAndSetIfChanged(ref _lastFloaterResult, value);
    }

    public MainViewModel()
    {
        OpenSheet1Command = ReactiveCommand.Create(() => IsSheet1Open = true);
        OpenSheet2Command = ReactiveCommand.Create(() => IsSheet2Open = true);
        OpenSheetTabBarOnlyCommand = ReactiveCommand.Create(() => IsSheetTabBarOnlyOpen = true);
        OpenSheet3Command = ReactiveCommand.Create(() =>
            new DialogService().OpenSheet(new TextBlock { Text = "Hello, from dynamic dialog!" }));
        OpenSheet4Command = ReactiveCommand.Create(() => new DialogService().OpenSheet(new InteractiveSheet()));
        OpenSheetResultCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var service = new DialogService();
            var result = await service.ShowSheetForResultAsync<string, ResultSheet>(
                new ResultSheet(),
                sheet =>
                {
                    sheet.Detents = [0.78, 0.56, 0.36];
                    sheet.InitialDetent = 0.56;
                });

            LastSheetResult = result ?? "cancelled";
        });
        OpenNativeSheet1Command = ReactiveCommand.Create(() =>
            Native.BottomSheetService.Current?.ShowBottomSheet(new Sheet1()));
        OpenToast1Command = ReactiveCommand.Create(() => IsToast1Open = true);
        OpenToast2Command = ReactiveCommand.Create(() => IsToast2Open = true);
        OpenToast3Command = ReactiveCommand.Create(() => IsToast3Open = true);
        OpenToast4Command = ReactiveCommand.Create(() =>
            new DialogService().OpenToast(
                new TextBlock { Text = "Hello, from dynamic dialog!", Padding = new Thickness(10) },
                toast => toast.Location = ToastLocation.Bottom));
        OpenToast5Command = ReactiveCommand.CreateFromTask(async () =>
        {
            var service = new DialogService();
            var result = await service.ShowToastForResultAsync<string, Toast2Result>(
                new Toast2Result(),
                toast =>
                {
                    toast.Location = ToastLocation.Bottom;
                    toast.ClickOutsideToDismiss = false;
                    toast.ClickToDismiss = false;
                    toast.ShowBackgroundMask = true;
                });
            LastToastResult = result ?? "cancelled";
        });
        OpenFloater1Command = ReactiveCommand.Create(() => IsFloater1Open = true);
        OpenFloater2Command = ReactiveCommand.Create(() => IsFloater2Open = true);
        OpenFloater3Command = ReactiveCommand.Create(() => IsFloater3Open = true);
        OpenFloater4Command = ReactiveCommand.CreateFromTask(async () =>
        {
            var service = new DialogService();
            var result = await service.ShowFloaterForResultAsync<string, Toast2Result>(
                new Toast2Result(),
                floater =>
                {
                    floater.Location = FloaterLocation.Bottom;
                    floater.ClickOutsideToDismiss = false;
                    floater.ClickToDismiss = false;
                    floater.ShowBackgroundMask = true;
                });
            LastFloaterResult = result ?? "cancelled";
        });

        OpenPopup1Command = ReactiveCommand.Create(() =>
            new DialogService().OpenPopup(
                new Popup1(),
                popup => { popup.ClickOutsideToDismiss = true; }));

        OpenPopup2Command = ReactiveCommand.Create(() =>
            new DialogService().OpenPopup(
                new Popup2(), popup =>
                {
                    popup.Position = PopupPosition.Bottom;
                    popup.OpenAnimationType = PopupAnimationType.Slide;
                    popup.CloseAnimationType = PopupAnimationType.Scale;
                    popup.ClickOutsideToDismiss = false;
                }));

        OpenPopup3Command = ReactiveCommand.Create(() =>
            new DialogService().OpenPopup(
                new Popup3(), popup =>
                {
                    popup.ContentMargin = new Thickness(0);
                    popup.CornerRadius = new CornerRadius(18, 0);
                    popup.Position = PopupPosition.Bottom;
                    popup.OpenAnimationType = PopupAnimationType.Slide;
                    popup.CloseAnimationType = PopupAnimationType.Slide;
                    popup.ClickOutsideToDismiss = true;
                }));

        DismissTopMostDemoCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var service = new DialogService();
            service.OpenPopup(
                new TextBlock
                {
                    Width = 280,
                    TextWrapping = TextWrapping.Wrap,
                    Text = "This popup was opened with OpenPopup(...). The sample dismisses it through DismissTopMostAsync without keeping a reference to the popup content."
                },
                popup =>
                {
                    popup.ClickOutsideToDismiss = false;
                });

            await Task.Delay(900);

            await service.DismissTopMostAsync();
        });

        DismissAllDemoCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var service = new DialogService();

            service.OpenPopup(
                new TextBlock
                {
                    Width = 260,
                    TextWrapping = TextWrapping.Wrap,
                    Text = "Legacy popup opened through OpenPopup(...)."
                },
                popup => popup.ClickOutsideToDismiss = false);

            service.OpenFloater(
                new TextBlock
                {
                    Text = "Legacy floater opened through OpenFloater(...).",
                    Padding = new Thickness(12)
                },
                floater =>
                {
                    floater.Location = FloaterLocation.Bottom;
                    floater.ClickOutsideToDismiss = false;
                    floater.ClickToDismiss = false;
                });

            await service.ShowToastAsync(
                new TextBlock
                {
                    Text = "Tracked toast opened through ShowToastAsync(...).",
                    Padding = new Thickness(12)
                },
                toast =>
                {
                    toast.Location = ToastLocation.Bottom;
                    toast.ClickOutsideToDismiss = false;
                    toast.ClickToDismiss = false;
                    toast.ShowBackgroundMask = true;
                });

            await Task.Delay(900);

            await service.DismissAllAsync();
        });
    }
}
