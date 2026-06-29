using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
using ZXing;

using ZN.LectorQR_Dialog.Models;

using System.Diagnostics;

namespace ZN.LectorQR_Dialog.Pages;

public partial class QRLectorPage : ContentPage
{
    private int _completed = 0;
    public TaskCompletionSource<List<QRContent>> ResultadoTask { get; set; } = new();

    string flashIcon = "";
    public string FlashIcon
    {
        get
        {
            return flashIcon;
        }
        set
        {
            if (value != null)
            {
                flashIcon = value;
                OnPropertyChanged();
            }
        }
    }

    public QRLectorPage()
	{
		InitializeComponent();

        // Formatos a detectar (ZXing). El control detecta cuando BarCodeDetectionEnabled = true.
        Camera.BarCodeOptions = new BarcodeDecodeOptions
        {
            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE, BarcodeFormat.CODE_39 },
            TryHarder = true
        };
        Camera.BarCodeDetectionEnabled = true;

        BindingContext = this;
    }

    async public Task<bool> RequestCameraPermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }

    private async void OnCamerasLoaded(object sender, EventArgs e)
    {
        if (!await RequestCameraPermission())
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

    private void OnCameraViewOnDetecte(object sender, BarcodeEventArgs e)
    {
        List<QRContent> QRs = new List<QRContent>();
        foreach (var r in e.Result)
        {
            QRs.Add(new QRContent { Type = r.BarcodeFormat.ToString(), Value = r.Text });
        }

        this.Dispatcher.Dispatch(async () =>
            {
                Camera.BarCodeDetectionEnabled = false;   // detener detección (antes: IsScanning = false)

                //ResultadoTask.SetResult(result);
                CompletarResultado(QRs);

                await Navigation.PopAsync();
            });
    }

    private async void OnActiveFlashClicked(object sender, EventArgs e)
    {
        try
        {
            Camera.TorchEnabled = !Camera.TorchEnabled;
            PaintFlashStatus();
        }
        catch
        {
            await DisplayAlertAsync("Alert", "No se pudo cambiar la linterna", "OK");
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        CompletarResultado(new List<QRContent>());
        await Navigation.PopAsync();
        //await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //if (!ResultadoTask.Task.IsCompleted)
        //{
        //    ResultadoTask.TrySetResult(null);
        //}

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

        PaintFlashStatus();

        DynamicLayout.IsEnabled = true;

        UpdateLayoutOrientation(DeviceDisplay.MainDisplayInfo.Orientation);
    }

    private void CompletarResultado(List<QRContent> result)
    {
        if (Interlocked.Exchange(ref _completed, 1) == 0)
        {
            ResultadoTask.TrySetResult(result);
        }
    }

    protected void PaintFlashStatus()
    {
        if (Camera.TorchEnabled) FlashIcon = "flash_on";
        else FlashIcon = "flash_off";
    }

    private void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
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
