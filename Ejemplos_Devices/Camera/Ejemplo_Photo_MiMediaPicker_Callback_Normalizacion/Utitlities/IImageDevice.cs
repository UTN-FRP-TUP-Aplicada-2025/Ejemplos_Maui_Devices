namespace Ejemplo_Imagen_Normalizacion.Utilities;

public interface IImageDevice
{
    public int MaxWidthHeight { get; set; }

    public int CompressionQuality { get; set; }

    public double CustomPhotoSize { get; set; }

    Task<byte[]?> ProcesarPhotoAsync(Stream simagen);

    /// <summary>
    /// Procesa la imagen leída desde <paramref name="inputPath"/> y la escribe
    /// en <paramref name="outputPath"/>. Si <paramref name="outputPath"/> es
    /// null, genera un archivo nuevo en CacheDirectory.
    /// Devuelve el path del archivo procesado o null si falló.
    /// </summary>
    Task<string?> ProcesarPhotoAsync(string inputPath, string? outputPath = null);
}
