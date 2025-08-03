using Avalonia.Controls.Templates;

namespace Jc.PopupView.Avalonia.Controls;

public interface IDialog : IDataTemplateHost
{
    bool IsOpen { get; internal set; }
    internal bool IsOpening { get; set; }
    internal bool IsClosing { get; set; }
    bool CloseOnClickOutside { get; internal set; }
    object? Content { get; internal set; }

    void UpdatePseudoClasses();
}