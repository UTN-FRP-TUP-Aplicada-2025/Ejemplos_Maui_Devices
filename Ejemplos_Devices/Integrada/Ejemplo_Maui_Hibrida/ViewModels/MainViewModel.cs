using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Ejemplo_Maui_Hibrida.Behaviors;
using Ejemplo_Maui_Hibrida.UrlCommands;

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

    private readonly UrlCommandDispatcher _dispatcher;

    public IWebViewBridge WebBridge { get; }

    public MainViewModel(NetworkOverlayViewModel network, GpsOverlayViewModel gps, CallOverlayViewModel call, UrlCommandDispatcher dispatcher, IWebViewBridge webBridge)
    {
        gpsOverlayViewModel = gps;
        networkOverlayViewModel = network;
        callOverlayViewModel = call;
        _dispatcher = dispatcher;
        WebBridge = webBridge;
    }

    // Botón manual: dispara una llamada usando el protocolo real de la web.
    [RelayCommand]
    private async Task TakePhone()
    {
        _ = await _dispatcher.DispatchAsync("phone=phone");
    }

    // Botón manual: abre el lector de QR usando el protocolo real de la web.
    [RelayCommand]
    private async Task TakeQR()
    {
        _ = await _dispatcher.DispatchAsync("qr=qr&param=contenidoQR");
    }

    // Botón manual: fuerza el marcador coordenadas=coordenadas sobre la URL actual y
    // delega en el dispatcher, que aplica la reescritura con coordenadas (sin duplicar
    // esa lógica acá).
    [RelayCommand]
    async private Task TakeGPS()
    {
        var url = Url.Contains("coordenadas=coordenadas", StringComparison.OrdinalIgnoreCase)
            ? Url
            : $"{Url}{(Url.Contains('?') ? "&" : "?")}coordenadas=coordenadas";

        var outcome = await _dispatcher.DispatchAsync(url);

        if (outcome.NavigateTo is not null)
            Url = outcome.NavigateTo;
    }

    [RelayCommand]
    private async Task Navigating(WebNavigatingEventArgs e)
    {
        if (_dispatcher.IsCommand(e.Url))
            e.Cancel = true;   // sincrónico: cancelar ANTES de cualquier await

        var outcome = await _dispatcher.DispatchAsync(e.Url);

        if (outcome.NavigateTo is not null)
            Url = outcome.NavigateTo;

        IsRefreshing = false;
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