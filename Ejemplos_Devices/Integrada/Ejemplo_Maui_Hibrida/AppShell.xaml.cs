using Ejemplo_Maui_Hibrida.Pages;

namespace Ejemplo_Maui_Hibrida;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(MyMediaPickerPage), typeof(MyMediaPickerPage));
    }
}

