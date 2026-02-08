namespace Ejemplo_Imagen_Normalizacion.Utilities;

public interface IImageDevice
{
    public int MaxWidthHeight { get; set; }

    public int CompressionQuality { get; set; }

    public double CustomPhotoSize { get; set; }

    Task<byte[]?> ProcesarPhotoAsync(Stream simagen);
}
