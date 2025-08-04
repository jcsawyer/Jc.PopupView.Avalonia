using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;
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

    public ICommand OpenSheet3Command { get; }
    
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

    public MainViewModel()
    {
        OpenSheet1Command = ReactiveCommand.Create(() => IsSheet1Open = true);
        OpenSheet2Command = ReactiveCommand.Create(() => IsSheet2Open = true);
        OpenSheet3Command = ReactiveCommand.Create(() => new DialogService().OpenSheet(new TextBlock { Text = "Hello, from dynamic dialog!" }));
        OpenToast1Command = ReactiveCommand.Create(() => IsToast1Open = true);
        OpenToast2Command = ReactiveCommand.Create(() => IsToast2Open = true);
        OpenToast3Command = ReactiveCommand.Create(() => IsToast3Open = true);
        OpenToast4Command = ReactiveCommand.Create(() => new DialogService().OpenToast(new TextBlock { Text = "Hello, from dynamic dialog!", Padding = new Thickness(10)}, toast => toast.Location = ToastLocation.Bottom));
    }
}