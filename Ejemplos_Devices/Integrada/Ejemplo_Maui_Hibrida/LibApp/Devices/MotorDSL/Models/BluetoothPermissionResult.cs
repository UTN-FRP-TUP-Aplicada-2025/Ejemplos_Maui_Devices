namespace LibApp.Devices.MotorDSL.Models;

// Resultado normalizado del permiso Bluetooth, espejo de CallPermissionResult.
// Sólo relevante en Android (otras plataformas no usan este flujo).
public enum BluetoothPermissionResult
{
    Granted,         // Permiso concedido
    DeniedCanRetry,  // Android: denegado pero se puede volver a pedir (rationale)
    Denied,          // Android "no volver a preguntar": solo ajustes
    Restricted       // Política del dispositivo (MDM, control parental)
}
