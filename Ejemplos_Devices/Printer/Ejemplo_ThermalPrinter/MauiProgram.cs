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
        // Registrar el servicio de impresión térmica
        builder.Services.AddSingleton<IThermalPrinterService, ThermalPrinterService>();

        // Registrar la página principal
        builder.Services.AddTransient<MainPage>();

        return builder;

    }
}
