#if ANDROID
using Android.Content;
using AndroidUri = Android.Net.Uri;
#endif

namespace Ejemplo_Maui_DirectCall.Services;

// Encapsula la operación de llamada directa.
// Devuelve un CallResult tipado para que el caller (CallCoordinator) decida
// cómo reflejarlo en la UI. No conoce al overlay ni al ViewModel.
//
// Diseño por plataforma:
//   - Android : permiso runtime CALL_PHONE + Intent.ActionCall (llama directo)
//   - iOS / MacCatalyst : PhoneDialer.Default (abre el dialer del SO; el usuario
//                         confirma manualmente; requiere "tel" en
//                         LSApplicationQueriesSchemes del Info.plist)
//   - Windows / otros : NotSupported
public class PhoneDialerDevice
{
    public async Task<CallResult> CallPhoneAsync(string numero, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested) return new CallResult.Cancelled();

#if ANDROID
        // 1) Permiso runtime CALL_PHONE.
        var permResult = await PedirPermisoAndroidAsync(ct);
        if (permResult is not null) return permResult;

        if (ct.IsCancellationRequested) return new CallResult.Cancelled();

        // 2) Intent.ActionCall — llamada directa.
        try
        {
            return RealizarLlamadaAndroid(numero);
        }
        catch (FeatureNotSupportedException) { return new CallResult.NotSupported(); }
        catch (Exception ex) { return new CallResult.Failure(ex.Message); }

#elif IOS || MACCATALYST
        // iOS no expone API de llamada directa: abrimos el dialer del sistema
        // y el usuario confirma. No hay permiso runtime: requiere "tel" en
        // LSApplicationQueriesSchemes del Info.plist (ya declarado en este proyecto).
        try
        {
            if (!PhoneDialer.Default.IsSupported)
                return new CallResult.NotSupported();

            PhoneDialer.Default.Open(numero);
            // En iOS "Success" significa "el dialer se abrió"; la llamada
            // efectiva la inicia el usuario al tocar el botón verde.
            return new CallResult.Success(numero);
        }
        catch (FeatureNotSupportedException) { return new CallResult.NotSupported(); }
        catch (ArgumentNullException)        { return new CallResult.Failure("Número inválido."); }
        catch (Exception ex)                 { return new CallResult.Failure(ex.Message); }

#else
        // Windows / otras plataformas: no hay API de teléfono.
        await Task.CompletedTask;
        return new CallResult.NotSupported();
#endif
    }

#if ANDROID
    // Devuelve null si el permiso fue concedido. Devuelve el CallResult que corresponda
    // (Denied, DeniedPermanent, Restricted) si no se puede continuar.
    private static async Task<CallResult?> PedirPermisoAndroidAsync(CancellationToken ct)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Phone>();

        if (status == PermissionStatus.Granted) return null;
        if (status == PermissionStatus.Restricted) return new CallResult.PermissionRestricted();

        if (ct.IsCancellationRequested) return new CallResult.Cancelled();

        status = await Permissions.RequestAsync<Permissions.Phone>();

        if (status == PermissionStatus.Granted) return null;
        if (status == PermissionStatus.Restricted) return new CallResult.PermissionRestricted();

        // ShouldShowRationale==true => el SO sigue dispuesto a mostrar el diálogo.
        // ShouldShowRationale==false (post-deny) => "no volver a preguntar" → ajustes.
        var puedeReintentar = Permissions.ShouldShowRationale<Permissions.Phone>();
        return puedeReintentar
            ? new CallResult.PermissionDenied()
            : new CallResult.PermissionDeniedPermanent();
    }

    private static CallResult RealizarLlamadaAndroid(string numero)
    {
        var activity = Platform.CurrentActivity;
        if (activity is null)
            return new CallResult.Failure("No hay actividad activa para iniciar la llamada.");

        using var intent = new Intent(Intent.ActionCall);
        intent.SetData(AndroidUri.Parse($"tel:{numero}"));
        intent.SetFlags(ActivityFlags.NewTask);
        activity.StartActivity(intent);
        return new CallResult.Success(numero);
    }
#endif
}
