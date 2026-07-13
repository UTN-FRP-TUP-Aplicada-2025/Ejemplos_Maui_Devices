using LibApp.Devices.MotorDSL.Models;
using MotorDsl.Printing;

namespace LibApp.Devices.MotorDSL.Services;

/// <summary>
/// Servicio de alto nivel: compone permisos + descubrimiento + conexión +
/// envío sobre <see cref="IThermalPrinterService"/> y devuelve resultados
/// tipados. Espejo de CallService/GpsService. No referencia ningún ViewModel.
/// </summary>
public class PrinterService
{
    private readonly IThermalPrinterService _printer;

    public PrinterService(IThermalPrinterService printer)
    {
        _printer = printer;
    }

    /// <summary>BT Classic SPP: sólo soportado en Android.</summary>
    public bool IsSupported =>
#if ANDROID
        true;
#else
        false;
#endif

    /// <summary>
    /// Resuelve el permiso Bluetooth de forma awaitable (patrón Gps/Call:
    /// CheckStatusAsync / RequestAsync / ShouldShowRationale).
    /// </summary>
    public async Task<BluetoothPermissionResult> EnsurePermissionsAsync()
    {
#if ANDROID
        var status = await Permissions.CheckStatusAsync<BluetoothPermissions>();
        if (status == PermissionStatus.Granted) return BluetoothPermissionResult.Granted;

        status = await Permissions.RequestAsync<BluetoothPermissions>();
        if (status == PermissionStatus.Granted) return BluetoothPermissionResult.Granted;
        if (status == PermissionStatus.Restricted) return BluetoothPermissionResult.Restricted;

        return Permissions.ShouldShowRationale<BluetoothPermissions>()
            ? BluetoothPermissionResult.DeniedCanRetry
            : BluetoothPermissionResult.Denied;
#else
        await Task.CompletedTask;
        return BluetoothPermissionResult.Granted;
#endif
    }

    /// <summary>Descubre impresoras Bluetooth y normaliza el resultado.</summary>
    public async Task<DiscoverResult> DiscoverAsync()
    {
        if (!IsSupported) return new DiscoverResult.NotSupported();
        try
        {
            var devices = await _printer.DiscoverDevicesAsync(kind: "bluetooth");
            return devices.Count == 0
                ? new DiscoverResult.Empty()
                : new DiscoverResult.Found(devices);
        }
        catch (Exception ex)
        {
            return new DiscoverResult.BluetoothOff(ex.Message);
        }
    }

    /// <summary>
    /// Lee Preferences y devuelve el device guardado SOLO si está en la lista
    /// detectada (impresora predeterminada disponible).
    /// </summary>
    public PrinterDevice? GetDefaultIfPresent(IReadOnlyList<PrinterDevice> devices)
    {
        var id = Preferences.Default.Get("default_printer_id", string.Empty);
        if (string.IsNullOrEmpty(id)) return null;
        return devices.FirstOrDefault(d => d.Id == id);
    }

    /// <summary>Conecta y, si tiene éxito, memoriza la predeterminada.</summary>
    public async Task<bool> ConnectAsync(PrinterDevice device)
    {
        var ok = await _printer.ConnectAsync(device);
        if (ok) SaveDefault(device);   // C: auto-guardar al conectar
        return ok;
    }

    /// <summary>Envía los bytes ESC/POS ya renderizados.</summary>
    public async Task<PrintResult> SendAsync(byte[] bytes)
    {
        try
        {
            await _printer.SendBytesAsync(bytes);
            return new PrintResult.Success();
        }
        catch (Exception ex)
        {
            return new PrintResult.Failure(ex.Message);
        }
    }

    public void SaveDefault(PrinterDevice device)
    {
        Preferences.Default.Set("default_printer_id", device.Id);
        Preferences.Default.Set("default_printer_name", device.Name ?? "");
    }

    public void OpenAppSettings() => AppInfo.Current.ShowSettingsUI();
}
