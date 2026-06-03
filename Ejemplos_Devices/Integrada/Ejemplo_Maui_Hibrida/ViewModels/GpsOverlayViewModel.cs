using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.Services;

namespace Ejemplo_Maui_Hibrida.ViewModels;

public partial class GpsOverlayViewModel : ObservableObject
{
    GpsService _gpsService = default;

    public GpsOverlayViewModel(GpsService gpsService)
    {
        _gpsService = gpsService;
        Hide();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool isDenied;

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string message = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MustOpenSettings))]
    private bool canRetry;

    [ObservableProperty]
    private string coordenadas = "";

    public bool MustOpenSettings => !canRetry;

    /// <summary>
    /// Overlay visible si está esperando GPS o mostrando panel de permisos denegados.
    /// </summary>
    public bool IsVisible => isBusy || isDenied;

    async public Task<GpsResult> SolicitarGeolocalizacion()
    {
        ShowBusy();
        try
        {
            var result = await _gpsService.ObtenerUbicacionAsync();

            if (result is GpsResult.Success)
            {
                Hide();
                return result;
            }
            else
            {
                MostrarResultado(result);
            }
        }
        finally
        {
        }
        return new GpsResult.Failure("");
    }

    [RelayCommand]
    public void AbrirAjustes()
    {
        _gpsService.OpenAppSettings();
    }

    [RelayCommand]
    public void CerrarOverlay()
    {
        Hide();
    }

    /// <summary>
    /// Reintenta solicitar el permiso/ubicación GPS al usuario.
    /// </summary>
    [RelayCommand]
    public Task PedirPermiso()
    {
        return SolicitarGeolocalizacion();
    }

    public void ShowBusy()
    {
        IsDenied = false;
        IsBusy = true;
    }

    public void ShowPermissionDenied(bool canRetry)
    {
        Title = canRetry ? "Permiso de ubicación necesario" : "Acceso a la ubicación denegado";
        Message = canRetry
            ? "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Podés intentar conceder el permiso."
            : "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Habilitalo desde los ajustes de la aplicación.";
        CanRetry = canRetry;
        IsBusy = false;
        IsDenied = true;
    }

    public void ShowRestricted()
    {
        Title = "Acceso restringido";
        Message = "El acceso a la ubicación está restringido por una política del dispositivo. Consultá con el administrador.";
        CanRetry = false;
        IsBusy = false;
        IsDenied = true;
    }

    public void Hide()
    {
        IsBusy = false;
        IsDenied = false;
    }

    private void MostrarResultado(GpsResult result)
    {
        switch (result)
        {
            case GpsResult.Success s:
                Coordenadas = $"Lat: {s.Location.Latitude:F6}, Lng: {s.Location.Longitude:F6}";
                Hide();
                break;

            case GpsResult.PermissionDenied d:
                ShowPermissionDenied(d.CanRetry);
                break;

            case GpsResult.PermissionRestricted:
                ShowRestricted();
                break;

            case GpsResult.GpsDisabled:
                Coordenadas = "El GPS está desactivado. Activalo desde ajustes.";
                Hide();
                break;

            case GpsResult.NotSupported:
                Coordenadas = "Este dispositivo no soporta GPS.";
                Hide();
                break;

            case GpsResult.NoSignal:
                Coordenadas = "No se pudo obtener ubicación (GPS sin señal).";
                Hide();
                break;

            case GpsResult.Cancelled:
                Coordenadas = "Operación cancelada por el usuario.";
                Hide();
                break;

            case GpsResult.Failure f:
                Coordenadas = $"Error: {f.Message}";
                Hide();
                break;
        }
    }
}
