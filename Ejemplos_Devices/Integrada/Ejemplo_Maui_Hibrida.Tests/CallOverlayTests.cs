using Ejemplo_Maui_Hibrida.Tests.Fakes;
using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.Phone.Models;
using LibApp.Devices.Phone.ViewModels;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

public class CallOverlayTests
{
    private const string Numero = "3434807427";

    private static (CallOverlayViewModel vm, FakeCallService svc) Crear(CallResult resultado)
    {
        var svc = new FakeCallService { Resultado = resultado };
        return (new CallOverlayViewModel(svc), svc);
    }

    [Fact]
    public async Task Exito_oculta_el_overlay()
    {
        var (vm, _) = Crear(new CallResult.Success(Numero, CallMode.Direct));

        await vm.LlamarAsync(Numero);

        Assert.Equal(OverlayMode.None, vm.Mode);
    }

    [Theory]
    [MemberData(nameof(VariantesNoSuccess))]
    public async Task Toda_variante_no_exitosa_muestra_una_pantalla(CallResult entrada, string caso)
    {
        var (vm, _) = Crear(entrada);

        await vm.LlamarAsync(Numero);

        Invariantes.MuestraUnaPantalla(vm, caso);
    }

    [Theory]
    [MemberData(nameof(VariantesNoSuccess))]
    public async Task Toda_pantalla_tiene_un_unico_boton_primario(CallResult entrada, string caso)
    {
        var (vm, _) = Crear(entrada);

        await vm.LlamarAsync(Numero);

        Invariantes.TieneUnSoloPrimario(vm, caso);
    }

    // I-3 · el defecto que impresión ya corrigió y telefonía no: el usuario ve el texto de una
    // excepción de Android, en inglés y sin acción posible.
    [Fact]
    public async Task Un_fallo_no_le_muestra_al_usuario_el_texto_de_la_excepcion()
    {
        const string tecnico = "Unable to resolve activity for Intent { act=android.intent.action.CALL }";
        var (vm, _) = Crear(new CallResult.Failure(tecnico));

        await vm.LlamarAsync(Numero);

        Invariantes.NoFiltraTextoTecnico(vm, tecnico, "Failure");
    }

    [Fact]
    public void Todas_las_variantes_declaradas_estan_cubiertas_por_los_tests()
    {
        var declaradas = Invariantes.VariantesDe<CallResult>()
            .Select(t => t.Name)
            .Where(n => n != nameof(CallResult.Success))
            .ToHashSet();

        var cubiertas = VariantesNoSuccess()
            .Select(d => ((CallResult)d[0]).GetType().Name)
            .ToHashSet();

        var sinCubrir = declaradas.Except(cubiertas).ToList();

        Assert.True(sinCubrir.Count == 0,
            $"I-2 · Variantes de CallResult sin pantalla verificada: {string.Join(", ", sinCubrir)}.");
    }

    [Fact]
    public async Task Permiso_denegado_definitivo_lleva_a_los_ajustes_de_la_app()
    {
        var (vm, svc) = Crear(new CallResult.PermissionDenied(CanRetry: false));
        await vm.LlamarAsync(Numero);

        var accion = Assert.Single(vm.Actions, a => a.Text.Contains("Abrir configuración"));
        accion.Command.Execute(null);

        Assert.Equal(1, svc.VecesOpenAppSettings);
    }

    // El VM es singleton: guarda el último número para que el reintento funcione aunque el
    // caller original ya no exista.
    [Fact]
    public async Task El_reintento_reusa_el_ultimo_numero()
    {
        var (vm, svc) = Crear(new CallResult.Failure("boom"));
        await vm.LlamarAsync(Numero);

        var accion = Assert.Single(vm.Actions, a => a.Text.Contains("Reintentar"));
        accion.Command.Execute(null);

        Assert.Equal(Numero, svc.UltimoNumero);
    }

    public static IEnumerable<object[]> VariantesNoSuccess() => new List<object[]>
    {
        new object[] { new CallResult.PermissionDenied(CanRetry: true),  "Permiso denegado (reintentable)" },
        new object[] { new CallResult.PermissionDenied(CanRetry: false), "Permiso denegado (definitivo)" },
        new object[] { new CallResult.PermissionRestricted(),            "Permiso restringido" },
        new object[] { new CallResult.NotSupported(),                    "No soportado" },
        new object[] { new CallResult.InvalidNumber(),                   "Número inválido" },
        new object[] { new CallResult.Cancelled(),                       "Cancelado" },
        new object[] { new CallResult.Failure("boom"),                   "Fallo genérico" },
    };
}
