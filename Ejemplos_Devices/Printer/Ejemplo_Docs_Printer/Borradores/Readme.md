


## 

- Escaneo de dispositivos Bluetooth
 - Conexión/desconexión
 - Impresión de texto con formato (tamaños, negrita, alineación)
 - Tickets de venta completos
 - Códigos QR
 - Códigos de barras (EAN13, CODE128, etc.)
 - Ejemplos de tickets de restaurante, facturas, eventos, turnos y más

Uso rápido:

  Instala los paquetes NuGet: ESCPOS_NET y Plugin.BLE
  Copia los archivos a tu proyecto MAUI
  Empareja tu impresora en Android
  Ejecuta la app y selecciona tu impresora

¡Comienza a imprimir!
La solución está optimizada para impresoras ESC/POS de 58mm como la que mostraste en la imagen.




# Impresora Térmica Bluetooth - Protocolo ESC/POS

## Descripción General

La solución está optimizada para impresoras ESC/POS de **58mm**, utilizando la librería **ESCPOS_NET**
para comunicación Bluetooth con dispositivos Android.

## Protocolo ESC/POS

### ¿Qué es ESC/POS?

ESC/POS (Escape/Point of Sale) es un protocolo estándar de comunicación para impresoras térmicas 
desarrollado por **Epson**. Define un conjunto de comandos de control que permiten:

- Controlar la alineación del texto
- Definir estilos (negrita, subrayado, fuente)
- Ajustar el tamaño de fuente
- Imprimir códigos de barras y códigos QR
- Controlar el avance de papel
- Cortar papel

### Características de la solución

| Aspecto   | Detalles |
|--------|----------|
| **Ancho de papel** | 58mm (384 puntos DPI) |
| **Conexión** | Bluetooth (SPP - Serial Port Profile) |
| **Protocolo** | ESC/POS (Epson) |
| **Codificación** | PC858 (Latin-9 Euro) |
| **Plataforma** | .NET MAUI para Android |
| **Librería** | ESCPOS_NET v2.2.1 |

---

## Ejemplos de Uso de la Librería ESCPOS_NET

### 1. Inicializar la Conexión Bluetooth

```csharp
// Inyectar el servicio
private readonly IThermalPrinterService _printerService;

// Obtener dispositivos emparejados
var devices = await _printerService.ScanDevicesAsync();

// Conectar con un dispositivo específico
bool success = await _printerService.ConnectAsync(deviceAddress);

if (success)
{
    StatusLabel.Text = "Conectado";
    StatusLabel.TextColor = Colors.Green;
}
```

### 2. Imprimir Texto Simple

```csharp
// Texto centrado, tamaño normal
await _printerService.PrintTextAsync(
    text: "BIENVENIDO",
    fontSize: 1,
    bold: false,
    centered: true
);

// Texto grande y negrita
await _printerService.PrintTextAsync(
    text: "TÍTULO IMPORTANTE",
    fontSize: 2,
    bold: true,
    centered: true
);
```

**Parámetros:**
- `text`: El texto a imprimir
- `fontSize`: Tamaño de fuente (1-8, donde 1 es normal)
- `bold`: Aplicar negrita
- `centered`: Centrar el texto

---

### 3. Imprimir un Ticket Completo

```csharp
var items = new List<(string name, int qty, decimal price)>
{
    ("Café Americano", 2, 3.50m),
    ("Croissant", 1, 4.50m),
    ("Agua Mineral", 3, 1.00m)
};

decimal subtotal = 15.50m;
decimal tax = 2.48m;
decimal total = 17.98m;

await _printerService.PrintReceiptAsync(
    storeName: "MI CAFETERÍA",
    items: items,
    subtotal: subtotal,
    tax: tax,
    total: total,
    footer: "¡Gracias por tu compra!"
);
```

**Características:**
- Encabezado con nombre del negocio en negrita
- Fecha y hora automática
- Lista de artículos con cantidad, precio unitario y total
- Cálculo automático de totales
- Pie de página personalizado
- Avance automático de papel y corte

---

### 4. Imprimir Código de Barras

```csharp
await _printerService.PrintBarcodeAsync(
    data: "1234567890128",
    barcodeType: BarcodeType.CODE128
);
```

**Tipos de códigos soportados:**
```csharp
public enum BarcodeType
{
    UPC_A,      // Código Universal de Producto A
    UPC_E,      // Código Universal de Producto E
    EAN13,      // European Article Number (13 dígitos)
    EAN8,       // European Article Number (8 dígitos)
    CODE39,     // Alfanumérico
    ITF,        // Interleaved 2 of 5
    CODABAR,    // Numérico
    CODE93,     // Comprimido
    CODE128     // Máxima densidad (recomendado)
}
```

---

### 5. Imprimir Código QR

```csharp
await _printerService.PrintQRCodeAsync(
    data: "https://ejemplo.com/products/12345",
    size: 8
);
```

**Parámetros:**
- `data`: Datos a codificar (URL, texto, número de producto)
- `size`: Tamaño del QR (1-16, 8 es recomendado)

---

### 6. Controlar el Papel

```csharp
// Avanzar 5 líneas
await _printerService.FeedLinesAsync(lines: 5);

// Cortar el papel (si la impresora tiene cortador automático)
await _printerService.CutPaperAsync();
```

---

### 7. Desconectar

```csharp
await _printerService.DisconnectAsync();
```

---

## Flujo Completo de Ejemplo

