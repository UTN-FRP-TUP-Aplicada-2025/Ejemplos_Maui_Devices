using System.Globalization;

using Ejemplo_Maui_Hibrida.Tests.Fakes;
using LibApp.Devices.GPS.Models;
using LibApp.Devices.GPS.ViewModels;
using LibApp.UrlCommands;
using LibApp.UrlCommands.Handlers;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

// ADR-0009 · D2 (cultura) + entrega del resultado GPS a la web.
public class GpsCommandHandlerTests
{
    private const double Lat = -31.7496689;
    private const double Lng = -60.5213019;

    private static (GpsCommandHandler h, List<string> scripts) Crear(double lat, double lng)
    {
        var gps = new GpsOverlayViewModel(new FakeGpsService
        {
            Resultado = new GpsResult.Success(new Location(lat, lng))
        });
        var bridge = new FakeWebViewBridge();
        var scripts = new List<string>();
        bridge.ScriptRequested += (_, js) => scripts.Add(js);
        return (new GpsCommandHandler(gps, bridge), scripts);
    }

    // Camino web ("Tomar Coordenadas", param presente): inyecta por JS SIN re-navegar, y las
    // coordenadas llevan PUNTO decimal aunque la cultura del dispositivo use coma.
    [Fact]
    public async Task Con_param_inyecta_por_js_sin_renavegar_y_con_punto_decimal()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-AR");   // coma como separador decimal
            var (h, scripts) = Crear(Lat, Lng);

            var outcome = await h.HandleAsync("/panel?coordenadas=coordenadas&param=contenidoCoordenada");

            Assert.True(outcome.CancelNavigation);
            Assert.Null(outcome.NavigateTo);                 // NO re-navega (no recarga la página)
            var js = Assert.Single(scripts);
            Assert.Contains("contenidoCoordenada", js);
            Assert.Contains("-31.7496689", js);              // punto, no coma
            Assert.DoesNotContain("-31,7496689", js);
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    // Fallback (botón nativo "Geo Pos", sin param): re-navega con las coordenadas invariantes + nonce.
    [Fact]
    public async Task Sin_param_renavega_con_coordenadas_invariantes_y_nonce()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-AR");
            var (h, scripts) = Crear(Lat, Lng);

            var outcome = await h.HandleAsync("/panel?coordenadas=coordenadas");

            Assert.Empty(scripts);                           // no inyecta
            Assert.NotNull(outcome.NavigateTo);
            Assert.Contains("Latitud=-31.7496689", outcome.NavigateTo);
            Assert.Contains("_ts=", outcome.NavigateTo);     // nonce
            Assert.DoesNotContain("coordenadas=coordenadas", outcome.NavigateTo);
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    // El nonce garantiza que la URL cambie SIEMPRE (si no, WebView.Source no re-navega ante una
    // coordenada idéntica y la página no vuelve a mostrar el dato).
    [Fact]
    public async Task El_nonce_hace_la_url_distinta_en_cada_llamada()
    {
        var (h, _) = Crear(Lat, Lng);

        var u1 = (await h.HandleAsync("/panel?coordenadas=coordenadas")).NavigateTo;
        var u2 = (await h.HandleAsync("/panel?coordenadas=coordenadas")).NavigateTo;

        Assert.NotEqual(u1, u2);
    }

    // Plan 1 §3: en modo Substitution (sin "param") un fallo del dispositivo DEBE re-navegar igual,
    // con el centinela 0.0/0.0. Antes devolvía (true, null) y la página quedaba congelada: navegación
    // cancelada y sin re-navegación.
    [Fact]
    public async Task Sin_param_y_sin_senal_renavega_igual_con_centinela_cero()
    {
        var gps = new GpsOverlayViewModel(new FakeGpsService { Resultado = new GpsResult.NoSignal() });
        var h = new GpsCommandHandler(gps, new FakeWebViewBridge());

        var outcome = await h.HandleAsync("/panel?coordenadas=coordenadas");

        Assert.True(outcome.CancelNavigation);
        Assert.NotNull(outcome.NavigateTo);                          // ← lo que el defecto NO hacía
        Assert.Contains("Latitud=0.0", outcome.NavigateTo);
        Assert.Contains("Longitud=0.0", outcome.NavigateTo);
        Assert.DoesNotContain("coordenadas=coordenadas", outcome.NavigateTo);
    }

    // En modo Injection, en cambio, un fallo NO re-navega: la continuación es el overlay y la página
    // sigue viva porque la navegación se canceló.
    [Fact]
    public async Task Con_param_y_sin_senal_no_renavega_ni_inyecta()
    {
        var gps = new GpsOverlayViewModel(new FakeGpsService { Resultado = new GpsResult.NoSignal() });
        var bridge = new FakeWebViewBridge();
        var scripts = new List<string>();
        bridge.ScriptRequested += (_, js) => scripts.Add(js);
        var h = new GpsCommandHandler(gps, bridge);

        var outcome = await h.HandleAsync("/panel?coordenadas=coordenadas&param=contenidoCoordenada");

        Assert.True(outcome.CancelNavigation);
        Assert.Null(outcome.NavigateTo);
        Assert.Empty(scripts);
    }

    // El modo se deduce de la URL, no del handler (Plan 1 §4, eje B).
    [Theory]
    [InlineData("/panel?coordenadas=coordenadas",                           CommandDelivery.Substitution)]
    [InlineData("/panel?coordenadas=coordenadas&param=contenidoCoordenada", CommandDelivery.Injection)]
    public void DeliveryFor_distingue_los_dos_modos(string url, CommandDelivery esperado)
    {
        var h = new GpsCommandHandler(new GpsOverlayViewModel(new FakeGpsService()), new FakeWebViewBridge());

        Assert.Equal(esperado, h.DeliveryFor(url));
    }
}
