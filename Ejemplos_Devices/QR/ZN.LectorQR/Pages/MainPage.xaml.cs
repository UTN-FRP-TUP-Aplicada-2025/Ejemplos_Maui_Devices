using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
using ZXing;

namespace ZN.LectorQR.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Formatos a detectar (ZXing). El control detecta cuando BarCodeDetectionEnabled = true.
        Camera.BarCodeOptions = new BarcodeDecodeOptions
        {
            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE, BarcodeFormat.CODE_39 },
            TryHarder = true
        };
        Camera.BarCodeDetectionEnabled = true;
    }

    private async void OnCamerasLoaded(object sender, EventArgs e)
    {
        if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Alert", "Dale permiso si queres QR!", "OK");
            return;
        }

        if (Camera.NumCamerasDetected > 0)
        {
            // Cámara trasera por defecto.
            Camera.Camera = Camera.Cameras.FirstOrDefault(c => c.Position == CameraPosition.Back)
                            ?? Camera.Cameras.First();
            await Camera.StartCameraAsync();
        }
    }

    private void OnCameraViewOnDetected(object sender, BarcodeEventArgs e)
    {
        var textos = e.Result
            .Select(r => $"{r.BarcodeFormat}: {r.Text}")
            .ToList();

        if (textos.Count == 0)
            return;

        this.Dispatcher.Dispatch(async () =>
        {
            Camera.BarCodeDetectionEnabled = false;   // pausa la detección mientras se muestra el resultado
            await DisplayAlertAsync("QR detectado", string.Join("\n", textos), "OK");
            Camera.BarCodeDetectionEnabled = true;    // reanuda
        });
    }

    private async void OnFlashLightButtonClicked(object sender, EventArgs e)
    {
        try
        {
            Camera.TorchEnabled = !Camera.TorchEnabled;
        }
        catch
        {
            await DisplayAlertAsync("Alert", "No se pudo cambiar la linterna", "OK");
        }
    }

    private async void OnSwitchCameraButtonClicked(object sender, EventArgs e)
    {
        if (Camera.NumCamerasDetected > 1 && Camera.Camera != null)
        {
            var nueva = Camera.Cameras.FirstOrDefault(c => c.Position != Camera.Camera.Position);
            if (nueva != null)
            {
                Camera.Camera = nueva;
                await Camera.StartCameraAsync();
            }
        }
    }
}
