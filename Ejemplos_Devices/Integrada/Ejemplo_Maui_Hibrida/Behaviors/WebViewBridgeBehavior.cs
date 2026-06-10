namespace Ejemplo_Maui_Hibrida.Behaviors;

// Puente declarativo: traduce los pedidos del IWebViewBridge en acciones imperativas
// sobre el WebView. El VM no conoce el control. Reemplaza a WebViewReloadBehavior.
public class WebViewBridgeBehavior : Behavior<WebView>
{
    public static readonly BindableProperty BridgeProperty =
        BindableProperty.Create(nameof(Bridge), typeof(IWebViewBridge),
            typeof(WebViewBridgeBehavior), propertyChanged: OnBridgeChanged);

    public IWebViewBridge? Bridge
    {
        get => (IWebViewBridge?)GetValue(BridgeProperty);
        set => SetValue(BridgeProperty, value);
    }

    private WebView? _webView;

    protected override void OnAttachedTo(WebView bindable)
    {
        base.OnAttachedTo(bindable);
        _webView = bindable;
        // IMPORTANTE: la behavior no está en el árbol visual, su BindingContext no
        // se hereda solo. Hay que propagárselo desde el control o el Binding de
        // Bridge queda en null SIN error visible.
        bindable.BindingContextChanged += OnControlBindingContextChanged;
        BindingContext = bindable.BindingContext;
    }

    protected override void OnDetachingFrom(WebView bindable)
    {
        bindable.BindingContextChanged -= OnControlBindingContextChanged;
        Unsubscribe(Bridge);
        _webView = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnControlBindingContextChanged(object? sender, EventArgs e)
        => BindingContext = _webView?.BindingContext;

    private static void OnBridgeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (WebViewBridgeBehavior)bindable;
        self.Unsubscribe(oldValue as IWebViewBridge);
        self.Subscribe(newValue as IWebViewBridge);
    }

    private void Subscribe(IWebViewBridge? b)
    {
        if (b is null) return;
        b.ReloadRequested += OnReloadRequested;
        b.ScriptRequested += OnScriptRequested;
    }

    private void Unsubscribe(IWebViewBridge? b)
    {
        if (b is null) return;
        b.ReloadRequested -= OnReloadRequested;
        b.ScriptRequested -= OnScriptRequested;
    }

    // SIEMPRE en UI thread, fire-and-forget.
    private void OnReloadRequested(object? sender, EventArgs e)  => MainThread.BeginInvokeOnMainThread(() => _webView?.Reload());

    private void OnScriptRequested(object? sender, string js) => MainThread.BeginInvokeOnMainThread(() => _ = _webView?.EvaluateJavaScriptAsync(js));
}
