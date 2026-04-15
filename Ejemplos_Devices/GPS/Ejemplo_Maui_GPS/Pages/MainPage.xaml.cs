
using Ejemplo_Maui_GPS.Services;

namespace Ejemplo_Maui_GPS.Pages;

public partial class MainPage : ContentPage
{
    GpsService _gps = default!;
    private CancellationTokenSource? _cts;

    string coordenadas;
    public string Coordenadas
    {
        get { return coordenadas; }
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

    async private void OnGetGeoLocalizacion_Clicked(object sender, EventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        btnMostrarCoordenadas.IsEnabled = false;
        btnCancelarCoordenadas.IsVisible = true;
        Coordenadas = "";

        try
        {
            //obtener la ubicacion gps
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
        catch (PermissionException)                                    // ← FALTA ESTE
        {
            Coordenadas = "Permiso de ubicación denegado.";
        }
        catch (FeatureNotSupportedException)                           // ← opcional
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
        Location? location = await _gps.ObtenerUbicacionAsync();

        if (location == null)
        {
            await DisplayAlertAsync("Advertencia", "Primero debe obtener la localización", "OK");
            return;
        }

        try
        {
            // Formato: https://maps.google.com/?q=latitude,longitude
            string url = $"https://maps.google.com/?q={location.Latitude},{location.Longitude}";
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo abrir Google Maps: {ex.Message}", "OK");
        }
    }
}
