using System.Windows.Input;
using Ejemplo_Maui_GPS.Services;

namespace Ejemplo_Maui_GPS.ViewModels;

// Solo orquesta texto y comandos: el overlay y la cancelación viven en GpsCoordinator (singleton).
public class MainPageViewModel : ViewModelBase
{
    private readonly GpsCoordinator _coord;
    private readonly GoogleMapService _mapService;

    public GpsOverlayViewModel Overlay { get; }

    private string _coordenadas = "";
    public string Coordenadas
    {
        get => _coordenadas;
        set => SetProperty(ref _coordenadas, value);
    }

    private string _domicilio = "";
    public string Domicilio
    {
        get => _domicilio;
        set => SetProperty(ref _coordenadas, value);
    }

    public bool MostrandoContenido => !Overlay.IsVisible;
     
    public ICommand ObtenerUbicacionCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand MostrarEnMapaCommand { get; }
     
    public MainPageViewModel(GpsCoordinator coord, GoogleMapService mapService)
    {
        _coord = coord;
        Overlay = _coord.Overlay;
        _mapService = mapService;

        // Reflejar cambios de visibilidad del overlay en MostrandoContenido.
        Overlay.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(GpsOverlayViewModel.IsVisible))
                OnPropertyChanged(nameof(MostrandoContenido));
        };

        ObtenerUbicacionCommand = new AsyncRelayCommand(ObtenerUbicacionAsync);
        CancelarCommand = new RelayCommand(() => _coord.Cancelar());
        MostrarEnMapaCommand = new AsyncRelayCommand(MostrarEnMapaAsync);
        ObtenerDominicilioCommand = new AsyncRelayCommand(ObtenerDominicilioAsync);
    }
    private async Task ObtenerUbicacionAsync(CancellationToken ct)
    {
        Coordenadas = "Obteniendo ubicación GPS...";
        var result = await _coord.CapturarAsync(ct);
        ActualizarTexto(result);
    }


    private async Task MostrarEnMapaAsync()
    {
        var result = await _coord.CapturarAsync();

        if (result is GpsResult.Success s)
        {
            try
            {
                var url = $"https://maps.google.com/?q={s.Location.Latitude},{s.Location.Longitude}";
                await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                Coordenadas = $"No se pudo abrir Google Maps: {ex.Message}";
            }
        }
        else
        {
            ActualizarTexto(result);
        }
    }

    // El overlay ya lo manejó el coordinator; acá solo se traduce a texto.
    private void ActualizarTexto(GpsResult result)
    {
        Coordenadas = result switch
        {
            GpsResult.Success s => $"Lat: {s.Location.Latitude:F6}, Lng: {s.Location.Longitude:F6}",
            GpsResult.PermissionDenied => "Permiso de ubicación necesario.",
            GpsResult.PermissionRestricted => "Acceso a la ubicación restringido.",
            GpsResult.GpsDisabled => "El GPS está desactivado. Activalo desde ajustes.",
            GpsResult.NotSupported => "Este dispositivo no soporta GPS.",
            GpsResult.NoSignal => "No se pudo obtener ubicación (GPS sin señal).",
            GpsResult.Cancelled => "Operación cancelada por el usuario.",
            GpsResult.Failure f => $"Error: {f.Message}",
            _ => ""
        };
    }

    public ICommand ObtenerDominicilioCommand { get; }

    public async Task ObtenerDominicilioAsync(CancellationToken ct = default)
    {
        Coordenadas = "Obteniendo ubicación GPS...";
        var result = await _coord.CapturarAsync(ct);
        if (result is GpsResult.Success s)
        {
            var domicilio = await _mapService.GetDomicilioAsync(s.Location.Latitude, s.Location.Longitude);
            Coordenadas = domicilio;           
        }
        else
        {
            ActualizarTexto(result);
        }
    }

}
