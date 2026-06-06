namespace Ejemplo_Maui_Hibrida.UrlCommands;

// Contrato de un comando interpretable desde la URL del WebView.
// Agregar un comando nuevo = una clase que implemente esto + una línea de DI.
public interface IUrlCommandHandler
{
    bool CanHandle(string url);
    Task<BridgeOutcome> HandleAsync(string url);
}
