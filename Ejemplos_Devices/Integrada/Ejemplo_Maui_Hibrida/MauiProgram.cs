using CommunityToolkit.Maui;

using Ejemplo_Maui_Hibrida.Services;
using Ejemplo_Maui_Hibrida.ViewModels;

using Microsoft.Extensions.Logging;

namespace Ejemplo_Maui_Hibrida;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            //
            .UseMauiCommunityToolkit()
            //
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
    static MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<GpsService>();

        builder.Services.AddSingleton<GpsOverlayViewModel>();
        builder.Services.AddSingleton<MainViewModel>();

        builder.Services.AddSingleton<MainPage>();

        return builder;
    }

}

