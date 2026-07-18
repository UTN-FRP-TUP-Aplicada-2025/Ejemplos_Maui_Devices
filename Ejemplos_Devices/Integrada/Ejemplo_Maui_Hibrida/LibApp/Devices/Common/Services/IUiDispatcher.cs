namespace LibApp.Devices.Common.Services;

/// <summary>
/// Marshalling al hilo de UI. Existe porque <c>MainThread</c> es un estático de plataforma:
/// un ViewModel que lo invoque directo no se puede ejercitar fuera de un dispositivo, aunque
/// no tenga una sola directiva <c>#if</c>.
/// <para>
/// Lo necesita sólo el overlay de Red, que es el único reactivo: los demás se disparan desde
/// el hilo de UI y no tienen nada que marshalar.
/// </para>
/// </summary>
public interface IUiDispatcher
{
    void BeginInvoke(Action action);
}

/// <summary>Implementación real: delega en <c>MainThread</c> de MAUI.</summary>
public sealed class MainThreadDispatcher : IUiDispatcher
{
    public void BeginInvoke(Action action) => MainThread.BeginInvokeOnMainThread(action);
}
