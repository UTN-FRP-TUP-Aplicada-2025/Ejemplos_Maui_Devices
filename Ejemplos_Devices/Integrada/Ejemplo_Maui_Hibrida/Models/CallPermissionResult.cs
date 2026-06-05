namespace Ejemplo_Maui_Hibrida.Models;

// Resultado normalizado del permiso CALL_PHONE, espejo de LocationPermissionResult.
// Sólo relevante en Android + CallMode.Direct (el dialer no pide permiso runtime).
public enum CallPermissionResult
{
    Granted,         // Permiso concedido
    DeniedCanRetry,  // Android: denegado pero se puede volver a pedir (rationale)
    Denied,          // Android "no volver a preguntar": solo ajustes
    Restricted       // Política del dispositivo (MDM, control parental)
}
