
## Referencias
https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/camera-view?tabs=windows


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