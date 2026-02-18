using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Ejemplo_Maui_Mapas.Pages;

public partial class MainPage : ContentPage
{
    
    public MainPage()
    {
        InitializeComponent();
     
    }

    async protected override void OnAppearing()
    {
        base.OnAppearing();

        await PedirPermisosAsync();
        CentrarMapa(-31.749788,  -60.520532);
        AgregarPins();
    }

    private async Task PedirPermisosAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            MyMap.IsShowingUser = true; // muestra el punto azul en el mapa
        else
            await DisplayAlertAsync("Permiso denegado", "No se puede mostrar tu ubicación.", "OK");
    }

    //private void CentrarMapa()
    //{
    //    // Buenos Aires
    //    var centro = new Location(-34.6037, -58.3816);
    //    var span = new MapSpan(centro, 0.1, 0.1); // grados de zoom
    //    MyMap.MoveToRegion(span);
    //}

    private void CentrarMapa(double lat, double lon, double radioKm = 2)
    {
        var ubicacion = new Location(lat, lon);
        var region = MapSpan.FromCenterAndRadius( ubicacion,  Distance.FromKilometers(radioKm));
        MyMap.MoveToRegion(region);
    }

    private void AgregarPins()
    {
        var pins = new[]
        {
            new Pin
            {
                Label = "Obelisco",
                Address = "Calle 25",
                Location = new Location(-34.6037, -58.3816),
                Type = PinType.Place
            },
            new Pin
            {
                Label = "Casa Rosada",
                Address = "Calle 33",
                Location = new Location(-34.6083, -58.3712),
                Type = PinType.Place
            }
        };

        foreach (var pin in pins)
        {
            pin.MarkerClicked += OnPinClicked;
            MyMap.Pins.Add(pin);
        }
    }

    private async void OnPinClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is Pin pin)
            await DisplayAlertAsync("📍 Pin", $"{pin.Label}\n{pin.Address}", "OK");

        e.HideInfoWindow = false; // muestra el tooltip nativo
    }

    private void OnNormalClicked(object sender, EventArgs e) => MyMap.MapType = MapType.Street;

    private void OnSateliteClicked(object sender, EventArgs e) => MyMap.MapType = MapType.Satellite;

    private void OnGoToLocation(object sender, EventArgs e)
    {
        var span = MapSpan.FromCenterAndRadius(  new Location(-31.749788, -60.520532),
            Distance.FromKilometers(2)
        );
        MyMap.MoveToRegion(span);
    }
}
