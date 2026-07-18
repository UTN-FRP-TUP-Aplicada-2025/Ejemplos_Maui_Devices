using System.Diagnostics;

using LibApp.Devices.MotorDSL.Models;
using MotorDsl.Printing;

namespace LibApp.Devices.MotorDSL.Services;

/// <summary>
/// Servicio de alto nivel: compone permisos + descubrimiento + conexión +
/// envío sobre <see cref="IThermalPrinterService"/> y devuelve resultados
/// tipados. Espejo de CallService/GpsService. No referencia ningún ViewModel.
/// </summary>
public class PrinterService : IPrinterService
{
    private const string DefaultIdKey = "default_printer_id";
    private const string DefaultNameKey = "default_printer_name";

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

    /// <summary>
    /// Descubre impresoras Bluetooth y normaliza el resultado.
    /// <para>
    /// El chequeo previo del adaptador y del permiso no es redundante con el transport: el
    /// transport SÍ distingue estas causas y lanza, pero <c>ThermalPrinterService</c> captura
    /// esas excepciones y devuelve la lista vacía (por diseño: un transport caído no debe
    /// abortar el barrido de los demás). Sin este chequeo, "Bluetooth apagado" y "permiso
    /// revocado" llegan a la UI como "no se encontraron impresoras", que manda al usuario a
    /// revisar la impresora cuando el problema está en el teléfono.
    /// </para>
    /// </summary>
    public async Task<DiscoverResult> DiscoverAsync()
    {
        if (!IsSupported) return new DiscoverResult.NotSupported();

#if ANDROID
        var ctx = Android.App.Application.Context;

        // Vía BluetoothManager y no BluetoothAdapter.DefaultAdapter, que está deprecado en
        // Android 12+ (API 31).
        var manager = ctx.GetSystemService(Android.Content.Context.BluetoothService)
            as Android.Bluetooth.BluetoothManager;
        var adapter = manager?.Adapter;
        if (adapter is null) return new DiscoverResult.NotSupported();

        if (!adapter.IsEnabled)
            return new DiscoverResult.BluetoothOff(
                PrinterErrorCatalog.BluetoothApagado("BluetoothAdapter.IsEnabled == false"));

        // El permiso puede revocarse desde Ajustes con la app corriendo, después de que
        // EnsurePermissionsAsync ya lo verificó (o salteándolo, si se llega por "Reintentar").
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            var perm = ctx.CheckSelfPermission(Android.Manifest.Permission.BluetoothConnect);
            if (perm != Android.Content.PM.Permission.Granted)
                return new DiscoverResult.PermissionRevoked(
                    PrinterErrorCatalog.PermisoRevocado("CheckSelfPermission(BLUETOOTH_CONNECT) != Granted"));
        }
#endif

        try
        {
            var devices = await _printer.DiscoverDevicesAsync(kind: "bluetooth");
            return devices.Count == 0
                ? new DiscoverResult.Empty()
                : new DiscoverResult.Found(devices);
        }
        catch (Exception ex)
        {
            // Red de seguridad: hoy inalcanzable porque ThermalPrinterService se traga las
            // excepciones de los transports. Deja de serlo si la librería cambia esa política.
            Log(ex.Message, ex);
            return new DiscoverResult.BluetoothOff(PrinterErrorCatalog.BluetoothApagado(ex.Message));
        }
    }

    /// <summary>
    /// Lee Preferences y devuelve el device guardado SOLO si está en la lista
    /// detectada (impresora predeterminada disponible).
    /// <para>
    /// "Presente" acá significa <b>emparejado</b>, no encendido ni en rango: el descubrimiento
    /// sólo lista <c>BondedDevices</c>. Una predeterminada apagada igual aparece, y el fallo
    /// recién se manifiesta al conectar (ver PrinterErrorCatalog.PredeterminadaAusente).
    /// </para>
    /// </summary>
    public PrinterDevice? GetDefaultIfPresent(IReadOnlyList<PrinterDevice> devices)
    {
        var id = Preferences.Default.Get(DefaultIdKey, string.Empty);
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
            // ThermalPrinterService envuelve el fallo real en un Exception genérico
            // ("Print failed after N attempt(s): …") pero conserva la causa en InnerException.
            // Es esa causa la que sabe clasificar el catálogo.
            var original = ex.InnerException ?? ex;
            Log(ex.Message, ex);
            return new PrintResult.Failure(PrinterErrorCatalog.Describe(original, ex.Message));
        }
    }

    public void SaveDefault(PrinterDevice device)
    {
        Preferences.Default.Set(DefaultIdKey, device.Id);
        Preferences.Default.Set(DefaultNameKey, device.Name ?? "");
    }

    /// <summary>
    /// Olvida la impresora predeterminada. Es la salida del caso en que el usuario cambió de
    /// impresora: sin esto, la predeterminada ausente se reintenta en cada impresión y la
    /// única forma de romper el ciclo es desemparejarla desde los ajustes de Android.
    /// </summary>
    public void ClearDefault()
    {
        Preferences.Default.Remove(DefaultIdKey);
        Preferences.Default.Remove(DefaultNameKey);
    }

    private static string AliasKey(string deviceId) => $"printer_alias_{deviceId}";

    /// <summary>Alias legible que el usuario le puso a esta impresora ("Mostrador"). Vacío si no tiene.</summary>
    public string GetAlias(string deviceId) => Preferences.Default.Get(AliasKey(deviceId), string.Empty);

    public void SetAlias(string deviceId, string alias) => Preferences.Default.Set(AliasKey(deviceId), alias);

    public void OpenAppSettings() => AppInfo.Current.ShowSettingsUI();

    /// <summary>
    /// Abre los ajustes de Bluetooth del SO. Distinto de <see cref="OpenAppSettings"/>, que abre
    /// los ajustes de la aplicación: ahí no se puede encender el Bluetooth ni emparejar.
    /// <para>
    /// NO enciende el adaptador: sólo lleva al usuario al panel donde puede hacerlo.
    /// <c>BluetoothAdapter.Enable()</c> está deprecado desde Android 13 (API 33) y falla en
    /// silencio, y <c>ActionRequestEnable</c> exige una Activity para el resultado, que no
    /// encaja con el patrón overlay.
    /// </para>
    /// </summary>
    public void OpenBluetoothSettings()
    {
#if ANDROID
        var intent = new Android.Content.Intent(Android.Provider.Settings.ActionBluetoothSettings);
        intent.SetFlags(Android.Content.ActivityFlags.NewTask);
        Android.App.Application.Context.StartActivity(intent);
#endif
    }

    // El mensaje original se preserva en PrintFailure.TechnicalMessage/Exception; esto sólo
    // deja rastro en depuración. Cuando se defina el destino del log (o se suscriba el evento
    // ErrorOccurred de IThermalPrinterService, que ya expone todo esto clasificado), este es
    // el punto único a cambiar.
    private static void Log(string message, Exception? ex = null)
        => Debug.WriteLine($"[PRINT] {message}{(ex is null ? "" : $" | {ex}")}");
}
