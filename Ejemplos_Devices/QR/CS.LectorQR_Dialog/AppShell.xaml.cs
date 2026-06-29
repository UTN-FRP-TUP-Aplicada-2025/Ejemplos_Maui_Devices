using CS.LectorQR_Dialog.Pages;

namespace CS.LectorQR_Dialog;
 
public partial class AppShell : Shell
{

    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute( nameof(QRLectorPage), typeof(QRLectorPage) );
    }
}
