namespace Ejemplo_Maui_Hibrida.UrlCommands;

// Recorre los handlers en orden y delega en el primero que pueda manejar la URL.
// Sin switch/if por comando: sólo el loop sobre el contrato (abierto/cerrado).
public sealed class UrlCommandDispatcher
{
    private readonly IEnumerable<IUrlCommandHandler> _handlers;

    public UrlCommandDispatcher(IEnumerable<IUrlCommandHandler> handlers)
    {
        _handlers = handlers;
    }

    // Permite cancelar la navegación de forma sincrónica, antes de cualquier await.
    public bool IsCommand(string url) => _handlers.Any(h => h.CanHandle(url));

    public async Task<BridgeOutcome> DispatchAsync(string url)
    {
        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(url))
                return await handler.HandleAsync(url);
        }

        // Ningún comando matchea: navegación normal.
        return new BridgeOutcome(false, url);
    }
}
