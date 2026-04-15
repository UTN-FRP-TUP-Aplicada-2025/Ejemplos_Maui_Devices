namespace Ejemplo_Maui_GPS.Services;

public class GpsService
{
    public async Task<Location?> ObtenerUbicacionAsync(CancellationToken ct = default)
    {
        // verifica si tiene permisos
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        // sino tiene los pide 
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        // si despues de pedir sigue sin permisos, salir
        if (status != PermissionStatus.Granted)
            return null;

        // con permisos confirmados, pedir ubicación
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15));
        return await Geolocation.Default.GetLocationAsync(request, ct);
    }
}
