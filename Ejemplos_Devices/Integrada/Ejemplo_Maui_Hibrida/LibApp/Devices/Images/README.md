
#  Guía de uso rápido

## Propósito

Normalización de imágenes y rotación automática de imágenes según la orientación del dispositivo.

## Dependencias NuGet

```
 <PackageReference Include="MetadataExtractor" Version="2.9.0" />
		<PackageReference Include="SkiaSharp" Version="3.119.1" />
```

## Registro de servicios en el Program

```csharp

public static class MauiProgram 
{
  public static MauiApp CreateMauiApp()
  {
    var builder = MauiApp.CreateBuilder();
    builder.UseMauiApp<App>()
    ...
    //
            .AddServices()
    //
            ...
    }

   ...

    public static MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        //add services
        builder.Services.AddSingleton<IImageService, ImageDeviceAutoRotateService>();

        return builder;
    }

```