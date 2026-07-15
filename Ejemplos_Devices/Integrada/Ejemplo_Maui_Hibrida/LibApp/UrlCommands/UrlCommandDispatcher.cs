namespace LibApp.UrlCommands;

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
            if (handler.CanHandle(url)) return await handler.HandleAsync(url);
        }

        // Ningún comando matchea: navegación normal, no hay que reescribir la URL.
        // NavigateTo debe quedar en null: es exclusivo del caso GPS (re-navegar con
        // coordenadas). Devolver el propio "url" acá provoca que MainViewModel.Navigating
        // reasigne Url = e.Url en TODA navegación normal (incluido el reload del gesto
        // pull-to-refresh), lo que reasigna WebView.Source y dispara una segunda
        // navegación superpuesta a la que ya está en curso, impidiendo que Navigated
        // llegue a cerrar el RefreshView (IsRefreshing nunca vuelve a false de forma limpia).
        return new BridgeOutcome(false, null);
    }
}
