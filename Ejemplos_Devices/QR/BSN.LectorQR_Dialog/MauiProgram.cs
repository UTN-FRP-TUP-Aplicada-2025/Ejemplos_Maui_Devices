using BarcodeScanning;
using Microsoft.Extensions.Logging;

namespace BSN.LectorQR_Dialog;
  
public static class MauiProgram 
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            //
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
            })
        #region QR 
            .UseBarcodeScanning();
        #endregion

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
