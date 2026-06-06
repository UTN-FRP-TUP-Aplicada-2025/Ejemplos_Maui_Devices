using Ejemplo_Maui_Hibrida.ViewModels;

namespace Ejemplo_Maui_Hibrida.Behaviors;

// Puente declarativo: cuando el IReloadRequester pide recargar, ejecuta Reload()
// sobre el WebView. El VM no conoce el control.
public class WebViewReloadBehavior : Behavior<WebView>
{
    public static readonly BindableProperty RequesterProperty =
        BindableProperty.Create(nameof(Requester), typeof(IReloadRequester),
            typeof(WebViewReloadBehavior), propertyChanged: OnRequesterChanged);

    public IReloadRequester? Requester
    {
        get => (IReloadRequester?)GetValue(RequesterProperty);
        set => SetValue(RequesterProperty, value);
    }

    private WebView? _webView;

    protected override void OnAttachedTo(WebView bindable)
    {
        base.OnAttachedTo(bindable);
        _webView = bindable;
        // IMPORTANTE: la behavior no está en el árbol visual, su BindingContext no
        // se hereda solo. Hay que propagárselo desde el control o el Binding de
        // Requester queda en null SIN error visible.
        bindable.BindingContextChanged += OnControlBindingContextChanged;
        BindingContext = bindable.BindingContext;
    }

    protected override void OnDetachingFrom(WebView bindable)
    {
        bindable.BindingContextChanged -= OnControlBindingContextChanged;
        Unsubscribe(Requester);
        _webView = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnControlBindingContextChanged(object? sender, EventArgs e)
        => BindingContext = _webView?.BindingContext;

    private static void OnRequesterChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (WebViewReloadBehavior)bindable;
        self.Unsubscribe(oldValue as IReloadRequester);
        self.Subscribe(newValue as IReloadRequester);
    }

    private void Subscribe(IReloadRequester? r)   { if (r is not null) r.ReloadRequested += OnReloadRequested; }
    private void Unsubscribe(IReloadRequester? r) { if (r is not null) r.ReloadRequested -= OnReloadRequested; }
    private void OnReloadRequested(object? sender, EventArgs e) => _webView?.Reload();
}
