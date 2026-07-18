using System.Net.Sockets;
using LibApp.Devices.MotorDSL.Models;
using MotorDsl.Core.Models;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

/// <summary>
/// El catálogo de códigos de impresión: la pieza que traduce lo que la librería reporta a lo
/// que el usuario puede hacer. Es la única función pura del patrón, y la que más se rompe en
/// silencio: depende de literales de `MotorDsl.Bluetooth` que pueden cambiar sin avisar.
/// </summary>
public class PrinterErrorCatalogTests
{
    // ThermalPrinterService envuelve el fallo real en un Exception genérico
    // ("Print failed after N attempt(s): …") pero conserva la causa en InnerException.
    // Es exactamente lo que PrinterService.SendAsync desenvuelve antes de clasificar.
    private static Exception ComoLaLibreria(Exception causa)
        => new Exception($"Print failed after 1 attempt(s): {causa.Message}", causa);

    [Fact]
    public void Falta_de_papel_da_PRN_HW_PAPER()
    {
        var causa = new PrinterHardwareException("paper out");
        var f = PrinterErrorCatalog.Describe(causa, ComoLaLibreria(causa).Message);

        Assert.Equal(PrinterErrorCatalog.HwPaper, f.Code);
        Assert.Equal("Sin papel", f.Title);
    }

    [Fact]
    public void Tapa_abierta_da_PRN_HW_COVER()
    {
        var causa = new PrinterHardwareException("cover open");
        var f = PrinterErrorCatalog.Describe(causa, ComoLaLibreria(causa).Message);

        Assert.Equal(PrinterErrorCatalog.HwCover, f.Code);
    }

    // Hardware sin causa reconocida NO debe caer en PRN-UNKNOWN: la impresora sí reportó un
    // problema físico, y eso ya es accionable ("revisala").
    [Fact]
    public void Hardware_no_reconocido_da_PRN_HW_OTHER()
    {
        var causa = new PrinterHardwareException("something odd");
        var f = PrinterErrorCatalog.Describe(causa, "x");

        Assert.Equal(PrinterErrorCatalog.HwOther, f.Code);
    }

    [Theory]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(SocketException))]
    public void Fallos_de_enlace_dan_PRN_LINK_LOST(Type tipo)
    {
        var causa = (Exception)Activator.CreateInstance(tipo)!;
        var f = PrinterErrorCatalog.Describe(causa, "x");

        Assert.Equal(PrinterErrorCatalog.LinkLost, f.Code);
    }

    [Theory]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(TaskCanceledException))]
    public void Timeouts_dan_PRN_TIMEOUT(Type tipo)
    {
        var causa = (Exception)Activator.CreateInstance(tipo)!;
        var f = PrinterErrorCatalog.Describe(causa, "x");

        Assert.Equal(PrinterErrorCatalog.Timeout, f.Code);
    }

    [Fact]
    public void Causa_desconocida_da_PRN_UNKNOWN()
    {
        var f = PrinterErrorCatalog.Describe(new InvalidOperationException("raro"), "x");
        Assert.Equal(PrinterErrorCatalog.Unknown, f.Code);
    }

    // I-3, en la única pieza donde puede verificarse de forma pura.
    [Fact]
    public void El_mensaje_de_usuario_nunca_es_el_tecnico()
    {
        var causa = new PrinterHardwareException("paper out");
        var tecnico = ComoLaLibreria(causa).Message;
        var f = PrinterErrorCatalog.Describe(causa, tecnico);

        Assert.DoesNotContain("paper out", f.UserMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Print failed", f.UserMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("attempt", f.UserMessage, StringComparison.OrdinalIgnoreCase);
    }

    // …pero el original tiene que seguir accesible programáticamente: es el requisito que
    // habilita log y reporte automático de errores.
    [Fact]
    public void El_error_original_se_conserva_para_el_log()
    {
        var causa = new PrinterHardwareException("paper out");
        var tecnico = ComoLaLibreria(causa).Message;
        var f = PrinterErrorCatalog.Describe(causa, tecnico);

        Assert.Equal(tecnico, f.TechnicalMessage);
        Assert.Same(causa, f.Exception);
    }

    [Fact]
    public void DisplayMessage_lleva_el_codigo_para_dictarselo_a_soporte()
    {
        var f = PrinterErrorCatalog.SinImpresoras();
        Assert.Contains(f.UserMessage, f.DisplayMessage);
        Assert.Contains(f.Code, f.DisplayMessage);
    }

    [Fact]
    public void Los_codigos_del_catalogo_son_unicos()
    {
        var codigos = typeof(PrinterErrorCatalog)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        Assert.NotEmpty(codigos);
        Assert.Equal(codigos.Count, codigos.Distinct().Count());
        Assert.All(codigos, c => Assert.StartsWith("PRN-", c));
    }

    // La predeterminada ausente es el caso que el stack NO puede diagnosticar: "está apagada" y
    // "no es la que se emparejó" son el mismo fallo de socket. El mensaje debe nombrar las dos.
    [Fact]
    public void PRN_DEV_ABSENT_nombra_las_dos_causas_posibles()
    {
        var f = PrinterErrorCatalog.PredeterminadaAusente("Mostrador");

        Assert.Equal(PrinterErrorCatalog.DeviceAbsent, f.Code);
        Assert.Contains("Mostrador", f.UserMessage);
        Assert.Contains("apagada", f.UserMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("emparejada", f.UserMessage, StringComparison.OrdinalIgnoreCase);
    }
}
