using AndroidX.Navigation;
using Ejemplo_Imagen_Normalizacion.Utilities;

namespace Ejemplo_Photo_MiMediaPicker_Callback_Normalizacion.Pages;

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
            Action<string?> resultadoCallback = async (path) =>
            {
                string? outPath = null;
                try
                {
                    if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

                    outPath = await new ImageDeviceAutoRotate()
                    {
                        MaxWidthHeight = 1000,
                        CompressionQuality = 75,
                        CustomPhotoSize = 50
                    }.ProcesarPhotoAsync(path);

                    byte[] bytes = File.ReadAllBytes(outPath ?? "");

                    //encola la acción para que se ejecute en el UI thread y así evitar problemas de acceso a la UI desde un thread de background.          
                    //Dispatcher.Dispatch no crea un thread; sirve precisamente para volver al UI thread desde uno secundario.
                    Dispatcher.Dispatch(() =>
                    {
                        ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al tomar la foto: {ex.Message}");
                }
                finally
                {
                    try
                    {
                        if (path != null) File.Delete(path);
                        if (outPath != null) File.Delete(outPath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"No se pudo borrar el temporal: {ex.Message}");
                    }
                }
            };

            var pageParams = new ShellNavigationQueryParameters{ { "OnPhotoCallback", resultadoCallback }};

            await Shell.Current.GoToAsync(nameof(MyMediaPickerPage), pageParams);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
        finally
        {
            BtnPhoto.IsEnabled = true;
        }
    }
}
