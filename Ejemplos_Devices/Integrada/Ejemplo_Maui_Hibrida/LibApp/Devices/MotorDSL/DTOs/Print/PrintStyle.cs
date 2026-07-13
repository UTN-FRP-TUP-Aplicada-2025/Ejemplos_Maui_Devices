using System.Text.Json.Serialization;

namespace LibApp.Devices.MotorDSL.DTOs.Print;

/// <summary>
/// Estilo aplicable a nodos de texto e imagen (réplica de GDA.Core.API.Client → Models/PrintActa/PrintStyle).
/// </summary>
public class PrintStyle
{
    /// <summary>"left" | "center" | "right"</summary>
    [JsonPropertyName("align")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Align { get; set; }

    [JsonPropertyName("bold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Bold { get; set; }
}
