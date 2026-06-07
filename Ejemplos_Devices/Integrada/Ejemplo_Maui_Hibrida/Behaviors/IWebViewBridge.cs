namespace Ejemplo_Maui_Hibrida.Behaviors;

// Única abstracción para las acciones imperativas sobre el WebView (recargar y
// ejecutar JS). El VM/handler piden la acción; sólo la behavior toca el control.
public interface IWebViewBridge
{
    event EventHandler? ReloadRequested;
    event EventHandler<string>? ScriptRequested;
    void Reload();
    void RunScript(string js);
}
