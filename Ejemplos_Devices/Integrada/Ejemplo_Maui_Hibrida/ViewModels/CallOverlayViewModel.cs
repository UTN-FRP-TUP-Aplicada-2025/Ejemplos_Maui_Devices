using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.Services;

namespace Ejemplo_Maui_Hibrida.ViewModels;

public partial class CallOverlayViewModel : StatusOverlayViewModel
{
    private readonly CallService _callService;

    // Último número y modo, para poder reintentar tras conceder el permiso.
    private string _ultimoNumero = "";
    private CallMode _ultimoModo = CallMode.Direct;

    [ObservableProperty]
    private string estado = "";

    public CallOverlayViewModel(CallService callService)
    {
        _callService = callService;
        Hide();
    }

    async public Task<CallResult> LlamarAsync(string numero, CallMode mode = CallMode.Direct)
    {
        _ultimoNumero = numero;
        _ultimoModo = mode;

        ShowBusy("Iniciando llamada", "Aguarde un instante, conectando la llamada…", "timer.gif");

        var result = await _callService.LlamarAsync(numero, mode);

        if (result is CallResult.Success)
        {
            Hide();
            return result;
        }

        MostrarResultado(result);
        return result;
    }

    [RelayCommand]
    public void AbrirAjustes() => _callService.OpenAppSettings();

    [RelayCommand]
    public void CerrarOverlay() => Hide();

    /// <summary>
    /// Reintenta solicitar el permiso de llamadas y reinvoca con el último número/modo.
    /// </summary>
    [RelayCommand]
    public Task PedirPermiso() => LlamarAsync(_ultimoNumero, _ultimoModo);

    /// <summary>
    /// Reintenta la llamada con el último número/modo.
    /// </summary>
    [RelayCommand]
    public Task Reintentar() => LlamarAsync(_ultimoNumero, _ultimoModo);

    private void ShowPermissionDenied(bool canRetry)
    {
        var title = canRetry ? "Permiso de llamadas necesario" : "Acceso a llamadas denegado";
        var message = canRetry
            ? "Para llamar directamente necesitamos permiso para realizar llamadas. Podés intentar concederlo."
            : "Para llamar directamente necesitamos permiso para realizar llamadas. Habilitalo desde los ajustes de la aplicación.";

        // Android con posibilidad de reintento => "Pedir permiso".
        // Android "no volver a preguntar" => "Abrir configuración".
        var primary = canRetry
            ? new OverlayAction("Pedir permiso", PedirPermisoCommand)
            : new OverlayAction("Abrir configuración", AbrirAjustesCommand, OverlayActionStyle.Secondary);

        ShowError("phone_locked", title, message,
            primary,
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
    }

    private void ShowRestricted()
    {
        ShowError("phone_disabled", "Acceso restringido",
            "Las llamadas están restringidas por una política del dispositivo. Consultá con el administrador.",
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
    }

    private void MostrarResultado(CallResult result)
    {
        switch (result)
        {
            case CallResult.Success s:
                Estado = $"Llamada iniciada a {s.Numero} ({s.Mode}).";
                Hide();
                break;

            case CallResult.PermissionDenied d:
                ShowPermissionDenied(d.CanRetry);
                break;

            case CallResult.PermissionRestricted:
                ShowRestricted();
                break;

            case CallResult.NotSupported:
                ShowError("dialpad", "Llamadas no disponibles",
                    "Este dispositivo no puede realizar llamadas telefónicas.",
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case CallResult.InvalidNumber:
                ShowError("error", "Número inválido",
                    "El número de teléfono está vacío o no es válido.",
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case CallResult.Cancelled:
                Estado = "Operación cancelada por el usuario.";
                Hide();
                break;

            case CallResult.Failure f:
                ShowError("error", "No se pudo realizar la llamada", f.Message,
                    new OverlayAction("Reintentar", ReintentarCommand),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;
        }
    }
}
