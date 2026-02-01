namespace Ejemplo_Photo_Dialog.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
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
}
