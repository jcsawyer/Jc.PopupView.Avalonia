namespace Jc.PopupView.Avalonia.Exceptions;

public sealed class InvalidDialogDragBehaviorControl : Exception
{
    public InvalidDialogDragBehaviorControl() : base("The control must be a Grid that contains a dialog control (IDialog) as an ancestor.")
    {
    }
}