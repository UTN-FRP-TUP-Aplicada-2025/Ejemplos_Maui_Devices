namespace Ejemplo_Maui_GPS.Services;

// Encapsula la mecánica de permisos de ubicación.
// Reutilizable desde cualquier servicio/feature que requiera Location.
public class LocationPermissionService
{
    public async Task<LocationPermissionResult> CheckAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        return Map(status, puedeReintentar: true);
    }

    public async Task<LocationPermissionResult> RequestAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return LocationPermissionResult.Granted;

        // Solicita el permiso al SO. En iOS solo abre el diálogo la primera vez;
        // en Android con "no volver a preguntar" retorna Denied sin diálogo.
        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return LocationPermissionResult.Granted;

        if (status == PermissionStatus.Restricted)
            return LocationPermissionResult.Restricted;

        // ShouldShowRationale solo existe en Android:
        //   true  → denegado sin "no volver a preguntar": se puede reintentar
        //   false → denegado con "no volver a preguntar" (o primera vez): hay que ir a ajustes
        // En iOS no aplica → siempre Denied (forzar ajustes).
        bool puedeReintentar = false;
#if ANDROID
        puedeReintentar = Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
#endif
        return puedeReintentar
            ? LocationPermissionResult.DeniedCanRetry
            : LocationPermissionResult.Denied;
    }

    public void OpenAppSettings() => AppInfo.ShowSettingsUI();

    private static LocationPermissionResult Map(PermissionStatus status, bool puedeReintentar) => status switch
    {
        PermissionStatus.Granted => LocationPermissionResult.Granted,
        PermissionStatus.Restricted => LocationPermissionResult.Restricted,
        _ => puedeReintentar ? LocationPermissionResult.DeniedCanRetry : LocationPermissionResult.Denied
    };
}
