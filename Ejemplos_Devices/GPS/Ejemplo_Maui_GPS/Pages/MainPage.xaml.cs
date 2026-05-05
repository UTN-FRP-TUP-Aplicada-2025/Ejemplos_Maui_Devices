using Ejemplo_Maui_GPS.Services;
using Ejemplo_Maui_GPS.ViewModels;

namespace Ejemplo_Maui_GPS.Pages;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;
    private readonly GpsCoordinator _coord;

    public MainPage(MainPageViewModel vm, GpsCoordinator coord)
    {
        InitializeComponent();
        _vm = vm;
        _coord = coord;
        BindingContext = _vm;
    }

    // Caso de prueba: dispara el GPS desde fuera del VM. El overlay y los permisos
    // los maneja el coordinator automáticamente.
    private async void Button_Clicked(object sender, EventArgs e)
    {
        var result = await _coord.CapturarAsync();
        lbSalida.Text = result is GpsResult.Success s
            ? $"Lat: {s.Location.Latitude:F6}, Lon: {s.Location.Longitude:F6}"
            : "Sin ubicación.";
    }
}
