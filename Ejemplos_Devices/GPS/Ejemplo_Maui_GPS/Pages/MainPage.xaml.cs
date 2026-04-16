using Ejemplo_Maui_GPS.Services;
using Ejemplo_Maui_GPS.ViewModels;

namespace Ejemplo_Maui_GPS.Pages;

public partial class MainPage : ContentPage
{
    GpsService _gps = default!;
    private CancellationTokenSource? _cts;

    private MainPageViewModel _mainPageViewModel = new();

    public MainPage(GpsService gps)
    {
        InitializeComponent();
        BindingContext = _mainPageViewModel;
        _gps = gps;

        _mainPageViewModel.DeniedGPS = false;
        _mainPageViewModel.EsperandoGPS = false;
    }

    // ── PERMISOS ──
    // Escenario 1 (Android/iOS): el sistema solicita el permiso y el usuario lo concede → retorna true
    // Escenario 2 (Android/iOS): el sistema solicita el permiso y el usuario lo deniega → muestra overlay, retorna false
    // Escenario 3 (Android): permiso denegado previamente sin "no volver a preguntar" → muestra botón "Pedir permiso"
    // Escenario 3 (Android "no volver a preguntar" / iOS denegado): solo puede ir a ajustes → muestra botón "Abrir configuración"

    private async Task<bool> EvaluarPermisosGpsAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            OcultarOverlayPermiso();
            return true;
        }

        // Solicita el permiso al sistema operativo.
        // En iOS solo funciona la primera vez; si ya fue denegado, retorna Denied sin mostrar diálogo.
        // En Android con "no volver a preguntar", también retorna Denied sin diálogo.
        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            OcultarOverlayPermiso();
            return true;
        }

        if (status == PermissionStatus.Restricted)
        {
            // iOS: política del dispositivo (MDM, control parental)
            MostrarOverlayPermiso(
                titulo: "Acceso restringido",
                mensaje: "El acceso a la ubicación está restringido por una política del dispositivo. Consultá con el administrador.",
                puedeReintentar: false);
            return false;
        }

        // Determina si se puede volver a solicitar el permiso en tiempo de ejecución.
        // ShouldShowRationale solo existe en Android:
        //   - true  → denegado sin "no volver a preguntar": se puede reintentar
        //   - false → denegado con "no volver a preguntar" (o primera vez): hay que ir a ajustes
        // En iOS siempre es false (puedeReintentar = false), lo que fuerza ir a ajustes.
        bool puedeReintentar = false;

#if ANDROID
        puedeReintentar = Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
#endif

        MostrarOverlayPermiso(
            titulo: puedeReintentar
                ? "Permiso de ubicación necesario"
                : "Acceso a la ubicación denegado",
            mensaje: puedeReintentar
                ? "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Podés intentar conceder el permiso."
                : "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Habilitalo desde los ajustes de la aplicación.",
            puedeReintentar: puedeReintentar);

        return false;
    }

    // ── OVERLAY ──

    // Muestra el panel de permisos denegados.
    // Usa el ViewModel para controlar la visibilidad, consistente con los bindings del XAML:
    //   EsperandoGPS = true  → muestra el contenedor externo (oculta el contenido principal)
    //   DeniedGPS    = true  → muestra el overlay de permisos, oculta el panel de espera animada
    private void MostrarOverlayPermiso(string titulo, string mensaje, bool puedeReintentar)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LblPermissionTitle.Text = titulo;
            LblPermissionMessage.Text = mensaje;
            BtnPedirPermiso.IsVisible = puedeReintentar;
            BtnGoToSettings.IsVisible = !puedeReintentar;
            _mainPageViewModel.EsperandoGPS = true;  // hace visible el contenedor externo
            _mainPageViewModel.DeniedGPS = true;     // activa el overlay, desactiva el panel de espera
        });
    }

    // Oculta el overlay de permisos y vuelve al estado normal.
    // EsperandoGPS = false oculta el contenedor externo y, por binding inverso, muestra el contenido principal.
    private void OcultarOverlayPermiso()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _mainPageViewModel.DeniedGPS = false;
            _mainPageViewModel.EsperandoGPS = false;
        });
    }

    // ── HANDLERS DEL OVERLAY ──

    private async void OnPedirPermisoClicked(object sender, EventArgs e)
    {
        // Vuelve a evaluar permisos (Android: solo funciona si no fue "no volver a preguntar")
        await EvaluarPermisosGpsAsync();
    }

    private void OnGoToSettingsClicked(object sender, EventArgs e)
    {
        // Abre la pantalla de ajustes de la aplicación (Android e iOS)
        AppInfo.ShowSettingsUI();
    }

    // ── Contenido principal ──

    private async void OnGetGeoLocalizacion_Clicked(object sender, EventArgs e)
    {
        // Evalúa permisos antes de iniciar la búsqueda.
        // Si se deniegan, MostrarOverlayPermiso ya activa EsperandoGPS y DeniedGPS.
        if (!await EvaluarPermisosGpsAsync())
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        btnMostrarCoordenadas.IsEnabled = false;
        btnCancelarCoordenadas.IsVisible = true;
        _mainPageViewModel.Coordenadas = "";

        try
        {
            // Escenario 4: muestra el panel de espera animada mientras se obtiene la coordenada
            _mainPageViewModel.EsperandoGPS = true;
            _mainPageViewModel.DeniedGPS = false;
            _mainPageViewModel.Coordenadas = "Obteniendo ubicación GPS...";

            var location = await _gps.ObtenerUbicacionAsync(_cts.Token);

            if (location == null)
            {
                _mainPageViewModel.Coordenadas = "No se pudo obtener ubicación (GPS sin señal).";
                return;
            }

            _mainPageViewModel.Coordenadas = $"Lat: {location.Latitude:F6}, Lng: {location.Longitude:F6}";
        }
        catch (OperationCanceledException)
        {
            _mainPageViewModel.Coordenadas = "Operación cancelada por el usuario.";
        }
        catch (FeatureNotEnabledException)
        {
            _mainPageViewModel.Coordenadas = "El GPS está desactivado. Activalo desde ajustes.";
        }
        catch (FeatureNotSupportedException)
        {
            _mainPageViewModel.Coordenadas = "Este dispositivo no soporta GPS.";
        }
        finally
        {
            // Siempre oculta el panel de espera al terminar (éxito, error o cancelación)
            _mainPageViewModel.EsperandoGPS = false;
            btnMostrarCoordenadas.IsEnabled = true;
            btnCancelarCoordenadas.IsVisible = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void OnCancelarGeoLocalizacion_Clicked(object sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    private async void OnMostrarLocalizacionEnMapaClicked(object sender, EventArgs e)
    {
        if (!await EvaluarPermisosGpsAsync())
            return;

        Location? location = await _gps.ObtenerUbicacionAsync();

        if (location == null)
        {
            await DisplayAlertAsync("Advertencia", "No se pudo obtener la localización", "OK");
            return;
        }

        try
        {
            string url = $"https://maps.google.com/?q={location.Latitude},{location.Longitude}";
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo abrir Google Maps: {ex.Message}", "OK");
        }
    }

    private void OnCerrarOverlayClicked(object sender, EventArgs e)
    {
        OcultarOverlayPermiso();
        _mainPageViewModel.Coordenadas = "Permiso de ubicación denegado.";
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}