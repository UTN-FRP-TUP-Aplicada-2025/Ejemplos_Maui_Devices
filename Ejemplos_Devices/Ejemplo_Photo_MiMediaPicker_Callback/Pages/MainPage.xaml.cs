using Ejemplo_Photo_MiMediaPicker_Callback.Pages;

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
            Action<Image> resultadoCallback = async (image) =>
            {
                await this.Dispatcher.DispatchAsync(new Action(async () =>
                {
                    if (image != null)
                    {
                        ImgPhoto.Source = image.Source;
                    }
                    else
                    {
                        await DisplayAlertAsync("Cancelado", "No se recibió ningún dato", "OK");
                    }
                }));
            };

            var pageParams = new ShellNavigationQueryParameters
                {
                    {"OnPhotoCallback" , resultadoCallback}
                };

            await Shell.Current.GoToAsync(nameof(MyMediaPickerPage), pageParams);

        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Ocurrió un error al abrir la cámara: {ex.Message}", "OK");
        }
        finally
        {
            BtnPhoto.IsEnabled = true;
        }
    }
}
