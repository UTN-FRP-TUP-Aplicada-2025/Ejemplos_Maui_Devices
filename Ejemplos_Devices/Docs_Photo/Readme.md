
# Introducción.

hay dos ejemplos:

1- Utilizando MediaPicker, usa el dialogo nativo del dispositivo
2- Mediante la libreria CommunityToolKit.Maui se recrea un dialogo. Esto es así porque supe tener problemas
con el dialogo nativo en algunas versiones de android, entonces cree un dialogo minimalista
para ver y tomar la foto.

# Dialogo nativo - MediaPicker

[Media Picker - Microsoft](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device-media/picker?view=net-maui-10.0&tabs=android)


```
<!-- Needed for Picking photo/video -->
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" android:maxSdkVersion="32" />
<uses-permission android:name="android.permission.READ_MEDIA_AUDIO" />
<uses-permission android:name="android.permission.READ_MEDIA_IMAGES" />
<uses-permission android:name="android.permission.READ_MEDIA_VIDEO" />

<!-- Needed for Taking photo/video -->
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" android:maxSdkVersion="32" />

<!-- Add these properties if you would like to filter out devices that do not have cameras, or set to false to make them optional -->
<uses-feature android:name="android.hardware.camera" android:required="true" />
<uses-feature android:name="android.hardware.camera.autofocus" android:required="true" />
```


# Utilizando  CommunityToolKit.Maui.Camera


## Referencias
[CommunityToolKit.Maui.Camera - Microsoft](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/camera-view?tabs=windows)


## Install Nugget
```
dotnet add package CommunityToolkit.Maui.Core
dotnet add package CommunityToolkit.Maui.Camera 
```

## .NET MAUI Community Toolkit - `MauiProgram.cs` 

```csharp
using CommunityToolkit.Maui.Core;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			// Initialize the .NET MAUI Community Toolkit by adding the below line of code
			.UseMauiCommunityToolkitCore()
			// After initializing the .NET MAUI Community Toolkit, optionally add additional fonts
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Continue initializing your .NET MAUI App here

		return builder.Build();
	}
}
```

## Manifiesto

Agregar
```xml
<uses-permission android:name="android.permission.CAMERA" />
```