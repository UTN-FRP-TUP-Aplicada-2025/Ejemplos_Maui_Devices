using LibApp.UrlCommands;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

// Plan 1 §7: la clasificación (Plan) se separa de la ejecución (ExecuteAsync), y la cancelación
// pasa de "es comando ⇒ cancelo" a un OR sobre los handlers que matchean.

// Handlers mínimos para ejercitar los dos ejes del Plan 1.
file sealed class HandlerCancelable : IUrlCommandHandler
{
    public bool CanHandle(string url) => url.Contains("cmdC=cmdC", StringComparison.OrdinalIgnoreCase);
    public Task<BridgeOutcome> HandleAsync(string url) => Task.FromResult(new BridgeOutcome(true, null));
}

file sealed class HandlerNoCancelable : IUrlCommandHandler
{
    public int VecesSync { get; private set; }
    public bool CanHandle(string url) => url.Contains("cmdN=cmdN", StringComparison.OrdinalIgnoreCase);
    public bool CancelsNavigation => false;
    public void OnMatchedSync(string url) => VecesSync++;
    public Task<BridgeOutcome> HandleAsync(string url) => Task.FromResult(new BridgeOutcome(false, null));
}

public class UrlCommandDispatcherTests
{
    [Fact]
    public void Sin_matches_no_cancela()
    {
        var d = new UrlCommandDispatcher(new IUrlCommandHandler[] { new HandlerCancelable() });

        var plan = d.Plan("/panel?algo=otracosa");

        Assert.False(plan.HasMatches);
        Assert.False(plan.Cancel);
    }

    [Fact]
    public void Handler_no_cancelable_solo_no_cancela()
    {
        var d = new UrlCommandDispatcher(new IUrlCommandHandler[] { new HandlerNoCancelable() });

        var plan = d.Plan("/panel?cmdN=cmdN");

        Assert.True(plan.HasMatches);
        Assert.False(plan.Cancel);            // ← lo que el modelo anterior no podía expresar
    }

    // Eje A (Plan 1 §4): alcanza con UN handler cancelable para que el plan cancele.
    [Fact]
    public void Un_cancelable_entre_varios_hace_cancelar_el_plan()
    {
        var d = new UrlCommandDispatcher(new IUrlCommandHandler[] { new HandlerCancelable(), new HandlerNoCancelable() });

        var plan = d.Plan("/panel?cmdC=cmdC&cmdN=cmdN");

        Assert.Equal(2, plan.Matches.Count);
        Assert.True(plan.Cancel);
    }

    // El gancho síncrono corre para TODOS los que matchean, no sólo para el Primary.
    [Fact]
    public void OnMatchedSync_corre_para_todos_los_matches()
    {
        var noCancelable = new HandlerNoCancelable();
        var d = new UrlCommandDispatcher(new IUrlCommandHandler[] { new HandlerCancelable(), noCancelable });

        d.Plan("/panel?cmdC=cmdC&cmdN=cmdN");

        Assert.Equal(1, noCancelable.VecesSync);   // aunque el Primary es el cancelable
    }

    // Se conserva first-match-wins: ejecuta el primero que matchea en orden de registro.
    //
    // Esta combinación —cancela un handler pero ejecuta otro que no cancelaba— es exactamente la
    // que la aserción de DEBUG de Plan() marca como URL mal formada (Plan 1 §7). Por eso el test
    // sólo corre en Release: en Debug el Debug.Fail dispararía y voltearía el runner. El
    // comportamiento que congela (first-match-wins) es el mismo en las dos configuraciones.
#if DEBUG
    [Fact(Skip = "Dispara el Debug.Fail del invariante de continuación (Plan 1 §7). Se ejecuta en Release.")]
#else
    [Fact]
#endif
    public async Task Ejecuta_el_primer_match_en_orden_de_registro()
    {
        var d = new UrlCommandDispatcher(new IUrlCommandHandler[] { new HandlerNoCancelable(), new HandlerCancelable() });
        var url = "/panel?cmdC=cmdC&cmdN=cmdN";

        var outcome = await d.ExecuteAsync(d.Plan(url), url);

        Assert.False(outcome.CancelNavigation);    // el del HandlerNoCancelable, que va primero
    }
}
