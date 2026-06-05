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

    [ObservableProperty]
    CallOverlayViewModel callOverlayViewModel;

    public MainViewModel(NetworkOverlayViewModel network, GpsOverlayViewModel gps, CallOverlayViewModel call)
    {
        gpsOverlayViewModel = gps;
        networkOverlayViewModel = network;
        callOverlayViewModel = call;
    }

    [RelayCommand]
    private async Task TakePhone()
    {
        // Ejemplo: llamada directa (Android) o marcador del SO (iOS/MacCatalyst).
        string telephone = "3434807427";
        _ = await callOverlayViewModel.LlamarAsync(telephone, CallMode.Direct);
    }

    [RelayCommand]
    async private Task TakeGPS()
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
            e.Cancel = true;
            await TakeGPS();
            IsRefreshing = false;
        }
        else if (Url.Contains("photo=2", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            await TakePhone();
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