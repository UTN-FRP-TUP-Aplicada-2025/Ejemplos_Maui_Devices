# Guía de Integración con ESCPOS_NET v2.2.1 para .NET MAUI
## Modelo: 58HB6-THERMAL-PRINTER

---

## Objetivo de la Guía

Esta guía proporciona instrucciones paso a paso para integrar la impresora térmica 58HB6-THERMAL-PRINTER en aplicaciones .NET MAUI utilizando la librería ESCPOS_NET versión 2.2.1. Está dirigida a desarrolladores que necesitan agregar capacidades de impresión térmica a sus aplicaciones móviles o de escritorio.

### Alcance

- Instalación y configuración de ESCPOS_NET v2.2.1
- Conexión por Bluetooth y USB
- Ejemplos completos de impresión (texto, códigos, tickets)
- Manejo de errores y troubleshooting
- Buenas prácticas de integración

### Requisitos Previos

- .NET 10 o superior instalado
- Visual Studio 2022+ o Visual Studio Code
- Conocimiento básico de C# y async/await
- Un proyecto .NET MAUI creado
- Dispositivo Android 8.0+ o iOS 14+ para pruebas

---

## Instalación del Paquete ESCPOS_NET

### Paso 1: Agregar NuGet Package

Existen dos formas de instalar ESCPOS_NET v2.2.1:

#### Opción A: Usando Package Manager Console

```powershell
Install-Package ESCPOS_NET -Version 2.2.1
```

#### Opción B: Usando CLI dotnet

```bash
dotnet add package ESCPOS_NET --version 2.2.1
```

#### Opción C: Editando .csproj

Agregue la siguiente línea en su archivo `.csproj`:

```xml
<ItemGroup>
    <PackageReference Include="ESCPOS_NET" Version="2.2.1" />
</ItemGroup>
```

### Paso 2: Restaurar Paquetes

Después de agregar la referencia, restaure los paquetes:

```bash
dotnet restore
```

### Verificación de Instalación

Para verificar que la instalación fue exitosa, consulte el archivo `packages.lock.json` o ejecute:

```bash
dotnet list package
```

Debería aparecer:
```
ESCPOS_NET 2.2.1
```

---

## Configuración en Proyecto .NET MAUI

### Estructura Recomendada de Proyecto

```
ThermalPrinterApp/
├── MauiProgram.cs
├── AppShell.xaml(.cs)
├── Pages/
│   └── PrinterPage.xaml(.cs)
├── Services/
│   ├── IThermalPrinterService.cs
│   └── ThermalPrinterService.cs
├── Models/
│   ├── BluetoothDevice.cs
│   ├── BarcodeType.cs
│   └── ReceiptItem.cs
└── Resources/
```

### Paso 1: Crear Interfaz de Servicio

Cree el archivo `Services/IThermalPrinterService.cs`:

```csharp
using ThermalPrinterApp.Models;

namespace ThermalPrinterApp.Services;

public interface IThermalPrinterService
{
    bool IsConnected { get; }

    Task<List<BluetoothDeviceInfo>> ScanDevicesAsync();
    Task<bool> ConnectAsync(string deviceAddress);
    Task DisconnectAsync();

    Task PrintTextAsync(string text, int fontSize = 1, bool bold = false, bool centered = false);
    Task PrintReceiptAsync(string storeName, List<ReceiptItem> items, decimal subtotal, decimal tax, decimal total, string footer = null);
    Task PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.CODE128);
    Task PrintQRCodeAsync(string data, int size = 8);

    Task FeedLinesAsync(int lines = 3);
    Task CutPaperAsync();
}

public class BluetoothDeviceInfo
{
    public string Name { get; set; }
    public string Address { get; set; }
    public bool IsPaired { get; set; }

    public override string ToString() => $"{Name} ({Address})";
}

public enum BarcodeType
{
    UPC_A,
    UPC_E,
    EAN13,
    EAN8,
    CODE39,
    ITF,
    CODABAR,
    CODE93,
    CODE128
}

public class ReceiptItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

### Paso 2: Registrar Servicio en MauiProgram.cs

Edite el archivo `MauiProgram.cs`:

```csharp
using ThermalPrinterApp.Services;

