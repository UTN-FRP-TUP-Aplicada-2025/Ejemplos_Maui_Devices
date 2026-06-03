using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Ejemplo_Maui_Hibrida.Models;

namespace Ejemplo_Maui_Hibrida.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string url = string.Empty;

    [ObservableProperty]
    GpsOverlayViewModel gpsOverlayViewModel;

    [ObservableProperty]
    private bool mostrarNavegador=true;

    public MainViewModel(GpsOverlayViewModel gps)
    {
        gpsOverlayViewModel = gps;
    }

    [RelayCommand]
    async private Task GoUrl()
    {
        //"https://geolocate.somee.com/geolocate?geo=1";

        var result=await gpsOverlayViewModel.SolicitarGeolocalizacion();

        if (result is GpsResult.Success s)
        {
            //https://geolocate.somee.com/geolocate?Latitud=-37.062438416743746&Longitud=-61.93923378411248, 
            Url = Url.Replace("geo=1", $"Latitud={s.Location.Latitude}&Longitud={s.Location.Longitude}&");
        }

        MostrarNavegador = true;
    }
        
    [RelayCommand]
    private async Task Navigating(WebNavigatingEventArgs e)
    {
        Url = e.Url;

        if (Url.Contains("geo=1", StringComparison.OrdinalIgnoreCase))
        {
            // Importante: setear Cancel = true antes del primer await,
            // ya que el WebView vuelve del evento sincrónicamente.
            e.Cancel = true;
            await GoUrl();
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private void Navigated(WebNavigatedEventArgs e)
    {
        IsRefreshing = false;
        // lógica post-navegación
    }
}