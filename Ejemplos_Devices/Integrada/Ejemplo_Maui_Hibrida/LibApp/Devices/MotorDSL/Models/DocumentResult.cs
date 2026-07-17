namespace LibApp.Devices.MotorDSL.Models;

// Resultado tipado de la obtención del comprobante desde el backend.
//
// Antes ObtenerDocumentoAsync devolvía string vacío ante CUALQUIER fallo (timeout, caída de
// red, backend no disponible, contrato inválido), y todos terminaban en el mismo render
// fallido. Son problemas distintos para el usuario: NetworkError se reintenta, InvalidContract
// se reporta a soporte. Mantiene el patrón de DiscoverResult/PrintResult.
public abstract record DocumentResult
{
    public sealed record Ok(string Json) : DocumentResult;
    public sealed record NetworkError(string TechnicalMessage) : DocumentResult;
    public sealed record InvalidContract(string TechnicalMessage) : DocumentResult;
}
