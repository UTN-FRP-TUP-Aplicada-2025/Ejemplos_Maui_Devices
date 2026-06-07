using CommunityToolkit.Mvvm.Input;

using Ejemplo_Maui_Hibrida.Behaviors;
using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.Services;

using Microsoft.Maui.ApplicationModel;

namespace Ejemplo_Maui_Hibrida.ViewModels;

/// <summary>
/// Coordina el overlay de red con prioridad de estados. La verdad de fondo
/// sobre "cargó o no" es el <see cref="WebNavigationResult"/> del WebView;
/// el evento de conectividad y la sonda son ayudas.
/// </summary>
public partial class NetworkOverlayViewModel : StatusOverlayViewModel
{
    private readonly NetworkService _net;
    private readonly IWebViewBridge _bridge;

    // true  => al recuperar la conexión hay que REFRESCAR el WebView (fallo de navegación).
    // false => corte emergente con página ya cargada: sólo subir/bajar el overlay.
    private bool _needsReload;

    public NetworkOverlayViewModel(NetworkService net, IWebViewBridge bridge)
    {
        _net = net;
        _bridge = bridge;
        _net.ConnectivityChanged += online =>
            MainThread.BeginInvokeOnMainThread(() => OnConnectivity(online));
        Hide();
    }

    /// <summary>La navegación del WebView terminó con éxito.</summary>
    public void NotifyNavigationSucceeded(string url)
    {
        _needsReload = false;
        Hide();
    }

    /// <summary>La navegación del WebView falló: probamos la red y procesamos el resultado.</summary>
    public async Task NotifyNavigationFailedAsync(string url, WebNavigationResult result)
    {
        _needsReload = true;
        ShowBusy("Reconectando…", "Comprobando el acceso al sitio…", "reconexion.gif");
        Procesar(await _net.CheckUrlAsync(url));
    }

    /// <summary>Reacciona al evento de conectividad del SO (ya en el hilo de UI).</summary>
    private void OnConnectivity(bool online)
    {
        if (!online)
        {
            // Máxima prioridad: pisa cualquier estado.
            MostrarOffline();
            return;
        }

        // online:
        if (Mode == OverlayMode.None)
            return; // nada que hacer, el sitio ya estaba visible.

        if (_needsReload)
            _ = RecargarAsync();
        else
            Hide(); // sólo destapar, sin recargar.
    }

    [RelayCommand]
    private Task Reintentar() => RecargarAsync();

    private Task RecargarAsync()
    {
        ShowBusy("Reconectando…", "Cargando el sitio…", "reconexion.gif");
        _bridge.Reload();
        return Task.CompletedTask; // El resultado real vuelve por NotifyNavigation*.
    }

    private void Procesar(NetworkResult result)
    {
        switch (result)
        {
            case NetworkResult.Online:
                _ = RecargarAsync();
                break;

            case NetworkResult.Offline:
                MostrarOffline();
                break;

            case NetworkResult.Timeout:
                ShowError("schedule", "Tiempo de espera agotado",
                    "El servidor tardó demasiado en responder. Probá nuevamente en unos instantes.",
                    new OverlayAction("Reintentar", ReintentarCommand));
                break;

            case NetworkResult.DnsFailure d:
                ShowError("dns", "No se pudo resolver el servidor",
                    $"No fue posible encontrar «{d.Host}». Verificá tu conexión e intentá de nuevo.",
                    new OverlayAction("Reintentar", ReintentarCommand));
                break;

            case NetworkResult.HttpFailure h:
                ShowError("error", "El sitio no está disponible",
                    $"El servidor respondió con un error (código {h.StatusCode}).",
                    new OverlayAction("Reintentar", ReintentarCommand));
                break;

            case NetworkResult.RequestFailure:
                ShowError("wifi_off", "Error de conexión",
                    "Ocurrió un problema al conectar con el sitio. Revisá tu conexión e intentá de nuevo.",
                    new OverlayAction("Reintentar", ReintentarCommand));
                break;
        }
    }

    private void MostrarOffline()
    {
        ShowError("wifi_off", "Sin conexión a internet",
            "Comprobá tu conexión Wi-Fi o tus datos móviles para continuar.",
            new OverlayAction("Reintentar", ReintentarCommand),
            new OverlayAction("Abrir configuración", AbrirConfiguracionCommand, OverlayActionStyle.Secondary));
    }

    [RelayCommand]
    private void AbrirConfiguracion() => AppInfo.ShowSettingsUI();
}
