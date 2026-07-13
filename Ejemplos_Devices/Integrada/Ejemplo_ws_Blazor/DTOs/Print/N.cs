namespace Ejemplo_ws_Blazor.DTOs.Print;

/// <summary>
/// Fábrica de nodos de impresión (réplica del helper <c>N</c> de GDA.Core.API.Client →
/// Models/PrintActa/PrintNodeBuilder). Construye el árbol <see cref="PrintNode"/> de forma declarativa.
/// </summary>
public static class N
{
    public static PrintNode Text(string value, bool bold = false, string? align = null) => new()
    {
        Type = "text",
        Value = value,
        Style = (bold || align != null) ? new PrintStyle { Bold = bold, Align = align } : null
    };

    public static PrintNode Separator() => Text("================================");

    public static PrintNode Empty() => Text("");

    public static PrintNode Image(string source, string imageType, int? width = null, string? align = null) => new()
    {
        Type = "image",
        Source = source,
        ImageType = imageType,
        Width = width,
        Style = align != null ? new PrintStyle { Align = align } : null
    };

    public static PrintNode QrCode(string url) => Image(url, "qrcode");

    public static PrintNode Container(params PrintNode[] children) => new()
    {
        Type = "container",
        Layout = "vertical",
        Children = children.ToList()
    };
}
