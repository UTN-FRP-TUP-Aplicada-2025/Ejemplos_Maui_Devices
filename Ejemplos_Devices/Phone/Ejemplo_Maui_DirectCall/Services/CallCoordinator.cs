using Ejemplo_Maui_DirectCall.ViewModels;

namespace Ejemplo_Maui_DirectCall.Services;

// Punto único de entrada para realizar llamadas desde cualquier parte de la app.
// Es dueño del CallOverlayViewModel (singleton) y de la cancelación.
//
// Cualquier caller (página, comando, servicio, deep link) hace:
//     var result = await _coord.CallAsync(numero);
// y el overlay aparece/desaparece automáticamente según el resultado.
//
// El último número se recuerda para que el botón "Pedir permiso" del overlay
// pueda reintentar la operación sin que el caller original esté presente.
public class CallCoordinator
{
    private readonly PhoneDialerDevice _dialer;
    private CancellationTokenSource? _cts;
    private string? _ultimoNumero;

    public CallOverlayViewModel Overlay { get; }

    public CallCoordinator(PhoneDialerDevice dialer)
    {
        _dialer = dialer;

        Overlay = new CallOverlayViewModel(
            onRetry: ReintentarAsync,
            onOpenSettings: AbrirAjustes);
    }

    public async Task<CallResult> CallAsync(string numero, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            var fail = new CallResult.Failure("Ingresá un número de teléfono.");
            await MainThread.InvokeOnMainThreadAsync(() => Aplicar(fail));
            return fail;
        }

        _ultimoNumero = numero;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = ct == default
            ? new CancellationTokenSource()
            : CancellationTokenSource.CreateLinkedTokenSource(ct);

        await MainThread.InvokeOnMainThreadAsync(() => Overlay.ShowBusy());

        try
        {
            var result = await _dialer.CallPhoneAsync(numero, _cts.Token);
            await MainThread.InvokeOnMainThreadAsync(() => Aplicar(result));
            return result;
        }
        catch (OperationCanceledException)
        {
            var cancelled = new CallResult.Cancelled();
            await MainThread.InvokeOnMainThreadAsync(() => Aplicar(cancelled));
            return cancelled;
        }
        catch (Exception ex)
        {
            var failure = new CallResult.Failure(ex.Message);
            await MainThread.InvokeOnMainThreadAsync(() => Aplicar(failure));
            return failure;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Cancelar() => _cts?.Cancel();

    private Task ReintentarAsync()
    {
        // Reintento desde el botón del overlay: usa el último número conocido.
        if (string.IsNullOrWhiteSpace(_ultimoNumero))
        {
            Overlay.Hide();
            return Task.CompletedTask;
        }
        return CallAsync(_ultimoNumero);
    }

    private static void AbrirAjustes()
    {
        try { AppInfo.Current.ShowSettingsUI(); }
        catch { /* no todos los dispositivos lo soportan; se ignora */ }
    }

    private void Aplicar(CallResult result)
    {
        switch (result)
        {
            case CallResult.Success:
                Overlay.Hide();
                break;
            case CallResult.PermissionDenied:
                Overlay.ShowPermissionDenied(canRetry: true);
                break;
            case CallResult.PermissionDeniedPermanent:
                Overlay.ShowPermissionDenied(canRetry: false);
                break;
            case CallResult.PermissionRestricted:
                Overlay.ShowRestricted();
                break;
            case CallResult.NotSupported:
                Overlay.ShowNotSupported();
                break;
            case CallResult.Cancelled:
                Overlay.Hide();
                break;
            case CallResult.Failure f:
                Overlay.ShowFailure(f.Message);
                break;
        }
    }
}
