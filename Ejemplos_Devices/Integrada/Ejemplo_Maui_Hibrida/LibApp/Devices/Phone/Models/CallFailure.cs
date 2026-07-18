namespace LibApp.Devices.Phone.Models;

/// <summary>
/// Fallo clasificado del flujo de llamada. <see cref="UserMessage"/> es lo único que se le
/// muestra al usuario; <see cref="TechnicalMessage"/> conserva el original para log y reporte.
/// Misma forma que <c>PrintFailure</c> y <c>GpsFailure</c>; ver el porqué de la duplicación
/// deliberada en <see cref="GPS.Models.GpsFailure"/>.
/// </summary>
public sealed record CallFailure(
    string Code,
    string Title,
    string UserMessage,
    string TechnicalMessage)
{
    public string DisplayMessage => $"{UserMessage}\n\n{Code}";
}

/// <summary>
/// Traduce cada variante de <see cref="CallResult"/> a un fallo con código estable.
/// <para>Los códigos son un contrato con soporte: se agregan, no se renombran.</para>
/// </summary>
public static class CallErrorCatalog
{
    public const string PermisoNecesario = "TEL-PERM-ASK";
    public const string PermisoDenegado = "TEL-PERM-DENIED";
    public const string PermisoRestringido = "TEL-PERM-MDM";
    public const string NoSoportado = "TEL-UNSUPPORTED";
    public const string NumeroInvalido = "TEL-BAD-NUMBER";
    public const string Cancelado = "TEL-CANCELLED";
    public const string SinActividad = "TEL-NO-ACTIVITY";
    public const string Desconocido = "TEL-UNKNOWN";

    public static CallFailure PermisoReintentable() => new(
        PermisoNecesario,
        "Permiso de llamadas necesario",
        "Para llamar directamente necesitamos permiso para realizar llamadas. Podés intentar concederlo.",
        "CallPermissionResult.DeniedCanRetry (ShouldShowRationale == true)");

    public static CallFailure PermisoDefinitivo() => new(
        PermisoDenegado,
        "Acceso a llamadas denegado",
        "Para llamar directamente necesitamos permiso para realizar llamadas. Habilitalo desde los ajustes de la aplicación.",
        "CallPermissionResult.Denied (no volver a preguntar)");

    public static CallFailure Restringido() => new(
        PermisoRestringido,
        "Acceso restringido",
        "Las llamadas están restringidas por una política del dispositivo. Consultá con el administrador.",
        "PermissionStatus.Restricted");

    public static CallFailure NoDisponible() => new(
        NoSoportado,
        "Llamadas no disponibles",
        "Este dispositivo no puede realizar llamadas telefónicas.",
        "PhoneDialer.IsSupported == false o FeatureNotSupportedException");

    public static CallFailure NumeroNoValido() => new(
        NumeroInvalido,
        "Número inválido",
        "El número de teléfono está vacío o no es válido.",
        "string.IsNullOrWhiteSpace(numero)");

    public static CallFailure CanceladoPorUsuario() => new(
        Cancelado,
        "Llamada cancelada",
        "Se canceló el inicio de la llamada.",
        "CancellationToken cancelado por el llamador");

    /// <summary>
    /// Clasifica un <c>CallResult.Failure</c>, cuyo mensaje es el de una excepción de Android.
    /// Antes ese texto iba crudo a la pantalla: en inglés y sin acción posible para el usuario.
    /// </summary>
    public static CallFailure Describir(string technical)
    {
        // Único punto que depende del literal del servicio (CallService: "No hay una actividad
        // activa para iniciar la llamada."). Si cambia, degrada a TEL-UNKNOWN: se pierde
        // precisión, no corrección.
        if (technical.Contains("actividad activa", StringComparison.OrdinalIgnoreCase))
            return new CallFailure(SinActividad,
                "No se pudo iniciar la llamada",
                "La app no está en primer plano. Abrila y reintentá.",
                technical);

        return new CallFailure(Desconocido,
            "No se pudo realizar la llamada",
            "Ocurrió un problema al iniciar la llamada. Reintentá.",
            technical);
    }

    /// <summary>Mapea la variante a su fallo. Toda variante no-<c>Success</c> tiene el suyo.</summary>
    public static CallFailure Describir(CallResult result) => result switch
    {
        CallResult.PermissionDenied d => d.CanRetry ? PermisoReintentable() : PermisoDefinitivo(),
        CallResult.PermissionRestricted => Restringido(),
        CallResult.NotSupported => NoDisponible(),
        CallResult.InvalidNumber => NumeroNoValido(),
        CallResult.Cancelled => CanceladoPorUsuario(),
        CallResult.Failure f => Describir(f.Message),
        _ => Describir($"Variante no mapeada: {result.GetType().Name}"),
    };
}
