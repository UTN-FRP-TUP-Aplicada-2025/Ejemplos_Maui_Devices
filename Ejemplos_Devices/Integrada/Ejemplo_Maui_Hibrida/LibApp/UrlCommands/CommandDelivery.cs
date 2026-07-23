namespace LibApp.UrlCommands;

// Forma en que un comando le devuelve su resultado a la web. Ver Plan 1 §4 (eje B).
//
// No es una propiedad del handler sino del COMANDO CONCRETO: el mismo handler puede operar en
// dos modos según la URL. GpsCommandHandler es el caso: con "param" inyecta, sin "param" sustituye.
// Por eso se consulta con DeliveryFor(url), no con una propiedad sin argumentos.
public enum CommandDelivery
{
    /// <summary>
    /// No hay resultado que devolverle a la web: la respuesta al usuario es la UI nativa
    /// (overlay). Casos actuales: CallCommandHandler, PrintCommandHandler.
    /// </summary>
    None,

    /// <summary>
    /// El resultado se inyecta en el DOM de la página viva vía IWebViewBridge.RunScript.
    /// Requiere que la navegación se haya cancelado: si la página recarga, el elemento
    /// destino desaparece. Casos actuales: foto, selfie, QR, sendAPI, GPS con "param".
    /// </summary>
    Injection,

    /// <summary>
    /// El resultado se devuelve RE-NAVEGANDO la misma URL con el query param de comando
    /// sustituido por query params de valor. Patrón de APPGDA
    /// (GDA.APP/APPGDA/Services/NotionsLocationsService.cs:139-150).
    ///
    /// INVARIANTE: un comando que declara Substitution DEBE devolver BridgeOutcome.NavigateTo
    /// no nulo, pase lo que pase con el dispositivo. Si no, la navegación queda muerta: se
    /// canceló y no se re-navegó. Ver Plan 1 §3 y la aserción de UrlCommandDispatcher.
    /// </summary>
    Substitution,
}