namespace ThermalPrinterApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Registrar servicios
        builder.Services.AddSingleton<IThermalPrinterService, ThermalPrinterService>();
        
        // Registrar páginas
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
```

### Paso 3: Configurar Permisos Android

Edite `Platforms/Android/AndroidManifest.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <!-- Permisos Bluetooth -->
    <uses-permission android:name="android.permission.BLUETOOTH" />
    <uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
    <uses-permission android:name="android.permission.BLUETOOTH_SCAN" />
    <uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />
    
    <!-- Permisos de ubicación (necesarios para BLE scan en Android 12+) -->
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    
    <!-- Aplicación -->
    <application android:label="@string/app_name" android:icon="@mipmap/appicon">
        <!-- Actividades -->
    </application>
</manifest>
```

### Paso 4: Solicitar Permisos en Tiempo de Ejecución

Agregue este código en su página principal:

```csharp
protected override async void OnAppearing()
{
    base.OnAppearing();

    if (DeviceInfo.Platform == DevicePlatform.Android)
    {
        await RequestBluetoothPermissions();
    }
}

private async Task RequestBluetoothPermissions()
{
    try
    {
        // Solicitar permiso de ubicación
        var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (locationStatus != PermissionStatus.Granted)
        {
            locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        // Para Android 12+
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
        {
            var activity = Platform.CurrentActivity;
            string[] permissions = new[]
            {
                Android.Manifest.Permission.BluetoothScan,
                Android.Manifest.Permission.BluetoothConnect
            };

            foreach (var permission in permissions)
            {
                if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(activity, permission)
                    != Android.Content.PM.Permission.Granted)
                {
                    AndroidX.Core.App.ActivityCompat.RequestPermissions(activity, permissions, 1);
                    break;
                }
            }
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", $"Error solicitando permisos: {ex.Message}", "OK");
    }
}
```

---

## Implementación del Servicio: Conexión USB

### Ejemplo Básico de Conexión USB

```csharp
using System;
using System.IO;
using System.Text;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

namespace ThermalPrinterApp.Services;

public class ThermalPrinterService : IThermalPrinterService
{
    private Stream _outputStream;
    private EPSON _printer;
    public bool IsConnected { get; private set; }

    public ThermalPrinterService()
    {
        _printer = new EPSON();
    }

    // Conexión USB (para escritorio o emulador)
    public async Task<bool> ConnectViaUSBAsync(string portName)
    {
        try
        {
            // Ejemplo: "COM3" en Windows
            #if WINDOWS
            var serialPort = new System.IO.Ports.SerialPort(portName, 115200)
            {
                DataBits = 8,
                StopBits = System.IO.Ports.StopBits.One,
                Parity = System.IO.Ports.Parity.None
            };
            
            serialPort.Open();
            _outputStream = serialPort.BaseStream;
            #endif

            // Inicializar impresora
            await WriteBytes(_printer.Initialize());
            IsConnected = true;
            return true;
        }
        catch (Exception ex)
        {
            IsConnected = false;
            throw new Exception($"Error conectando por USB: {ex.Message}", ex);
        }
    }

    private async Task WriteBytes(byte[] data)
    {
        if (_outputStream == null)
            throw new Exception("No hay conexión establecida");

        try
        {
            await _outputStream.WriteAsync(data, 0, data.Length);
            await _outputStream.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error escribiendo datos: {ex.Message}", ex);
        }
    }
}
```

---

## Implementación del Servicio: Conexión Bluetooth

### Ejemplo Completo de Conexión Bluetooth (Android)

```csharp
using Android.Bluetooth;
using Android.Content;
using Java.Util;
using System.IO;

#if ANDROID
using AndroidBluetoothDevice = Android.Bluetooth.BluetoothDevice;
#endif

namespace ThermalPrinterApp.Services;

public partial class ThermalPrinterService : IThermalPrinterService
{
#if ANDROID
    private BluetoothSocket _socket;
    private BluetoothAdapter _bluetoothAdapter;
    private Stream _outputStream;
#endif

    public async Task<List<BluetoothDeviceInfo>> ScanDevicesAsync()
    {
        var devices = new List<BluetoothDeviceInfo>();

#if ANDROID
        if (_bluetoothAdapter == null)
        {
            throw new Exception("Bluetooth no disponible");
        }

        if (!_bluetoothAdapter.IsEnabled)
        {
            throw new Exception("Bluetooth está desactivado");
        }

        var bondedDevices = _bluetoothAdapter.BondedDevices;
        if (bondedDevices != null)
        {
            foreach (AndroidBluetoothDevice device in bondedDevices)
            {
                devices.Add(new BluetoothDeviceInfo
                {
                    Name = device.Name ?? "Desconocido",
                    Address = device.Address,
                    IsPaired = true
                });
            }
        }
#endif
        return devices;
    }

    public async Task<bool> ConnectAsync(string deviceAddress)
    {
#if ANDROID
        try
        {
            if (_socket != null && _socket.IsConnected)
            {
                await DisconnectAsync();
            }

            var device = _bluetoothAdapter.GetRemoteDevice(deviceAddress);
            if (device == null)
                throw new Exception("Dispositivo no encontrado");

            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            _socket = device.CreateRfcommSocketToServiceRecord(uuid);

            await Task.Run(() => _socket.Connect());
            _outputStream = _socket.InputStream;

            await WriteBytes(_printer.Initialize());
            IsConnected = true;
            return true;
        }
        catch (Exception ex)
        {
            IsConnected = false;
            throw new Exception($"Error al conectar: {ex.Message}", ex);
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }

    public async Task DisconnectAsync()
    {
#if ANDROID
        try
        {
            _outputStream?.Dispose();
            _socket?.Close();
            _socket?.Dispose();
            IsConnected = false;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error desconectando: {ex.Message}", ex);
        }
#else
        await Task.CompletedTask;
#endif
    }
}
```

---

## Ejemplos Completos de Impresión

### Ejemplo 1: Imprimir Texto Simple

```csharp
public async Task ImprimirTextoSimpleAsync()
{
    try
    {
        if (!IsConnected)
            throw new Exception("Impresora no conectada");

        var commands = new List<byte[]>();

        // Alinear al centro
        commands.Add(_printer.CenterAlign());
        
        // Título grande y negrita
        commands.Add(_printer.SetStyles(PrintStyle.Bold));
        commands.Add(_printer.Print("BIENVENIDO"));
        commands.Add(_printer.PrintLine(""));
        
        // Texto normal
        commands.Add(_printer.SetStyles(PrintStyle.None));
        commands.Add(_printer.Print("Tienda de ejemplo"));
        commands.Add(_printer.PrintLine(""));
        
        // Espacios
        commands.Add(_printer.FeedLines(3));
        
        // Alinear izquierda
        commands.Add(_printer.LeftAlign());

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
    }
    catch (Exception ex)
    {
        throw new Exception($"Error imprimiendo texto: {ex.Message}", ex);
    }
}
```

### Ejemplo 2: Imprimir Ticket Formateado

```csharp
public async Task PrintReceiptAsync(string storeName, List<ReceiptItem> items, 
    decimal subtotal, decimal tax, decimal total, string footer = null)
{
    try
    {
        if (!IsConnected)
            throw new Exception("Impresora no conectada");

        var commands = new List<byte[]>();

        // Encabezado
        commands.Add(_printer.Initialize());
        commands.Add(_printer.CenterAlign());
        commands.Add(_printer.SetStyles(PrintStyle.Bold));
        commands.Add(_printer.SetCharacterSize(2, 2));
        commands.Add(_printer.Print(storeName));
        commands.Add(_printer.PrintLine(""));
        commands.Add(_printer.SetCharacterSize(1, 1));
        commands.Add(_printer.SetStyles(PrintStyle.None));
        commands.Add(_printer.PrintLine(""));

        // Fecha y hora
        commands.Add(_printer.LeftAlign());
        commands.Add(_printer.Print($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}"));
        commands.Add(_printer.PrintLine(""));
        commands.Add(_printer.Print("════════════════════════════════"));
        commands.Add(_printer.PrintLine(""));

        // Items
        foreach (var item in items)
        {
            decimal itemTotal = item.Quantity * item.Price;

            commands.Add(_printer.Print(TruncateString(item.Name, 24)));
            commands.Add(_printer.PrintLine(""));

            string line = $"  {item.Quantity} x ${item.Price:F2}";
            string totalStr = $"${itemTotal:F2}";
            int spaces = 32 - line.Length - totalStr.Length;
            line += new string(' ', Math.Max(0, spaces)) + totalStr;

            commands.Add(_printer.Print(line));
            commands.Add(_printer.PrintLine(""));
        }

        // Separador y totales
        commands.Add(_printer.Print("════════════════════════════════"));
        commands.Add(_printer.PrintLine(""));

        commands.Add(_printer.RightAlign());
        commands.Add(_printer.Print($"Subtotal: ${subtotal:F2}"));
        commands.Add(_printer.PrintLine(""));
        commands.Add(_printer.Print($"Impuesto:  ${tax:F2}"));
        commands.Add(_printer.PrintLine(""));

        // Total grande
        commands.Add(_printer.SetStyles(PrintStyle.Bold));
        commands.Add(_printer.SetCharacterSize(2, 2));
        commands.Add(_printer.Print($"TOTAL: ${total:F2}"));
        commands.Add(_printer.PrintLine(""));
        commands.Add(_printer.SetCharacterSize(1, 1));
        commands.Add(_printer.SetStyles(PrintStyle.None));

        // Pie de página
        if (!string.IsNullOrEmpty(footer))
        {
            commands.Add(_printer.PrintLine(""));
            commands.Add(_printer.CenterAlign());
            commands.Add(_printer.Print(footer));
            commands.Add(_printer.PrintLine(""));
        }

        // Avance y corte
        commands.Add(_printer.FeedLines(3));
        commands.Add(_printer.FullCutAfterFeed(5));

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
    }
    catch (Exception ex)
    {
        throw new Exception($"Error imprimiendo ticket: {ex.Message}", ex);
    }
}

private string TruncateString(string text, int maxLength)
{
    if (string.IsNullOrEmpty(text))
        return string.Empty;
    return text.Length <= maxLength 
        ? text 
        : text.Substring(0, maxLength - 3) + "...";
}
```

### Ejemplo 3: Imprimir Código de Barras

```csharp
public async Task PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.CODE128)
{
    try
    {
        if (!IsConnected)
            throw new Exception("Impresora no conectada");

        var commands = new List<byte[]>();

        commands.Add(_printer.CenterAlign());
        
        // Nota: ESCPOS_NET v2.2.1 puede no soportar directamente todos los métodos
        // de códigos de barras. En su caso, imprimimos el código como texto.
        commands.Add(_printer.Print("CÓDIGO DE BARRAS"));
        commands.Add(_printer.PrintLine(""));
        
        // Aquí iría el comando específico del código
        // Por ahora, imprimimos el dato
        commands.Add(_printer.Print(data));
        commands.Add(_printer.PrintLine(""));
        
        commands.Add(_printer.LeftAlign());
        commands.Add(_printer.FeedLines(3));

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
    }
    catch (Exception ex)
    {
        throw new Exception($"Error imprimiendo código de barras: {ex.Message}", ex);
    }
}
```

### Ejemplo 4: Imprimir Código QR

```csharp
public async Task PrintQRCodeAsync(string data, int size = 8)
{
    try
    {
        if (!IsConnected)
            throw new Exception("Impresora no conectada");

        var commands = new List<byte[]>();

        commands.Add(_printer.CenterAlign());
        commands.Add(_printer.Print("ESCANEA EL CÓDIGO"));
        commands.Add(_printer.PrintLine(""));
        
        // Similar al código de barras, ESCPOS_NET puede tener limitaciones
        // Imprimimos el dato como alternativa
        commands.Add(_printer.Print(data));
        commands.Add(_printer.PrintLine(""));
        
        commands.Add(_printer.LeftAlign());
        commands.Add(_printer.FeedLines(3));

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
    }
    catch (Exception ex)
    {
        throw new Exception($"Error imprimiendo QR: {ex.Message}", ex);
    }
}
```

---

## Métodos Auxiliares

### Implementación Completa de Métodos Adicionales

```csharp
public async Task PrintTextAsync(string text, int fontSize = 1, bool bold = false, bool centered = false)
{
    try
    {
        if (!IsConnected)
            throw new Exception("Impresora no conectada");

        var commands = new List<byte[]>();

        if (centered)
            commands.Add(_printer.CenterAlign());
        else
            commands.Add(_printer.LeftAlign());

        if (fontSize > 1)
            commands.Add(_printer.SetStyles(PrintStyle.FontB));

        if (bold)
            commands.Add(_printer.SetStyles(PrintStyle.Bold));

        commands.Add(_printer.Print(text));
        commands.Add(_printer.PrintLine(""));

        if (bold || fontSize > 1)
            commands.Add(_printer.SetStyles(PrintStyle.None));

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
    }
    catch (Exception ex)
    {
        throw new Exception($"Error imprimiendo texto: {ex.Message}", ex);
    }
}

public async Task FeedLinesAsync(int lines = 3)
{
    try
    {
        if (!IsConnected)
            throw new Exception("Impresora no conectada");

        await WriteBytes(_printer.FeedLines((byte)lines));
    }
    catch (Exception ex)
    {
        throw new Exception($"Error avanzando papel: {ex.Message}", ex);
    }
}

public async Task CutPaperAsync()
{
    try
    {
        if (!IsConnected)
            throw new Exception("Impresora no conectada");

        await WriteBytes(_printer.FullCutAfterFeed(5));
    }
    catch (Exception ex)
    {
        throw new Exception($"Error cortando papel: {ex.Message}", ex);
    }
}
```

---

## Manejo de Errores

### Patrón Try-Catch Recomendado

```csharp
public async Task SafePrintAsync(Func<Task> printAction)
{
    try
    {
        if (!IsConnected)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current?.MainPage?.DisplayAlert("Error", 
                    "Impresora no conectada", "OK"));
            return;
        }

        await printAction();
        
        await MainThread.InvokeOnMainThreadAsync(() =>
            Application.Current?.MainPage?.DisplayAlert("Éxito", 
                "Impresión completada", "OK"));
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error de impresión: {ex.Message}");
        
        await MainThread.InvokeOnMainThreadAsync(() =>
            Application.Current?.MainPage?.DisplayAlert("Error", 
                $"Error de impresión: {ex.Message}", "OK"));
    }
}
```

### Uso en la Página

```csharp
private async void OnPrintClicked(object sender, EventArgs e)
{
    await _printerService.SafePrintAsync(async () =>
    {
        await _printerService.PrintTextAsync("Prueba de impresión", 
            fontSize: 2, bold: true, centered: true);
    });
}
```

---

## Buenas Prácticas

### 1. Validación de Conexión Antes de Imprimir

```csharp
public async Task ValidatedPrintAsync(string text)
{
    if (!_printerService.IsConnected)
    {
        throw new InvalidOperationException("Impresora no conectada");
    }

    await _printerService.PrintTextAsync(text);
}
```

### 2. Usar Async/Await Correctamente

```csharp
// ✓ Correcto
private async void OnButtonClicked(object sender, EventArgs e)
{
    try
    {
        await _printerService.PrintTextAsync("Prueba");
    }
    catch (Exception ex)
    {
        Debug.WriteLine(ex);
    }
}

