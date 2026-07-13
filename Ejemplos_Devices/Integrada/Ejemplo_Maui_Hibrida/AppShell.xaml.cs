using LibApp.Devices.Camera.Pages;
using LibApp.Devices.QRLector.Pages;

namespace Ejemplo_Maui_Hibrida;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(MyMediaPickerPage), typeof(MyMediaPickerPage));
        Routing.RegisterRoute(nameof(MyMediaSelfiePickerPage), typeof(MyMediaSelfiePickerPage));
        Routing.RegisterRoute(nameof(QRLectorPage), typeof(QRLectorPage));
    }
}

