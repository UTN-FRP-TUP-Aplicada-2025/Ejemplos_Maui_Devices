namespace Ejemplo_Maui_Hibrida.Models;

// Modo de llamada. Determina la mecánica de la llamada y el permiso requerido.
public enum CallMode
{
    // Llamada directa: Android Intent.ActionCall. Requiere permiso runtime
    // CALL_PHONE y marca el número sin confirmación del usuario.
    // No existe en iOS/MacCatalyst.
    Direct,

    // Marcador del SO: abre el dialer precargado con el número; el usuario
    // confirma. No requiere permiso runtime. Único modo en iOS/MacCatalyst.
    Dialer
}
