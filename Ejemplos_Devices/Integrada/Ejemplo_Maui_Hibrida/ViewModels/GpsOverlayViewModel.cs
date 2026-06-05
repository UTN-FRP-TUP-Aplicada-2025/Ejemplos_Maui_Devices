using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.Services;

namespace Ejemplo_Maui_Hibrida.ViewModels;

public partial class GpsOverlayViewModel : StatusOverlayViewModel
{
    private readonly GpsService _gpsService;

    [ObservableProperty]
    private string coordenadas = "";

    public GpsOverlayViewModel(GpsService gpsService)
    {
        _gpsService = gpsService;
        Hide();
    }

    async public Task<GpsResult> SolicitarGeolocalizacion()
    {
        ShowBusy("Buscando posición GPS",
            "Aguarde unos segundos, y será redirigido automáticamente",
            "satelite.gif");

        var result = await _gpsService.ObtenerUbicacionAsync();

        if (result is GpsResult.Success)
        {
            Hide();
            return result;
        }

        MostrarResultado(result);
        return new GpsResult.Failure("");
    }

    [RelayCommand]
    public void AbrirAjustes() => _gpsService.OpenAppSettings();

    [RelayCommand]
    public void CerrarOverlay() => Hide();

    /// <summary>
    /// Reintenta solicitar el permiso/ubicación GPS al usuario.
    /// </summary>
    [RelayCommand]
    public Task PedirPermiso() => SolicitarGeolocalizacion();

    private void ShowPermissionDenied(bool canRetry)
    {
        var title = canRetry ? "Permiso de ubicación necesario" : "Acceso a la ubicación denegado";
        var message = canRetry
            ? "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Podés intentar conceder el permiso."
            : "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Habilitalo desde los ajustes de la aplicación.";

        // Android con posibilidad de reintento => "Pedir permiso".
        // Android "no volver a preguntar" / iOS => "Abrir configuración".
        var primary = canRetry
            ? new OverlayAction("Pedir permiso", PedirPermisoCommand)
            : new OverlayAction("Abrir configuración", AbrirAjustesCommand, OverlayActionStyle.Secondary);

        ShowError("location_off", title, message,
            primary,
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
    }

    private void ShowRestricted()
    {
        ShowError("location_off", "Acceso restringido",
            "El acceso a la ubicación está restringido por una política del dispositivo. Consultá con el administrador.",
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
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
