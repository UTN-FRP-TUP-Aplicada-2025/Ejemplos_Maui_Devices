using System.Net.Sockets;

using Ejemplo_Maui_Hibrida.Models;

using Microsoft.Maui.Networking;

namespace Ejemplo_Maui_Hibrida.Services;

/// <summary>
/// Servicio de red: expone el estado de conectividad del SO y una sonda
/// activa que valida si hay internet REAL (no sólo enlace).
/// </summary>
public class NetworkService
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
    /// </summary>
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
                return new NetworkResult.HttpFailure((int)response.StatusCode, ProbeUrl);

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
            return new NetworkResult.Timeout(ProbeUrl);
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException se &&
            (se.SocketErrorCode == SocketError.HostNotFound ||
             se.SocketErrorCode == SocketError.TryAgain ||
             se.SocketErrorCode == SocketError.NoData))
        {
            return new NetworkResult.DnsFailure(new Uri(ProbeUrl).Host);
        }
        catch (HttpRequestException ex)
        {
            return new NetworkResult.RequestFailure(ex.Message);
        }
    }
}
