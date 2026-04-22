using Android.Media;
using Ejemplo_Photo_MediaPicker.Services;

namespace Ejemplo_Photo_MediaPicker.Pages;

public partial class MainPage : ContentPage
{

    CamaraService _camaraService;

    public MainPage(CamaraService camaraService)
    {
        InitializeComponent();
        _camaraService = camaraService;
    }
    
    async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
    {
        // Bien — el servicio encapsula su propia restricción
        var stream = await _camaraService.TomarFotoAsync();

        ImgPhoto.Source = stream is not null
            ? ImageSource.FromStream(() => stream)
            : null;
    }


    //async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
    //{
    //    if (MediaPicker.Default.IsCaptureSupported)
    //    {
    //        FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

    //        if (photo != null)
    //        {
    //            // OPCIÓN A — FromFile: Glide administra su propio FileStream sobre la ruta temporal.
    //            // Correcto en Debug y Release. Ver MEDIAPICKER_FIX.md para fundamentos.
    //            ImgPhoto.Source = ImageSource.FromFile(photo.FullPath);

    //            // OPCIÓN B — MemoryStream: útil si se necesita transformar los bytes antes de mostrar.
    //            // Descomentar este bloque y comentar la Opción A para probarla.
    //            // ADVERTENCIA: carga toda la imagen en RAM; puede fallar con fotos de alta resolución.
    //            //
    //            // using Stream sourceStream = await photo.OpenReadAsync();
    //            // var buffer = new MemoryStream();
    //            // await sourceStream.CopyToAsync(buffer);
    //            // byte[] imageBytes = buffer.ToArray();
    //            // ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

    //            // CÓDIGO ORIGINAL (buggy) — NO usar.
    //            // El using cierra el stream antes de que Glide lo lea → ObjectDisposedException.
    //            //
    //            // using Stream sourceStream = await photo.OpenReadAsync();
    //            // var image = new Image { Source = ImageSource.FromStream(() => sourceStream) };
    //            // ImgPhoto.Source = image.Source;
    //        }
    //    }
    //}
}
