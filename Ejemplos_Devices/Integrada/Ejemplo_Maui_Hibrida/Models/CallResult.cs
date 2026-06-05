namespace Ejemplo_Maui_Hibrida.Models;

// Resultado tipado de una operación de llamada (espejo de GpsResult).
// El ViewModel hace switch sobre estos casos y evita try/catch.
public abstract record CallResult
{
    public sealed record Success(string Numero, CallMode Mode) : CallResult;
    public sealed record PermissionDenied(bool CanRetry) : CallResult;
    public sealed record PermissionRestricted : CallResult;
    public sealed record NotSupported : CallResult;
    public sealed record InvalidNumber : CallResult;
    public sealed record Cancelled : CallResult;
    public sealed record Failure(string Message) : CallResult;
}
