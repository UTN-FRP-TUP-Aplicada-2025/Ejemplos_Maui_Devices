using System.Windows.Input;
using Ejemplo_Maui_GPS.Services;

namespace Ejemplo_Maui_GPS.ViewModels;

// Orquesta la página principal: dispara GpsService, traduce GpsResult a estado
// del Overlay y a la propiedad Coordenadas. Maneja cancelación con CancellationTokenSource
// (de ahí que sí implemente IDisposable).
public class MainPageViewModel : ViewModelBase, IDisposable
{
    private readonly GpsService _gps;
    private readonly LocationPermissionService _permissions;
    private CancellationTokenSource? _cts;

    public MainPageViewModel(GpsService gps, LocationPermissionService permissions)
    {
        _gps = gps;
        _permissions = permissions;
        Overlay = new GpsOverlayViewModel(
            onRetry: () => ObtenerUbicacionAsync(CancellationToken.None),
            onOpenSettings: () => _permissions.OpenAppSettings());

        // Reflejar cambios de visibilidad del overlay en MostrandoContenido.
        Overlay.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(GpsOverlayViewModel.IsVisible))
                OnPropertyChanged(nameof(MostrandoContenido));
        };

        ObtenerUbicacionCommand = new AsyncRelayCommand(ObtenerUbicacionAsync);
        CancelarCommand = new RelayCommand(Cancelar, () => _cts is not null);
        MostrarEnMapaCommand = new AsyncRelayCommand(MostrarEnMapaAsync);
    }

    // ── Propiedades enlazadas ─────────────────────────────

    public GpsOverlayViewModel Overlay { get; }

    private string _coordenadas = "";
    public string Coordenadas
    {
        get => _coordenadas;
        set => SetProperty(ref _coordenadas, value);
    }

    // Mostrar contenido principal cuando el overlay no es visible.
    // Se notifica desde Overlay.PropertyChanged.
    public bool MostrandoContenido => !Overlay.IsVisible;

    // ── Comandos ──────────────────────────────────────────

    public ICommand ObtenerUbicacionCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand MostrarEnMapaCommand { get; }

    // ── Acciones ──────────────────────────────────────────

    private async Task ObtenerUbicacionAsync(CancellationToken _)
    {
        Overlay.ShowBusy();
        OnPropertyChanged(nameof(MostrandoContenido));

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        (CancelarCommand as RelayCommand)?.RaiseCanExecuteChanged();

        try
        {
            Coordenadas = "Obteniendo ubicación GPS...";
            var result = await _gps.ObtenerUbicacionAsync(_cts.Token);
            AplicarResultado(result);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            (CancelarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(MostrandoContenido));
        }
    }

    private async Task MostrarEnMapaAsync()
    {
        Overlay.ShowBusy();
        OnPropertyChanged(nameof(MostrandoContenido));

        try
        {
            var result = await _gps.ObtenerUbicacionAsync();

            if (result is GpsResult.Success s)
            {
                Overlay.Hide();
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
                AplicarResultado(result);
            }
        }
        finally
        {
            OnPropertyChanged(nameof(MostrandoContenido));
        }
    }

    private void Cancelar()
    {
        _cts?.Cancel();
    }

    // ── Mapeo de GpsResult a UI ───────────────────────────

    private void AplicarResultado(GpsResult result)
    {
        switch (result)
        {
            case GpsResult.Success s:
                Coordenadas = $"Lat: {s.Location.Latitude:F6}, Lng: {s.Location.Longitude:F6}";
                Overlay.Hide();
                break;

            case GpsResult.PermissionDenied d:
                Overlay.ShowPermissionDenied(d.CanRetry);
                break;

            case GpsResult.PermissionRestricted:
                Overlay.ShowRestricted();
                break;

            case GpsResult.GpsDisabled:
                Coordenadas = "El GPS está desactivado. Activalo desde ajustes.";
                Overlay.Hide();
                break;

            case GpsResult.NotSupported:
                Coordenadas = "Este dispositivo no soporta GPS.";
                Overlay.Hide();
                break;

            case GpsResult.NoSignal:
                Coordenadas = "No se pudo obtener ubicación (GPS sin señal).";
                Overlay.Hide();
                break;

            case GpsResult.Cancelled:
                Coordenadas = "Operación cancelada por el usuario.";
                Overlay.Hide();
                break;

            case GpsResult.Failure f:
                Coordenadas = $"Error: {f.Message}";
                Overlay.Hide();
                break;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        GC.SuppressFinalize(this);
    }
}
