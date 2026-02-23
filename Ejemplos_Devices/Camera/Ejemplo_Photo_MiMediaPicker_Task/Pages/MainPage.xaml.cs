namespace Ejemplo_Photo_MiMediaPicker_Task.Pages;

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
                ImgPhoto.Source = imagen.Source;
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
