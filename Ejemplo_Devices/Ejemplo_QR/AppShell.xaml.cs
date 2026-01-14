namespace Ejemplo_QR;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Pages.QRPage), typeof(Pages.QRPage));
        Routing.RegisterRoute(nameof(Pages.MainPage), typeof(Pages.MainPage));
    }
}
