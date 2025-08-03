using Jc.PopupView.Avalonia.Controls;

namespace Jc.PopupView.Avalonia.Exceptions;

public class InvalidDialogHostException : Exception
{
    public InvalidDialogHostException()
        : base($"A single {nameof(DialogHost)} control must exist in the visual tree.")
    {
    }
}