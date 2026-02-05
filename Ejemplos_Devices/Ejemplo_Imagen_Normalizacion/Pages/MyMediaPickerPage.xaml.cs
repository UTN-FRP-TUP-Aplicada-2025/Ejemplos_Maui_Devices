
using CommunityToolkit.Maui.Core;
using System.Diagnostics;

namespace Ejemplo_Imagen_Normalizacion.Pages;

public partial class MyMediaPickerPage : ContentPage
{
    private bool _isCapturingImage = false;
    private CancellationTokenSource? _captureCancellationTokenSource;

    public TaskCompletionSource<Image> ResultadoTask { get; set; } = new();

    private string _flashIcon = "_"; // Default icon (flash off)
    public string FlashIcon
    {
        get => _flashIcon;
        set
        {
            if (_flashIcon != value)
            {
                _flashIcon = value;
                OnPropertyChanged(nameof(FlashIcon));
            }
        }
    }

    public MyMediaPickerPage()
    {
        InitializeComponent();
        BindingContext = this;
        StatusFlashToIcons();
    }

    async private void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
    {
        if (Camera.IsAvailable == true)
        {
            if (e.Media != null)
            {
                var image = new Image { Source = ImageSource.FromStream(() => e.Media) };
                ResultadoTask.SetResult(image);
            }
            await Navigation.PopAsync();
        }
    }

    private void OnMediaCaptureFailed(object sender, MediaCaptureFailedEventArgs e)
    {
        _isCapturingImage = false;
        DynamicLayout.IsEnabled = true;

        _captureCancellationTokenSource?.Dispose();
        _captureCancellationTokenSource = null;
    }

    async protected override void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await CheckForCameraPermissionAsync();

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

    async protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var availableCameras = await Camera.GetAvailableCameras(CancellationToken.None);
        var frontCamera = availableCameras.FirstOrDefault(c => c.Position == CameraPosition.Rear);

        if (frontCamera != null)
            Camera.SelectedCamera = frontCamera;
    }

    private async void OnTomarFotoClicked(object sender, EventArgs e)
    {
        if (_isCapturingImage)
        {
            return;
        }

        _isCapturingImage = true;
        //await Camera.CaptureImage(CancellationToken.None);

        DynamicLayout.IsEnabled = false;

        try
        {
            // Cancelar cualquier captura anterior pendiente
            _captureCancellationTokenSource?.Dispose();
            _captureCancellationTokenSource = new CancellationTokenSource();

            await Camera.CaptureImage(_captureCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            _isCapturingImage = false;
            DynamicLayout.IsEnabled = true;
        }
    }

    private async void btnSwitchCamera_Clicked(object sender, EventArgs e)
    {
        // Camera.CameraFacing = Camera.CameraFacing == CameraFacing.Back ? CameraFacing.Front : CameraFacing.Back;
        await Task.CompletedTask;
    }

    private async void OnActiveFlashClicked(object sender, EventArgs e)
    {
        if (Camera.CameraFlashMode == CameraFlashMode.Off)
        {
            Camera.CameraFlashMode = CameraFlashMode.On;
        }
        else if (Camera.CameraFlashMode == CameraFlashMode.On)
        {
            Camera.CameraFlashMode = CameraFlashMode.Auto;
        }
        else
        {
            Camera.CameraFlashMode = CameraFlashMode.Off;
        }

        StatusFlashToIcons();
    }

    public void StatusFlashToIcons()
    {
        if (Camera.CameraFlashMode == CameraFlashMode.Off)
        {
            FlashIcon = "flash_off";
        }
        else if (Camera.CameraFlashMode == CameraFlashMode.On)
        {
            FlashIcon = "flash_on";
        }
        else if (Camera.CameraFlashMode == CameraFlashMode.Auto)
        {
            FlashIcon = "flash_auto";
        }
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

                    Grid.SetRow(BtnTomarFoto, 0);
                    Grid.SetColumn(BtnTomarFoto, 0);
                    Grid.SetColumnSpan(BtnTomarFoto, 1);

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

                    Grid.SetRow(BtnTomarFoto, 2);
                    Grid.SetColumn(BtnTomarFoto, 0);
                    Grid.SetColumnSpan(BtnTomarFoto, 1);
                }

                DynamicLayout.BatchCommit();
            }
        }
        catch (Exception ex) { }
    }

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();

    //    // Si la página se cierra y nadie completó el Task, lo cancelamos
    //    // para liberar el hilo que está esperando en MainPage.
    //    if (!ResultadoTask.Task.IsCompleted)
    //    {
    //        ResultadoTask.TrySetResult(null);
    //    }
    //}

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

            _captureCancellationTokenSource?.Cancel();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error desregistrando el evento: {ex.Message}");
        }

#if ANDROID
        var activity = Platform.CurrentActivity;
        if (activity != null)
        {
            activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Unspecified; //restablece la orientación predeterminada
        }
#elif IOS
                // Implementa un handler para restaurar orientación en iOS
#endif
    }

    private async Task<bool> CheckForCameraPermissionAsync()
    {
        #region verifica permisos
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }
        #endregion

        #region explica porque se necesitan los permisos - no se muestra 
        if (Permissions.ShouldShowRationale<Permissions.Camera>())
        {
            await Shell.Current.DisplayAlertAsync("Necesidad de permisos de uso de la cámara", "Usamos la cámara para que los usuarios puedan adjuntar fotografías a los formularios que lo requieran.", "OK");
        }
        #endregion

        #region los solicita!
        status = await Permissions.RequestAsync<Permissions.Camera>();

        if (status == PermissionStatus.Granted) return true;
        #endregion

#if ANDROID
        try
        {
            if (status == PermissionStatus.Denied)
            {
                await Shell.Current.DisplayAlertAsync("Permisos de cámara denegados",
                    "Se necesitan habilitar los permisos",
                    "Abrir configuración"
                );
                AppInfo.ShowSettingsUI();
                return false;
            }

            if (status != PermissionStatus.Restricted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status == PermissionStatus.Granted) return true;
            }

            await Shell.Current.DisplayAlertAsync("Permiso no concedido", "Sin el acceso a la cámara no se podrá tomar la foto.", "OK");
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.Write(ex);
#endif
        }
#endif
        return false;// status == PermissionStatus.Granted;
    }
}