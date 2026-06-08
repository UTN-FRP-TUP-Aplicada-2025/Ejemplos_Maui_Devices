using Ejemplo_Maui_Hibrida.Behaviors;
using Ejemplo_Maui_Hibrida.Pages;
using Ejemplo_Maui_Hibrida.Services;

namespace Ejemplo_Maui_Hibrida.UrlCommands.Handlers;

// Interpreta "selfie=selfie&param={id}": toma una foto, la normaliza, la pasa a
// base64 y la inyecta en el DOM llamando al hook window.recibirFoto('{id}', dataUri).
// NO re-navega: se queda en la página.
public sealed class SelfieCommandHandler : IUrlCommandHandler
{
    private readonly IImageService _img;
    private readonly IWebViewBridge _bridge;

    public SelfieCommandHandler(IImageService img, IWebViewBridge bridge)
    {
        _img = img;
        _bridge = bridge;
    }

    public bool CanHandle(string url) =>
        url.Contains("selfie=selfie", StringComparison.OrdinalIgnoreCase);

    public async Task<BridgeOutcome> HandleAsync(string url)
    {
        var targetId = GetQueryValue(url, "param");
        if (string.IsNullOrEmpty(targetId))
            return new BridgeOutcome(true, null);

        // Navegar a la cámara y esperar el path con TaskCompletionSource.
        var tcs = new TaskCompletionSource<string?>();
        Action<string?> callback = p => tcs.TrySetResult(p);

        await Shell.Current.GoToAsync(nameof(MyMediaSelfiePickerPage),
            new ShellNavigationQueryParameters { { "OnPhotoCallback", callback } });

        var tempPath = await tcs.Task;            // null = canceló
        if (string.IsNullOrEmpty(tempPath))
            return new BridgeOutcome(true, null);

        // Normalizar y pasar a base64.
        byte[]? bytes;
        using (var fs = File.OpenRead(tempPath))
            bytes = await _img.ProcesarPhotoAsync(fs);

        try { File.Delete(tempPath); } catch { /* ignorado */ }

        if (bytes is null)
            return new BridgeOutcome(true, null);

        var dataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}";

        // Inyectar vía el bridge: la página resuelve el DOM (img/input).
        //_bridge.RunScript($"window.recibirFoto('{targetId}', '{dataUri}');");
                
        string scriptjs=$@"document.getElementById('{targetId}').src = '{dataUri}';
        document.getElementById('{targetId}').value = '{dataUri}';";

        _bridge.RunScript(scriptjs);

        return new BridgeOutcome(true, null);     // se queda en la página
    }

    private static string? GetQueryValue(string url, string key)
    {
        var q = url.Contains('?') ? url[(url.IndexOf('?') + 1)..] : url;
        foreach (var pair in q.Split('&'))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }
}
