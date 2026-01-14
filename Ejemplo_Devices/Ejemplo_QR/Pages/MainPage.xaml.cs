namespace Ejemplo_QR.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    async private void OnLeerQRClicked(object? sender, EventArgs e)
    {
        Dictionary<string, object> parametros = new()
        {
            { "Contenido", new TaskCompletionSource<string>() }
        };  

        Shell.Current.GoToAsync(nameof(Pages.QRPage), parametros);

        string contenido= await (parametros["Contenido"] as TaskCompletionSource<string>).Task;

        await DisplayAlertAsync("resultado",contenido,"OK");
    }
}
