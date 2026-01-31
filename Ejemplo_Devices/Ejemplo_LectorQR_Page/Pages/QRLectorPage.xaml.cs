using BarcodeScanner.Mobile;

namespace Ejemplo_LectorQR_Page.Pages;

public partial class QRLectorPage : ContentPage
{
    public TaskCompletionSource<string> ResultadoTask { get; set; } = new();

    public QRLectorPage()
	{
		InitializeComponent();

#if ANDROID
        BarcodeScanner.Mobile.Methods.SetSupportBarcodeFormat(BarcodeScanner.Mobile.BarcodeFormats.QRCode | BarcodeScanner.Mobile.BarcodeFormats.Code39);
#endif

    }

    async protected override void OnAppearing()
    {
        base.OnAppearing();
        await RequestCameraPermission();
    }

    async public Task<bool> RequestCameraPermission()
    {
        bool allowed = await BarcodeScanner.Mobile.Methods.AskForRequiredPermission();
        return allowed;
    }

    async private void OnCameraViewOnDetecte(object sender, BarcodeScanner.Mobile.OnDetectedEventArg e)
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
                
                ResultadoTask.SetResult(result);

                await Navigation.PopAsync();
            });
        }        
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!ResultadoTask.Task.IsCompleted)
        {
            ResultadoTask.TrySetResult(null);
        }
    }
}