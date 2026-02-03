using Ejemplo_Photo_Dialog.Pages;

namespace Ejemplo_Photo_Dialog;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(MyMediaPickerPage), typeof(MyMediaPickerPage));
    }
}
