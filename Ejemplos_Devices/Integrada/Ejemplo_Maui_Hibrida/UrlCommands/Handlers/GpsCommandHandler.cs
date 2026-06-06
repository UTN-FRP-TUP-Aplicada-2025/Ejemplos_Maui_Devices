using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.ViewModels;

namespace Ejemplo_Maui_Hibrida.UrlCommands.Handlers;

// Interpreta "geo=1": pide geolocalización y reescribe la URL con las coordenadas.
public sealed class GpsCommandHandler : IUrlCommandHandler
{
    private readonly GpsOverlayViewModel _gps;

    public GpsCommandHandler(GpsOverlayViewModel gps)
    {
        _gps = gps;
    }

    public bool CanHandle(string url) =>
        url.Contains("geo=1", StringComparison.OrdinalIgnoreCase);

    public async Task<BridgeOutcome> HandleAsync(string url)
    {
        var result = await _gps.SolicitarGeolocalizacion();

        if (result is GpsResult.Success s)
        {
            var next = url.Replace("geo=1",
                $"Latitud={s.Location.Latitude}&Longitud={s.Location.Longitude}&",
                StringComparison.OrdinalIgnoreCase);

            return new BridgeOutcome(true, next);
        }

        // El overlay ya muestra el error; se queda en la página.
        return new BridgeOutcome(true, null);
    }
}
