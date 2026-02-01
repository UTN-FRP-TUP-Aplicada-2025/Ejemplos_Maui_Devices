using Ejemplo_LectorQR_Dialog.Pages;

namespace Ejemplo_LectorQR_Dialog;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute( nameof(QRLectorPage), typeof(QRLectorPage) );
    }
}
