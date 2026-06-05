using Ejemplo_Maui_Hibrida.Models;

#if ANDROID
using Android.Content;
#endif

namespace Ejemplo_Maui_Hibrida.Services;

/// <summary>
/// Servicio de alto nivel: compone permisos + ejecución de la llamada y
/// devuelve un <see cref="CallResult"/> tipado. Espejo de GpsService.
/// </summary>
public class CallService
{
    public CallService()
    {
    }

    /// <summary>
    /// Único punto de entrada. <see cref="CallMode.Direct"/> usa el Intent
    /// de llamada de Android (requiere permiso CALL_PHONE); cualquier otra
    /// plataforma o <see cref="CallMode.Dialer"/> cae al marcador del SO.
    /// </summary>
    public async Task<CallResult> LlamarAsync(string numero, CallMode mode = CallMode.Direct, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return new CallResult.InvalidNumber();

        if (ct.IsCancellationRequested)
            return new CallResult.Cancelled();

#if ANDROID
        if (mode == CallMode.Direct)
            return await LlamarDirectoAndroidAsync(numero);
#endif

        // iOS / MacCatalyst / Windows / Android+Dialer => marcador del SO.
        return LlamarConDialer(numero);
    }

    /// <summary>
    /// Abre el marcador del SO con el número precargado. El usuario confirma
    /// la llamada. No requiere permiso runtime.
    /// </summary>
    private CallResult LlamarConDialer(string numero)
    {
        try
        {
            if (!PhoneDialer.Default.IsSupported)
                return new CallResult.NotSupported();

            PhoneDialer.Default.Open(numero);
            return new CallResult.Success(numero, CallMode.Dialer);
        }
        catch (FeatureNotSupportedException)
        {
            return new CallResult.NotSupported();
        }
        catch (Exception ex)
        {
            return new CallResult.Failure(ex.Message);
        }
    }

#if ANDROID
    /// <summary>
    /// Llamada directa vía Intent.ActionCall, resolviendo antes el permiso
    /// runtime CALL_PHONE.
    /// </summary>
    private async Task<CallResult> LlamarDirectoAndroidAsync(string numero)
    {
        #region Resolver permisos
        var perm = await RequestAsync();
        switch (perm)
        {
            case CallPermissionResult.Granted:
                break;
            case CallPermissionResult.Restricted:
                return new CallResult.PermissionRestricted();
            case CallPermissionResult.DeniedCanRetry:
                return new CallResult.PermissionDenied(CanRetry: true);
            case CallPermissionResult.Denied:
            default:
                return new CallResult.PermissionDenied(CanRetry: false);
        }
        #endregion

        #region Ejecutar la llamada
        try
        {
            var activity = Platform.CurrentActivity;
            if (activity is null)
                return new CallResult.Failure("No hay una actividad activa para iniciar la llamada.");

            var uri = Android.Net.Uri.Parse($"tel:{numero}");
            using var intent = new Intent(Intent.ActionCall, uri);
            intent.AddFlags(ActivityFlags.NewTask);
            activity.StartActivity(intent);

            return new CallResult.Success(numero, CallMode.Direct);
        }
        catch (Exception ex)
        {
            return new CallResult.Failure(ex.Message);
        }
        #endregion
    }

    private async Task<CallPermissionResult> RequestAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Phone>();

        if (status == PermissionStatus.Granted)
            return CallPermissionResult.Granted;

        // Solicita el permiso al SO. Con "no volver a preguntar" retorna
        // Denied sin diálogo.
        status = await Permissions.RequestAsync<Permissions.Phone>();

        if (status == PermissionStatus.Granted)
            return CallPermissionResult.Granted;

        if (status == PermissionStatus.Restricted)
            return CallPermissionResult.Restricted;

        // ShouldShowRationale (sólo Android):
        //   true  → denegado sin "no volver a preguntar": se puede reintentar
        //   false → denegado con "no volver a preguntar": hay que ir a ajustes
        bool puedeReintentar = Permissions.ShouldShowRationale<Permissions.Phone>();
        return puedeReintentar
            ? CallPermissionResult.DeniedCanRetry
            : CallPermissionResult.Denied;
    }
#endif

    public void OpenAppSettings() => AppInfo.Current.ShowSettingsUI();
}
