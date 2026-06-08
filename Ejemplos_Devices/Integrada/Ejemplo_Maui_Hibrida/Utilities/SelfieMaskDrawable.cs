using Microsoft.Maui.Graphics;

namespace Ejemplo_Maui_Hibrida.Utilities;

public class SelfieMaskDrawable : IDrawable
{
    // Si quedan en null, los colores se eligen según el tema actual (blanco en Light, negro en Dark).
    public Color? OverlayColor { get; set; }
    public Color? BorderColor { get; set; }
    public float BorderThickness { get; set; } = 0f;

    // Proporción del óvalo respecto del área visible
    public float WidthFraction { get; set; } = 0.75f;
    public float HeightToWidthRatio { get; set; } = 1.35f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        if (w <= 0 || h <= 0) return;

        bool isDark = (Application.Current?.RequestedTheme ?? AppTheme.Light) == AppTheme.Dark;
        Color overlay = OverlayColor ?? (isDark ? Colors.Black : Colors.White);
        Color border = BorderColor ?? overlay;

        float ew = w * WidthFraction;
        float eh = ew * HeightToWidthRatio;
        if (eh > h * 0.9f)
        {
            eh = h * 0.9f;
            ew = eh / HeightToWidthRatio;
        }

        float ex = (w - ew) / 2f;
        float ey = (h - eh) / 2f;

        var maskPath = new PathF();
        maskPath.AppendRectangle(dirtyRect);
        maskPath.AppendEllipse(ex, ey, ew, eh);
        canvas.FillColor = overlay;
        canvas.FillPath(maskPath, WindingMode.EvenOdd);

        if (BorderThickness > 0f)
        {
            canvas.StrokeColor = border;
            canvas.StrokeSize = BorderThickness;
            canvas.DrawEllipse(ex, ey, ew, eh);
        }
    }
}
