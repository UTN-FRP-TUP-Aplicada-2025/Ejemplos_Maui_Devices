using System.Text.Json.Serialization;

namespace Ejemplo_ws_Blazor.DTOs.Print;

/// <summary>
/// Nodo genérico del árbol de impresión: texto, imagen (bitmap/qrcode), contenedor o separador.
/// Réplica de GDA.Core.API.Client → Models/PrintActa/PrintNode. Es el formato que consume MotorDsl.
/// </summary>
public class PrintNode
{
    /// <summary>"text" | "image" | "container"</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    // --- text ---
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }

    // --- image ---
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }

    /// <summary>"bitmap" | "qrcode"</summary>
    [JsonPropertyName("imageType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageType { get; set; }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    // --- container ---
    [JsonPropertyName("layout")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Layout { get; set; }

    [JsonPropertyName("children")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PrintNode>? Children { get; set; }

    // --- style (texto e imagen) ---
    [JsonPropertyName("style")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PrintStyle? Style { get; set; }
}
