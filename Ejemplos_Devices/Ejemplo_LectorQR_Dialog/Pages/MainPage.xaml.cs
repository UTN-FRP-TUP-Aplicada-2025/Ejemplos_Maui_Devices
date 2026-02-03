namespace Ejemplo_LectorQR_Dialog.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    async private void OnLeerQRClicked(object sender, EventArgs e)
    {
        var destinoPage = new QRLectorPage();

        await Navigation.PushAsync(destinoPage);

        var parametro = await destinoPage.ResultadoTask.Task;

        if (parametro != null)
        {
            LbQR.Text = parametro;
        }
        else
        {
            await DisplayAlertAsync("Cancelado", "No se recibió ningún dato", "OK");
        }
    }
}
