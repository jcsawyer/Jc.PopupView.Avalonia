using Avalonia.Controls;
using Avalonia.Interactivity;
using Jc.PopupView.Avalonia.Services;

namespace Jc.PopupView.Avalonia.Sample.Views.Toasts;

public partial class Toast2 : UserControl
{
    public Toast2()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        new DialogService().CloseToast(this);
    }
}