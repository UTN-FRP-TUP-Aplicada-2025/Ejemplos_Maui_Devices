using LibApp.Devices.Networks.Models;
using System.Net.Sockets;

namespace LibApp.Devices.Networks.Services;

/// <summary>
/// Servicio de red: expone el estado de conectividad del SO y una sonda
/// activa que valida si hay internet REAL (no sólo enlace).
/// </summary>
public class NetworkService : INetworkService
{
    // Endpoint de sonda que devuelve un cuerpo de texto conocido.
    // Validar el cuerpo contra el marcador permite detectar portales
    // cautivos / operadoras sin crédito que redirigen con 200 OK.
    private const string ProbeUrl = "http://www.msftconnecttest.com/connecttest.txt";
    private const string ProbeMarker = "Microsoft Connect Test";

    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(10);

    private readonly IConnectivity _connectivity;
    private readonly HttpClient _http = new();

    /// <summary>Se dispara al cambiar la conectividad del SO. online = hay acceso a Internet.</summary>
    public event Action<bool>? ConnectivityChanged;

    public NetworkService(IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    public bool IsOnline => _connectivity.NetworkAccess == NetworkAccess.Internet;

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        => ConnectivityChanged?.Invoke(e.NetworkAccess == NetworkAccess.Internet);

    /// <summary>
    /// Sonda activa de internet real. Devuelve un <see cref="NetworkResult"/> tipado.
    /// <para>
    /// La petición SIEMPRE va a <see cref="ProbeUrl"/>, no a <paramref name="url"/>: detectar un
    /// portal cautivo exige un endpoint de cuerpo conocido contra el que comparar. Pero lo que
    /// se <b>reporta</b> es el host de <paramref name="url"/>, que es el sitio que el usuario
    /// quiso abrir. Antes se reportaba el de la sonda, y el mensaje de DNS le nombraba
    /// «www.msftconnecttest.com» — un dominio que nunca visitó.
    /// </para>
    /// </summary>
    /// <param name="url">Sitio cuya navegación falló. Sólo se usa para describir el fallo.</param>
    public async Task<NetworkResult> CheckUrlAsync(string url, CancellationToken ct = default)
    {
        if (_connectivity.NetworkAccess == NetworkAccess.None)
            return new NetworkResult.Offline();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(ProbeTimeout);

        try
        {
            using var response = await _http.GetAsync(ProbeUrl, HttpCompletionOption.ResponseContentRead, cts.Token);

            if ((int)response.StatusCode >= 400)
                return new NetworkResult.HttpFailure((int)response.StatusCode, url);

            var body = await response.Content.ReadAsStringAsync(cts.Token);

            // Conectividad de enlace pero sin internet real: el cuerpo no es
            // el marcador esperado (portal cautivo / redirección de operadora).
            if (!body.Contains(ProbeMarker, StringComparison.OrdinalIgnoreCase))
                return new NetworkResult.Offline();

            return new NetworkResult.Online();
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // El cancel vino del timeout interno, no del llamador.
            return new NetworkResult.Timeout(url);
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException se &&
            (se.SocketErrorCode == SocketError.HostNotFound ||
             se.SocketErrorCode == SocketError.TryAgain ||
             se.SocketErrorCode == SocketError.NoData))
        {
            // Si el DNS no resuelve la sonda, tampoco resuelve el sitio: el host que le importa
            // al usuario es el suyo.
            return new NetworkResult.DnsFailure(HostDe(url));
        }
        catch (HttpRequestException ex)
        {
            return new NetworkResult.RequestFailure(ex.Message);
        }
    }

    public void OpenAppSettings() => AppInfo.ShowSettingsUI();

    /// <summary>Host legible de una URL; la URL entera si no se puede parsear.</summary>
    private static string HostDe(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var u) ? u.Host : url;
}
