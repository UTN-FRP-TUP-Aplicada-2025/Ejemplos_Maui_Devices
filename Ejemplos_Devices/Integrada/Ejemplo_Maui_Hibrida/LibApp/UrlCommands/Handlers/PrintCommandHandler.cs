using System.Diagnostics;
using System.Text.Json;

using LibApp.Devices.MotorDSL.DTOs.Print;
using LibApp.Devices.MotorDSL.ViewModels;
using LibApp.UrlCommands;
using MotorDsl.Core.Contracts;
using MotorDsl.Core.Models;

namespace Ejemplo_Maui_Hibrida.LibApp.UrlCommands.Handlers;

public class PrintCommandHandler : IUrlCommandHandler
{
    // Endpoint que devuelve el documento imprimible (GET → PrintDocument). Es el mismo formato
    // (JSON del árbol PrintDocument/PrintNode) que consume MotorDsl para generar el ESC/POS.
    private const string EndpointComprobante = "https://aplicada.somee.com/api/Tikects/comprobante";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    private static readonly JsonSerializerOptions JsonReadOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly PrinterOverlayViewModel _printer;
    private readonly IDocumentEngine _engine;
    private readonly HttpClient _http = new();

    public PrintCommandHandler(IDocumentEngine engine, PrinterOverlayViewModel printerOverlay)
    {
        _engine = engine;
        _printer = printerOverlay;
    }

    public bool CanHandle(string url) => url.Contains("action=print", StringComparison.OrdinalIgnoreCase);

    public async Task<BridgeOutcome> HandleAsync(string url)
    {
        // 1. Render SIEMPRE primero, antes de tocar la impresora.
        var profile = new DeviceProfile("58HB6", 32, "escpos-bitmap");
        profile.SetCapability("supports_bitmap", true);
        profile.SetCapability("bitmap_max_width_px", 320);
        profile.SetCapability("bitmap_binarization_threshold", 128);

        // 1a. Traer el PrintDocument desde la API (GET) y obtener el JSON que consume el engine.
        //     endpoint: https://aplicada.somee.com/api/Tikects/comprobante
        string document = await ObtenerDocumentoAsync();

        //"{\"id\":\"comprobante-ticket-48213\",\"version\":\"1.0\",\"format\":\"integrated\",\"root\":{\"type\":\"container\",\"layout\":\"vertical\",\"children\":[{\"type\":\"image\",\"source\":\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==\",\"imageType\":\"bitmap\",\"width\":200,\"style\":{\"align\":\"center\"}},{\"type\":\"text\",\"value\":\"MUNICIPALIDAD DE SAN FERNANDO\",\"style\":{\"align\":\"center\",\"bold\":true}},{\"type\":\"text\",\"value\":\"COMPROBANTE DE TICKET\",\"style\":{\"align\":\"center\",\"bold\":true}},{\"type\":\"text\",\"value\":\"Sistema de Gestión de Incidentes\",\"style\":{\"align\":\"center\"}},{\"type\":\"text\",\"value\":\"================================\"},{\"type\":\"text\",\"value\":\"Ticket N°: 48213\",\"style\":{\"bold\":true}},{\"type\":\"text\",\"value\":\"Fecha: 13/07/2026 10:42\"},{\"type\":\"text\",\"value\":\"Tipo de ticket: Reclamo\"},{\"type\":\"text\",\"value\":\"Título: Bache sobre calzada\"},{\"type\":\"text\",\"value\":\"Origen: App Ciudadano\"},{\"type\":\"text\",\"value\":\"Usuario: jperez\"},{\"type\":\"text\",\"value\":\"===========================..."
        var render = _engine.Render(document, profile);

        //"BitmapEscPos rendering failed: SkiaSharp no pudo decodificar la imagen desde base64."
        if (render.Errors.Count > 0)
        {
            // como atraves del overlay se puede mostrar el error, no es necesario cancelar la navegación ni redirigir a otra URL.?
            return new BridgeOutcome(true, null);
        }

        // 2. El overlay maneja permisos, descubrimiento, selección, conexión e impresión.
        //    Si el documento vino vacío (fallo de red / contrato inválido), el render falla
        //    y el overlay muestra el error correspondiente ("No se pudo generar el documento").
        await _printer.ImprimirAsync(render);

        return new BridgeOutcome(true, null);
    }

    /// <summary>
    /// GET al endpoint de comprobante. Deserializa la respuesta al DTO <see cref="PrintDocument"/>
    /// para validar el contrato y devuelve el JSON original (sin re-serializar) para entregárselo
    /// tal cual a MotorDsl. Devuelve string vacío ante cualquier fallo.
    /// </summary>
    private async Task<string> ObtenerDocumentoAsync()
    {
        using var cts = new CancellationTokenSource(RequestTimeout);
        try
        {
            // JSON crudo: es exactamente lo que MotorDsl espera renderizar.
            string json = await _http.GetStringAsync(EndpointComprobante, cts.Token);

            // Se deserializa al DTO sólo para verificar que la respuesta ES un PrintDocument válido.
            PrintDocument? doc = JsonSerializer.Deserialize<PrintDocument>(json, JsonReadOpts);
            if (doc is null || string.IsNullOrWhiteSpace(doc.Root?.Type))
            {
                Debug.WriteLine("[PRINT] La respuesta no es un PrintDocument válido.");
                return string.Empty;
            }

            return json;
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"[PRINT] Timeout ({RequestTimeout.TotalSeconds}s) obteniendo el comprobante.");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PRINT] No se pudo obtener el comprobante desde {EndpointComprobante}: {ex.Message}");
            return string.Empty;
        }
    }
}
