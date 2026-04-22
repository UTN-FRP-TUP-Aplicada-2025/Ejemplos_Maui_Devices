namespace Ejemplo_Maui_GPS.Services;

// Resultado tipado de una operación GPS.
// El ViewModel hace switch sobre estos casos y evita try/catch.
public abstract record GpsResult
{
    public sealed record Success(Location Location) : GpsResult;
    public sealed record PermissionDenied(bool CanRetry) : GpsResult;
    public sealed record PermissionRestricted : GpsResult;
    public sealed record GpsDisabled : GpsResult;
    public sealed record NotSupported : GpsResult;
    public sealed record NoSignal : GpsResult;
    public sealed record Cancelled : GpsResult;
    public sealed record Failure(string Message) : GpsResult;
}
