using Ejemplo_Maui_Hibrida.Tests.Fakes;
using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.MotorDSL.Models;
using LibApp.Devices.MotorDSL.ViewModels;
using MotorDsl.Core.Models;
using MotorDsl.Printing;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

/// <summary>
/// Impresión es el único dominio ya validado en dispositivo. Esta clase es, sobre todo, la
/// **red de no-regresión** de esa validación: si la armonización de los otros dominios rompe
/// algo acá, se entera antes de llegar a un teléfono.
/// </summary>
public class PrinterOverlayTests
{
    private static readonly PrinterDevice Mostrador = new("AA:BB:CC:DD:3F:A2", "58HB6", "bluetooth", IsPaired: true);
    private static readonly PrinterDevice Deposito  = new("AA:BB:CC:DD:7B:14", "58HB6", "bluetooth", IsPaired: true);

    private static RenderResult RenderOk() => new("escpos-bitmap", new byte[] { 0x1B, 0x40 });

    private static PrinterOverlayViewModel Crear(FakePrinterService svc) => new(svc);

    // --- Camino feliz ---------------------------------------------------------------------

    [Fact]
    public async Task Con_una_sola_impresora_imprime_sin_preguntar()
    {
        var svc = new FakePrinterService { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }) };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Equal(OverlayMode.None, vm.Mode);
    }

    // El criterio del equipo sobre H-5: el flujo normal no se interrumpe. Si hay predeterminada
    // emparejada, se reusa y se imprime — sin selector.
    [Fact]
    public async Task La_predeterminada_se_reusa_sin_interrumpir_al_usuario()
    {
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.Found(new[] { Mostrador, Deposito }),
            Predeterminada = Deposito,
        };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Equal(OverlayMode.None, vm.Mode);
    }

    // --- Selector y desambiguación --------------------------------------------------------

    [Fact]
    public async Task Con_varias_y_sin_predeterminada_muestra_el_selector()
    {
        var svc = new FakePrinterService { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador, Deposito }) };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Equal(OverlayMode.Error, vm.Mode);
        Assert.Equal("Elegí una impresora", vm.Title);
    }

    // H-4: dos 58HB6 producían dos botones idénticos y el usuario elegía a ciegas.
    [Fact]
    public async Task Dos_impresoras_homonimas_se_distinguen_por_sufijo_de_MAC()
    {
        var svc = new FakePrinterService { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador, Deposito }) };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        var etiquetas = vm.Actions.Select(a => a.Text).ToList();
        Assert.Contains("58HB6 (3F:A2)", etiquetas);
        Assert.Contains("58HB6 (7B:14)", etiquetas);
    }

    // Privacidad: se muestran 2 octetos, no la MAC entera.
    [Fact]
    public async Task El_sufijo_no_expone_la_MAC_completa()
    {
        var svc = new FakePrinterService { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador, Deposito }) };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.All(vm.Actions, a => Assert.DoesNotContain(Mostrador.Id, a.Text));
    }

    [Fact]
    public async Task El_alias_del_usuario_reemplaza_al_nombre_del_modelo()
    {
        var svc = new FakePrinterService { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador, Deposito }) };
        svc.SetAlias(Mostrador.Id, "Mostrador");
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Contains(vm.Actions, a => a.Text == "Mostrador");
    }

    // Nombres distintos ⇒ no se ensucia con el sufijo.
    [Fact]
    public async Task Sin_homonimia_no_se_agrega_sufijo()
    {
        var otra = new PrinterDevice("11:22:33:44:55:66", "MTP-II", "bluetooth", IsPaired: true);
        var svc = new FakePrinterService { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador, otra }) };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Contains(vm.Actions, a => a.Text == "58HB6");
        Assert.Contains(vm.Actions, a => a.Text == "MTP-II");
    }

    // --- H-6: el bucle sin salida ---------------------------------------------------------

    [Fact]
    public async Task Si_la_predeterminada_no_responde_nombra_las_dos_causas()
    {
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }),
            Predeterminada = Mostrador,
            ConexionOk = false,
        };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Contains(PrinterErrorCatalog.DeviceAbsent, vm.Message);
        Assert.Contains("apagada", vm.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("emparejada", vm.Message, StringComparison.OrdinalIgnoreCase);
    }

    // La corrección de H-6: sin esta salida, "Elegir otra" volvía a encontrar la predeterminada
    // en BondedDevices y reconectaba a la misma — un bucle que sólo se rompía desemparejando
    // desde Android.
    [Fact]
    public async Task Si_la_predeterminada_no_responde_se_puede_olvidar_y_emparejar_otra()
    {
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }),
            Predeterminada = Mostrador,
            ConexionOk = false,
        };
        var vm = Crear(svc);
        await vm.ImprimirAsync(RenderOk());

        var accion = Assert.Single(vm.Actions, a => a.Text.Contains("Olvidar"));
        accion.Command.Execute(null);

        Assert.Equal(1, svc.VecesClearDefault);
        Assert.Equal(1, svc.VecesOpenBluetoothSettings);
    }

    // "Elegir otra" sólo tiene sentido si hay otra a la que cambiar.
    [Fact]
    public async Task Con_una_sola_emparejada_no_se_ofrece_elegir_otra()
    {
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }),
            Predeterminada = Mostrador,
            ConexionOk = false,
        };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.DoesNotContain(vm.Actions, a => a.Text.Contains("Elegir otra"));
    }

    [Fact]
    public async Task Con_varias_emparejadas_se_ofrece_elegir_otra()
    {
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.Found(new[] { Mostrador, Deposito }),
            Predeterminada = Mostrador,
            ConexionOk = false,
        };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Contains(vm.Actions, a => a.Text.Contains("Elegir otra"));
    }

    // --- Errores de envío -----------------------------------------------------------------

    [Fact]
    public async Task Sin_papel_el_boton_confirma_el_gesto_fisico()
    {
        var causa = new PrinterHardwareException("paper out");
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }),
            Envio = new PrintResult.Failure(PrinterErrorCatalog.Describe(causa, "Print failed after 1 attempt(s): paper out")),
        };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Equal("Sin papel", vm.Title);
        Assert.Contains(vm.Actions, a => a.Text == "Ya cargué papel — Reintentar");
    }

    // I-3: el usuario leía literalmente "Print failed after 1 attempt(s): paper out".
    [Fact]
    public async Task Sin_papel_no_le_muestra_al_usuario_el_texto_de_la_libreria()
    {
        var causa = new PrinterHardwareException("paper out");
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }),
            Envio = new PrintResult.Failure(PrinterErrorCatalog.Describe(causa, "Print failed after 1 attempt(s): paper out")),
        };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Invariantes.NoFiltraTextoTecnico(vm, "paper out", "Sin papel");
        Invariantes.NoFiltraTextoTecnico(vm, "Print failed", "Sin papel");
    }

    // --- H-2: los estados que estaban muertos ---------------------------------------------

    [Fact]
    public async Task Bluetooth_apagado_ofrece_activarlo()
    {
        var svc = new FakePrinterService
        {
            Descubrimiento = new DiscoverResult.BluetoothOff(PrinterErrorCatalog.BluetoothApagado("IsEnabled == false")),
        };
        var vm = Crear(svc);
        await vm.ImprimirAsync(RenderOk());

        Assert.Contains(PrinterErrorCatalog.BtOff, vm.Message);

        var accion = Assert.Single(vm.Actions, a => a.Text.Contains("Activar Bluetooth"));
        accion.Command.Execute(null);
        Assert.Equal(1, svc.VecesOpenBluetoothSettings);
    }

    [Fact]
    public async Task Sin_impresoras_sugiere_emparejar_no_solo_encender()
    {
        var svc = new FakePrinterService { Descubrimiento = new DiscoverResult.Empty() };
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Assert.Contains(PrinterErrorCatalog.DeviceNone, vm.Message);
        Assert.Contains(vm.Actions, a => a.Text.Contains("Emparejar"));
    }

    // --- H-1: el render fallido tiene que llegar al overlay --------------------------------

    [Fact]
    public async Task Un_render_fallido_se_comunica_y_no_toca_la_impresora()
    {
        var render = new RenderResult("escpos-bitmap");
        render.Errors.Add("nodo desconocido");
        var svc = new FakePrinterService();
        var vm = Crear(svc);

        await vm.ImprimirAsync(render);

        Assert.Equal(OverlayMode.Error, vm.Mode);
        Assert.Contains(PrinterErrorCatalog.DocRender, vm.Message);
    }

    [Fact]
    public void Un_fallo_de_documento_reintentable_rehace_el_GET()
    {
        var vm = Crear(new FakePrinterService());
        var veces = 0;

        vm.MostrarFalloDocumento(PrinterErrorCatalog.DocumentoSinConexion("timeout"), () => { veces++; return Task.CompletedTask; });

        var accion = Assert.Single(vm.Actions, a => a.Text == "Reintentar");
        accion.Command.Execute(null);

        Assert.Equal(1, veces);
    }

    // Contrato inválido: reintentar daría el mismo resultado, así que no se ofrece.
    [Fact]
    public void Un_contrato_invalido_no_ofrece_reintentar()
    {
        var vm = Crear(new FakePrinterService());

        vm.MostrarFalloDocumento(PrinterErrorCatalog.DocumentoInvalido("Root.Type ausente"));

        Assert.DoesNotContain(vm.Actions, a => a.Text == "Reintentar");
    }

    // --- I-4 sobre todas las pantallas de impresión ----------------------------------------

    [Theory]
    [MemberData(nameof(EscenariosDeError))]
    public async Task Toda_pantalla_tiene_un_unico_boton_primario(FakePrinterService svc, string caso)
    {
        var vm = Crear(svc);

        await vm.ImprimirAsync(RenderOk());

        Invariantes.TieneUnSoloPrimario(vm, caso);
    }

    public static IEnumerable<object[]> EscenariosDeError() => new List<object[]>
    {
        new object[] { new FakePrinterService { IsSupported = false }, "Plataforma no soportada" },
        new object[] { new FakePrinterService { Permiso = BluetoothPermissionResult.DeniedCanRetry }, "Permiso reintentable" },
        new object[] { new FakePrinterService { Permiso = BluetoothPermissionResult.Denied }, "Permiso denegado" },
        new object[] { new FakePrinterService { Permiso = BluetoothPermissionResult.Restricted }, "Permiso restringido" },
        new object[] { new FakePrinterService { Descubrimiento = new DiscoverResult.Empty() }, "Sin impresoras" },
        new object[] { new FakePrinterService { Descubrimiento = new DiscoverResult.NotSupported() }, "Discover no soportado" },
        new object[] { new FakePrinterService
            { Descubrimiento = new DiscoverResult.BluetoothOff(PrinterErrorCatalog.BluetoothApagado("x")) }, "Bluetooth apagado" },
        new object[] { new FakePrinterService
            { Descubrimiento = new DiscoverResult.PermissionRevoked(PrinterErrorCatalog.PermisoRevocado("x")) }, "Permiso revocado" },
        new object[] { new FakePrinterService
            { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }), ConexionOk = false }, "No conecta (elegida)" },
        new object[] { new FakePrinterService
            { Descubrimiento = new DiscoverResult.Found(new[] { Mostrador }), Predeterminada = Mostrador, ConexionOk = false },
            "No conecta (predeterminada)" },
    };
}