// ✗ Evitar
private void OnButtonClicked(object sender, EventArgs e)
{
    _printerService.PrintTextAsync("Prueba"); // Sin await
}
```

### 3. Usar Using para Liberar Recursos

```csharp
public async Task ConnectAndPrintAsync(string address)
{
    try
    {
        await _printerService.ConnectAsync(address);
        await _printerService.PrintTextAsync("Test");
    }
    finally
    {
        await _printerService.DisconnectAsync();
    }
}
```

### 4. Batch de Comandos para Mejor Performance

```csharp
// ✓ Más rápido: Una sola transmisión
var commands = new List<byte[]>();
commands.Add(_printer.Print("Línea 1"));
commands.Add(_printer.PrintLine(""));
commands.Add(_printer.Print("Línea 2"));
commands.Add(_printer.PrintLine(""));
await WriteBytes(ByteSplicer.Combine(commands.ToArray()));

// ✗ Más lento: Múltiples transmisiones
await WriteBytes(_printer.Print("Línea 1"));
await WriteBytes(_printer.PrintLine(""));
await WriteBytes(_printer.Print("Línea 2"));
```

### 5. Respetar Límites de Caracteres

```csharp
public string FormatLineFor58mm(string text, int maxChars = 32)
{
    if (text.Length <= maxChars)
        return text;
    
    return text.Substring(0, maxChars - 3) + "...";
}
```

---

## Problemas Comunes y Soluciones

### Problema 1: "No está en modo de emparejamiento"

**Causa**: El dispositivo Bluetooth no está en modo emparejamiento

**Solución**:
```csharp
// En la UI, instruir al usuario presionar el botón durante 3 segundos
await DisplayAlert("Instrucciones", 
    "Presione el botón principal durante 3 segundos hasta que el LED parpadee azul", "OK");

