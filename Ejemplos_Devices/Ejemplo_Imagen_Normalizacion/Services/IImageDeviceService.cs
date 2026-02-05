using System;
using System.Collections.Generic;
using System.Text;

namespace Ejemplo_Imagen_Normalizacion.Services;

public interface IImageDeviceService
{
    public int MaxWidthHeight { get; set; }

    public int CompressionQuality { get; set; }

    public double CustomPhotoSize { get; set; }

    Task<byte[]?> ProcesarPhotoAsync(Stream simagen);
}
