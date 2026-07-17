namespace Ejemplo_MotorDSL_Dialog.Models;

/// <summary>
/// Fallo clasificado del flujo de impresión. <see cref="UserMessage"/> es lo único que se le
/// muestra al usuario; <see cref="TechnicalMessage"/> y <see cref="Exception"/> conservan el
/// error original íntegro para log y reporte automático, y nunca se muestran crudos.
/// </summary>
/// <param name="Code">Código estable del catálogo (ver <see cref="PrinterErrorCatalog"/>).</param>
/// <param name="Title">Título de la capa de error.</param>
/// <param name="UserMessage">Mensaje accionable, en español y sin jerga técnica.</param>
/// <param name="TechnicalMessage">Mensaje original, tal como lo produjo la librería o el stack.</param>
/// <param name="Exception">Excepción de origen, cuando el fallo vino de una.</param>
public sealed record PrintFailure(
    string Code,
    string Title,
    string UserMessage,
    string TechnicalMessage,
    Exception? Exception = null)
{
    /// <summary>
    /// Texto para la capa de error: el mensaje accionable y, debajo, el código. El código va
    /// separado y al final para que el usuario pueda dictárselo a soporte sin que compita con
    /// la acción que tiene que ejecutar.
    /// </summary>
    public string DisplayMessage => $"{UserMessage}\n\n{Code}";
}
