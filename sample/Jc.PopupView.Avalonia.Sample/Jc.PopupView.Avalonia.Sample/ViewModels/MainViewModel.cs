using System.Windows.Input;
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

    public MainViewModel()
    {
        OpenSheet1Command = ReactiveCommand.Create(() => IsSheet1Open = true);
        OpenSheet2Command = ReactiveCommand.Create(() => IsSheet2Open = true);
    }
}