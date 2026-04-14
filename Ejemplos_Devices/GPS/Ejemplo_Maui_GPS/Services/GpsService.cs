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

    /*
     // En tu GpsService, el enfoque correcto con toda la guardia:
public async Task<Location?> ObtenerUbicacionAsync(CancellationToken ct = default)
{
    // 1. ¿El dispositivo tiene GPS? (hardware)
    //    En simulador → sí (simulado), en tablet sin chip → no
    // Esta verificación no tiene API directa limpia en MAUI.
    // GetLocationAsync lanza FeatureNotSupportedException si no hay hardware.
    // Pero podemos verificar permisos primero:

    // 2. ¿Tiene permiso?
    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
    if (status != PermissionStatus.Granted)
    {
        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
    }
    if (status != PermissionStatus.Granted)
        return null;  // Sin permiso → null limpio

    // 3. Intentar obtener ubicación
    var request = new GeolocationRequest(
        GeolocationAccuracy.Best, TimeSpan.FromSeconds(30));
    return await Geolocation.Default.GetLocationAsync(request, ct);
}
     */

}
