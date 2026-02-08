
## Connectivity
https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/networking?view=net-maui-10.0&tabs=android

status 

NetworkAccess accessType = Connectivity.Current.NetworkAccess;

if (accessType == NetworkAccess.Internet)
{
    // Connection to internet is available
}


Internet : Acceso local e Internet.
ConstrainedInternet — Acceso limitado a internet. Este valor significa que existe un portal cautivo, donde se proporciona acceso local a un portal web. Una vez que se utiliza el portal para proporcionar credenciales de autenticación, se concede el acceso a internet.
Local : solo acceso a red local.
Ninguno — No hay conectividad disponible.
Desconocido : no se puede determinar la conectividad a Internet.

```
public class ConnectivityTest
{
    public ConnectivityTest() =>
        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

    ~ConnectivityTest() =>
        Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

    void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.ConstrainedInternet)
            Console.WriteLine("Internet access is available but is limited.");

        else if (e.NetworkAccess != NetworkAccess.Internet)
            Console.WriteLine("Internet access has been lost.");

        // Log each active connection
        Console.Write("Connections active: ");

        foreach (var item in e.ConnectionProfiles)
        {
            switch (item)
            {
                case ConnectionProfile.Bluetooth:
                    Console.Write("Bluetooth");
                    break;
                case ConnectionProfile.Cellular:
                    Console.Write("Cell");
                    break;
                case ConnectionProfile.Ethernet:
                    Console.Write("Ethernet");
                    break;
                case ConnectionProfile.WiFi:
                    Console.Write("WiFi");
                    break;
                default:
                    break;
            }
        }

        Console.WriteLine();
    }
}
```