```csharp
public async Task ImprimirTicketCompleto()
{
    try
    {
        // 1. Escanear dispositivos
        var devices = await _printerService.ScanDevicesAsync();
        
        // 2. Conectar con el primero disponible
        if (devices.Any())
        {
            await _printerService.ConnectAsync(devices[0].Address);
        }

        // 3. Imprimir encabezado
        await _printerService.PrintTextAsync(
            "RECIBO DE VENTA",
            fontSize: 2,
            bold: true,
            centered: true
        );

        await _printerService.PrintTextAsync("", centered: true);

        // 4. Imprimir fecha
        await _printerService.PrintTextAsync(
            $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
            centered: false
        );

        // 5. Imprimir código QR
        await _printerService.PrintQRCodeAsync(
            $"ID:{Guid.NewGuid()}",
            size: 8
        );

        // 6. Avanzar papel
        await _printerService.FeedLinesAsync(3);

        // 7. Cortar papel
        await _printerService.CutPaperAsync();

        // 8. Desconectar
        await _printerService.DisconnectAsync();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
}
```

---

## Arquitectura de la Solución

### Servicios Implementados

#### IThermalPrinterService (Interfaz)
```csharp
public interface IThermalPrinterService
{
    bool IsConnected { get; }
    Task<List<BluetoothDevice>> ScanDevicesAsync();
    Task<bool> ConnectAsync(string deviceAddress);
    Task DisconnectAsync();
    Task PrintTextAsync(string text, int fontSize = 1, bool bold = false, bool centered = false);
    Task PrintReceiptAsync(string storeName, List<(string, int, decimal)> items, decimal subtotal, decimal tax, decimal total, string footer = null);
    Task PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.CODE128);
    Task PrintQRCodeAsync(string data, int size = 8);
    Task PrintImageAsync(string imagePath);
    Task FeedLinesAsync(int lines = 3);
    Task CutPaperAsync();
}
```

#### ThermalPrinterService (Implementación)
- Gestiona la conexión Bluetooth
- Encapsula comandos ESC/POS
- Maneja excepciones de conexión
- Ejecuta comandos de forma asincrónica

---

## Requisitos y Configuración

### Dependencias NuGet

```xml
<PackageReference Include="ESCPOS_NET" Version="2.2.1" />
<PackageReference Include="Microsoft.Maui.Controls" Version="10.0.20" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.0" />
```

### Permisos Android (AndroidManifest.xml)

```xml
<!-- Bluetooth -->
<uses-permission android:name="android.permission.BLUETOOTH" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
<uses-permission android:name="android.permission.BLUETOOTH_SCAN" />
<uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />

<!-- Ubicación (requerida para escaneo BLE) -->
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

### Solicitud de Permisos en Tiempo de Ejecución

```csharp
// Para Android 12+ (API 31+)
if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
{
    var activity = Platform.CurrentActivity;
    
    string[] permissions = new[]
    {
        Manifest.Permission.BluetoothScan,
        Manifest.Permission.BluetoothConnect
    };
    
    ActivityCompat.RequestPermissions(activity, permissions, 1);
}
```

---

## Notas Técnicas

### Comandos ESC/POS Utilizados

| Comando | Función |
|---------|---------|
| `ESC @` | Inicializar impresora |
| `ESC a` | Alineación (izquierda, centro, derecha) |
| `ESC E` | Negrita on/off |
| `ESC !` | Estilo de fuente |
| `GS w` | Ancho de carácter |
| `GS h` | Alto de carácter |
| `GS H` | Posición de impresor de código de barras |
| `GS k` | Imprimir código de barras |
| `GS ( k` | Imprimir código QR |

### Ancho de Papel

- **58mm** = 384 puntos (DPI estándar de 203 DPI)
- Aproximadamente **32 caracteres** por línea (fuente normal)
- Aproximadamente **16 caracteres** por línea (fuente grande)

### Codificación de Caracteres

La solución utiliza **PC858 (Latin-9 Euro)** para soportar:
- Acentos (á, é, í, ó, ú)
- Símbolos especiales (€, ñ, ç)
- Caracteres latinos extendidos

---

## Troubleshooting

### Problema: Dispositivo no se conecta

**Causas:**
- Dispositivo no emparejado
- Bluetooth desactivado
- UUID incorrecto

**Solución:**
```csharp
// Verificar que el dispositivo está emparejado
var devices = await _printerService.ScanDevicesAsync();
if (!devices.Any(d => d.IsPaired))
{
    // Emparejar manualmente desde configuración de Android
}
```

### Problema: Texto mal alineado o cortado

**Solución:**
```csharp
// Usar TruncateString para textos largos
private string TruncateString(string text, int maxLength)
{
    if (string.IsNullOrEmpty(text))
        return string.Empty;
    return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
}
```

### Problema: Códigos de barras/QR no imprimen

**Nota:** La versión actual de ESCPOS_NET v2.2.1 tiene limitaciones con ciertos tipos de códigos. Se recomienda:
- Usar CODE128 para códigos de barras
- Probar con datos pequeños (< 200 caracteres)
- Verificar que el formato sea válido para el tipo de código

---

## Referencias

- **ESCPOS_NET**: https://github.com/lukevp/ESCPOS_NET
- **Protocolo ESC/POS**: https://www.epson.com/cgi-bin/Store/support/supPortType.jsp
- **.NET MAUI**: https://learn.microsoft.com/es-es/dotnet/maui/
- **Bluetooth SPP**: https://www.bluetooth.com/specifications/specs/serial-port-profile-1-2/

---

## Autor y Licencia

Proyecto de ejemplos desarrollado para **UTN - Facultad Regional del Paraná**

Versión: 1.0 | Actualizado: 2025
