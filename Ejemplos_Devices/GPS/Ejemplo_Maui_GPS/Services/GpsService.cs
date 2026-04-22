namespace Ejemplo_Maui_GPS.Services;

public class GpsService
{
    public async Task<Location?> ObtenerUbicacionAsync(CancellationToken ct = default)
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15));
        return await Geolocation.Default.GetLocationAsync(request, ct);
    }
}