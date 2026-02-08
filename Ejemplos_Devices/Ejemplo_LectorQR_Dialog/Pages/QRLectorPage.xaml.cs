using BarcodeScanner.Mobile;
using System.Diagnostics;

namespace Ejemplo_LectorQR_Dialog.Pages;

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
       
    async public Task<bool> RequestCameraPermission()
    {
        bool allowed = await BarcodeScanner.Mobile.Methods.AskForRequiredPermission();
        return allowed;
    }

    async private void OnCameraViewOnDetecte(object sender, BarcodeScanner.Mobile.OnDetectedEventArg e)
    {
        //if (await RequestCameraPermission())
        //{
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
       // }        
    }

    private async void OnActiveFlashClicked(object sender, EventArgs e)
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

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!ResultadoTask.Task.IsCompleted)
        {
            ResultadoTask.TrySetResult(null);
        }

        try
        {
            DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error desregistrando el evento: {ex.Message}");
        }
    }

    async protected override void OnAppearing()
    {
        base.OnAppearing();

        await RequestCameraPermission();

        try
        {
            DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error desregistrando el evento: {ex.Message}");
        }

        DynamicLayout.IsEnabled = true;

        UpdateLayoutOrientation(DeviceDisplay.MainDisplayInfo.Orientation);
    }

    private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        if (e != null)
            UpdateLayoutOrientation(e.DisplayInfo.Orientation);
    }

    private void UpdateLayoutOrientation(DisplayOrientation orientation)
    {
        try
        {
            if (DynamicLayout == null || !DynamicLayout.IsEnabled) return;

            if (DynamicLayout.IsEnabled == true)
            {
                DynamicLayout.BatchBegin();

                DynamicLayout.RowDefinitions.Clear();
                DynamicLayout.ColumnDefinitions.Clear();

                if (orientation == DisplayOrientation.Landscape)
                {
                    DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                    DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                    DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    Grid.SetRow(BtnFlashButton, 0);
                    Grid.SetColumn(BtnFlashButton, 2);
                    Grid.SetColumnSpan(BtnFlashButton, 1);

                    Grid.SetRow(Camera, 0);
                    Grid.SetColumn(Camera, 1);
                    Grid.SetColumnSpan(Camera, 1);

                    Grid.SetRow(BtnVolver, 0);
                    Grid.SetColumn(BtnVolver, 0);
                    Grid.SetColumnSpan(BtnVolver, 1);

                }
                else if (orientation == DisplayOrientation.Portrait)
                {
                    DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                    DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                    Grid.SetRow(BtnFlashButton, 0);
                    Grid.SetColumn(BtnFlashButton, 0);
                    Grid.SetColumnSpan(BtnFlashButton, 1);

                    Grid.SetRow(Camera, 1);
                    Grid.SetColumn(Camera, 0);
                    Grid.SetColumnSpan(Camera, 1);

                    Grid.SetRow(BtnVolver, 2);
                    Grid.SetColumn(BtnVolver, 0);
                    Grid.SetColumnSpan(BtnVolver, 1);
                }

                DynamicLayout.BatchCommit();
            }
        }
        catch (Exception ex) { }
    }
}