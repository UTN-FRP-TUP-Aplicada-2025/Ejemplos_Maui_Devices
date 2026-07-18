using LibApp.Devices.Networks.Models;

namespace LibApp.Devices.Networks.Services;

/// <summary>
/// Costura entre <c>NetworkOverlayViewModel</c> y la plataforma. Ver
/// <see cref="GPS.Services.IGpsService"/> para el fundamento.
/// <para>
/// A diferencia de los demás dominios, éste además <b>empuja</b>: el overlay de red es el único
/// reactivo, y <see cref="ConnectivityChanged"/> es lo que le permite aparecer sin que nadie lo
/// invoque. Que el evento esté en la interfaz es lo que hace testeable ese comportamiento.
/// </para>
/// </summary>
public interface INetworkService
{
    /// <summary>Cambio de conectividad ya normalizado a booleano: true = hay Internet pleno.</summary>
    event Action<bool>? ConnectivityChanged;

    bool IsOnline { get; }

    Task<NetworkResult> CheckUrlAsync(string url, CancellationToken ct = default);

    /// <summary>Ajustes de la aplicación. Los otros tres dominios lo exponen igual.</summary>
    void OpenAppSettings();
}
