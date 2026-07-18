using System.Globalization;

using Ejemplo_Maui_Hibrida.Tests.Fakes;
using LibApp.Devices.GPS.Models;
using LibApp.Devices.GPS.ViewModels;
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
}
