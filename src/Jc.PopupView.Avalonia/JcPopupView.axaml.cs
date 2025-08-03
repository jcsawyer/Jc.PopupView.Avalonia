using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Jc.PopupView.Avalonia;

public partial class JcPopupView : Styles
{
    public JcPopupView(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}