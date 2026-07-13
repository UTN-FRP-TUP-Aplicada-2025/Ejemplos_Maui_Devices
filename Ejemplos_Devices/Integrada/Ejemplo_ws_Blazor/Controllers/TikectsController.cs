using Ejemplo_ws_Blazor.DTOs.Print;
using Microsoft.AspNetCore.Mvc;

namespace Ejemplo_ws_Blazor.Controllers;

/// <summary>
/// Controlador de prueba que devuelve un <see cref="PrintDocument"/> hardcodeado, representativo de un
/// "comprobante de ticket" municipal. El JSON resultante es el mismo formato que <c>ThermalPrintDevice</c>
/// (GDA.APP) descarga por GET y renderiza a ESC/POS con MotorDsl.
/// Espeja el patrón real de <c>PrintController.PrintActaById</c> (GDA.Core.API), pero sin base de datos.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TikectsController : ControllerBase
{
    private readonly ILogger<TikectsController> _logger;

    public TikectsController(ILogger<TikectsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Devuelve un comprobante de ticket completo (todas las secciones) como <see cref="PrintDocument"/>.
    /// </summary>
    /// <remarks>GET /api/Tikects/comprobante</remarks>
    [HttpGet("comprobante")]
    [ProducesResponseType(typeof(PrintDocument), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<PrintDocument> GetComprobanteTicket()
    {
        try
        {
            var doc = BuildComprobanteTicketHardcoded();
            return Ok(doc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al construir el comprobante de ticket hardcodeado.");
            return Problem(
                detail: "Ocurrió un error al construir el comprobante de ticket.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Construye un <see cref="PrintDocument"/> hardcodeado lo más representativo posible: ejercita
    /// texto normal/negrita/centrado, separadores, contenedores anidados, imagen bitmap (logo) e imagen QR.
    /// Los valores imitan a <c>DTOPrintComprobanteTicket</c> (sys_Incidentes) de GDA.Core.
    /// </summary>
    private static PrintDocument BuildComprobanteTicketHardcoded()
    {
        // Datos "de negocio" simulados (equivalen a lo que PrintTicketService.GetModelo() trae de la BD).
        const string municipioNombre = "MUNICIPALIDAD DE SAN FERNANDO";
        const string endpointFront = "https://desa-inspectoresapp.gobdigital.com.ar/";
        const string idTicket = "48213";

        // Logo placeholder (PNG 1x1 transparente en base64). Reemplazar por el logo real del municipio.
        const string logoBase64 =
            "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42m" +
            "NkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

        // QR hacia el detalle del ticket (mismo patrón que el acta: FRONT + ruta + id encriptado simulado).
        string srcQr = $"{endpointFront}DetalleTicket?idc=fHZswp%2BcW9cN5kytJucNjg%3D%3D";

        var children = new List<PrintNode>
        {
            // ── Encabezado ──
            N.Image(logoBase64, "bitmap", width: 200, align: "center"),
            N.Text(municipioNombre, bold: true, align: "center"),
            N.Text("COMPROBANTE DE TICKET", bold: true, align: "center"),
            N.Text("Sistema de Gestión de Incidentes", align: "center"),
            N.Separator(),

            // ── Datos del ticket ──
            N.Text($"Ticket N°: {idTicket}", bold: true),
            N.Text("Fecha: 13/07/2026 10:42"),
            N.Text("Tipo de ticket: Reclamo"),
            N.Text("Título: Bache sobre calzada"),
            N.Text("Origen: App Ciudadano"),
            N.Text("Usuario: jperez"),
            N.Separator(),

            // ── Ciudadano ──
            N.Text("DATOS DEL CIUDADANO:", bold: true),
            N.Text("DNI: 30.123.456"),
            N.Text("Apellido y Nombre: Pérez Juan Carlos"),
            N.Text("Email: jperez@example.com"),
            N.Text("Celular: 11-5555-1234"),
            N.Separator(),

            // ── Derivación (secretaría / sector / dirección) ──
            N.Text("DERIVACIÓN:", bold: true),
            N.Text("Secretaría: Obras Públicas"),
            N.Text("Dirección: Vialidad"),
            N.Text("Sector: Cuadrilla Norte"),
            N.Separator(),

            // ── Clasificación / estado ──
            N.Text("CLASIFICACIÓN:", bold: true),
            N.Text("Tipo de incidente: Infraestructura vial"),
            N.Text("Motivo: Bache / rotura de pavimento"),
            N.Text("Estado: En proceso"),
            N.Text("Avance: 60%"),
            N.Text("Fecha inicio: 13/07/2026"),
            N.Separator(),

            // ── Ubicación ──
            N.Text("UBICACIÓN DE LA INCIDENCIA:", bold: true),
            N.Text("Lugar: Av. del Libertador 1250, entrecalles: Belgrano y Sarmiento"),
            N.Separator(),

            // ── Comercio asociado (sección opcional, contenedor anidado) ──
            N.Text("COMERCIO ASOCIADO:", bold: true),
            N.Container(
                N.Text("Razón social: Kiosco El Rápido S.R.L."),
                N.Text("CUIT: 30-71234567-8"),
                N.Text("Titular: González Marta"),
                N.Text("Rubro: Comercio minorista"),
                N.Text("Dirección: Av. del Libertador 1248")
            ),
            N.Separator(),

            // ── Inmueble asociado (sección opcional, contenedor anidado) ──
            N.Text("INMUEBLE ASOCIADO:", bold: true),
            N.Container(
                N.Text("Código: INM-00987"),
                N.Text("Dirección: Av. del Libertador 1250"),
                N.Text("Datos catastrales: Circ. II - Secc. C - Manz. 45 - Parc. 12")
            ),
            N.Separator(),

            // ── Observaciones ──
            N.Text("OBSERVACIONES:", bold: true),
            N.Text("El bache abarca ambos carriles y representa riesgo para motocicletas."),
            N.Separator(),

            // ── Cierre (sección opcional) ──
            N.Text("CIERRE:", bold: true),
            N.Text("Fecha fin: —"),
            N.Text("Motivo de cierre: —"),
            N.Text("Observación de cierre: Sin datos."),
            N.Text("Usuario de cierre: Sin datos."),
            N.Separator(),

            // ── QR + pie ──
            N.Text("Escaneá para ver el detalle del ticket:", align: "center"),
            N.QrCode(srcQr),
            N.Empty(),
            N.Text("Comprobante generado electrónicamente.", align: "center"),
            N.Text("13.07.2026_10_42_31", align: "center"),
            N.Separator(),
        };

        return new PrintDocument
        {
            Id = $"comprobante-ticket-{idTicket}",
            Root = N.Container(children.ToArray())
        };
    }
}
