using Android.Net;
using Android.Telecom;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Ejemplo_MotorDSL_Dialog.Pages;
using Ejemplo_MotorDSL_Dialog.ViewModels;
using Microsoft.Extensions.Logging;
using MotorDsl.Bluetooth;
using MotorDsl.Core.Models;
using MotorDsl.Extensions;
using MotorDsl.Maui;

namespace Ejemplo_MotorDSL_Dialog
{
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
                //
                .AddServices()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
                });

            // Motor DSL: core pipeline + templates + profiles + renderers MAUI (PDF, ESC/POS bitmap, SkiaSharp).
            // El template registrado es un JSON integrado: ya tiene todos los valores resueltos.
            builder.Services.AddMotorDslEngine()
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

        static MauiAppBuilder AddServices(this MauiAppBuilder builder)
        {
            
            #region main
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();
            #endregion

            return builder;
        }
    }
}
