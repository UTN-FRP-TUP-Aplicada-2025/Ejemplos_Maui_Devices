using Ejemplo_Maui_Hibrida.Tests.Fakes;
using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.GPS.Models;
using LibApp.Devices.GPS.ViewModels;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

public class GpsOverlayTests
{
    private static (GpsOverlayViewModel vm, FakeGpsService svc) Crear(GpsResult resultado)
    {
        var svc = new FakeGpsService { Resultado = resultado };
        return (new GpsOverlayViewModel(svc), svc);
    }

    // --- Camino feliz ---------------------------------------------------------------------

    // Opción A (ADR-0009): en éxito el overlay se cierra de forma determinista y autocontenida,
    // sin depender de la re-navegación (que podía no disparar Navigated y colgar el overlay).
    [Fact]
    public async Task Exito_oculta_el_overlay()
    {
        var (vm, _) = Crear(new GpsResult.Success(new Location(-31.4, -64.2)));

        await vm.SolicitarGeolocalizacion();

        Assert.Equal(OverlayMode.None, vm.Mode);
    }

    // I-5 · el VM devuelve la variante que recibió. Antes colapsaba TODO lo no-Success en
    // Failure("") con mensaje vacío: las 7 variantes quedaban indistinguibles desde afuera.
    [Fact]
    public async Task Exito_devuelve_la_ubicacion_al_llamador()
    {
        var loc = new Location(-31.4, -64.2);
        var (vm, _) = Crear(new GpsResult.Success(loc));

        var r = await vm.SolicitarGeolocalizacion();

        var s = Assert.IsType<GpsResult.Success>(r);
        Assert.Equal(-31.4, s.Location.Latitude);
    }

    // --- I-5 ------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(VariantesNoSuccess))]
    public async Task No_colapsa_la_variante_al_devolverla(GpsResult entrada, string caso)
    {
        var (vm, _) = Crear(entrada);

        var salida = await vm.SolicitarGeolocalizacion();

        Assert.True(entrada.GetType() == salida.GetType(),
            $"I-5 · «{caso}»: entró {entrada.GetType().Name} y salió {salida.GetType().Name}. " +
            $"El llamador no puede distinguir la causa.");
    }

    // --- I-1 · el corazón: ninguna variante puede ser muda --------------------------------

    [Theory]
    [MemberData(nameof(VariantesNoSuccess))]
    public async Task Toda_variante_no_exitosa_muestra_una_pantalla(GpsResult entrada, string caso)
    {
        var (vm, _) = Crear(entrada);

        await vm.SolicitarGeolocalizacion();

        Invariantes.MuestraUnaPantalla(vm, caso);
    }

    // --- I-4 ------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(VariantesNoSuccess))]
    public async Task Toda_pantalla_tiene_un_unico_boton_primario(GpsResult entrada, string caso)
    {
        var (vm, _) = Crear(entrada);

        await vm.SolicitarGeolocalizacion();

        Invariantes.TieneUnSoloPrimario(vm, caso);
    }

    // --- I-3 ------------------------------------------------------------------------------

    [Fact]
    public async Task Un_fallo_no_le_muestra_al_usuario_el_texto_de_la_excepcion()
    {
        const string tecnico = "Object reference not set to an instance of an object.";
        var (vm, _) = Crear(new GpsResult.Failure(tecnico));

        await vm.SolicitarGeolocalizacion();

        Invariantes.NoFiltraTextoTecnico(vm, tecnico, "Failure");
    }

    // --- I-2 · defensa contra la variante que nadie muestra --------------------------------

    // Recorre por reflexión las variantes declaradas de GpsResult y verifica que la lista de
    // casos cubiertos por los tests esté completa. Si mañana alguien agrega una variante nueva
    // y no la mapea a una pantalla, este test falla — que es exactamente lo que el compilador
    // no hace y lo que dejó a BluetoothOff inalcanzable durante toda la vida del PoC.
    [Fact]
    public void Todas_las_variantes_declaradas_estan_cubiertas_por_los_tests()
    {
        var declaradas = Invariantes.VariantesDe<GpsResult>()
            .Select(t => t.Name)
            .Where(n => n != nameof(GpsResult.Success))
            .ToHashSet();

        var cubiertas = VariantesNoSuccess()
            .Select(d => ((GpsResult)d[0]).GetType().Name)
            .ToHashSet();

        var sinCubrir = declaradas.Except(cubiertas).ToList();

        Assert.True(sinCubrir.Count == 0,
            $"I-2 · Variantes de GpsResult sin pantalla verificada: {string.Join(", ", sinCubrir)}. " +
            $"Modelar el caso no es lo mismo que producirlo.");
    }

    // --- Comportamiento específico ---------------------------------------------------------

    [Fact]
    public async Task Permiso_denegado_reintentable_ofrece_pedirlo_de_nuevo()
    {
        var (vm, _) = Crear(new GpsResult.PermissionDenied(CanRetry: true));

        await vm.SolicitarGeolocalizacion();

        Assert.Contains(vm.Actions, a => a.Text.Contains("Pedir permiso"));
    }

    [Fact]
    public async Task Permiso_denegado_definitivo_lleva_a_los_ajustes_de_la_app()
    {
        var (vm, svc) = Crear(new GpsResult.PermissionDenied(CanRetry: false));
        await vm.SolicitarGeolocalizacion();

        var accion = Assert.Single(vm.Actions, a => a.Text.Contains("Abrir configuración"));
        accion.Command.Execute(null);

        Assert.Equal(1, svc.VecesOpenAppSettings);
    }

    // El permiso lo concede la app; el GPS se enciende desde el sistema. Son ajustes distintos:
    // mandar al usuario a los de la app cuando lo que falta es el GPS lo deja sin salida.
    [Fact]
    public async Task GPS_apagado_lleva_a_los_ajustes_de_ubicacion_del_SO()
    {
        var (vm, svc) = Crear(new GpsResult.GpsDisabled());
        await vm.SolicitarGeolocalizacion();

        var accion = Assert.Single(vm.Actions, a => a.Text.Contains("ubicación", StringComparison.OrdinalIgnoreCase));
        accion.Command.Execute(null);

        Assert.Equal(1, svc.VecesOpenLocationSettings);
    }

    [Fact]
    public async Task Sin_senal_ofrece_reintentar()
    {
        var (vm, _) = Crear(new GpsResult.NoSignal());

        await vm.SolicitarGeolocalizacion();

        Assert.Contains(vm.Actions, a => a.Text.Contains("Reintentar"));
    }

    public static IEnumerable<object[]> VariantesNoSuccess() => new List<object[]>
    {
        new object[] { new GpsResult.PermissionDenied(CanRetry: true),  "Permiso denegado (reintentable)" },
        new object[] { new GpsResult.PermissionDenied(CanRetry: false), "Permiso denegado (definitivo)" },
        new object[] { new GpsResult.PermissionRestricted(),            "Permiso restringido" },
        new object[] { new GpsResult.GpsDisabled(),                     "GPS apagado" },
        new object[] { new GpsResult.NotSupported(),                    "GPS no soportado" },
        new object[] { new GpsResult.NoSignal(),                        "Sin señal" },
        new object[] { new GpsResult.Cancelled(),                       "Cancelado" },
        new object[] { new GpsResult.Failure("boom"),                   "Fallo genérico" },
    };
}
