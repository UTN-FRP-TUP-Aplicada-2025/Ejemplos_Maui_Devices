
namespace Ejemplo_Photo_MediaPicker.Services;

public class CamaraService : ICamaraService
{
    public Task<Stream?> TomarFotoAsync() =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (!MediaPicker.Default.IsCaptureSupported) return null;

            var foto = await MediaPicker.Default.CapturePhotoAsync();
            if (foto is null) return (Stream?)null;

            // Copiar a memoria antes de salir — el archivo temporal puede
            // limpiarse apenas termine el camera intent
            await using var src = await foto.OpenReadAsync();
            var ms = new MemoryStream();
            await src.CopyToAsync(ms);
            ms.Position = 0;
            return (Stream?)ms;
        });

    public Task<Stream?> ElegirDeGaleriaAsync() =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var fotos = await MediaPicker.Default.PickPhotosAsync();
            var foto = fotos?.FirstOrDefault();
            if (foto is null) return (Stream?)null;

            await using var src = await foto.OpenReadAsync();
            var ms = new MemoryStream();
            await src.CopyToAsync(ms);
            ms.Position = 0;
            return (Stream?)ms;
        });
}
