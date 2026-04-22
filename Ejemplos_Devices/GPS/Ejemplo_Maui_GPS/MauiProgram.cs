using Ejemplo_Maui_GPS.Services;
using Microsoft.Extensions.Logging;

namespace Ejemplo_Maui_GPS;
 
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .AddServices()
            .ConfigureFonts(fonts =>
            { 
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static public MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        // Servicios
        builder.Services.AddSingleton<LocationPermissionService>();
        builder.Services.AddSingleton<GpsService>();

        // ViewModels
        builder.Services.AddTransient<ViewModels.MainPageViewModel>();

        // Pages
        builder.Services.AddTransient<Pages.MainPage>();

        return builder;
    }
}
