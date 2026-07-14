

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
```

AndroidManifest.xml
```xml
    ...

	<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
	<uses-permission android:name="android.permission.INTERNET" />

	<!-- Bluetooth (legacy API < 31) -->
	<uses-permission android:name="android.permission.BLUETOOTH" />
	<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
	<!-- Bluetooth (API 31+ / Android 12+) -->
	<uses-permission android:name="android.permission.BLUETOOTH_SCAN"
					 android:usesPermissionFlags="neverForLocation" />
	<uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />

	<!-- Ubicación (necesario para escaneo Bluetooth) -->
	<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
	<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />

	<queries>
		<intent>
			<action android:name="android.bluetooth.adapter.action.REQUEST_ENABLE" />
		</intent>
	</queries>
```