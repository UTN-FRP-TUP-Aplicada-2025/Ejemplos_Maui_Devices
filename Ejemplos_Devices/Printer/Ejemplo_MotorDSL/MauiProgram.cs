using Ejemplo_MotorDSL.Pages;
using Ejemplo_MotorDSL.Samples;
using Microsoft.Extensions.Logging;

using MotorDsl.Bluetooth;
using MotorDsl.Core.Models;
using MotorDsl.Extensions;
using MotorDsl.Maui;

namespace Ejemplo_MotorDSL;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .AddsServices()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
            });

        // Motor DSL: core pipeline + templates + profiles + renderers MAUI (PDF, ESC/POS bitmap, SkiaSharp).
        // El template registrado es un JSON integrado: ya tiene todos los valores resueltos.
        builder.Services.AddMotorDslEngine()
            //.AddTemplates(t =>
            //{
            //    t.Add("acta-infraccion-integrada", MultaIntegratedDsl.Document);
            //})
            .AddProfiles(p =>
            {
                p.Add(new DeviceProfile("thermal_58mm", 32, "escpos-bitmap"));
            })
            .AddMotorDslMaui();

        // Transport Bluetooth (Android Classic SPP)
        builder.Services.AddBluetoothPrinterTransport();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public static MauiAppBuilder AddsServices(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<MainPage>();
        return builder;
    }
}
