using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Ejemplo_Maui_Hibrida.Behaviors;
using Ejemplo_Maui_Hibrida.Pages;
using Ejemplo_Maui_Hibrida.Services;
using Ejemplo_Maui_Hibrida.UrlCommands;
using Ejemplo_Maui_Hibrida.UrlCommands.Handlers;
using Ejemplo_Maui_Hibrida.ViewModels;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Networking;

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
            .UseMauiCommunityToolkitCore()
            .UseMauiCommunityToolkitCamera()
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
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton<NetworkService>();
        builder.Services.AddSingleton<CallService>();

        builder.Services.AddSingleton<IWebViewBridge, WebViewBridge>();
        builder.Services.AddSingleton<IImageService, ImageDeviceAutoRotateService>();
        builder.Services.AddTransient<MyMediaPickerPage>();

        builder.Services.AddSingleton<GpsOverlayViewModel>();
        builder.Services.AddSingleton<NetworkOverlayViewModel>();
        builder.Services.AddSingleton<CallOverlayViewModel>();

        // Comandos de URL: el orden de registro = orden de evaluación.
        builder.Services.AddSingleton<IUrlCommandHandler, GpsCommandHandler>();
        builder.Services.AddSingleton<IUrlCommandHandler, CallCommandHandler>();
        builder.Services.AddSingleton<IUrlCommandHandler, CameraCommandHandler>();
        builder.Services.AddSingleton<UrlCommandDispatcher>();

        builder.Services.AddSingleton<MainViewModel>();

        builder.Services.AddSingleton<MainPage>();

        return builder;
    }

}

