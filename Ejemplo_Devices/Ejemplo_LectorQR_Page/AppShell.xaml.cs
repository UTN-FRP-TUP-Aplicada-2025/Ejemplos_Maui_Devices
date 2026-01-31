using Ejemplo_LectorQR_Page.Pages;

namespace Ejemplo_LectorQR_Page;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute( nameof(QRLectoPage), typeof(QRLectoPage) );
    }
}
