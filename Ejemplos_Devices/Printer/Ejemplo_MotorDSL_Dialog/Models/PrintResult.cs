namespace Ejemplo_MotorDSL_Dialog.Models;

// Resultado tipado de una operación de impresión (espejo de CallResult).
public abstract record PrintResult
{
    public sealed record Success : PrintResult;
    public sealed record Failure(string Message) : PrintResult;
}