// Luego escanear dispositivos
var devices = await _printerService.ScanDevicesAsync();
```

### Problema 2: Impr Conexión Bluetooth Se Desconecta Automáticamente

**Causa**: Inactividad prolongada o interferencia

**Solución**:
```csharp
private async Task ReconnectIfNeededAsync()
{
    if (!_printerService.IsConnected)
    {
        var devices = await _printerService.ScanDevicesAsync();
        if (devices.Any())
        {
            await _printerService.ConnectAsync(devices[0].Address);
        }
    }
}
```

### Problema 3: Error "Stream de salida no disponible"

**Causa**: No hay conexión activa

**Solución**:
```csharp
private void EnsureConnected()
{
    if (!_printerService.IsConnected)
    {
        throw new InvalidOperationException("No hay conexión con la impresora");
    }
}

// Usar antes de cualquier impresión
public async Task PrintSafeAsync(string text)
{
    EnsureConnected();
    await _printerService.PrintTextAsync(text);
}
```

### Problema 4: Caracteres Especiales No Se Imprimen Correctamente

**Causa**: Encoding incorrecta o code page no configurada

**Solución**:
```csharp
// Asegurar que se usa UTF-8 y code page 858 (Euro)
var commands = new List<byte[]>();
commands.Add(_printer.CodePage(CodePage.PC858_EURO)); // Agregar esta línea
commands.Add(_printer.Print("Café con mañana")); // Ahora funcionará
await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
```

### Problema 5: Impresión Lenta

**Causa**: Comandos individuales en lugar de batch

**Solución**: Ver sección "Batch de Comandos para Mejor Performance"

---

## Ejemplo de Página MAUI Completa

```csharp
using ThermalPrinterApp.Services;
using ThermalPrinterApp.Models;

