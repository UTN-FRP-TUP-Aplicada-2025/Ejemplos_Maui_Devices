using Ejemplo_Maui_Hibrida.ViewModels;
using Java.Net;

namespace Ejemplo_Maui_Hibrida;

public partial class MainPage : ContentPage
{

    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;
        //mainViewModel.Url = "https://geolocate.somee.com";

        //mainViewModel.Url = "https://www.google.com";
        //mainViewModel.Url = "https://nvtc4bdq-8080.brs.devtunnels.ms/";

        mainViewModel.Url = "https://nvtc4bdq-8080.brs.devtunnels.ms/";

        mainViewModel.MostrarNavegador = true;
    }
    
}
