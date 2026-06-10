using Ejemplo_Maui_Hibrida.Behaviors;
using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.Pages;
using System.Text.Json;

namespace Ejemplo_Maui_Hibrida.UrlCommands.Handlers;

// Interpreta "qr=qr&param={id}": abre el lector de QR, espera la lista de códigos
// leídos y la inyecta serializada en el DOM (element.textContent del {id} destino).
// "Volver" o lista vacía = cancelado (no inyecta). NO re-navega: se queda en la página.
public sealed class QrCommandHandler : IUrlCommandHandler
{
    private readonly IWebViewBridge _bridge;

    public QrCommandHandler(IWebViewBridge bridge)
    {
        _bridge = bridge;
    }

    public bool CanHandle(string url) =>
        url.Contains("qr=qr", StringComparison.OrdinalIgnoreCase);

    public async Task<BridgeOutcome> HandleAsync(string url)
    {
        var targetId = GetQueryValue(url, "param");
        if (string.IsNullOrEmpty(targetId))
            return new BridgeOutcome(true, null);

        // Navegar al lector y esperar la lista con TaskCompletionSource.
        var tcs = new TaskCompletionSource<List<QRContent>?>();
        Action<List<QRContent>?> callback = qrs => tcs.TrySetResult(qrs);

        await Shell.Current.GoToAsync(nameof(QRLectorPage),
            new ShellNavigationQueryParameters { { "OnQrCallback", callback } });

        var qrs = await tcs.Task;                 // null o vacío = canceló
        if (qrs is null || qrs.Count == 0)
            return new BridgeOutcome(true, null);

        // Inyectar la LISTA COMPLETA serializada (a prueba de comillas vía JsonSerializer):
        // la página Blazor decide si toma [0].Value o itera.
        var json = JsonSerializer.Serialize(qrs);
        string scriptjs =
            $"document.getElementById({JsonSerializer.Serialize(targetId)}).textContent = {JsonSerializer.Serialize(json)};";

        _bridge.RunScript(scriptjs);

        return new BridgeOutcome(true, null);     // se queda en la página
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
