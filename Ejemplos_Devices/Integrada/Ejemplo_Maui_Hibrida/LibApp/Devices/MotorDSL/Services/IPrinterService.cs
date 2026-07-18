using LibApp.Devices.MotorDSL.Models;
using MotorDsl.Printing;

namespace LibApp.Devices.MotorDSL.Services;

/// <summary>
/// Costura entre <c>PrinterOverlayViewModel</c> y la plataforma. Ver
/// <see cref="GPS.Services.IGpsService"/> para el fundamento; acá además aísla
/// <c>Preferences</c> (predeterminada y alias) y el transporte Bluetooth.
/// </summary>
public interface IPrinterService
{
    /// <summary>BT Classic SPP: sólo soportado en Android.</summary>
    bool IsSupported { get; }

    Task<BluetoothPermissionResult> EnsurePermissionsAsync();

    Task<DiscoverResult> DiscoverAsync();

    /// <summary>
    /// La predeterminada guardada, sólo si aparece en la lista detectada. «Presente» significa
    /// <b>emparejada</b>, no encendida: el fallo real recién aparece al conectar.
    /// </summary>
    PrinterDevice? GetDefaultIfPresent(IReadOnlyList<PrinterDevice> devices);

    Task<bool> ConnectAsync(PrinterDevice device);

    Task<PrintResult> SendAsync(byte[] bytes);

    void SaveDefault(PrinterDevice device);

    /// <summary>Olvida la predeterminada: la salida cuando el usuario cambió de impresora.</summary>
    void ClearDefault();

    /// <summary>Alias legible que el usuario le puso a esta impresora. Vacío si no tiene.</summary>
    string GetAlias(string deviceId);

    void SetAlias(string deviceId, string alias);

    void OpenAppSettings();

    /// <summary>Ajustes de Bluetooth del SO, distintos de los de la aplicación.</summary>
    void OpenBluetoothSettings();
}
