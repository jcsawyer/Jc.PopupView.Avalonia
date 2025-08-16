namespace Jc.PopupView.Avalonia.Controls;

[Flags]
public enum SheetScrollDirection
{
    None = 0x0,
    Vertical = 0x1,
    Horizontal = 0x2,
    Both = Vertical | Horizontal
}