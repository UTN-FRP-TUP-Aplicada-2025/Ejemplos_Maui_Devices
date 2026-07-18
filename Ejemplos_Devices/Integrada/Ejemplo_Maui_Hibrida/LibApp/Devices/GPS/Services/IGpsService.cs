using LibApp.Devices.GPS.Models;

namespace LibApp.Devices.GPS.Services;

/// <summary>
/// Costura entre <c>GpsOverlayViewModel</c> y la plataforma. El ViewModel depende de esta
/// interfaz y no de <see cref="GpsService"/>, que usa estáticos de MAUI (<c>Permissions</c>,
/// <c>Geolocation</c>, <c>AppInfo</c>) imposibles de ejercitar fuera de un dispositivo.
/// Sólo declara lo que el overlay consume.
/// </summary>
public interface IGpsService
{
    Task<GpsResult> ObtenerUbicacionAsync(CancellationToken ct = default);

    /// <summary>Ajustes de la aplicación: es donde se concede un permiso denegado.</summary>
    void OpenAppSettings();

    /// <summary>
    /// Ajustes de ubicación del SO. Distinto de <see cref="OpenAppSettings"/>: el permiso lo da
    /// la app, pero el GPS se enciende desde el sistema.
    /// </summary>
    void OpenLocationSettings();
}
