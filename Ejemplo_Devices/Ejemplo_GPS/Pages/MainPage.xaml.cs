using GPS.Services;

namespace Ejemplo_GPS.Pages;

public partial class MainPage : ContentPage
{
    GeoLocationsServices _geoLocationsServices=default!;

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

    public MainPage(GeoLocationsServices geo)
    {
        InitializeComponent();
        BindingContext = this;
        _geoLocationsServices = geo;
    }

    async private void OnGetGeoLocalizacionClicked(object sender, EventArgs e)
    {
        Location location=await _geoLocationsServices.GetCurrentLocation();
        if (location != null)
        {
            Coordenadas = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}";
        }
    }

    async private void OnMostrarLocalizacionEnMapaClicked(object sender, EventArgs e)
    {
        Location location = await _geoLocationsServices.GetCurrentLocation();

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
