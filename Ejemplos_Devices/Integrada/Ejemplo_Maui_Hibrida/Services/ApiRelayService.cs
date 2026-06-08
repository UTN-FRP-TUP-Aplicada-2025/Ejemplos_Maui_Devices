using System.Text;

using Ejemplo_Maui_Hibrida.Models;

namespace Ejemplo_Maui_Hibrida.Services;

/// <summary>
/// Relay REST genérico: reenvía un request (verbo + url + body opcional) que
/// arma la página y devuelve un <see cref="ApiCallResult"/> tipado. No conoce
/// ViewModels ni el WebView: sólo ejecuta la petición y mapea el resultado.
/// </summary>
public class ApiRelayService
{
    // Guardrail: sólo se permiten requests a estos hosts. Cualquier otro => Blocked.
    private static readonly HashSet<string> HostsPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "geolocate.somee.com",
    };

    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    private readonly HttpClient _http = new();

    /// <summary>
    /// Ejecuta el request. <paramref name="jsonBody"/> sólo se envía en POST
    /// (Content-Type: application/json); en GET se ignora.
    /// </summary>
    public async Task<ApiCallResult> SendAsync(HttpMethod method, string url, string? jsonBody, CancellationToken ct = default)
    {
        // URL inválida o host fuera de la allowlist: no se hace el request.
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !HostsPermitidos.Contains(uri.Host))
            return new ApiCallResult.Blocked();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(RequestTimeout);

        using var request = new HttpRequestMessage(method, uri);
        if (method == HttpMethod.Post && !string.IsNullOrEmpty(jsonBody))
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            var status = (int)response.StatusCode;

            return status >= 400
                ? new ApiCallResult.HttpError(status, body)
                : new ApiCallResult.Success(status, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // El cancel vino del llamador, no del timeout interno.
            return new ApiCallResult.Cancelled();
        }
        catch (OperationCanceledException)
        {
            return new ApiCallResult.Timeout();
        }
        catch (HttpRequestException ex)
        {
            return new ApiCallResult.NetworkError(ex.Message);
        }
    }
}
