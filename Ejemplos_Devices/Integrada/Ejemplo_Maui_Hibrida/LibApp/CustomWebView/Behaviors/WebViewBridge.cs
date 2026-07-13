namespace LibApp.CustomWebView.Behaviors;

// Singleton que sólo dispara los eventos; la behavior los traduce en acciones
// imperativas sobre el WebView. Así el VM/handler quedan desacoplados del control.
public sealed class WebViewBridge : IWebViewBridge
{
    public event EventHandler? ReloadRequested;
    public event EventHandler<string>? ScriptRequested;

    public void Reload() => ReloadRequested?.Invoke(this, EventArgs.Empty);
    public void RunScript(string js) => ScriptRequested?.Invoke(this, js);
}
