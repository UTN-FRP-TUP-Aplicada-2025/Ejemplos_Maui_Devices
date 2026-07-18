using CommunityToolkit.Mvvm.Input;
using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.GPS.Models;
using LibApp.Devices.GPS.Services;

namespace LibApp.Devices.GPS.ViewModels;

/// <summary>
/// Orquesta la captura de ubicación sobre la capa de overlay: permisos → lectura → resultado.
/// Espejo de PrinterOverlayViewModel y CallOverlayViewModel.
/// </summary>
public partial class GpsOverlayViewModel : StatusOverlayViewModel
{
    private readonly IGpsService _gpsService;

    public GpsOverlayViewModel(IGpsService gpsService)
    {
        _gpsService = gpsService;
        Hide();
    }

    /// <summary>
    /// Punto de entrada del flujo. Devuelve <b>la variante que produjo el servicio</b>: antes
    /// colapsaba todo lo no-<c>Success</c> en <c>Failure("")</c> con mensaje vacío, y las siete
    /// causas quedaban indistinguibles para el llamador.
    /// </summary>
    public async Task<GpsResult> SolicitarGeolocalizacion()
    {
        ShowBusy("Buscando posición GPS",
            "Aguarde unos segundos, y será redirigido automáticamente",
            "satelite.gif");

        var result = await _gpsService.ObtenerUbicacionAsync();
        Procesar(result);
        return result;
    }

    // Un solo camino: sin el guard previo que dejaba el 'case Success' del switch inalcanzable
    // y, con él, muerta la única asignación real de la propiedad de salida.
    private void Procesar(GpsResult result)
    {
        // Opción A (ADR-0009): en éxito se oculta ACÁ, de forma determinista y autocontenida.
        // NO se delega el cierre en MainViewModel.Navigated: si la re-navegación apunta a una URL
        // idéntica (pedir de nuevo la misma coordenada), el WebView no navega, Navigated no se
        // dispara y el overlay quedaba colgado en "Buscando posición GPS". La lectura fresca del
        // servicio hace que la espera sea real (objetivo de 1.b) sin necesitar la Opción B.
        if (result is GpsResult.Success)
        {
            Hide();
            return;
        }

        Mostrar(GpsErrorCatalog.Describir(result));
    }

    /// <summary>
    /// Cada fallo tiene su pantalla con su botonera. Ninguna variante puede terminar en un
    /// <c>Hide()</c> silencioso: apagar el GPS no producía ningún mensaje porque el texto se
    /// escribía en una propiedad que no estaba bindeada al overlay.
    /// </summary>
    private void Mostrar(GpsFailure fallo)
    {
        var acciones = new List<OverlayAction>();

        switch (fallo.Code)
        {
            case GpsErrorCatalog.PermisoNecesario:
                acciones.Add(new OverlayAction("Pedir permiso", PedirPermisoCommand));
                break;

            case GpsErrorCatalog.PermisoDenegado:
                acciones.Add(new OverlayAction("Abrir configuración", AbrirAjustesCommand));
                break;

            // El permiso lo concede la app; el GPS se enciende desde el sistema. Son ajustes
            // distintos: mandarlo a los de la app acá lo dejaría sin salida.
            case GpsErrorCatalog.Apagado:
                acciones.Add(new OverlayAction("Activar ubicación", AbrirAjustesUbicacionCommand));
                acciones.Add(new OverlayAction("Reintentar", ReintentarCommand, OverlayActionStyle.Secondary));
                break;

            case GpsErrorCatalog.SinSenal:
            case GpsErrorCatalog.Cancelado:
            case GpsErrorCatalog.Desconocido:
                acciones.Add(new OverlayAction("Reintentar", ReintentarCommand));
                break;

            // Restringido por política y GPS inexistente no tienen acción posible: el único
            // botón es Cerrar, y por eso es el primario.
            default:
                acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand));
                break;
        }

        if (!acciones.Any(a => a.Text == "Cerrar"))
            acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));

        ShowError("location_off", fallo.Title, fallo.DisplayMessage, acciones.ToArray());
    }

    [RelayCommand] private void AbrirAjustes() => _gpsService.OpenAppSettings();

    [RelayCommand] private void AbrirAjustesUbicacion() => _gpsService.OpenLocationSettings();

    [RelayCommand] private void CerrarOverlay() => Hide();

    /// <summary>Reintenta el permiso rehaciendo el flujo completo.</summary>
    [RelayCommand] private Task PedirPermiso() => SolicitarGeolocalizacion();

    [RelayCommand] private Task Reintentar() => SolicitarGeolocalizacion();
}