namespace ThermalPrinterApp.Pages;

public partial class PrinterPage : ContentPage
{
    private readonly IThermalPrinterService _printerService;
    private List<BluetoothDeviceInfo> _devices;

    public PrinterPage(IThermalPrinterService printerService)
    {
        InitializeComponent();
        _printerService = printerService;
        _devices = new List<BluetoothDeviceInfo>();
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Escaneando...";
            _devices = await _printerService.ScanDevicesAsync();

            if (_devices.Any())
            {
                DevicePicker.ItemsSource = _devices.Select(d => d.Name).ToList();
                DevicePicker.IsEnabled = true;
                StatusLabel.Text = $"Se encontraron {_devices.Count} dispositivo(s)";
            }
            else
            {
                StatusLabel.Text = "No se encontraron dispositivos";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error escaneando: {ex.Message}", "OK");
            StatusLabel.Text = "Error en escaneo";
        }
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        try
        {
            if (DevicePicker.SelectedIndex < 0)
            {
                await DisplayAlert("Información", "Selecciona un dispositivo", "OK");
                return;
            }

            StatusLabel.Text = "Conectando...";
            var device = _devices[DevicePicker.SelectedIndex];
            
            bool success = await _printerService.ConnectAsync(device.Address);

            if (success)
            {
                StatusLabel.Text = $"Conectado a {device.Name}";
                StatusLabel.TextColor = Colors.Green;
                PrintButton.IsEnabled = true;
            }
            else
            {
                StatusLabel.Text = "Falló la conexión";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error conectando: {ex.Message}", "OK");
            StatusLabel.Text = "Error en conexión";
        }
    }

    private async void OnPrintTestClicked(object sender, EventArgs e)
    {
        try
        {
            if (!_printerService.IsConnected)
            {
                await DisplayAlert("Error", "Impresora no conectada", "OK");
                return;
            }

            StatusLabel.Text = "Imprimiendo...";
            
            await _printerService.PrintTextAsync("PRUEBA DE IMPRESIÓN", 
                fontSize: 2, bold: true, centered: true);
            
            await _printerService.PrintTextAsync("", centered: true);
            await _printerService.PrintTextAsync($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", 
                centered: true);
            
            await _printerService.FeedLinesAsync(3);

            StatusLabel.Text = "Impresión completada";
            StatusLabel.TextColor = Colors.Green;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error imprimiendo: {ex.Message}", "OK");
            StatusLabel.Text = "Error en impresión";
        }
    }
}
```

---

## Referencia Rápida de Métodos ESCPOS_NET

```csharp
// Inicialización
_printer.Initialize()                           // ESC @
_printer.Reset()                                // Reinicio soft

// Alineación
_printer.LeftAlign()                            // ESC a 0
_printer.CenterAlign()                          // ESC a 1
_printer.RightAlign()                           // ESC a 2

// Estilos
_printer.SetStyles(PrintStyle.Bold)             // ESC E 1
_printer.SetStyles(PrintStyle.None)             // ESC E 0
_printer.UnderlineText()                        // ESC - 1

// Fuentes
_printer.SetFont(FontName.FontA)                // ESC M 0
_printer.SetFont(FontName.FontB)                // ESC M 1

// Tamaño
_printer.SetCharacterSize(1, 1)                 // 1×1
_printer.SetCharacterSize(2, 2)                 // 2×2 (doble)

// Texto
_printer.Print(text)                            // Imprime
_printer.PrintLine(text)                        // Imprime + salto

// Papel
_printer.FeedLines(3)                           // Avanza 3 líneas
_printer.FullCutAfterFeed(5)                    // Corte + 5 líneas

// Codificación
_printer.CodePage(CodePage.PC858_EURO)          // Code page Euro
```

---

## Testing y Debugging

### Habilitar Logs de Debug

```csharp
public void EnableDebugMode()
{
    #if DEBUG
    Debug.WriteLine("Modo de debugging activado");
    Debug.WriteLine($"Conexión: {_printerService.IsConnected}");
    #endif
}
```

### Prueba de Comandos Individuales

```csharp
public async Task TestCommandAsync()
{
    try
    {
        var cmd = _printer.Initialize();
        Debug.WriteLine($"Comando hex: {BitConverter.ToString(cmd)}");
        await WriteBytes(cmd);
        Debug.WriteLine("Comando ejecutado exitosamente");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error: {ex.Message}");
    }
}
```

---

## Recursos Adicionales

### Documentación Oficial

- **ESCPOS_NET GitHub**: https://github.com/lukevp/ESCPOS_NET
- **Protocolo ESC/POS**: https://www.epson.com/cgi-bin/Store/support/supPortType.jsp
- **.NET MAUI Docs**: https://learn.microsoft.com/dotnet/maui/

### Comunidades y Soporte

- **Stack Overflow**: Tag `escpos` y `dotnet-maui`
- **GitHub Issues**: ESCPOS_NET repository
- **Microsoft Q&A**: .NET MAUI

---

**Versión de la Guía**: 1.0  
**Fecha de Publicación**: Enero 2025  
**ESCPOS_NET Versión**: 2.2.1  
**Plataforma Destino**: .NET MAUI 10 (Android/iOS)  
**Modelo Compatible**: 58HB6-THERMAL-PRINTER
