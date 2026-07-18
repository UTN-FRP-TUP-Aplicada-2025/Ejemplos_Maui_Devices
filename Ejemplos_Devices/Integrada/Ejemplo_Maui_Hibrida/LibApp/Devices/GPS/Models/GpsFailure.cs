namespace LibApp.Devices.GPS.Models;

/// <summary>
/// Fallo clasificado del flujo de GPS. <see cref="UserMessage"/> es lo único que se le muestra
/// al usuario; <see cref="TechnicalMessage"/> conserva el detalle de origen para log y reporte.
/// <para>
/// Tiene la misma forma que <c>PrintFailure</c> y <c>CallFailure</c>, deliberadamente. Se evaluó
/// unificarlos en un tipo común de <c>Common/</c> y se descartó: obligaba a reabrir el dominio
/// de impresión —el único ya validado en dispositivo— y a reprobarlo entero, además de
/// desincronizar la app híbrida del PoC `Ejemplo_MotorDSL_Dialog`, que no tiene carpeta
/// `Common`. Lo que armoniza el patrón son los invariantes, no un tipo compartido.
/// </para>
/// </summary>
public sealed record GpsFailure(
    string Code,
    string Title,
    string UserMessage,
    string TechnicalMessage)
{
    /// <summary>
    /// Texto para la capa de error: el mensaje accionable y, debajo, el código, separado y al
    /// final para que el usuario pueda dictárselo a soporte sin que compita con la acción.
    /// </summary>
    public string DisplayMessage => $"{UserMessage}\n\n{Code}";
}
