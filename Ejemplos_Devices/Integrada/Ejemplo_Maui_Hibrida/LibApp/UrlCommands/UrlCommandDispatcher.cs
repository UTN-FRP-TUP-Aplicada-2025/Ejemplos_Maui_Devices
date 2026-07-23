using System.Diagnostics;

namespace LibApp.UrlCommands;

// Recorre los handlers en orden y delega en el primero que pueda manejar la URL.
// Sin switch/if por comando: sólo el loop sobre el contrato (abierto/cerrado).
//
// PLAN 1: la clasificación (Plan) se separa de la ejecución (ExecuteAsync). La cancelación deja
// de ser "es comando ⇒ cancelo" y pasa a ser un OR sobre los handlers que matchean.
public sealed class UrlCommandDispatcher
{
    private readonly IEnumerable<IUrlCommandHandler> _handlers;

    public UrlCommandDispatcher(IEnumerable<IUrlCommandHandler> handlers)
    {
        _handlers = handlers;
    }

    /// <summary>
    /// Clasifica la URL. 100% SÍNCRONO a propósito: el WebView lee e.Cancel apenas retorna el
    /// handler de Navigating, así que la decisión no puede quedar detrás de un await.
    /// Evalúa CanHandle UNA sola vez por handler y corre los ganchos síncronos de los que matchean.
    /// </summary>
    public UrlPlan Plan(string url)
    {
        List<IUrlCommandHandler>? matches = null;

        foreach (var h in _handlers)
        {
            if (h.CanHandle(url) == false) continue;
            (matches ??= new()).Add(h);
        }

        if (matches is null) return UrlPlan.Ninguno;

        // OR sobre los que matchean: alcanza con uno cancelable para cancelar (Plan 1 §4, eje A).
        var cancel = false;
        foreach (var h in matches)
            if (h.CancelsNavigation) { cancel = true; break; }

        // Fase síncrona: corre para TODOS los que matchean, no sólo para el que se va a ejecutar.
        // Así el gancho no depende de la política first-match-wins.
        foreach (var h in matches)
            h.OnMatchedSync(url);

#if DEBUG
        // Invariante de continuación (Plan 1 §5): si se canceló, el handler que efectivamente se
        // va a ejecutar tiene que ser uno de los que pidieron cancelar. Si no, la página queda
        // congelada sin inyección ni re-navegación. Falla ruidoso acá en vez de manifestarse como
        // un WebView colgado en el dispositivo.
        if (cancel && matches[0].CancelsNavigation == false)
        {
            Debug.Fail(
                $"URL mal formada: cancela por {matches.First(h => h.CancelsNavigation).GetType().Name} " +
                $"pero ejecuta {matches[0].GetType().Name} (first-match-wins). Navegación muerta. URL: {url}");
        }
#endif

        return new UrlPlan(matches, cancel);
    }

    public async Task<BridgeOutcome> ExecuteAsync(UrlPlan plan, string url)
    {
        var handler = plan.Primary;

        // Ningún comando matchea: navegación normal, no hay que reescribir la URL.
        // NavigateTo debe quedar en null: es exclusivo del modo Substitution (re-navegar con el
        // query param de comando sustituido). Devolver el propio "url" acá provoca que
        // MainViewModel.Navigating reasigne Url = e.Url en TODA navegación normal (incluido el
        // reload del gesto pull-to-refresh), lo que reasigna WebView.Source y dispara una segunda
        // navegación superpuesta a la que ya está en curso, impidiendo que Navigated llegue a
        // cerrar el RefreshView (IsRefreshing nunca vuelve a false de forma limpia).
        if (handler is null) return new BridgeOutcome(false, null);

        var outcome = await handler.HandleAsync(url);

#if DEBUG
        // La parte VERIFICABLE del invariante de continuación (Plan 1 §5): un comando que declara
        // Substitution y no re-navega dejó la navegación muerta, sin excepción posible.
        if (handler.DeliveryFor(url) == CommandDelivery.Substitution && outcome.NavigateTo is null)
        {
            Debug.Fail(
                $"{handler.GetType().Name} declara CommandDelivery.Substitution para esta URL pero " +
                $"devolvió NavigateTo=null: la navegación se canceló y no se re-navegó. URL: {url}");
        }
#endif

        return outcome;
    }

    /// <summary>Conveniencia para los botones nativos, que no vienen de un evento Navigating.</summary>
    public Task<BridgeOutcome> DispatchAsync(string url) => ExecuteAsync(Plan(url), url);

    /// <summary>
    /// Se conserva por compatibilidad de firma pública. MainViewModel ya no la usa: necesita el
    /// plan completo, no un booleano.
    /// </summary>
    public bool IsCommand(string url) => Plan(url).HasMatches;
}
