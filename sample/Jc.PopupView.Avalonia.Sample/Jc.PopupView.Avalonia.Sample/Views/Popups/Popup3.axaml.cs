using Avalonia.Controls;
using Avalonia.Interactivity;
using Jc.PopupView.Avalonia.Services;

namespace Jc.PopupView.Avalonia.Sample.Views.Popups;

public partial class Popup3 : UserControl
{
    public Popup3()
    {
        InitializeComponent();
    }

    private void OnReadMoreClick(object? sender, RoutedEventArgs e)
    {
        new DialogService().ClosePopup(this);
    }
}
