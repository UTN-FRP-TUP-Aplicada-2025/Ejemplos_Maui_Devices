namespace LibApp.UrlCommands;

// Contrato de un comando interpretable desde la URL del WebView.
// Agregar un comando nuevo = una clase que implemente esto + una línea de DI.
//
// Los tres miembros nuevos son DEFAULT INTERFACE MEMBERS: los handlers existentes no requieren
// ninguna edición y conservan su comportamiento actual (cancelar siempre, sin entrega declarada).
public interface IUrlCommandHandler
{
    bool CanHandle(string url);

    /// <summary>
    /// ¿Esta URL-comando cancela la navegación del WebView? Ver Plan 1 §4 (eje A).
    ///
    /// Default true = comportamiento anterior: MainViewModel cancelaba ante CUALQUIER URL
    /// reconocida por el dispatcher. Los 7 handlers actuales son cancelables, así que el
    /// default los deja intactos.
    ///
    /// Si alguna vez hace falta decidirlo por URL (como el eje B), esto pasa a
    /// bool CancelsNavigation(string url). Hoy ningún handler lo necesita.
    /// </summary>
    bool CancelsNavigation => true;

    /// <summary>
    /// Cómo devuelve el resultado esta invocación concreta. Ver Plan 1 §4 (eje B).
    /// Default None: no declara entrega, y por lo tanto no se le aplica la aserción de
    /// continuación del dispatcher.
    /// </summary>
    CommandDelivery DeliveryFor(string url) => CommandDelivery.None;

    /// <summary>
    /// Gancho SÍNCRONO, ejecutado durante la clasificación de la URL, en el mismo pase en que
    /// se decide e.Cancel y ANTES de cualquier await.
    ///
    /// Existe para el trabajo que no puede perder una carrera contra la navegación en curso.
    /// Ningún handler de dispositivo lo necesita (default no-op).
    ///
    /// Contrato: barato, sin I/O bloqueante de red, sin excepciones. Corre para TODOS los
    /// handlers que matchean, no sólo para el que después se ejecuta.
    /// </summary>
    void OnMatchedSync(string url) { }

    Task<BridgeOutcome> HandleAsync(string url);
}
