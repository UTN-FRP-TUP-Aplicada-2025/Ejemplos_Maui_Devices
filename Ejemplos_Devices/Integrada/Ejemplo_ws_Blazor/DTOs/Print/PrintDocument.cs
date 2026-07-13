using System.Text.Json.Serialization;

namespace Ejemplo_ws_Blazor.DTOs.Print;

/// <summary>
/// Documento imprimible raíz (réplica de GDA.Core.API.Client → Models/PrintActa/PrintDocument).
/// Serializado como JSON es el "DSL" que <c>ThermalPrintDevice</c> descarga por GET y pasa a
/// <c>MotorDsl.IDocumentEngine.Render(doc, profile)</c> para generar los bytes ESC/POS.
/// </summary>
public class PrintDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("format")]
    public string Format { get; set; } = "integrated";

    [JsonPropertyName("root")]
    public PrintNode Root { get; set; } = new();
}
