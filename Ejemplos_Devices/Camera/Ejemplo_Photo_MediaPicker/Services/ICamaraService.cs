using System;
using System.Collections.Generic;
using System.Text;

namespace Ejemplo_Photo_MediaPicker.Services;

public interface ICamaraService
{
    Task<Stream?> TomarFotoAsync();
    Task<Stream?> ElegirDeGaleriaAsync();
}
