using Ejemplo_Maui_Hibrida.Models;

namespace Ejemplo_Maui_Hibrida.Services;

/// <summary>
/// Servicio de alto nivel: compone permisos + lectura GPS y devuelve un GpsResult tipado.
/// </summary>
public class GpsService
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

    public GpsService()
    {
    }

    public async Task<GpsResult> ObtenerUbicacionAsync(CancellationToken ct = default)
    {
        #region Resolver permisos
        var perm = await RequestAsync();
        switch (perm)
        {
            case LocationPermissionResult.Granted:
                break;
            case LocationPermissionResult.Restricted:
                return new GpsResult.PermissionRestricted();
            case LocationPermissionResult.DeniedCanRetry:
                return new GpsResult.PermissionDenied(CanRetry: true);
            case LocationPermissionResult.Denied:
            default:
                return new GpsResult.PermissionDenied(CanRetry: false);
        }
        #endregion 

        #region Leer ubicación
        try
        {
            /*
            var request = new GeolocationRequest(GeolocationAccuracy.Best, DefaultTimeout);
            var location = await Geolocation.Default.GetLocationAsync(request, ct);

            return location is null  ? new GpsResult.NoSignal() : new GpsResult.Success(location);
            */

            var location = await Geolocation.GetLastKnownLocationAsync();
            if (!(location != null && (DateTimeOffset.Now - location.Timestamp) < TimeSpan.FromMinutes(1)))
            {
                var req = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                location = await Geolocation.GetLocationAsync(req, ct);
            }

            return location is null  ? new GpsResult.NoSignal() : new GpsResult.Success(location);
        }
        catch (OperationCanceledException)
        {
            return new GpsResult.Cancelled();
        }
        catch (FeatureNotEnabledException)
        {
            return new GpsResult.GpsDisabled();
        }
        catch (FeatureNotSupportedException)
        {
            return new GpsResult.NotSupported();
        }
        catch (Exception ex)
        {
            return new GpsResult.Failure(ex.Message);
        }
        #endregion 
    }

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