namespace Ejemplo_Maui_DirectCall.Services;

// Resultado tipado de un intento de llamada. Cada caller (botón, servicio,
// timer, etc.) recibe esto y decide si tiene que reaccionar; el overlay
// ya fue actualizado por el CallCoordinator.
public abstract record CallResult
{
    public sealed record Success(string Numero) : CallResult;

    // Permiso denegado pero el usuario puede volver a intentar
    // (Android: ShouldShowRationale == true; iOS: estado .NotDetermined).
    public sealed record PermissionDenied : CallResult;

    // Permiso denegado de forma persistente: hay que abrir ajustes.
    public sealed record PermissionDeniedPermanent : CallResult;

    // Restringido por política del dispositivo (MDM, controles parentales, etc.).
    public sealed record PermissionRestricted : CallResult;

    // El dispositivo no soporta llamadas (tablet, simulador, etc.).
    public sealed record NotSupported : CallResult;

    // El caller canceló (CancellationToken).
    public sealed record Cancelled : CallResult;

    // Cualquier otro error inesperado.
    public sealed record Failure(string Message) : CallResult;
}
