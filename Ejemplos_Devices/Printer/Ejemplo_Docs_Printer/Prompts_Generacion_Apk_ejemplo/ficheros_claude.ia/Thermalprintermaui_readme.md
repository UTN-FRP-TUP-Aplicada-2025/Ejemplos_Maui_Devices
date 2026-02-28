# Impresión Térmica Bluetooth en .NET MAUI

## Solución para Impresora Térmica Portátil 58mm

Esta implementación está diseñada para trabajar con impresoras térmicas ESC/POS de 58mm como la mostrada en tu imagen.

## Características
- ✅ Soporte Android
- ✅ Conexión Bluetooth
- ✅ Comandos ESC/POS
- ✅ Impresión de texto, códigos de barras y códigos QR
- ✅ Ajuste de fuentes y alineación

## Paquetes NuGet Requeridos

```xml
<PackageReference Include="ESCPOS_NET" Version="4.5.0" />
<PackageReference Include="Plugin.BLE" Version="3.1.0" />
```

## Permisos Android

### AndroidManifest.xml
```xml
<uses-permission android:name="android.permission.BLUETOOTH" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
<uses-permission android:name="android.permission.BLUETOOTH_SCAN" />
<uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />

<queries>
  <intent>
    <action android:name="android.bluetooth.adapter.action.REQUEST_ENABLE" />
  </intent>
</queries>
```

### Versión Target SDK
Asegúrate de tener en tu archivo `.csproj`:
```xml
<TargetFramework>net8.0-android34.0</TargetFramework>
```

## Archivos Incluidos

1. **IThermalPrinterService.cs** - Interface del servicio
2. **ThermalPrinterService.cs** - Implementación del servicio
3. **BluetoothPrinterHelper.cs** - Helper para conexión Bluetooth Android
4. **MainPage.xaml** - Interfaz de usuario de ejemplo
5. **MainPage.xaml.cs** - Código behind con ejemplos de uso

## Uso Básico

### 1. Registrar el servicio en MauiProgram.cs
```csharp
builder.Services.AddSingleton<IThermalPrinterService, ThermalPrinterService>();
```

### 2. Inyectar en tu página
```csharp
private readonly IThermalPrinterService _printerService;

public MainPage(IThermalPrinterService printerService)
{
    InitializeComponent();
    _printerService = printerService;
}
```

### 3. Escanear dispositivos
```csharp
var devices = await _printerService.ScanDevicesAsync();
```

### 4. Conectar
```csharp
await _printerService.ConnectAsync(deviceId);
```

### 5. Imprimir
```csharp
await _printerService.PrintTextAsync("Hola Mundo!");
```

## Ejemplos de Impresión

### Texto Simple
```csharp
await _printerService.PrintTextAsync("Mi tienda", fontSize: 2, bold: true, centered: true);
```

### Ticket de Venta
```csharp
await _printerService.PrintReceiptAsync(
    storeName: "Mi Negocio",
    items: new List<(string name, int qty, decimal price)>
    {
        ("Producto 1", 2, 15.50m),
        ("Producto 2", 1, 25.00m)
    },
    subtotal: 56.00m,
    tax: 8.96m,
    total: 64.96m
);
```

### Código de Barras
```csharp
await _printerService.PrintBarcodeAsync("123456789012", BarcodeType.EAN13);
```

### Código QR
```csharp
await _printerService.PrintQRCodeAsync("https://mitienda.com");
```

## Notas Importantes

1. **Permisos en Runtime**: Solicita permisos Bluetooth antes de usarlos
2. **Compatibilidad**: Funciona con impresoras que soporten comandos ESC/POS
3. **Ancho de papel**: Esta implementación está optimizada para 58mm (384 puntos)
4. **Android Only**: Por ahora solo implementado para Android

## Solución de Problemas

### No encuentra dispositivos
- Verifica que Bluetooth esté activado
- Confirma los permisos en configuración de Android
- Asegúrate que la impresora esté emparejada

### No imprime
- Verifica que la impresora tenga papel
- Confirma que la batería esté cargada
- Intenta emparejar nuevamente el dispositivo

### Caracteres extraños
- Verifica la codificación (por defecto es PC858_EURO)
- Algunas impresoras usan diferentes code pages

## Recursos Adicionales

- [ESCPOS_NET Documentation](https://github.com/lukevp/ESC-POS-.NET)
- [Plugin.BLE Documentation](https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le)