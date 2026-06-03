using Ejemplo_Maui_Hibrida.ViewModels;
using Java.Net;

namespace Ejemplo_Maui_Hibrida;

public partial class MainPage : ContentPage
{

    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;

        // URL con geo=1 para que el flujo del overlay se dispare al iniciar.
        //mainViewModel.Url = "https://geolocate.somee.com/geolocate?geo=1";
        //mainViewModel.Url = "https://www.google.com";
        mainViewModel.Url = "https://nvtc4bdq-8080.brs.devtunnels.ms/?geo=1";

        mainViewModel.MostrarNavegador = true;
    }
    
}
