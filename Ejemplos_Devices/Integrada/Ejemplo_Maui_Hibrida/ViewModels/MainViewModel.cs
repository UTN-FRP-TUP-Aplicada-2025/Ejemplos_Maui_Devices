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
    NetworkOverlayViewModel networkOverlayViewModel;

    public MainViewModel(NetworkOverlayViewModel network, GpsOverlayViewModel gps)
    {
        gpsOverlayViewModel = gps;
        networkOverlayViewModel = network;
    }

    [RelayCommand]
    async private Task GoUrl()
    {
        //"https://geolocate.somee.com/geolocate?geo=1";

        var result = await GpsOverlayViewModel.SolicitarGeolocalizacion();

        if (result is GpsResult.Success s)
        {
            //https://geolocate.somee.com/geolocate?Latitud=-37.062438416743746&Longitud=-61.93923378411248,
            Url = Url.Replace("geo=1", $"Latitud={s.Location.Latitude}&Longitud={s.Location.Longitude}&");
        }
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
    private async Task Navigated(WebNavigatedEventArgs e)
    {
        IsRefreshing = false;

        if (e.Result == WebNavigationResult.Success)
            NetworkOverlayViewModel.NotifyNavigationSucceeded(e.Url);
        else
            await NetworkOverlayViewModel.NotifyNavigationFailedAsync(e.Url, e.Result);
    }
}