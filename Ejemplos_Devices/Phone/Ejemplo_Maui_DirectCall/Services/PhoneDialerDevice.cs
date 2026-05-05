
#if ANDROID
using Android;
using Android.Content;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Ejemplo_Maui_DirectCall.ViewModels;
using AndroidUri = Android.Net.Uri;
#endif

namespace Ejemplo_Maui_DirectCall.Services;

public class PhoneDialerDevice
{
    async public Task CallPhoneAsync(string numero)
    {
#if ANDROID
        var activity = Platform.CurrentActivity;
        if (activity == null) return;

        var permiso = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.CallPhone);

        if (permiso == Android.Content.PM.Permission.Granted)
        {
            RealizarLlamada(numero);
            return;
        }

        // Pedir CALL_PHONE (no READ_PHONE_STATE)
        ActivityCompat.RequestPermissions(activity, new[] { Manifest.Permission.CallPhone }, 1001);
#else
        PhoneDialer.Default.Open(numero);
#endif

    }

    private void RealizarLlamada(string numero)
    {
#if ANDROID
        var intent = new Intent(Intent.ActionCall);
        intent.SetData(AndroidUri.Parse($"tel:{numero}"));
        intent.SetFlags(ActivityFlags.NewTask);
        Platform.CurrentActivity?.StartActivity(intent);
#else
    PhoneDialer.Default.Open(numero);
#endif
    }
}
