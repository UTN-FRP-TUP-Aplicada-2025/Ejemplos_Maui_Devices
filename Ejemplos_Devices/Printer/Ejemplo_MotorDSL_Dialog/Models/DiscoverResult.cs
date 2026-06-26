using MotorDsl.Printing;

namespace Ejemplo_MotorDSL_Dialog.Models;

// Resultado tipado del descubrimiento de impresoras (espejo de CallResult/GpsResult).
// El ViewModel hace switch sobre estos casos y evita try/catch.
public abstract record DiscoverResult
{
    public sealed record Found(IReadOnlyList<PrinterDevice> Devices) : DiscoverResult;
    public sealed record Empty : DiscoverResult;
    public sealed record BluetoothOff(string Message) : DiscoverResult;
    public sealed record NotSupported : DiscoverResult;
}
