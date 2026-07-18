using Ejemplo_Maui_Hibrida.Tests.Fakes;
using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.Networks.Models;
using LibApp.Devices.Networks.ViewModels;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

/// <summary>
/// El único overlay reactivo: se suscribe a la conectividad y puede aparecer sin que nadie lo
/// invoque. Su flag `_needsReload` distingue «se cortó la red con la página ya cargada» (basta
/// destapar) de «la navegación falló» (hay que recargar) — una distinción que no aplica a los
/// dispositivos que no cambian de estado por su cuenta.
/// </summary>
public class NetworkOverlayTests
{
    private const string Url = "https://aplicada.somee.com/";

    private static (NetworkOverlayViewModel vm, FakeNetworkService net, FakeWebViewBridge bridge) Crear(
        NetworkResult sonda)
    {
        var net = new FakeNetworkService { Resultado = sonda };
        var bridge = new FakeWebViewBridge();
        return (new NetworkOverlayViewModel(net, bridge, new FakeUiDispatcher()), net, bridge);
    }

    [Fact]
    public void Arranca_oculto()
    {
        var (vm, _, _) = Crear(new NetworkResult.Online());
        Assert.Equal(OverlayMode.None, vm.Mode);
    }

    [Fact]
    public void Una_navegacion_exitosa_oculta_el_overlay()
    {
        var (vm, _, _) = Crear(new NetworkResult.Online());

        vm.NotifyNavigationSucceeded(Url);

        Assert.Equal(OverlayMode.None, vm.Mode);
    }

    // --- Reactividad: la característica que lo distingue de los otros tres -----------------

    [Fact]
    public void Se_muestra_solo_al_perderse_la_conexion_sin_que_nadie_lo_invoque()
    {
        var (vm, net, _) = Crear(new NetworkResult.Online());

        net.EmitirConectividad(false);

        Assert.Equal(OverlayMode.Error, vm.Mode);
        Assert.Contains("Sin conexión", vm.Title);
    }

    // _needsReload == false: corte con la página ya renderizada ⇒ sólo destapar.
    [Fact]
    public void Al_volver_la_red_con_la_pagina_cargada_solo_destapa_sin_recargar()
    {
        var (vm, net, bridge) = Crear(new NetworkResult.Online());
        vm.NotifyNavigationSucceeded(Url);
        net.EmitirConectividad(false);

        net.EmitirConectividad(true);

        Assert.Equal(OverlayMode.None, vm.Mode);
        Assert.Equal(0, bridge.VecesReload);
    }

    // _needsReload == true: la navegación falló ⇒ hay que refrescar el WebView.
    [Fact]
    public async Task Al_volver_la_red_tras_un_fallo_de_navegacion_recarga_sola()
    {
        var (vm, net, bridge) = Crear(new NetworkResult.Offline());
        await vm.NotifyNavigationFailedAsync(Url, WebNavigationResult.Failure);

        net.EmitirConectividad(true);

        Assert.Equal(1, bridge.VecesReload);
    }

    // --- I-1 e I-4 sobre las variantes de la sonda ----------------------------------------

    [Theory]
    [MemberData(nameof(VariantesDeFallo))]
    public async Task Toda_variante_de_fallo_muestra_una_pantalla(NetworkResult sonda, string caso)
    {
        var (vm, _, _) = Crear(sonda);

        await vm.NotifyNavigationFailedAsync(Url, WebNavigationResult.Failure);

        Invariantes.MuestraUnaPantalla(vm, caso);
    }

    [Theory]
    [MemberData(nameof(VariantesDeFallo))]
    public async Task Toda_pantalla_tiene_un_unico_boton_primario(NetworkResult sonda, string caso)
    {
        var (vm, _, _) = Crear(sonda);

        await vm.NotifyNavigationFailedAsync(Url, WebNavigationResult.Failure);

        Invariantes.TieneUnSoloPrimario(vm, caso);
    }

    [Fact]
    public void Todas_las_variantes_declaradas_estan_cubiertas_por_los_tests()
    {
        var declaradas = Invariantes.VariantesDe<NetworkResult>()
            .Select(t => t.Name)
            .Where(n => n != nameof(NetworkResult.Online))
            .ToHashSet();

        var cubiertas = VariantesDeFallo()
            .Select(d => ((NetworkResult)d[0]).GetType().Name)
            .ToHashSet();

        var sinCubrir = declaradas.Except(cubiertas).ToList();

        Assert.True(sinCubrir.Count == 0,
            $"I-2 · Variantes de NetworkResult sin pantalla verificada: {string.Join(", ", sinCubrir)}.");
    }

    [Fact]
    public async Task Una_sonda_exitosa_recarga_el_sitio()
    {
        var (vm, _, bridge) = Crear(new NetworkResult.Online());

        await vm.NotifyNavigationFailedAsync(Url, WebNavigationResult.Failure);

        Assert.Equal(1, bridge.VecesReload);
    }

    public static IEnumerable<object[]> VariantesDeFallo() => new List<object[]>
    {
        new object[] { new NetworkResult.Offline(),                        "Sin conexión" },
        new object[] { new NetworkResult.Timeout("http://probe"),          "Timeout" },
        new object[] { new NetworkResult.DnsFailure("probe.host"),         "Fallo de DNS" },
        new object[] { new NetworkResult.HttpFailure(503, "http://probe"), "Error HTTP" },
        new object[] { new NetworkResult.RequestFailure("boom"),           "Error de red" },
    };
}
