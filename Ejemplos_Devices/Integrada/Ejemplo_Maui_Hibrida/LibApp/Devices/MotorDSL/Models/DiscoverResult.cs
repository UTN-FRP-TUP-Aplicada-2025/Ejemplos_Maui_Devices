using MotorDsl.Printing;

namespace LibApp.Devices.MotorDSL.Models;

// Resultado tipado del descubrimiento de impresoras (espejo de CallResult/GpsResult).
// El ViewModel hace switch sobre estos casos y evita try/catch.
//
// Cada variante debe ser ALCANZABLE: modelar el caso no es lo mismo que producirlo.
// BluetoothOff existió acá durante toda la vida del PoC sin que nadie la construyera nunca
// (el transport lanzaba, pero ThermalPrinterService se tragaba la excepción y devolvía lista
// vacía → siempre Empty). PrinterService.DiscoverAsync ahora chequea el adaptador y el
// permiso antes de descubrir, que es lo que hace alcanzables BluetoothOff y PermissionRevoked.
public abstract record DiscoverResult
{
    public sealed record Found(IReadOnlyList<PrinterDevice> Devices) : DiscoverResult;
    public sealed record Empty : DiscoverResult;
    public sealed record BluetoothOff(PrintFailure Error) : DiscoverResult;
    public sealed record PermissionRevoked(PrintFailure Error) : DiscoverResult;
    public sealed record NotSupported : DiscoverResult;
}
