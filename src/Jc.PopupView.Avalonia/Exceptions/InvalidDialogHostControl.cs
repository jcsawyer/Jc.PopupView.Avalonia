using Avalonia.Controls;
using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Exceptions;

public sealed class InvalidDialogHostControl : Exception
{
    public InvalidDialogHostControl() : base(
        $"{nameof(DialogHost)} controls must be inherit from '{nameof(Control)}' and implement '{nameof(IDialog)}'.")
    {
    }
}