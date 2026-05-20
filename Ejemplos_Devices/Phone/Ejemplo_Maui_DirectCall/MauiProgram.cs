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
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
            });
         
#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static public MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        // Servicios de dispositivo
        builder.Services.AddTransient<PhoneDialerDevice>();

        // Coordinador y overlay: singleton para que el estado del overlay
        // sea compartido entre todos los callers (botón, otros servicios, etc.).
        builder.Services.AddSingleton<CallCoordinator>();

        // ViewModel de la página principal: transient para no arrastrar estado
        // entre navegaciones.
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder;
    }
}
