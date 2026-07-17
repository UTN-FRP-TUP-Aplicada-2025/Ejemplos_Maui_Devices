using MotorDsl.Core.Models;

namespace Ejemplo_MotorDSL_Dialog.Models;

/// <summary>
/// Traduce cada causa que el stack puede reportar a un <see cref="PrintFailure"/> con código
/// estable, mensaje de usuario accionable y el error original preservado.
/// <para>
/// Los códigos son un contrato de cara a soporte: una vez que un usuario reporta un
/// "PRN-HW-PAPER", renombrarlo rompe el historial. Se agregan códigos nuevos; no se renombran.
/// </para>
/// <para>
/// Hay un código por <b>acción distinta del usuario</b>, no por causa técnica distinta. Dos
/// causas que se resuelven con el mismo gesto comparten código.
/// </para>
/// </summary>
public static class PrinterErrorCatalog
{
    // --- Documento (previo a tocar la impresora) ---
    public const string DocNetwork = "PRN-DOC-NET";
    public const string DocContract = "PRN-DOC-CONTRACT";
    public const string DocRender = "PRN-DOC-RENDER";

    // --- Bluetooth / permisos ---
    public const string BtOff = "PRN-BT-OFF";
    public const string BtPermission = "PRN-BT-PERM";

    // --- Dispositivo ---
    public const string DeviceNone = "PRN-DEV-NONE";
    public const string DeviceAbsent = "PRN-DEV-ABSENT";
    public const string ConnFail = "PRN-CONN-FAIL";

    // --- Impresión ---
    public const string HwPaper = "PRN-HW-PAPER";
    public const string HwCover = "PRN-HW-COVER";
    public const string HwOther = "PRN-HW-OTHER";
    public const string LinkLost = "PRN-LINK-LOST";
    public const string Timeout = "PRN-TIMEOUT";
    public const string Unknown = "PRN-UNKNOWN";

    public static PrintFailure DocumentoSinConexion(string technical) => new(
        DocNetwork,
        "Sin conexión",
        "No pudimos obtener el comprobante. Revisá tu conexión y reintentá.",
        technical);

    public static PrintFailure DocumentoInvalido(string technical) => new(
        DocContract,
        "Comprobante inválido",
        "El comprobante recibido no tiene el formato esperado. Avisá a soporte con este código.",
        technical);

    public static PrintFailure RenderFallido(string technical) => new(
        DocRender,
        "No se pudo generar el documento",
        "No pudimos preparar el comprobante para imprimir. Avisá a soporte con este código.",
        technical);

    public static PrintFailure BluetoothApagado(string technical) => new(
        BtOff,
        "Bluetooth desactivado",
        "Activá el Bluetooth para buscar impresoras.",
        technical);

    public static PrintFailure PermisoRevocado(string technical) => new(
        BtPermission,
        "Permiso Bluetooth revocado",
        "La app perdió el permiso de Bluetooth. Habilitalo desde los ajustes de la aplicación.",
        technical);

    public static PrintFailure SinImpresoras() => new(
        DeviceNone,
        "No se encontraron impresoras",
        "Verificá que la impresora esté encendida y emparejada con este dispositivo.",
        "DiscoverDevicesAsync devolvió 0 dispositivos emparejados.");

    /// <summary>
    /// Falla la conexión con la impresora <b>predeterminada</b>. El stack no puede distinguir
    /// entre "está apagada" y "no es la que se emparejó": ambas son el mismo fallo de socket
    /// RFCOMM, porque la MAC guardada simplemente no responde. Por eso el mensaje nombra las
    /// dos causas y deja que decida el usuario, que es quien ve el hardware.
    /// </summary>
    public static PrintFailure PredeterminadaAusente(string etiqueta) => new(
        DeviceAbsent,
        "La impresora no responde",
        $"No pudimos conectar con «{etiqueta}». Puede que esté apagada o fuera de alcance, " +
        "o que la impresora que tenés delante no sea la que está emparejada con este teléfono.",
        $"ConnectAsync devolvió false para la impresora predeterminada ({etiqueta}).");

    /// <summary>Falla la conexión con una impresora que el usuario eligió explícitamente.</summary>
    public static PrintFailure NoSePudoConectar(string etiqueta) => new(
        ConnFail,
        "No se pudo conectar",
        $"No fue posible conectar con «{etiqueta}». Verificá que esté encendida y cerca.",
        $"ConnectAsync devolvió false para {etiqueta}.");

    /// <summary>
    /// Clasifica un fallo de envío reusando <see cref="PrintError.FromException"/>, que es la
    /// misma clasificación que aplica la librería: no se reimplementa acá.
    /// </summary>
    /// <param name="original">
    /// Excepción de origen. <c>ThermalPrinterService</c> envuelve el fallo real en un
    /// <see cref="Exception"/> genérico ("Print failed after N attempt(s): …") pero conserva la
    /// causa en <c>InnerException</c>; es esa causa la que hay que pasar acá.
    /// </param>
    /// <param name="technical">Mensaje original completo, incluida la envoltura, para el log.</param>
    public static PrintFailure Describe(Exception original, string technical)
    {
        var error = PrintError.FromException(original, 1, 1);

        return error.Type switch
        {
            PrintErrorType.Hardware => Hardware(original, technical),

            PrintErrorType.Connection => new PrintFailure(LinkLost, "Se perdió la conexión",
                "Se perdió la conexión con la impresora. Verificá que esté encendida y cerca.",
                technical, original),

            PrintErrorType.Timeout => new PrintFailure(Timeout, "La impresora no responde",
                "La impresora no respondió a tiempo. Reintentá.",
                technical, original),

            // Protocol y Unknown no tienen una acción propia para el usuario: comparten código.
            _ => new PrintFailure(Unknown, "Falló la impresión",
                "No se pudo completar la impresión. Reintentá.",
                technical, original),
        };
    }

    /// <summary>
    /// Distingue las dos causas físicas que la impresora sí reporta por <c>DLE EOT</c>.
    /// </summary>
    private static PrintFailure Hardware(Exception original, string technical)
    {
        // Único punto que depende de los literales de la librería ("paper out" / "cover open",
        // ver BluetoothPrinterTransport.Detection.cs). Si esos textos cambian, degrada a
        // PRN-HW-OTHER: se pierde precisión, no corrección. Lo elimina del todo que la librería
        // exponga la causa tipada en PrinterHardwareException (propuesta T-L1 del plan).
        if (original.Message.Contains("paper", StringComparison.OrdinalIgnoreCase))
            return new PrintFailure(HwPaper, "Sin papel",
                "La impresora se quedó sin papel. Cargá un rollo nuevo y reintentá.",
                technical, original);

        if (original.Message.Contains("cover", StringComparison.OrdinalIgnoreCase))
            return new PrintFailure(HwCover, "Tapa abierta",
                "La tapa de la impresora está abierta. Cerrala y reintentá.",
                technical, original);

        return new PrintFailure(HwOther, "Problema en la impresora",
            "La impresora reportó un problema físico. Revisala y reintentá.",
            technical, original);
    }
}
