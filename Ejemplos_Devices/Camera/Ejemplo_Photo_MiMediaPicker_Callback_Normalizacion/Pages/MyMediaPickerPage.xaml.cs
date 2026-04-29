using Android.Text;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace Ejemplo_Photo_MiMediaPicker_Callback_Normalizacion.Pages;

[QueryProperty(nameof(OnPhotoCallback), "OnPhotoCallback")]
public partial class MyMediaPickerPage : ContentPage
{
    private bool _isCapturingImage = false;
    private CancellationTokenSource? _captureCancellationTokenSource;
    private CameraView? _cameraView;

    public Action<string?>? OnPhotoCallback { get; set; }

    private string _flashIcon = "flash_off";
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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
        UpdateLayoutOrientation(DeviceDisplay.MainDisplayInfo.Orientation);

        await EvaluarYMostrarEstadoPermisoAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
            _captureCancellationTokenSource?.Cancel();

            if (_cameraView != null)
            {
                _cameraView.MediaCaptured -= OnMediaCaptured;
                _cameraView.MediaCaptureFailed -= OnMediaCaptureFailed;
                CameraContainer.Content = null;
                _cameraView = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnDisappearing error: {ex.Message}");
        }

#if ANDROID
        var activity = Platform.CurrentActivity;
        if (activity != null)
            activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Unspecified;
#endif
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted && _cameraView != null)
            await SeleccionarCamaraAsync();
    }

    // PERMISOS

    private async Task EvaluarYMostrarEstadoPermisoAsync()
    {
        // Guardia: simulador de iOS no soporta CameraView
#if IOS
        if (DeviceInfo.Current.DeviceType == DeviceType.Virtual)
        {
            MostrarOverlayPermiso(
                "C�mara no disponible",
                "La c�mara no est� disponible en el simulador de iOS. Prob� en un dispositivo f�sico.",
                puedeReintentar: false);
            return;
        }
#endif

        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (status == PermissionStatus.Granted)
        {
            if (MediaPicker.Default.IsCaptureSupported)
                MostrarVisorCamara();
            else
                MostrarOverlayPermiso("C�mara no disponible",
                    "Este dispositivo no tiene c�mara disponible.",
                    puedeReintentar: false);
            return;
        }

        status = await Permissions.RequestAsync<Permissions.Camera>();

        if (status == PermissionStatus.Granted)
        {
            if (MediaPicker.Default.IsCaptureSupported)
                MostrarVisorCamara();
            else
                MostrarOverlayPermiso("C�mara no disponible",
                    "Este dispositivo no tiene c�mara disponible.",
                    puedeReintentar: false);
            return;
        }

        if (status == PermissionStatus.Restricted)
        {
            MostrarOverlayPermiso("Acceso restringido",
                "El acceso a la c�mara est� restringido por una pol�tica del dispositivo. Consult� con el administrador.",
                puedeReintentar: false);
            return;
        }

        bool puedeReintentar = false;

#if ANDROID
        puedeReintentar = Permissions.ShouldShowRationale<Permissions.Camera>();
#endif

        MostrarOverlayPermiso(
            titulo: puedeReintentar
                ? "Permiso de c�mara necesario"
                : "Acceso a la c�mara denegado",
            mensaje: puedeReintentar
                ? "Para tomar fotos necesitamos acceso a la c�mara. Pod�s intentar conceder el permiso."
                : "Para tomar fotos necesitamos acceso a la c�mara. Habilitalo desde los ajustes de la aplicaci�n.",
            puedeReintentar: puedeReintentar);
    }

    // OVERLAY � mostrar / ocultar

    private void MostrarVisorCamara()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PermissionDeniedOverlay.IsVisible = false;

            if (_cameraView == null)
            {
                _cameraView = new CameraView
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0)
                };
                _cameraView.MediaCaptured += OnMediaCaptured;
                _cameraView.MediaCaptureFailed += OnMediaCaptureFailed;
                CameraContainer.Content = _cameraView;
            }

            CameraContainer.IsVisible = true;
            BtnTomarFoto.IsVisible = true;
            BtnFlashButton.IsVisible = true;
            StatusFlashToIcons();
        });
    }

    private void MostrarOverlayPermiso(string titulo, string mensaje, bool puedeReintentar)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LblPermissionTitle.Text = titulo;
            LblPermissionMessage.Text = mensaje;

            BtnPedirPermiso.IsVisible = puedeReintentar;
            BtnGoToSettings.IsVisible = !puedeReintentar;

            CameraContainer.IsVisible = false;
            BtnTomarFoto.IsVisible = false;
            BtnFlashButton.IsVisible = false;
            PermissionDeniedOverlay.IsVisible = true;
        });
    }

    // HANDLERS DEL OVERLAY

    private async void OnPedirPermisoClicked(object sender, EventArgs e)
    {
        await EvaluarYMostrarEstadoPermisoAsync();
    }

    private void OnGoToSettingsClicked(object sender, EventArgs e)
    {
        AppInfo.ShowSettingsUI();
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        // null = el usuario canceló sin tomar foto.
        OnPhotoCallback?.Invoke(null);
        await Shell.Current.GoToAsync("..");
    }

    // C�MARA

    private async Task SeleccionarCamaraAsync()
    {
        if (_cameraView == null) return;

        try
        {
            var cameras = await _cameraView.GetAvailableCameras(CancellationToken.None);
            var rear = cameras.FirstOrDefault(c => c.Position == CameraPosition.Rear)
                       ?? cameras.FirstOrDefault();

            if (rear != null)
                MainThread.BeginInvokeOnMainThread(() => _cameraView.SelectedCamera = rear);
            else
                MostrarOverlayPermiso("C�mara no disponible",
                    "Este dispositivo no tiene c�mara disponible.",
                    puedeReintentar: false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SeleccionarCamaraAsync error: {ex.Message}");
        }
    }

    private async void OnTomarFotoClicked(object sender, EventArgs e)
    {
        if (_isCapturingImage || _cameraView == null) return;

        _isCapturingImage = true;
        DynamicLayout.IsEnabled = false;

        try
        {
            _captureCancellationTokenSource?.Dispose();
            _captureCancellationTokenSource = new CancellationTokenSource();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _cameraView.CaptureImage(_captureCancellationTokenSource.Token);
            });
        }
        catch (OperationCanceledException) { }
        catch (InvalidCastException)
        {
            // CameraView no disponible en este dispositivo/simulador
            MostrarOverlayPermiso(
                "C�mara no disponible",
                "Este dispositivo no soporta la c�mara integrada.",
                puedeReintentar: false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnTomarFotoClicked error: {ex}");
        }
        finally
        {
            _isCapturingImage = false;
            DynamicLayout.IsEnabled = true;
        }
    }

    private async void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
    {
        // 1) Materializamos el Stream a un archivo temporal en CacheDirectory,
        //    fuera del UI thread. Así el stream del toolkit nace y muere en su
        //    hilo natural y nunca cruza páginas.
        string? tempPath = null;

        if (e.Media != null)
        {
            tempPath = Path.Combine(
                FileSystem.CacheDirectory,
                $"photo_{Guid.NewGuid():N}.jpg");

            try
            {
                using var fs = File.Create(tempPath);
                await e.Media.CopyToAsync(fs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnMediaCaptured: error guardando temp: {ex.Message}");

                // Si quedó un archivo a medio escribir, lo limpiamos.
                try { if (File.Exists(tempPath)) File.Delete(tempPath); }
                catch { /* ignorado */ }

                tempPath = null;
            }
        }

        // 2) Recién ahora saltamos al UI thread para invocar el callback y
        //    navegar atrás. La página consumidora se ocupa de borrar el
        //    archivo cuando lo termine de usar.
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (_cameraView?.IsAvailable == true)
            {
                OnPhotoCallback?.Invoke(tempPath);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error del dispositivo", "El dispositivo no est� activo", "OK");
            }
        });
    }

    private void OnMediaCaptureFailed(object sender, MediaCaptureFailedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _isCapturingImage = false;
            DynamicLayout.IsEnabled = true;
            _captureCancellationTokenSource?.Dispose();
            _captureCancellationTokenSource = null;
        });
    }

    // FLASH Y ORIENTACI�N

    private void OnActiveFlashClicked(object sender, EventArgs e)
    {
        if (_cameraView == null) return;

        _cameraView.CameraFlashMode = _cameraView.CameraFlashMode switch
        {
            CameraFlashMode.Off => CameraFlashMode.On,
            CameraFlashMode.On => CameraFlashMode.Auto,
            _ => CameraFlashMode.Off
        };
        StatusFlashToIcons();
    }

    public void StatusFlashToIcons()
    {
        if (_cameraView == null) return;

        FlashIcon = _cameraView.CameraFlashMode switch
        {
            CameraFlashMode.Off => "flash_off",
            CameraFlashMode.On => "flash_on",
            CameraFlashMode.Auto => "flash_auto",
            _ => "flash_off"
        };
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
            if (DynamicLayout == null) return;

            DynamicLayout.BatchBegin();
            DynamicLayout.RowDefinitions.Clear();
            DynamicLayout.ColumnDefinitions.Clear();

            if (orientation == DisplayOrientation.Landscape)
            {
                DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                Grid.SetRow(BtnFlashButton, 0); Grid.SetColumn(BtnFlashButton, 2);
                Grid.SetRow(CameraContainer, 0); Grid.SetColumn(CameraContainer, 1);
                Grid.SetRow(BtnTomarFoto, 0); Grid.SetColumn(BtnTomarFoto, 0);
                Grid.SetRow(PermissionDeniedOverlay, 0); Grid.SetColumn(PermissionDeniedOverlay, 1);
            }
            else
            {
                DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                DynamicLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                DynamicLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                Grid.SetRow(BtnFlashButton, 0); Grid.SetColumn(BtnFlashButton, 0);
                Grid.SetRow(CameraContainer, 1); Grid.SetColumn(CameraContainer, 0);
                Grid.SetRow(BtnTomarFoto, 2); Grid.SetColumn(BtnTomarFoto, 0);
                Grid.SetRow(PermissionDeniedOverlay, 1); Grid.SetColumn(PermissionDeniedOverlay, 0);
            }

            DynamicLayout.BatchCommit();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateLayoutOrientation error: {ex.Message}");
        }
    }
}