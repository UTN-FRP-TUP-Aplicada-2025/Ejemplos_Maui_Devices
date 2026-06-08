namespace Ejemplo_Maui_Hibrida.Models;

// Resultado tipado de un relay REST genérico (sendAPI).
// El handler hace switch sobre estos casos y evita try/catch propagado.
public abstract record ApiCallResult
{
    /// <summary>La petición se completó con un código &lt; 400.</summary>
    public sealed record Success(int Status, string? Body) : ApiCallResult;

    /// <summary>El servidor respondió con un código de error (&gt;= 400).</summary>
    public sealed record HttpError(int Status, string? Body) : ApiCallResult;

    /// <summary>Falló la red al ejecutar la petición (DNS, socket, etc.).</summary>
    public sealed record NetworkError(string Message) : ApiCallResult;

    /// <summary>La petición superó el tiempo de espera.</summary>
    public sealed record Timeout : ApiCallResult;

    /// <summary>El llamador canceló la petición.</summary>
    public sealed record Cancelled : ApiCallResult;

    /// <summary>El host del destino no está permitido (o la URL es inválida): no se hace el request.</summary>
    public sealed record Blocked : ApiCallResult;
}
