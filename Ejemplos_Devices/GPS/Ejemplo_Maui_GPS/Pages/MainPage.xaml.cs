using Ejemplo_Maui_GPS.Services;

namespace Ejemplo_Maui_GPS.Pages;

public partial class MainPage : ContentPage
{
    GpsService _gps = default!;
    private CancellationTokenSource? _cts;

    string coordenadas = "";
    public string Coordenadas
    {
        get => coordenadas;
        set
        {
            if (coordenadas != value)
            {
                coordenadas = value;
                OnPropertyChanged();
            }
        }
    }

    public MainPage(GpsService gps)
    {
        InitializeComponent();
        BindingContext = this;
        _gps = gps;
    }

    // ── PERMISOS (misma lógica que cámara) ──

    private async Task<bool> EvaluarPermisosGpsAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            OcultarOverlayPermiso();
            return true;
        }

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            OcultarOverlayPermiso();
            return true;
        }

        if (status == PermissionStatus.Restricted)
        {
            MostrarOverlayPermiso("Acceso restringido",
                "El acceso a la ubicación está restringido por una política del dispositivo. Consultá con el administrador.",
                puedeReintentar: false);
            return false;
        }

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

    private void MostrarOverlayPermiso(string titulo, string mensaje, bool puedeReintentar)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LblPermissionTitle.Text = titulo;
            LblPermissionMessage.Text = mensaje;
            BtnPedirPermiso.IsVisible = puedeReintentar;
            BtnGoToSettings.IsVisible = !puedeReintentar;
            MainContent.IsVisible = false;
            PermissionDeniedOverlay.IsVisible = true;
        });
    }

    private void OcultarOverlayPermiso()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PermissionDeniedOverlay.IsVisible = false;
            MainContent.IsVisible = true;
        });
    }

    // ── HANDLERS DEL OVERLAY ──

    private async void OnPedirPermisoClicked(object sender, EventArgs e)
    {
        await EvaluarPermisosGpsAsync();
    }

    private void OnGoToSettingsClicked(object sender, EventArgs e)
    {
        AppInfo.ShowSettingsUI();
    }

    private void OnCerrarOverlayClicked(object sender, EventArgs e)
    {
        OcultarOverlayPermiso();
        Coordenadas = "Permiso de ubicación denegado.";
    }

    // ── GPS ──

    async private void OnGetGeoLocalizacion_Clicked(object sender, EventArgs e)
    {
        // Evaluar permisos ANTES de pedir ubicación
        if (!await EvaluarPermisosGpsAsync())
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        btnMostrarCoordenadas.IsEnabled = false;
        btnCancelarCoordenadas.IsVisible = true;
        Coordenadas = "";

        try
        {
            Coordenadas = "Obteniendo ubicación GPS...";
            var location = await _gps.ObtenerUbicacionAsync(_cts.Token);

            if (location == null)
            {
                Coordenadas = "No se pudo obtener ubicación (GPS sin señal).";
                return;
            }

            Coordenadas = $"Lat: {location.Latitude:F6}, Lng: {location.Longitude:F6}";
        }
        catch (OperationCanceledException)
        {
            Coordenadas = "Operación cancelada por el usuario.";
        }
        catch (FeatureNotEnabledException)
        {
            Coordenadas = "El GPS está desactivado. Activalo desde ajustes.";
        }
        catch (FeatureNotSupportedException)
        {
            Coordenadas = "Este dispositivo no soporta GPS.";
        }
        finally
        {
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    async private void OnMostrarLocalizacionEnMapaClicked(object sender, EventArgs e)
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
}