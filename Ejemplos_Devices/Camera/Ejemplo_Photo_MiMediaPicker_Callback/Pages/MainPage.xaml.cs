namespace Ejemplo_Photo_MiMediaPicker_Callback.Pages;

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
            Action<string?> resultadoCallback = (path) =>
            {
                // null o vac\u00edo = el usuario cancel\u00f3 sin tomar foto.
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return;

                byte[] bytes;
                try
                {
                    // 1) Leemos los bytes del archivo temporal a memoria.
                    bytes = File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error leyendo foto: {ex.Message}");
                    return;
                }
                finally
                {
                    // 2) Borramos el archivo temporal: ya tenemos los bytes en RAM,
                    //    no se necesita m\u00e1s en CacheDirectory.
                    try { File.Delete(path); }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"No se pudo borrar el temporal: {ex.Message}");
                    }
                }

                // 3) Mostramos desde memoria. Cada vez que MAUI re-eval\u00fae el
                //    ImageSource (rotaci\u00f3n, recycling) devolvemos un MemoryStream
                //    nuevo sobre los mismos bytes.
                Dispatcher.Dispatch(() =>
                {
                    ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                });
            };

            var pageParams = new ShellNavigationQueryParameters
            {
                { "OnPhotoCallback", resultadoCallback }
            };

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
