namespace LibApp.Devices.GPS.Models;

/// <summary>
/// Traduce cada variante de <see cref="GpsResult"/> a un fallo con código estable, mensaje
/// accionable y el original preservado. Espejo de <c>PrinterErrorCatalog</c>.
/// <para>
/// Los códigos son un contrato de cara a soporte: una vez que un usuario reporta un
/// «GPS-OFF», renombrarlo rompe el historial. Se agregan códigos nuevos; no se renombran.
/// </para>
/// <para>
/// Hay un código por <b>acción distinta del usuario</b>, no por causa técnica distinta.
/// </para>
/// </summary>
public static class GpsErrorCatalog
{
    public const string PermisoNecesario = "GPS-PERM-ASK";
    public const string PermisoDenegado = "GPS-PERM-DENIED";
    public const string PermisoRestringido = "GPS-PERM-MDM";
    public const string Apagado = "GPS-OFF";
    public const string NoSoportado = "GPS-UNSUPPORTED";
    public const string SinSenal = "GPS-NO-SIGNAL";
    public const string Cancelado = "GPS-CANCELLED";
    public const string Desconocido = "GPS-UNKNOWN";

    public static GpsFailure PermisoReintentable() => new(
        PermisoNecesario,
        "Permiso de ubicación necesario",
        "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Podés intentar conceder el permiso.",
        "LocationPermissionResult.DeniedCanRetry (ShouldShowRationale == true)");

    public static GpsFailure PermisoDefinitivo() => new(
        PermisoDenegado,
        "Acceso a la ubicación denegado",
        "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Habilitalo desde los ajustes de la aplicación.",
        "LocationPermissionResult.Denied (no volver a preguntar, o plataforma no-Android)");

    public static GpsFailure Restringido() => new(
        PermisoRestringido,
        "Acceso restringido",
        "El acceso a la ubicación está restringido por una política del dispositivo. Consultá con el administrador.",
        "PermissionStatus.Restricted");

    /// <summary>
    /// El GPS está apagado. El permiso lo concede la app, pero el GPS se enciende desde el
    /// sistema: mandar al usuario a los ajustes de la app acá lo dejaría sin salida.
    /// </summary>
    public static GpsFailure GpsApagado() => new(
        Apagado,
        "GPS desactivado",
        "El GPS está desactivado. Activalo desde los ajustes de ubicación para obtener tu posición.",
        "FeatureNotEnabledException");

    public static GpsFailure NoDisponible() => new(
        NoSoportado,
        "GPS no disponible",
        "Este dispositivo no tiene GPS o no puede usarlo.",
        "FeatureNotSupportedException");

    /// <summary>
    /// Sin señal. El mensaje original —«No se pudo obtener ubicación (GPS sin señal)»— describía
    /// la causa técnica; éste describe la acción, que es lo único que el usuario puede hacer.
    /// </summary>
    public static GpsFailure SinSenalGps() => new(
        SinSenal,
        "Sin señal GPS",
        "No pudimos obtener tu ubicación. Salí a un lugar despejado y reintentá.",
        "GetLastKnownLocationAsync y GetLocationAsync devolvieron null");

    public static GpsFailure CanceladoPorUsuario() => new(
        Cancelado,
        "Búsqueda cancelada",
        "Se canceló la búsqueda de tu ubicación.",
        "OperationCanceledException");

    public static GpsFailure Desconocida(string technical) => new(
        Desconocido,
        "No se pudo obtener la ubicación",
        "Ocurrió un problema al leer el GPS. Reintentá.",
        technical);

    /// <summary>Mapea la variante a su fallo. Toda variante no-<c>Success</c> tiene el suyo.</summary>
    public static GpsFailure Describir(GpsResult result) => result switch
    {
        GpsResult.PermissionDenied d => d.CanRetry ? PermisoReintentable() : PermisoDefinitivo(),
        GpsResult.PermissionRestricted => Restringido(),
        GpsResult.GpsDisabled => GpsApagado(),
        GpsResult.NotSupported => NoDisponible(),
        GpsResult.NoSignal => SinSenalGps(),
        GpsResult.Cancelled => CanceladoPorUsuario(),
        GpsResult.Failure f => Desconocida(f.Message),
        _ => Desconocida($"Variante no mapeada: {result.GetType().Name}"),
    };
}
