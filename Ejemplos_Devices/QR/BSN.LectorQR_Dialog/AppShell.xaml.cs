using BSN.LectorQR_Dialog.Pages;

namespace BSN.LectorQR_Dialog;
 
public partial class AppShell : Shell
{

    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute( nameof(QRLectorPage), typeof(QRLectorPage) );
    }
}
