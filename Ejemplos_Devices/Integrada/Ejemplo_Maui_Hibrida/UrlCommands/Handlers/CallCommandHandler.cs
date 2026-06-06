using Ejemplo_Maui_Hibrida.Models;
using Ejemplo_Maui_Hibrida.ViewModels;

namespace Ejemplo_Maui_Hibrida.UrlCommands.Handlers;

// Interpreta "photo=2": dispara una llamada directa al número configurado.
public sealed class CallCommandHandler : IUrlCommandHandler
{
    private const string NumeroPorDefecto = "3434807427";

    private readonly CallOverlayViewModel _call;

    public CallCommandHandler(CallOverlayViewModel call)
    {
        _call = call;
    }

    public bool CanHandle(string url) =>
        url.Contains("photo=2", StringComparison.OrdinalIgnoreCase);

    public async Task<BridgeOutcome> HandleAsync(string url)
    {
        _ = await _call.LlamarAsync(NumeroPorDefecto, CallMode.Direct);
        return new BridgeOutcome(true, null);
    }
}
