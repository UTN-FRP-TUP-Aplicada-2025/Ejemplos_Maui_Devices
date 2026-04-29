namespace Ejemplo_Maui_GPS.Services;

// Servicio de alto nivel: compone permisos + lectura GPS y devuelve un GpsResult tipado.
// El ViewModel solo hace switch sobre el resultado, sin try/catch ni chequeo manual de permisos.
public class GpsService
{
    private readonly LocationPermissionService _permissions;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

    public GpsService(LocationPermissionService permissions)
    {
        _permissions = permissions;
    }

    public async Task<GpsResult> ObtenerUbicacionAsync(CancellationToken ct = default)
    {
        // 1) Resolver permisos
        var perm = await _permissions.RequestAsync();
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

        // 2) Leer ubicación
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
    }
}