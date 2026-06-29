using CameraScanner.Maui;

namespace CS.LectorQR.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Los formatos se declaran en XAML (BarcodeFormats="QR,Code39").
        // Alternativa por código: Camera.BarcodeFormats = BarcodeFormats.QR | BarcodeFormats.Code39;
    }

    async public Task<bool> RequestCameraPermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
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
            Camera.CameraFacing = Camera.CameraFacing == CameraFacing.Back
              ? CameraFacing.Front
              : CameraFacing.Back;
        }
    }

    private void OnCameraViewOnDetected(object sender, OnDetectionFinishedEventArg e)
    {
        var textos = e.BarcodeResults
            .Select(b => $"{(b.BarcodeType == BarcodeTypes.Unknown ? "Text" : b.BarcodeType.ToString())}: {b.DisplayValue}")
            .ToList();

        if (textos.Count == 0)
            return;

        this.Dispatcher.Dispatch(async () =>
        {
            Camera.CameraEnabled = false;   // pausa el escaneo mientras se muestra el resultado
            await DisplayAlertAsync("QR detectado", string.Join("\n", textos), "OK");
            Camera.CameraEnabled = true;    // reanuda para escanear de nuevo
        });
    }
}
