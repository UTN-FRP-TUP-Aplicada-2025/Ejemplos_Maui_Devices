using Ejemplo_Maui_Hibrida.ViewModels;

namespace Ejemplo_Maui_Hibrida;

public partial class MainPage : ContentPage
{

    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;

        // La página sabe recargar el WebView; el overlay de Red lo dispara.
        mainViewModel.NetworkOverlayViewModel.ReloadRequested = () =>
        {
            webView.Reload();
            return Task.CompletedTask;
        };

        // URL con geo=1 para que el flujo del overlay se dispare al iniciar.
        mainViewModel.Url = "https://geolocate.somee.com/geolocate?geo=1";
        //mainViewModel.Url = "https://www.google.com";
        //mainViewModel.Url = "https://nvtc4bdq-8080.brs.devtunnels.ms/?geo=1";

        mainViewModel.Url = "https://geolocate.somee.com";

//#if IOS
//if (OperatingSystem.IsIOSVersionAtLeast(16, 4))
//    webView.Inspectable = true;
//#endif

    }

}
