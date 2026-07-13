
#  Guía de uso rápido

## Propósito

Diálogo de captura de imágenes y video con la cámara del dispositivo, usando la librería CommunityToolkit.Maui.Camera.


## Dependencias NuGet

```
  <PackageReference Include="CommunityToolkit.Maui.Camera" Version="6.0.0" />
  <PackageReference Include="CommunityToolkit.Maui.Core" Version="14.0.0" />
```

## Otras dependencias

Ver:
```
  # `/LibApp\Devices\Images\README.md`
```

## Registro de servicios en el Program

MauiProgram.cs
```csharp

public static class MauiProgram 
{
  public static MauiApp CreateMauiApp()
  {
    var builder = MauiApp.CreateBuilder();
    builder.UseMauiApp<App>()
    //
            .UseMauiCommunityToolkitCore()
            .UseMauiCommunityToolkitCamera()
    //
            //dependencia con los servicio de imagenes
    //
            ...
    }

   ...

```

## Ejemplo de uso en una página

```csharp

    async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
    {
        BtnPhoto.IsEnabled = false;

        try
        {
            Action<string?> resultadoCallback = async (path) =>
            {
                string? outPath = null;
                try
                {
                    if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

                    outPath = await _imageService.ProcesarPhotoAsync(path);

                    byte[] bytes = File.ReadAllBytes(outPath ?? "");

                    //encola la acción para que se ejecute en el UI thread y así evitar problemas de acceso a la UI desde un thread de background.          
                    //Dispatcher.Dispatch no crea un thread; sirve precisamente para volver al UI thread desde uno secundario.
                    Dispatcher.Dispatch(() =>
                    {
                        ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al tomar la foto: {ex.Message}");
                }
                finally
                {
                    try
                    {
                        if (path != null) File.Delete(path);
                        if (outPath != null) File.Delete(outPath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"No se pudo borrar el temporal: {ex.Message}");
                    }
                }
            };

            var pageParams = new ShellNavigationQueryParameters{ { "OnPhotoCallback", resultadoCallback }};

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
```


## Registrar las páginas (Dialogos de captura de imágenes)

App.xaml.cs
```
public partial class AppShell : Shell
{
    public AppShell()
    {
        ...

        Routing.RegisterRoute(nameof(MyMediaPickerPage), typeof(MyMediaPickerPage));
        Routing.RegisterRoute(nameof(MyMediaSelfiePickerPage), typeof(MyMediaSelfiePickerPage));

        ...
```

## Manifiestos

AndroidManifest.xml
```
<uses-permission android:name="android.permission.CAMERA" />
	<uses-permission android:name="android.permission.RECORD_AUDIO" />

	<queries>
		<intent>
			<action android:name="android.media.action.IMAGE_CAPTURE" />
		</intent>
	</queries>
```

Info.plist
```
	<key>NSCameraUsageDescription</key>
	<string>Require to use camera</string>
	<key>NSPhotoLibraryUsageDescription</key>
	<string>This app needs access to photos.</string>
	<key>NSPhotoLibraryAddUsageDescription</key>
	<string>This app needs access to the photo gallery.</string>
```