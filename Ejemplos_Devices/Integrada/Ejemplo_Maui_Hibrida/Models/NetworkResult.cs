namespace Ejemplo_Maui_Hibrida.Models;

// Resultado tipado de una sonda de red.
// El ViewModel hace switch sobre estos casos y evita try/catch propagado.
public abstract record NetworkResult
{
    /// <summary>Hay internet real (el marcador de la sonda coincidió).</summary>
    public sealed record Online : NetworkResult;

    /// <summary>Sin conexión, o conexión sin internet real (portal cautivo / sin crédito).</summary>
    public sealed record Offline : NetworkResult;

    /// <summary>La sonda superó el tiempo de espera.</summary>
    public sealed record Timeout(string Url) : NetworkResult;

    /// <summary>No se pudo resolver el host (fallo de DNS).</summary>
    public sealed record DnsFailure(string Host) : NetworkResult;

    /// <summary>El servidor respondió con un código de error (>= 400).</summary>
    public sealed record HttpFailure(int StatusCode, string Url) : NetworkResult;

    /// <summary>Otro error de red al ejecutar la petición.</summary>
    public sealed record RequestFailure(string Message) : NetworkResult;
}
