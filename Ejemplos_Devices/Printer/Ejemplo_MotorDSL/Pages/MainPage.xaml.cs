
using MotorDsl.Core.Contracts;
using MotorDsl.Core.Models;
using MotorDsl.Printing;
using Ejemplo_MotorDSL.Samples;


#if ANDROID
using Android;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
#endif

namespace Ejemplo_MotorDSL.Pages;

public partial class MainPage : ContentPage
{
    

    private readonly IDocumentEngine _engine;
    private readonly IThermalPrinterService _printer;

    public MainPage(IDocumentEngine engine, IThermalPrinterService printer)
    {
        InitializeComponent();
        _engine = engine;
        _printer = printer;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        StatusBadge.Service = _printer;
        DevicePicker.Service = _printer;
        DevicePicker.DeviceSelected += (_, device) => ShowMessage($"Conectado a {device.Name}.");
        DevicePicker.ScanError += (_, ex) => ShowMessage($"BT Error: {ex.Message}");

#if ANDROID
        var granted = await RequestBluetoothPermissions();
        if (granted)
            await DevicePicker.ScanAsync();
        else
            ShowMessage("Permisos BT denegados.");
#elif IOS
        await Task.CompletedTask;
#else
        await DevicePicker.ScanAsync();
#endif
    }

    // ─── Bluetooth Permissions (Android 12+) ───

#if ANDROID
    private async Task<bool> RequestBluetoothPermissions()
    {
        try
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                var activity = Platform.CurrentActivity!;
                string[] btPermissions = new[]
                {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect
                };

                bool allGranted = btPermissions.All(p =>
                    ContextCompat.CheckSelfPermission(activity, p) == (int)Android.Content.PM.Permission.Granted);

                if (!allGranted)
                {
                    ActivityCompat.RequestPermissions(activity, btPermissions, 1);
                    await Task.Delay(3000);

                    allGranted = btPermissions.All(p =>
                        ContextCompat.CheckSelfPermission(activity, p) == (int)Android.Content.PM.Permission.Granted);
                }

                if (!allGranted)
                {
                    ShowMessage("Permisos BT denegados. Aceptá los permisos y presioná Reescanear.");
                    return false;
                }
            }
            else
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    ShowMessage("Permisos de ubicación denegados (necesarios para BT en Android < 12).");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MULTA] BT Permissions error: {ex.Message}");
            ShowMessage($"Error permisos BT: {ex.Message}");
            return false;
        }
    }
#endif

    // ─── Helper ───

    private void ShowMessage(string msg)
    {
        MessageLabel.Text = $"{DateTime.Now:HH:mm:ss} — {msg}";
    }

    // ─── Imprimir ESC/POS ───

    private async void OnImprimirClicked(object? sender, EventArgs e)
    {
        MessageLabel.Text = "Iniciando impresión...";
        var doc = GetSelectedDocument();
        if (doc == null) return;

        try
        {
            // ── 1. Render SIEMPRE primero (diagnóstico independiente de impresora) ──
            ShowMessage("Generando ESC/POS...");
            var profile = new DeviceProfile("58HB6", 32, "escpos-bitmap");
            profile.SetCapability("supports_bitmap", true);
            profile.SetCapability("bitmap_max_width_px", 320);
            profile.SetCapability("bitmap_binarization_threshold", 128);
            var result = _engine.Render(doc, profile);

            if (!result.IsSuccessful || result.Output is not byte[] bytes)
            {
                var firstErr = result.Errors.FirstOrDefault() ?? "sin errores";
                var snippet = firstErr.Length > 200 ? firstErr[..200] : firstErr;
                MessageLabel.Text = $"{DateTime.Now:HH:mm:ss} RENDER FALLÓ:\n{snippet}";
                System.Console.WriteLine($"[MULTA-RENDER-ERROR] {firstErr}");
                foreach (var w in result.Warnings)
                    System.Console.WriteLine($"[MULTA-WARN] {w}");
                return;
            }

            ShowMessage($"Render OK — {bytes.Length} bytes");
            System.Console.WriteLine($"[MULTA] Render OK: {bytes.Length} bytes");

            // ── 2. Verificar impresora antes de enviar ──
            if (!_printer.IsConnected)
            {
                ShowMessage($"Render OK ({bytes.Length} bytes) pero no hay impresora conectada.");
                return;
            }

            await Task.Delay(500);
            await _printer.SendBytesAsync(bytes);
            ShowMessage($"Impreso OK — {bytes.Length} bytes enviados.");
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException;
            var msg = $"TIPO: {ex.GetType().Name}\n" +
                      $"MSG: {ex.Message}\n" +
                      $"INNER: {inner?.GetType().Name}: {inner?.Message}\n" +
                      $"INNER2: {inner?.InnerException?.Message}\n" +
                      $"STACK: {ex.StackTrace?.Replace("\n", " | ")?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}";
            MessageLabel.Text = $"{DateTime.Now:HH:mm:ss} — {msg}";
            System.Console.WriteLine($"[MULTA-ERROR] {msg}");
        }
    }

    private string? GetSelectedDocument()
    {
        return MultaIntegratedDsl.Document;
    }
}
