

````csharp
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder        
            .UseMauiApp<App>()
            ...
            //
             .AddPrintServices()
            //
            ...
        #region motor dsl

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

        #endregion


    static MauiAppBuilder AddPrintServices(this MauiAppBuilder builder)
    {
        #region printer overlay
        builder.Services.AddSingleton<PrinterService>();
        builder.Services.AddSingleton<PrinterOverlayViewModel>();
        #endregion

        return builder;
    }
````