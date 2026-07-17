namespace Ejemplo_MotorDSL_Dialog.Models;

// Resultado tipado de una operación de impresión (espejo de CallResult).
// El fallo lleva un PrintFailure clasificado, no un string: la UI muestra el mensaje de
// usuario y el código, y el mensaje original queda accesible para log y reporte.
public abstract record PrintResult
{
    public sealed record Success : PrintResult;
    public sealed record Failure(PrintFailure Error) : PrintResult;
}
