using BarcodeScanning;

using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;

using Microsoft.Extensions.Logging;

using LibApp.CustomWebView.Behaviors;
//
using LibApp.Devices.Camera.Pages;
using LibApp.Devices.GPS.Services;
using LibApp.Devices.GPS.ViewModels;
using LibApp.Devices.Images;
using LibApp.Devices.MotorDSL.Services;
using LibApp.Devices.MotorDSL.ViewModels;
using LibApp.Devices.Networks.Services;
using LibApp.Devices.Networks.ViewModels;
using LibApp.Devices.Phone.Services;
using LibApp.Devices.Phone.ViewModels;
using LibApp.Devices.Common.Services;
using LibApp.Devices.GPS;
//
using LibApp.UrlCommands;
using LibApp.UrlCommands.Handlers;

using MotorDsl.Bluetooth;
using MotorDsl.Core.Models;
using MotorDsl.Extensions;
using MotorDsl.Maui;

using Ejemplo_Maui_Hibrida.ViewModels;

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
            .AddPrintServices()
            .AddServices()
            //
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
            })
        #region QR 
            .UseBarcodeScanning()
        #endregion

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

    static MauiAppBuilder AddPrintServices(this MauiAppBuilder builder)
    {
        #region printer overlay
        builder.Services.AddSingleton<IPrinterService, PrinterService>();
        builder.Services.AddSingleton<PrinterOverlayViewModel>();
        #endregion

        return builder;
    }

    static MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        #region devices services
        builder.Services.AddSingleton<IGpsService, GpsService>();
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton<IUiDispatcher, MainThreadDispatcher>();
        builder.Services.AddSingleton<INetworkService, NetworkService>();
        builder.Services.AddSingleton<ICallService, CallService>();
        builder.Services.AddSingleton<ApiRelayService>();
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
        builder.Services.AddSingleton<IUrlCommandHandler, QrCommandHandler>();
        builder.Services.AddSingleton<IUrlCommandHandler, SendApiCommandHandler>();
        builder.Services.AddSingleton<IUrlCommandHandler, PrintCommandHandler>();        
        builder.Services.AddSingleton<UrlCommandDispatcher>();
        #endregion

        #region main
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
        #endregion

        return builder;
    }

}

