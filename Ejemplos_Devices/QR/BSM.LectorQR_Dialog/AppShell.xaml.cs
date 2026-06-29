using BSM.LectorQR_Dialog.Pages;

namespace BSM.LectorQR_Dialog;
 
public partial class AppShell : Shell
{

    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute( nameof(QRLectorPage), typeof(QRLectorPage) );
    }
}
