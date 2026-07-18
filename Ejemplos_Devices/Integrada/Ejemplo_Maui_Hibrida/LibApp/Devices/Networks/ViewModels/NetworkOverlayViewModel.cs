
using CommunityToolkit.Mvvm.Input;

using LibApp.CustomWebView.Behaviors;
using LibApp.Devices.Common.Services;
using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.Networks.Models;
using LibApp.Devices.Networks.Services;


namespace LibApp.Devices.Networks.ViewModels;

/// <summary>
/// Coordina el overlay de red. Es el único overlay <b>reactivo</b>: se suscribe a la
/// conectividad del SO y puede aparecer sin que nadie lo invoque.
/// <para>
/// La verdad de fondo sobre «cargó o no» es el <see cref="WebNavigationResult"/> del WebView;
/// el evento de conectividad y la sonda son ayudas.
/// </para>
/// </summary>
public partial class NetworkOverlayViewModel : StatusOverlayViewModel
{
    private readonly INetworkService _net;
    private readonly IWebViewBridge _bridge;
    private readonly IUiDispatcher _ui;

    // true  => al recuperar la conexión hay que REFRESCAR el WebView (fallo de navegación).
    // false => corte emergente con página ya cargada: sólo subir/bajar el overlay.
    private bool _needsReload;

    // Última URL que falló: es la que se resondea al reintentar.
    private string _ultimaUrl = string.Empty;

    public NetworkOverlayViewModel(INetworkService net, IWebViewBridge bridge, IUiDispatcher ui)
    {
        _net = net;
        _bridge = bridge;
        _ui = ui;

        // El evento llega en el hilo del SO. Marshalar es obligatorio: toca propiedades
        // observables. Va por IUiDispatcher y no por MainThread para que el VM siga siendo
        // ejercitable fuera de un dispositivo.
        _net.ConnectivityChanged += online => _ui.BeginInvoke(() => OnConnectivity(online));
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
        _ultimaUrl = url;
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
        // Sin "Cerrar": sin red no hay nada que ver detrás, así que no se ofrece salida. Es el
        // único overlay que además desmonta el WebView (ver MainPage.xaml).
        ShowError("wifi_off", "Sin conexión a internet",
            "Comprobá tu conexión Wi-Fi o tus datos móviles para continuar.",
            new OverlayAction("Reintentar", ReintentarCommand),
            new OverlayAction("Abrir configuración", AbrirConfiguracionCommand, OverlayActionStyle.Secondary));
    }

    [RelayCommand]
    private void AbrirConfiguracion() => _net.OpenAppSettings();
}
