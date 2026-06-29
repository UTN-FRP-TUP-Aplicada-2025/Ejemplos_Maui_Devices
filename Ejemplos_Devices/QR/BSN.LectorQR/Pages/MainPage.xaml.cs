namespace BSN.LectorQR.Pages;

using BarcodeScanning;
using System.Threading.Tasks;
using Xamarin.Google.MLKit.Vision.BarCode;
using static AndroidX.Media3.Container.ObuParser;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();

        // Formatos ahora se declaran en XAML (Symbologies="QrCode,Code39").
        // Alternativa por código: Camera.Symbologies = BarcodeFormats.QrCode | BarcodeFormats.Code39;
    }

    async public Task<bool> RequestCameraPermission()
    {
        //bool allowed = await BarcodeScanner.Mobile.Methods.AskForRequiredPermission();
        bool allowed = await BarcodeScanning.Methods.AskForRequiredPermissionAsync();
        return allowed;
    }

    private async void OnFlashLightButtonClicked(object sender, EventArgs e)
    {
        if (await RequestCameraPermission())
        {
            Camera.TorchOn = !Camera.TorchOn;
        }
        else
        {
            await DisplayAlertAsync("Alert", "Dale permiso si queres QR!", "OK");
        }
    }

    private async void OnSwitchCameraButtonClicked(object sender, EventArgs e)
    {
        if (await RequestCameraPermission())
        {
            Camera.CameraFacing = Camera.CameraFacing == BarcodeScanning.FromArrayMobile.CameraFacing.Back
              ? BarcodeScanning.FromArrayMobile.CameraFacing.Front
              : BarcodeScanning.FromArrayMobile.CameraFacing.Back;
        }
    }

    private void OnCameraViewOnDetecte(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
    {
        var QRs = new List<QRContent>();
        foreach (var b in e.BarcodeResults)
        {
            string type = b.BarcodeType == BarcodeTypes.Unknown ? "Text" : b.BarcodeType.ToString();
            QRs.Add(new QRContent { Type = type, Value = b.DisplayValue });
        }

        this.Dispatcher.Dispatch(async () =>
        {
            Camera.CameraEnabled = false;   // detener cámara (antes: IsScanning = false)
            CompletarResultado(QRs);
            await Navigation.PopAsync();
        });
    }
}
