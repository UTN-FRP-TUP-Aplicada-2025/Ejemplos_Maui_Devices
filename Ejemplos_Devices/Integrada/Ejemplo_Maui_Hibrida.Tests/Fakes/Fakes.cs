using LibApp.CustomWebView.Behaviors;
using LibApp.Devices.Common.Services;
using LibApp.Devices.GPS.Models;
using LibApp.Devices.GPS.Services;
using LibApp.Devices.MotorDSL.Models;
using LibApp.Devices.MotorDSL.Services;
using LibApp.Devices.Networks.Models;
using LibApp.Devices.Networks.Services;
using LibApp.Devices.Phone.Models;
using LibApp.Devices.Phone.Services;
using MotorDsl.Printing;

namespace Ejemplo_Maui_Hibrida.Tests.Fakes;

// Dobles escritos a mano: las interfaces tienen 3-6 miembros y una librería de mocking
// agregaría una dependencia sin ganar nada. Cada fake devuelve lo que el test le indica y
// registra qué se le invocó, que es lo que hace falta para verificar las botoneras.

public sealed class FakeGpsService : IGpsService
{
    public GpsResult Resultado { get; set; } = new GpsResult.NoSignal();
    public int VecesOpenAppSettings { get; private set; }
    public int VecesOpenLocationSettings { get; private set; }

    public Task<GpsResult> ObtenerUbicacionAsync(CancellationToken ct = default) => Task.FromResult(Resultado);
    public void OpenAppSettings() => VecesOpenAppSettings++;
    public void OpenLocationSettings() => VecesOpenLocationSettings++;
}

public sealed class FakeCallService : ICallService
{
    public CallResult Resultado { get; set; } = new CallResult.InvalidNumber();
    public int VecesOpenAppSettings { get; private set; }
    public string? UltimoNumero { get; private set; }

    public Task<CallResult> LlamarAsync(string numero, CallMode mode = CallMode.Direct, CancellationToken ct = default)
    {
        UltimoNumero = numero;
        return Task.FromResult(Resultado);
    }

    public void OpenAppSettings() => VecesOpenAppSettings++;
}

public sealed class FakeNetworkService : INetworkService
{
    public event Action<bool>? ConnectivityChanged;

    public NetworkResult Resultado { get; set; } = new NetworkResult.Online();
    public bool IsOnline { get; set; } = true;
    public string? UltimaUrlSondeada { get; private set; }
    public int VecesOpenAppSettings { get; private set; }

    public Task<NetworkResult> CheckUrlAsync(string url, CancellationToken ct = default)
    {
        UltimaUrlSondeada = url;
        return Task.FromResult(Resultado);
    }

    public void OpenAppSettings() => VecesOpenAppSettings++;

    /// <summary>Simula el evento del SO: es la vía por la que este overlay aparece solo.</summary>
    public void EmitirConectividad(bool online) => ConnectivityChanged?.Invoke(online);
}

/// <summary>
/// Ejecuta en el mismo hilo. En producción el marshalling es obligatorio (el evento de
/// conectividad llega en el hilo del SO); en test, ejecutar inline mantiene las aserciones
/// deterministas.
/// </summary>
public sealed class FakeUiDispatcher : IUiDispatcher
{
    public void BeginInvoke(Action action) => action();
}

public sealed class FakeWebViewBridge : IWebViewBridge
{
    public event EventHandler? ReloadRequested;
    public event EventHandler<string>? ScriptRequested;

    public int VecesReload { get; private set; }
    public void Reload() => VecesReload++;
    public void RunScript(string js) => ScriptRequested?.Invoke(this, js);
}

public sealed class FakePrinterService : IPrinterService
{
    public bool IsSupported { get; set; } = true;
    public BluetoothPermissionResult Permiso { get; set; } = BluetoothPermissionResult.Granted;
    public DiscoverResult Descubrimiento { get; set; } = new DiscoverResult.Empty();
    public PrinterDevice? Predeterminada { get; set; }
    public bool ConexionOk { get; set; } = true;
    public PrintResult Envio { get; set; } = new PrintResult.Success();

    public int VecesClearDefault { get; private set; }
    public int VecesOpenBluetoothSettings { get; private set; }
    public Dictionary<string, string> Alias { get; } = new();

    public Task<BluetoothPermissionResult> EnsurePermissionsAsync() => Task.FromResult(Permiso);
    public Task<DiscoverResult> DiscoverAsync() => Task.FromResult(Descubrimiento);
    public PrinterDevice? GetDefaultIfPresent(IReadOnlyList<PrinterDevice> devices) => Predeterminada;
    public Task<bool> ConnectAsync(PrinterDevice device) => Task.FromResult(ConexionOk);
    public Task<PrintResult> SendAsync(byte[] bytes) => Task.FromResult(Envio);
    public void SaveDefault(PrinterDevice device) => Predeterminada = device;
    public void ClearDefault() { VecesClearDefault++; Predeterminada = null; }
    public string GetAlias(string deviceId) => Alias.TryGetValue(deviceId, out var a) ? a : string.Empty;
    public void SetAlias(string deviceId, string alias) => Alias[deviceId] = alias;
    public void OpenAppSettings() { }
    public void OpenBluetoothSettings() => VecesOpenBluetoothSettings++;
}
