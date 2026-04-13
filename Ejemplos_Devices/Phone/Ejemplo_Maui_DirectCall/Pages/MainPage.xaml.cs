

#if ANDROID
using Android;
using Android.Content;
using AndroidUri = Android.Net.Uri;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace Ejemplo_Maui_DirectCall.Pages;


public partial class MainPage : ContentPage
{
    string telefono;
    public string Telefono
    {
        get => telefono;
        set
        {
            if (telefono != value) //importante! evita que entre en un bucle
            {
                telefono = value;
                OnPropertyChanged(); //para que funcione la bidireccionalidad
            }
        }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    async private void btnRealizarLLamada_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Telefono))
        {
            await DisplayAlertAsync("Atención", "Ingresá un número de teléfono", "OK");
            return;
        }

        try
        {
            await LlamarConPermisoAsync(telefono);
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlertAsync("Error", "Este dispositivo no puede hacer llamadas.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo abrir el marcador: {ex.Message}", "OK");
        }
    }

    private async Task LlamarConPermisoAsync(string numero)
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
