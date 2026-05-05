using Ejemplo_Maui_GPS.ViewModels;

namespace Ejemplo_Maui_GPS.Services;

// Punto único de entrada para capturar GPS desde cualquier parte de la app.
// Es dueño del GpsOverlayViewModel (singleton) y de la cancelación.
// Cualquier caller (página, comando, deep link, navegación por URL) hace:
//     await _coord.CapturarAsync();
// y el overlay aparece/desaparece automáticamente según el resultado.
public class GpsCoordinator
{
    private readonly GpsService _gps;
    private readonly LocationPermissionService _permissions;
    private CancellationTokenSource? _cts;

    public GpsOverlayViewModel Overlay { get; }

    public GpsCoordinator(GpsService gps, LocationPermissionService permissions)
    {
        _gps = gps;
        _permissions = permissions;

        // Los callbacks del overlay (reintentar / abrir ajustes) viven con el coordinador,
        // no atados a una página concreta.
        Overlay = new GpsOverlayViewModel( onRetry: () => CapturarAsync(), onOpenSettings: () => _permissions.OpenAppSettings());
    }

    public async Task<GpsResult> CapturarAsync(CancellationToken ct = default)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = ct == default
            ? new CancellationTokenSource()
            : CancellationTokenSource.CreateLinkedTokenSource(ct);

        await MainThread.InvokeOnMainThreadAsync(Overlay.ShowBusy);
        try
        {
            var result = await _gps.ObtenerUbicacionAsync(_cts.Token);
            await MainThread.InvokeOnMainThreadAsync(() => Aplicar(result));
            return result;
        }
        catch
        {
            await MainThread.InvokeOnMainThreadAsync(Overlay.Hide);
            throw;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Cancelar() => _cts?.Cancel();

    // Solo el overlay; el texto de coordenadas lo decide cada caller (VM, etc.)
    private void Aplicar(GpsResult result)
    {
        switch (result)
        {
            case GpsResult.PermissionDenied d:
                Overlay.ShowPermissionDenied(d.CanRetry);
                break;
            case GpsResult.PermissionRestricted:
                Overlay.ShowRestricted();
                break;
            default:
                Overlay.Hide();
                break;
        }
    }
}
