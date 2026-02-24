
dotnet add package BarcodeScanner.Mobile.Maui --version 9.0.1




CommunityToolkit.Maui.Camera 5.0.0

Microsoft.Maui.Controls 10.0.020

```csharp
using CommunityToolkit.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			// Initialize the .NET MAUI Community Toolkit CameraView by adding the below line of code
			.UseMauiCommunityToolkitCamera()
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


## XAML usage

In order to make use of the toolkit within XAML you can use this namespace:

xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"

## Further information

For more information please visit:

- Our documentation site: https://docs.microsoft.com/dotnet/communitytoolkit/maui

- Our GitHub repository: https://github.com/CommunityToolkit/Maui


## Ejemplos
https://github.com/JimmyPun610/BarcodeScanner.Mobile/tree/master/SampleApp.Maui


## Manifiesto

```xml
<key>NSCameraUsageDescription</key>
<string>Require to use camera</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>This app needs access to photos.</string>
<key>NSPhotoLibraryAddUsageDescription</key>
<string>This app needs access to the photo gallery.</string>
```

## temas de compilacion

uso para cuando se tiene varias librerías de Google (como Firebase + MLKit) que traen componentes duplicados
```
	<PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
		<NoWarn>$(NoWarn);MT5209;MT5212</NoWarn>
		<MtouchExtraArgs>--ignore-native-library-conflict</MtouchExtraArgs>
	</PropertyGroup>
```
o


se usa para cuando estás compilando específicamente para el Simulador en una Mac con procesador
Apple Silicon (iossimulator-arm64). Es el estándar para solucionar problemas de librerías nativas 
(como MLKit) que no fueron compiladas correctamente para los nuevos simuladores ARM de Apple.
```
	<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) AND '$(RuntimeIdentifier)' == 'iossimulator-arm64'">
		<MtouchExtraArgs>$(MtouchExtraArgs) --setenv:DYLD_LIBRARY_PATH=/usr/lib/system/introspection</MtouchExtraArgs>
		<NoWarn>$(NoWarn);MT5209;MT5212;MT5213</NoWarn>
	</PropertyGroup>
```


### Compilar para dispositivo real (Recomendado para CI/CD)

Si objetivo en el pipeline de GitHub/Azure es solo verificar que todo compile o 
generar un binario para Testing (TestFlight/AppCenter), no usar el simulador.
Usa el identificador de hardware real:

```xmml
dotnet build "..." \
    -c Release \
    -f net10.0-ios \
    -p:RuntimeIdentifier=ios-arm64 \
    -p:BuildIpa=true \
    -p:EnableCodeSigning=false
```

## Forzar al compilador a ignorar el error (para el simular)

Si se necesita que el build pase para el simulador sí o sí, puedes 
intentar añadir este bloque al archivo .csproj:

```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-ios')) AND '$(RuntimeIdentifier)' == 'iossimulator-arm64'">
    <MtouchExtraArgs>$(MtouchExtraArgs) --setenv:DYLD_LIBRARY_PATH=/usr/lib/system/introspection</MtouchExtraArgs>
    <NoWarn>$(NoWarn);MT5209;MT5212;MT5213</NoWarn>
</PropertyGroup>
```
