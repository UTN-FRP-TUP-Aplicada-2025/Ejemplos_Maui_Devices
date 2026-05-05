using Ejemplo_Maui_DirectCall.Pages;
using Ejemplo_Maui_DirectCall.Services;
using Ejemplo_Maui_DirectCall.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ejemplo_Maui_DirectCall;

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
            });
         
#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static public MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<PhoneDialerDevice>();

        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder;
    }
}
