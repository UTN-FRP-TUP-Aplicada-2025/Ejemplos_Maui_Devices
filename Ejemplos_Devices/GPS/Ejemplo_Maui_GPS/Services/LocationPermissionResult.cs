namespace Ejemplo_Maui_GPS.Services;

// Resultado normalizado para los 4 escenarios de permisos de ubicación
// (Android + iOS), independiente de la API de Permissions de MAUI.
public enum LocationPermissionResult
{
    Granted,         // Permiso concedido
    DeniedCanRetry,  // Android: denegado pero se puede volver a pedir (rationale)
    Denied,          // Android "no volver a preguntar" / iOS denegado: solo ajustes
    Restricted       // iOS: política del dispositivo (MDM, control parental)
}
