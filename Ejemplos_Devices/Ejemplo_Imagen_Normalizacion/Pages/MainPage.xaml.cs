using Ejemplo_Imagen_Normalizacion.Services;
using System.Diagnostics;

namespace Ejemplo_Imagen_Normalizacion.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

    }

    async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
    {
        BtnPhoto.IsEnabled = false;
        try
        {
            var destinoPage = new MyMediaPickerPage();

            await Navigation.PushAsync(destinoPage);

            var imagen = await destinoPage.ResultadoTask.Task;

            if (imagen != null)
            {
                using var memoryStream = new MemoryStream();

                if (imagen.Source is StreamImageSource streamImageSource)
                {
                    var stream = await streamImageSource.Stream(CancellationToken.None);
                    await stream.CopyToAsync(memoryStream);
                }

                byte[]? imageBytesC = await new ImageDeviceAutoRotateService()
                {
                    MaxWidthHeight = 1000,
                    CompressionQuality = 75,
                    CustomPhotoSize = 50
                }.ProcesarPhotoAsync(memoryStream);

                ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytesC!));
            }
            else
            {
                await DisplayAlertAsync("Cancelado", "No se recibió ningún dato", "OK");
            }
        }
        catch (Exception ex)
        {
        }
        finally
        {
            BtnPhoto.IsEnabled = true;
        }
    }
}
