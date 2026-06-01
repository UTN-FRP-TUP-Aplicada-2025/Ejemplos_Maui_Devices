using Ejemplo_Maui_Hibrida.ViewModels;

namespace Ejemplo_Maui_Hibrida;

public partial class MainPage : ContentPage
{

    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;
        mainViewModel.Url = "https://geolocate.somee.com";
        mainViewModel.MostrarNavegador = true;
    }
    
}
