namespace Ejemplo_Photo_MiMediaPicker_Callback.Pages;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }

    async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
    {
        BtnPhoto.IsEnabled = false;

        try
        {
            Action<Image> resultadoCallback = async (image) =>
            {
                await this.Dispatcher.DispatchAsync(new Action(async () =>
                {
                    if (image != null) ImgPhoto.Source = image.Source;
                }));
            };

            var pageParams = new ShellNavigationQueryParameters
            {
                { "OnPhotoCallback", resultadoCallback }
            };

            await Shell.Current.GoToAsync(nameof(MyMediaPickerPage), pageParams);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
        finally
        {
            BtnPhoto.IsEnabled = true;
        }
    }

    private async Task<PermissionStatus> RequestCameraPermissionAsync()
    {
        // Verifica el estado actual sin mostrar el diálogo todavía
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (status == PermissionStatus.Granted)
            return status;

        // Si fue denegado permanentemente, no se puede solicitar de nuevo
        //if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        //{
        //    // En iOS, una vez denegado hay que ir a Settings
        //    bool irASettings = await DisplayAlertAsync(
        //        "Permiso denegado",
        //        "El acceso a la cámara fue denegado. ¿Querés habilitarlo en Ajustes?",
        //        "Ir a Ajustes",
        //        "Cancelar");

        //    if (irASettings)
        //        AppInfo.ShowSettingsUI();

        //    return PermissionStatus.Denied;
        //}

        // Solicitar el permiso (muestra el diálogo nativo del SO)
        status = await Permissions.RequestAsync<Permissions.Camera>();
        return status;
    }
}
