namespace LibApp.UrlCommands;

// Resultado de CLASIFICAR una URL, calculado una sola vez y de forma síncrona.
//
// POR QUÉ EXISTE: hasta ahora CanHandle se evaluaba dos veces por navegación —una en IsCommand
// y otra en DispatchAsync— y era inocuo porque los 7 handlers eran funciones puras de la URL.
// Con handlers que consultan o mutan estado (p. ej. un token persistido) deja de serlo: las dos
// lecturas darían distinto y el comportamiento dependería del orden.
//
// El plan también es lo que permite que la decisión de cancelar sea 100% síncrona.
public sealed record UrlPlan(IReadOnlyList<IUrlCommandHandler> Matches, bool Cancel)
{
    public bool HasMatches => Matches.Count > 0;

    /// <summary>
    /// El handler que se va a ejecutar. Se mantiene la política first-match-wins del dispatcher
    /// anterior (FirstOrDefault): el orden de registro en MauiProgram sigue siendo el orden de
    /// precedencia.
    /// </summary>
    public IUrlCommandHandler? Primary => Matches.Count > 0 ? Matches[0] : null;

    public static readonly UrlPlan Ninguno = new(Array.Empty<IUrlCommandHandler>(), false);
}
