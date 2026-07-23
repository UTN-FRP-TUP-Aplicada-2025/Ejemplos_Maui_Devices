using System.Globalization;
using System.Text.Json;

using LibApp.CustomWebView.Behaviors;
using LibApp.Devices.GPS.Models;
using LibApp.Devices.GPS.ViewModels;

namespace LibApp.UrlCommands.Handlers;

// Interpreta "coordenadas=coordenadas": pide geolocalización y entrega el resultado a la web.
//
// Entrega (ver ADR-0009 y Plan 1 §4, eje B):
//  - Con "param={id}" (camino web "Tomar Coordenadas"): Injection. INYECTA el texto en
//    element#{id} por JS, igual que cámara/QR/API. NO recarga la página: el overlay de espera
//    queda visible sobre la página estática y no hay parpadeo/hueco ni el problema de
//    "URL idéntica no re-navega".
//  - Sin "param" (botón nativo "Geo Pos"): Substitution. Re-navega la misma URL con las
//    coordenadas en la query, con un nonce para forzar el cambio de URL aunque la coordenada sea
//    idéntica a la anterior.
public sealed class GpsCommandHandler : IUrlCommandHandler
{
    private readonly GpsOverlayViewModel _gps;
    private readonly IWebViewBridge _bridge;

    // Secuencia monótona para el nonce de la re-navegación. Interlocked por si dos solicitudes
    // se solapan.
    private int _seq;

    public GpsCommandHandler(GpsOverlayViewModel gps, IWebViewBridge bridge)
    {
        _gps = gps;
        _bridge = bridge;
    }

    public bool CanHandle(string url) => url.Contains("coordenadas=coordenadas", StringComparison.OrdinalIgnoreCase);

    // Eje B (Plan 1 §4): el modo depende de la URL, no del handler.
    //  - con "param={id}"  → Injection   (camino web "Tomar Coordenadas")
    //  - sin "param"       → Substitution (botón nativo "Geo Pos" y todo consumidor que espere
    //                        la coordenada en la query, como APPGDA/GetURLConCoordenadas)
    public CommandDelivery DeliveryFor(string url) => string.IsNullOrEmpty(GetQueryValue(url, "param"))
            ? CommandDelivery.Substitution
            : CommandDelivery.Injection;

    public async Task<BridgeOutcome> HandleAsync(string url)
    {
        var modo = DeliveryFor(url);
        var result = await _gps.SolicitarGeolocalizacion();

        if (modo == CommandDelivery.Injection)
        {
            // Sin coordenada no hay nada que inyectar. La continuación es el overlay, que ya
            // muestra el error, y la página sigue viva porque se canceló la navegación.
            if (result is not GpsResult.Success ok)
                return new BridgeOutcome(true, null);

            // InvariantCulture: la web espera/parsea punto decimal. Con la cultura del dispositivo
            // (es-* → coma) la coordenada llegaría sin separador decimal (-31,74 → -3174). Ver ADR-0009.
            var texto = $"Latitud: {Inv(ok.Location.Latitude)}, Longitud: {Inv(ok.Location.Longitude)}";
            var targetId = GetQueryValue(url, "param");

            // Serializado con JsonSerializer para no romper el JS / evitar inyección.
            var script =
                $"var el=document.getElementById({JsonSerializer.Serialize(targetId)});" +
                $"if(el){{el.textContent={JsonSerializer.Serialize(texto)};}}";
            _bridge.RunScript(script);

            return new BridgeOutcome(true, null);   // se queda en la página
        }

        // ── Substitution ────────────────────────────────────────────────────────────────
        // SIEMPRE re-navega, haya o no coordenada: la navegación original fue cancelada y esta
        // es la única continuación posible. Sin coordenada se sustituye por el centinela 0.0/0.0,
        // igual que APPGDA/Services/NotionsLocationsService.cs:145-149. La web ya interpreta ese
        // centinela como "sin coordenada" (Ejemplo_ws_Blazor/Components/Pages/Panel.razor:176).
        //
        // Antes del Plan 1, un fallo del dispositivo devolvía BridgeOutcome(true, null) sin mirar
        // el modo: navegación cancelada y sin re-navegar = página congelada. Ver Plan 1 §3.
        var (lat, lng) = result is GpsResult.Success s
            ? (Inv(s.Location.Latitude), Inv(s.Location.Longitude))
            : ("0.0", "0.0");

        // El nonce garantiza que la URL cambie SIEMPRE (si no, WebView.Source no dispara
        // PropertyChanged y la página no re-navega cuando la coordenada es idéntica a la anterior).
        // Blazor ignora el parámetro extra.
        var next = url.Replace("coordenadas=coordenadas",
            $"Latitud={lat}&Longitud={lng}&",
            StringComparison.OrdinalIgnoreCase);
        next += "_ts=" + Interlocked.Increment(ref _seq);

        return new BridgeOutcome(true, next);
    }

    private static string Inv(double v) => v.ToString(CultureInfo.InvariantCulture);

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
