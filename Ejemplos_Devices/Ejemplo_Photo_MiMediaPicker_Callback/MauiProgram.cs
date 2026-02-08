using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

namespace Ejemplo_Photo_MiMediaPicker_Callback;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitCore()
            .UseMauiCommunityToolkitCamera()
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
}
