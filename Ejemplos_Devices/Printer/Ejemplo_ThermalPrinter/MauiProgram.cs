using Ejemplo_ThermalPrinter.Pages;
using Ejemplo_ThermalPrinter.Services;
using Microsoft.Extensions.Logging;

namespace Ejemplo_ThermalPrinter;

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
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public static MauiAppBuilder AddsServices(this MauiAppBuilder builder)
    {

        builder.Services.AddSingleton<IThermalPrinterService, ThermalPrinterService>();

        builder.Services.AddTransient<MainPage>();

        return builder;

    }
}
