using Ejemplo_LectorQR_Dialog.Models;

namespace Ejemplo_LectorQR_Dialog.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    async private void OnLeerQRClicked(object sender, EventArgs e)
    {
        BtnLeerQR.IsEnabled = false;
        try
        {
            var destinoPage = new QRLectorPage();

            await Navigation.PushAsync(destinoPage);

            List<QRContent> qrs = await destinoPage.ResultadoTask.Task;

            var qr = qrs.FirstOrDefault();

            if (qr != null)
            {
                
                LbQR.Text = qr.Value;
            }
            else
            {
                await DisplayAlertAsync("Cancelado", "No se recibió ningún dato", "OK");
            }
        }
        finally
        {
            BtnLeerQR.IsEnabled = true;
        }
    }
}
