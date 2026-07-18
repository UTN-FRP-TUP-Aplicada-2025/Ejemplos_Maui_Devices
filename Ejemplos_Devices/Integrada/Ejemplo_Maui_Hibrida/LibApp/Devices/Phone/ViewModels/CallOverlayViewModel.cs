using CommunityToolkit.Mvvm.Input;

using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.Phone.Models;
using LibApp.Devices.Phone.Services;

namespace LibApp.Devices.Phone.ViewModels;

/// <summary>
/// Orquesta el inicio de una llamada sobre la capa de overlay. Espejo de
/// GpsOverlayViewModel y PrinterOverlayViewModel.
/// </summary>
public partial class CallOverlayViewModel : StatusOverlayViewModel
{
    private readonly ICallService _callService;

    // Último número y modo: el VM es singleton, así que el reintento funciona aunque el caller
    // original ya no exista.
    private string _ultimoNumero = "";
    private CallMode _ultimoModo = CallMode.Direct;

    public CallOverlayViewModel(ICallService callService)
    {
        _callService = callService;
        Hide();
    }

    public async Task<CallResult> LlamarAsync(string numero, CallMode mode = CallMode.Direct)
    {
        _ultimoNumero = numero;
        _ultimoModo = mode;

        ShowBusy("Iniciando llamada", "Aguarde un instante, conectando la llamada…", "timer.gif");

        var result = await _callService.LlamarAsync(numero, mode);
        Procesar(result);
        return result;
    }

    // Un solo camino: sin el guard previo que dejaba el 'case Success' del switch inalcanzable.
    private void Procesar(CallResult result)
    {
        if (result is CallResult.Success)
        {
            Hide();
            return;
        }

        Mostrar(CallErrorCatalog.Describir(result));
    }

    /// <summary>
    /// Cada fallo con su pantalla y su botonera. El mensaje que ve el usuario nunca es el de la
    /// excepción: antes <c>Failure</c> mostraba <c>f.Message</c> crudo — texto de Android, en
    /// inglés, sin acción posible.
    /// </summary>
    private void Mostrar(CallFailure fallo)
    {
        var acciones = new List<OverlayAction>();
        var glyph = "error";

        switch (fallo.Code)
        {
            case CallErrorCatalog.PermisoNecesario:
                glyph = "phone_locked";
                acciones.Add(new OverlayAction("Pedir permiso", PedirPermisoCommand));
                break;

            case CallErrorCatalog.PermisoDenegado:
                glyph = "phone_locked";
                acciones.Add(new OverlayAction("Abrir configuración", AbrirAjustesCommand));
                break;

            case CallErrorCatalog.PermisoRestringido:
                glyph = "phone_disabled";
                acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand));
                break;

            case CallErrorCatalog.NoSoportado:
                glyph = "dialpad";
                acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand));
                break;

            // Número inválido: no lo elige el usuario, viene del handler. Reintentar daría lo
            // mismo, así que sólo se cierra.
            case CallErrorCatalog.NumeroInvalido:
                acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand));
                break;

            default:
                acciones.Add(new OverlayAction("Reintentar", ReintentarCommand));
                break;
        }

        if (!acciones.Any(a => a.Text == "Cerrar"))
            acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));

        ShowError(glyph, fallo.Title, fallo.DisplayMessage, acciones.ToArray());
    }

    [RelayCommand] private void AbrirAjustes() => _callService.OpenAppSettings();

    [RelayCommand] private void CerrarOverlay() => Hide();

    /// <summary>
    /// Reintenta con el último número. `PedirPermiso` y `Reintentar` hacen lo mismo —rehacer el
    /// flujo—, pero se conservan separados porque nombran intenciones distintas en la botonera.
    /// </summary>
    [RelayCommand] private Task PedirPermiso() => LlamarAsync(_ultimoNumero, _ultimoModo);

    [RelayCommand] private Task Reintentar() => LlamarAsync(_ultimoNumero, _ultimoModo);
}
