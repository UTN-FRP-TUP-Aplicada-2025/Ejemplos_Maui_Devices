namespace Ejemplo_Maui_GPS.Services;

internal class TestService
{
    private readonly GpsService _gps;
    public TestService(GpsService gps) => _gps = gps;

    public Task<GpsResult> GetLocationAsync(CancellationToken ct = default)
        => _gps.ObtenerUbicacionAsync(ct);

}
