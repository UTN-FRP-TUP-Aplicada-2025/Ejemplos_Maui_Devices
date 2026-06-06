namespace Ejemplo_Maui_Hibrida.UrlCommands;

// Resultado de interpretar una URL: si se cancela la navegación original y,
// opcionalmente, a qué URL re-navegar.
public sealed record BridgeOutcome(bool CancelNavigation, string? NavigateTo = null);
