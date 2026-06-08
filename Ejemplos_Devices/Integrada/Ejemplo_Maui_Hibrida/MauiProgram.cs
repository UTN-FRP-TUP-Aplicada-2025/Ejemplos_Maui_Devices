using BarcodeScanner.Mobile;

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

using MotorDsl.Bluetooth;
using MotorDsl.Core.Models;
using MotorDsl.Extensions;
using MotorDsl.Maui;

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
            })
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddBarcodeScannerHandler();
            })
        #region printer
            .Services.AddMotorDslEngine()
            .AddProfiles(p =>
            {
                p.Add(new DeviceProfile("thermal_58mm", 32, "escpos-bitmap"));
                p.Add(new DeviceProfile("a4-pdf", 80, "pdf"));
                p.Add(new DeviceProfile("pdf", 48, "pdf"));
            })
            .AddMotorDslMaui()
            // Transport Bluetooth (Android Classic SPP)
            .Services.AddBluetoothPrinterTransport();
        #endregion

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
    static MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        #region devices services
        builder.Services.AddSingleton<GpsService>();
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton<NetworkService>();
        builder.Services.AddSingleton<CallService>();
        #endregion

        #region pages
        builder.Services.AddSingleton<IWebViewBridge, WebViewBridge>();
        builder.Services.AddSingleton<IImageService, ImageDeviceAutoRotateService>();
        builder.Services.AddTransient<MyMediaPickerPage>();
        builder.Services.AddTransient<MyMediaSelfiePickerPage>();
        #endregion

        #region viewmodels
        builder.Services.AddSingleton<GpsOverlayViewModel>();
        builder.Services.AddSingleton<NetworkOverlayViewModel>();
        builder.Services.AddSingleton<CallOverlayViewModel>();
        #endregion

        #region handlers
        // Comandos de URL: el orden de registro = orden de evaluación.
        builder.Services.AddSingleton<IUrlCommandHandler, GpsCommandHandler>();
        builder.Services.AddSingleton<IUrlCommandHandler, CallCommandHandler>();
        builder.Services.AddSingleton<IUrlCommandHandler, CameraCommandHandler>();
        builder.Services.AddSingleton<IUrlCommandHandler, SelfieCommandHandler>();
        builder.Services.AddSingleton<UrlCommandDispatcher>();
        #endregion

        #region main
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
        #endregion

        return builder;
    }

}

