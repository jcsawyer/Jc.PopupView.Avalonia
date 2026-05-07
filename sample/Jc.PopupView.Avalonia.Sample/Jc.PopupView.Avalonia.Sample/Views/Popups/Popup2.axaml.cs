using Avalonia.Controls;
using Avalonia.Interactivity;
using Jc.PopupView.Avalonia.Services;

namespace Jc.PopupView.Avalonia.Sample.Views.Popups;

public partial class Popup2 : UserControl
{
    public Popup2()
    {
        InitializeComponent();
    }

    private void OnAcceptClick(object? sender, RoutedEventArgs e)
    {
        new DialogService().ClosePopup(this);
    }
}
