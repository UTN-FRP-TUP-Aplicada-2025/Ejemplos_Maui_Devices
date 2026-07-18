using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using LibApp.CustomWebView.Behaviors;
using LibApp.Devices.GPS.ViewModels;
using LibApp.Devices.MotorDSL.ViewModels;
using LibApp.Devices.Networks.ViewModels;
using LibApp.Devices.Phone.ViewModels;
using LibApp.UrlCommands;

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

    [ObservableProperty]
    PrinterOverlayViewModel printerOverlayViewModel;

    private readonly UrlCommandDispatcher _dispatcher;

    // Guard de reentrada del puente: hay un comando de dispositivo (cámara/QR/GPS…) en curso.
    // Evita que un segundo click dispare un segundo despacho concurrente. Ver ADR-0009.
    private bool comandoEnCurso;

    public IWebViewBridge WebBridge { get; }

    public MainViewModel(NetworkOverlayViewModel network, GpsOverlayViewModel gps, CallOverlayViewModel call, PrinterOverlayViewModel printer, UrlCommandDispatcher dispatcher, IWebViewBridge webBridge)
    {
        gpsOverlayViewModel = gps;
        networkOverlayViewModel = network;
        callOverlayViewModel = call;
        printerOverlayViewModel = printer;
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

    // Gesto pull-to-refresh: recarga el WebView vía el bridge. El spinner se cierra
    // cuando el WebView termina de navegar (Navigated => IsRefreshing = false).
    [RelayCommand]
    private void Refresh() => WebBridge.Reload();

    // AllowConcurrentExecutions = true es imprescindible: si el comando quedara bloqueado por
    // estar "en ejecución" (default de AsyncRelayCommand), el EventToCommandBehavior no lo
    // invocaría en el 2º Navigating y `e.Cancel` NO se fijaría → el WebView haría la navegación
    // real (recarga de la página, perdiendo la inyección del resultado). Ver ADR-0009.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Navigating(WebNavigatingEventArgs e)
    {
        if (_dispatcher.IsCommand(e.Url))
        {
            e.Cancel = true;             // SIEMPRE cancelar la URL-comando: nunca debe recargar

            if (comandoEnCurso)          // click duplicado mientras hay un comando en curso:
            {                            // ya se canceló la navegación; se descarta el duplicado.
                IsRefreshing = false;
                return;
            }

            comandoEnCurso = true;
            try
            {
                var outcome = await _dispatcher.DispatchAsync(e.Url);

                if (outcome.NavigateTo is not null)
                    Url = outcome.NavigateTo;
            }
            finally { comandoEnCurso = false; }
        }

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

    [RelayCommand]
    private async Task Volver()
    {
        Url = "https://aplicada.somee.com";
    }
}