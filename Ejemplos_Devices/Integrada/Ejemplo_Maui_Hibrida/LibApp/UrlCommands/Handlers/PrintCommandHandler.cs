using System.Diagnostics;
using System.Text.Json;

using LibApp.Devices.MotorDSL.DTOs.Print;
using LibApp.Devices.MotorDSL.Models;
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
        await ImprimirComprobanteAsync();

        // El overlay ya le comunicó al usuario lo que haya pasado (éxito o fallo): el comando
        // se cierra sin cancelar navegación ni redirigir, gane o pierda.
        return new BridgeOutcome(true, null);
    }

    /// <summary>
    /// GET → validación de contrato → render → impresión. Se pasa a sí mismo como reintento
    /// del overlay, de modo que "Reintentar" ante un fallo de red vuelva a la red.
    /// </summary>
    private async Task ImprimirComprobanteAsync()
    {
        _printer.MostrarObteniendoDocumento();

        var documento = await ObtenerDocumentoAsync();

        switch (documento)
        {
            // Reintentable: rehace el GET, no reusa nada.
            case DocumentResult.NetworkError e:
                _printer.MostrarFalloDocumento(
                    PrinterErrorCatalog.DocumentoSinConexion(e.TechnicalMessage),
                    ImprimirComprobanteAsync);
                return;

            // No reintentable: el backend respondió, pero con algo que no es un comprobante.
            // Reintentar daría el mismo resultado; esto va a soporte.
            case DocumentResult.InvalidContract e:
                _printer.MostrarFalloDocumento(PrinterErrorCatalog.DocumentoInvalido(e.TechnicalMessage));
                return;
        }

        var json = ((DocumentResult.Ok)documento).Json;

        // 1. Render SIEMPRE primero, antes de tocar la impresora.
        var profile = new DeviceProfile("58HB6", 32, "escpos-bitmap");
        profile.SetCapability("supports_bitmap", true);
        profile.SetCapability("bitmap_max_width_px", 320);
        profile.SetCapability("bitmap_binarization_threshold", 128);

        var render = _engine.Render(json, profile);

        // 2. El overlay maneja el render fallido, permisos, descubrimiento, selección, conexión
        //    e impresión. Se delega SIEMPRE, también con el render en error: es el único
        //    componente que puede comunicarle algo al usuario.
        //    Requiere los permisos Bluetooth declarados en AndroidManifest.xml (BLUETOOTH_SCAN/CONNECT).
        await _printer.ImprimirAsync(render);
    }

    /// <summary>
    /// GET al endpoint de comprobante. Deserializa la respuesta al DTO <see cref="PrintDocument"/>
    /// para validar el contrato y devuelve el JSON original (sin re-serializar) para entregárselo
    /// tal cual a MotorDsl.
    /// </summary>
    private async Task<DocumentResult> ObtenerDocumentoAsync()
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
                const string detalle = "La respuesta no es un PrintDocument válido (Root.Type ausente).";
                Debug.WriteLine($"[PRINT] {detalle}");
                return new DocumentResult.InvalidContract(detalle);
            }

            return new DocumentResult.Ok(json);
        }
        catch (JsonException ex)
        {
            // El backend respondió algo que no es JSON parseable: es contrato, no red.
            Debug.WriteLine($"[PRINT] Respuesta no parseable desde {EndpointComprobante}: {ex.Message}");
            return new DocumentResult.InvalidContract(ex.Message);
        }
        catch (OperationCanceledException)
        {
            var detalle = $"Timeout ({RequestTimeout.TotalSeconds}s) obteniendo el comprobante.";
            Debug.WriteLine($"[PRINT] {detalle}");
            return new DocumentResult.NetworkError(detalle);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PRINT] No se pudo obtener el comprobante desde {EndpointComprobante}: {ex.Message}");
            return new DocumentResult.NetworkError(ex.Message);
        }
    }
}
