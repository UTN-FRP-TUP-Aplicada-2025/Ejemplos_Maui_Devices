using LibApp.Devices.GPS.Models;

namespace LibApp.Devices.GPS.Services;

/// <summary>
/// Servicio de alto nivel: compone permisos + lectura GPS y devuelve un GpsResult tipado.
/// </summary>
public class GpsService : IGpsService
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
            // Lectura FRESCA del dispositivo (Opción A, ADR-0009). NO se usa
            // GetLastKnownLocationAsync: ese atajo resolvía casi instantáneo y hacía que la capa
            // de espera parpadeara (defecto 1.b). Con una lectura real, el "Buscando posición GPS"
            // representa una espera verdadera y el overlay se cierra de forma determinista en
            // GpsOverlayViewModel.Procesar (no depende de la re-navegación).
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, DefaultTimeout);
            var location = await Geolocation.Default.GetLocationAsync(request, ct);

            return location is null ? new GpsResult.NoSignal() : new GpsResult.Success(location);
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

    /// <summary>
    /// Abre los ajustes de ubicación del SO. Distinto de <see cref="OpenAppSettings"/>: el permiso
    /// lo concede la app, pero el GPS se enciende desde el sistema.
    /// <para>
    /// NO enciende el GPS: sólo lleva al usuario al panel donde puede hacerlo. No existe API para
    /// encenderlo sin intervención del usuario.
    /// </para>
    /// </summary>
    public void OpenLocationSettings()
    {
#if ANDROID
        var intent = new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings);
        intent.SetFlags(Android.Content.ActivityFlags.NewTask);
        Android.App.Application.Context.StartActivity(intent);
#else
        OpenAppSettings();
#endif
    }
}