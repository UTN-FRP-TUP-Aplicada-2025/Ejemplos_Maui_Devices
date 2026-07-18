using LibApp.Devices.Phone.Models;

namespace LibApp.Devices.Phone.Services;

/// <summary>
/// Costura entre <c>CallOverlayViewModel</c> y la plataforma. Ver <see cref="GPS.Services.IGpsService"/>
/// para el fundamento.
/// </summary>
public interface ICallService
{
    Task<CallResult> LlamarAsync(string numero, CallMode mode = CallMode.Direct, CancellationToken ct = default);

    void OpenAppSettings();
}
