using Ejemplo_Maui_Hibrida.Behaviors;
using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.Services;
using System.Text.Json;
using static QRCoder.PayloadGenerator;

namespace Ejemplo_Maui_Hibrida.UrlCommands.Handlers;

// Interpreta "sendAPI=sendAPI&tipoRequest=Post|Get&url={enc}&param={callId}&body={enc}":
// reenvía el request vía ApiRelayService y devuelve el resultado a la página llamando
// al hook window.recibirRespuestaApi(callId, resultado). NO re-navega: se queda en la página.
public sealed class SendApiCommandHandler : IUrlCommandHandler
{
    private readonly ApiRelayService _api;
    private readonly IWebViewBridge _bridge;

    public SendApiCommandHandler(ApiRelayService api, IWebViewBridge bridge)
    {
        _api = api;
        _bridge = bridge;
    }

    public bool CanHandle(string url) =>
        url.Contains("sendApi=sendApi", StringComparison.OrdinalIgnoreCase);

    public async Task<BridgeOutcome> HandleAsync(string url)
    {
        var callId = GetQueryValue(url, "param") ?? "";
        var verbo = GetQueryValue(url, "httpMethod");

        //"https://geolocate.somee.com/panel?sendAPI=sendAPI&httpMethod=Post&url=https%3a%2f%2fgeolocate.somee.com%2fapi%2fGeoReporter%2ftrack&param=apiPost1&body=%7b%22Latitude%22%3a-31.7496689%2c%22Longitude%22%3a-60.5213019%7d"
        var destino = GetQueryValue(url, "url");
        
        //"{\"Latitude\":-31.7496689,\"Longitude\":-60.5213019}"
        var body = GetQueryValue(url, "body");

        var method = ParseMethod(verbo);

        // Verbo desconocido o sin destino: no se llama al service (se trata como Blocked).
        ApiCallResult result = (method is null || string.IsNullOrEmpty(destino))
            ? new ApiCallResult.Blocked()
            : await _api.SendAsync(method, destino, body);

        EntregarAlHook(callId, result);

        return new BridgeOutcome(true, null);     // se queda en la página
    }

    // "Post"/"Get" (case-insensitive). Cualquier otro => null (se trata como Blocked).
    private static HttpMethod? ParseMethod(string? verbo)
    {
        if (string.Equals(verbo, "Post", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Post;
        if (string.Equals(verbo, "Get", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Get;
        return null;
    }

    private void EntregarAlHook(string callId, ApiCallResult result)
    {
        (bool ok, int status, string? body) = result switch
        {
            ApiCallResult.Success s => (true, s.Status, s.Body),
            ApiCallResult.HttpError h => (false, h.Status, h.Body),
            ApiCallResult.NetworkError n => (false, 0, n.Message),
            ApiCallResult.Timeout => (false, 0, "La petición superó el tiempo de espera."),
            ApiCallResult.Cancelled => (false, 0, "La petición fue cancelada."),
            ApiCallResult.Blocked => (false, 0, "El host no está permitido."),
            _ => (false, 0, "Error desconocido."),
        };

        // Serializar con System.Text.Json: no interpolar el body crudo (evita romper el JS / inyección).
        //var payload = new { ok, status, body };
        //var js = $"window.recibirRespuestaApi({JsonSerializer.Serialize(callId)}, {JsonSerializer.Serialize(payload)});";
        //_bridge.RunScript(js);

        var payload = new { ok, status, body };
        string scriptjs = $@"
        document.getElementById('{callId}').textContent= '{JsonSerializer.Serialize(payload)}';";

        // Opción 1: Reemplaza el contenido incluyendo etiquetas HTML si las hay
        //document.getElementById('miDiv').innerHTML = '<strong>Nuevo contenido</strong> con HTML';
        // Opción 2: Reemplaza solo el texto plano (más seguro contra ataques XSS)
        //document.getElementById('miDiv').textContent = 'Nuevo contenido en texto plano';

        _bridge.RunScript(scriptjs);
    }

    private static string? GetQueryValue(string url, string key)
    {
        var q = url.Contains('?') ? url[(url.IndexOf('?') + 1)..] : url;
        foreach (var pair in q.Split('&'))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }
}
