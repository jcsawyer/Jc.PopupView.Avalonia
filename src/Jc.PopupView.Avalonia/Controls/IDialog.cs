using Avalonia.Controls.Templates;

namespace Jc.PopupView.Avalonia.Controls;

public interface IDialog : IDataTemplateHost
{
    bool IsOpen { get; internal set; }
    bool ClickOutsideToDismiss { get; internal set; }
    bool ClickToDismiss { get; internal set; }
    object? Content { get; internal set; }
    bool ShowBackgroundMask { get; internal set; }

    void UpdatePseudoClasses();
    void Close();
}