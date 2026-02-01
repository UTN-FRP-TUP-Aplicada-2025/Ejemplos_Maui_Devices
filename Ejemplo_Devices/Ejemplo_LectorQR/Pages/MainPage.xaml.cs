namespace Ejemplo_LectorQR.Pages;

using BarcodeScanner.Mobile;
using System.Threading.Tasks;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();

        BarcodeScanner.Mobile.Methods.SetSupportBarcodeFormat(BarcodeScanner.Mobile.BarcodeFormats.QRCode);
    }

    async public Task<bool> RequestCameraPermission()
    {
        bool allowed = await BarcodeScanner.Mobile.Methods.AskForRequiredPermission();
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
            Camera.CameraFacing = Camera.CameraFacing == BarcodeScanner.Mobile.CameraFacing.Back
              ? BarcodeScanner.Mobile.CameraFacing.Front
              : BarcodeScanner.Mobile.CameraFacing.Back;
        }
    }

    private async void OnCameraViewOnDetected(object sender, OnDetectedEventArg e)
    {
        if (await RequestCameraPermission())
        {
            List<BarcodeResult> obj = e.BarcodeResults;

            string result = string.Empty;
            for (int i = 0; i < obj.Count; i++)
            {
                result += $"Type: {obj[i].BarcodeType}, Value: {obj[i].DisplayValue}{Environment.NewLine}";
            }

            this.Dispatcher.Dispatch(async () =>
            {
                Camera.IsScanning = false;
                await DisplayAlertAsync("QR Detected", result, "OK");
                Camera.IsScanning = true;
            });
        }
    }
}
