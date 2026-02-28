# Manual Técnico - Impresora Térmica Portátil Bluetooth 58mm
## Modelo: 58HB6-THERMAL-PRINTER

---

## Tabla de Contenidos

1. [Características Técnicas](#características-técnicas)
2. [Guía del Usuario](#guía-del-usuario)
3. [Interfaz de Control](#interfaz-de-control)
4. [Guía del Desarrollador](#guía-del-desarrollador)

---

# CARACTERÍSTICAS TÉCNICAS

## 1. Especificaciones del Dispositivo

### Información General
| Parámetro | Valor |
|-----------|-------|
| **Modelo** | 58HB6-THERMAL-PRINTER |
| **Tipo** | Impresora Térmica Portátil |
| **Ancho de Papel** | 58 mm |
| **Tecnología** | Impresión Térmica de Línea |
| **Peso** | Aproximadamente 450-500g |
| **Dimensiones** | 155 × 85 × 110 mm |
| **Color** | Negro |

### Especificaciones de Impresión
| Parámetro | Valor |
|-----------|-------|
| **Resolución** | 203 DPI (8 puntos/mm) |
| **Velocidad Máxima** | 150 mm/s |
| **Velocidad Estándar** | 100-120 mm/s |
| **Densidad de Impresión** | 8 puntos/mm (203 DPI) |
| **Ancho de Impresión** | 384 puntos / 48 mm |
| **Tipos de Fuentes** | Fuente A (12×24), Fuente B (9×17) |
| **Caracteres por Línea** | 32 caracteres (fuente normal) |
| **Modos de Impresión** | Texto, Códigos de Barras, Códigos QR |

### Sistema de Alimentación
| Aspecto | Especificación |
|--------|-----------------|
| **Tipo de Batería** | Li-ion Recargable 2000 mAh |
| **Voltaje Nominal** | 3.7 V DC |
| **Tiempo de Carga** | 2-3 horas (USB Type-C) |
| **Autonomía** | 8-10 horas (uso continuo) |
| **Tiempo en Espera** | Hasta 2 semanas |
| **Indicador de Batería** | LED (verde/rojo) |

### Conectividad
| Parámetro | Detalles |
|-----------|----------|
| **Bluetooth** | v5.0 / BLE (Bluetooth Low Energy) |
| **Rango de Alcance** | 10 metros |
| **Interfaz USB** | Type-C (carga y sincronización) |
| **Protocolo de Comunicación** | ESC/POS |
| **Velocidad de Transferencia** | 115200 bps (UART) |

### Sistema de Papel
| Especificación | Valor |
|---|---|
| **Ancho** | 58 mm |
| **Diámetro Máximo del Rollo** | 40 mm |
| **Tipo** | Papel térmico de 80 gsm |
| **Capacidad** | Hasta 30 metros de rollo |
| **Sensor de Papel** | Detector de Bajo Papel |

### Condiciones de Funcionamiento
| Parámetro | Rango |
|-----------|-------|
| **Temperatura de Operación** | 0°C a 45°C |
| **Temperatura de Almacenamiento** | -20°C a 60°C |
| **Humedad Relativa** | 10% a 90% (sin condensación) |
| **Altura de Operación** | Hasta 2,000 metros sobre el nivel del mar |

### Códigos y Formatos Soportados
- **Códigos de Barras**: UPC-A, UPC-E, EAN-13, EAN-8, CODE128, CODE39, ITF, CODABAR, CODE93
- **Códigos 2D**: QR Code (máx. versión 40)
- **Caracteres**: ASCII (32-127), Caracteres Extendidos (128-255)
- **Codificación**: UTF-8, GB2312, ISO-8859-1

### Características Adicionales
- ✅ Indicador LED de estado (verde/naranja/rojo)
- ✅ Botón de alimentación con múltiples funciones
- ✅ Detector automático de fin de papel
- ✅ Modo de bajo consumo automático
- ✅ Protección contra sobrecalentamiento
- ✅ Cabeza térmica resistente (1 millón de líneas)

---

# GUÍA DEL USUARIO

## 1. Instalación y Configuración Inicial

### Paso 1: Desempaque y Revisión
1. Abre el empaque cuidadosamente
2. Verifica que incluya:
   - Impresora térmica 58HB6
   - Cable USB Type-C
   - Rollo de papel térmico (muestra)
   - Manual de usuario
   - Adaptador de energía (opcional)

### Paso 2: Instalación del Papel
1. Levanta la cubierta frontal de la impresora
2. Inserta el rollo de papel:
   - El papel debe entrar por la parte superior
   - Asegúrate de que el papel esté alineado horizontalmente
3. Cierra la cubierta hasta escuchar un clic

### Paso 3: Carga Inicial
1. Conecta el cable USB Type-C al puerto trasero
2. Conecta el otro extremo a un adaptador de energía (5V/2A) o computadora
3. El LED parpadeará en naranja durante la carga
4. Cuando el LED sea verde sólido, la batería está completamente cargada (2-3 horas)

### Paso 4: Emparejamiento Bluetooth
1. Presiona y mantén el botón de encendido durante 3 segundos
   - El LED parpadeará azul rápidamente (modo de emparejamiento)
2. En tu dispositivo móvil/computadora:
   - Abre Configuración > Bluetooth
   - Busca "58HB6-THERMAL-PRINTER"
   - Selecciona y presiona "Emparejar"
3. El LED se volverá verde sólido cuando se empareje correctamente

---

## 2. Operaciones Básicas

### Encendido y Apagado
- **Encender**: Presiona el botón una vez (LED verde)
- **Apagar**: Presiona durante 3 segundos hasta que el LED se apague
- **Modo Espera**: Se activa automáticamente después de 5 minutos de inactividad

### Indicadores LED
| Color | Estado | Acción Recomendada |
|-------|--------|------------------|
| **Verde Sólido** | Listo/Conectado | Operación normal |
| **Verde Parpadeante** | En proceso de impresión | Esperar a que termine |
| **Naranja** | Cargando | Dejar conectado a energía |
| **Rojo Sólido** | Batería baja (<10%) | Cargar inmediatamente |
| **Rojo Parpadeante** | Error / Papel agotado | Ver sección de troubleshooting |
| **Azul Parpadeante** | Modo de emparejamiento | Emparejar dispositivo |

### Instalación de Papel
1. Presiona el botón de liberación en la parte superior derecha
2. Levanta la cubierta de acceso al papel
3. Inserta el rollo nueva de papel térmico (58 × 40 mm)
4. Cierra la cubierta completamente
5. Presiona el botón de avance para verificar que el papel se mueve correctamente

### Control de Volumen
- **Beep de confirmación**: Automático al conectarse
- **Beep de error**: Seis pitidos cortos si hay problema
- **Silenciador**: Mantén presionado el botón durante 5 segundos

---

## 3. Cuidado y Mantenimiento

### Limpieza Regular
- **Cabeza Térmica**: Limpia con un paño seco después de cada cambio de papel
- **Exterior**: Usa un paño húmedo (no mojado) para limpiar
- **Evita**: Líquidos, polvo excesivo, golpes

### Almacenamiento
- Guarda en lugar seco y fresco (15-25°C)
- Protege de luz solar directa
- Carga la batería cada 3 meses si no se usa

### Cambio de Batería
- **Vida útil esperada**: 2-3 años
- Si la batería no mantiene carga, contacta a servicio técnico
- No intentes reemplazar la batería por tu cuenta

### Reemplazo de Cabeza Térmica
- Vida útil: Aproximadamente 1-2 millones de líneas impresas
- Signo de desgaste: Impresión deficiente o líneas faltantes
- Reemplazo: Contactar a servicio autorizado

---

## 4. Solución de Problemas Básicos

### Problema: No enciende
**Causas posibles**: Batería descargada, falla de hardware
**Solución**:
1. Conecta a energía durante al menos 1 hora
2. Reinicia presionando el botón durante 10 segundos
3. Si no funciona, contacta a servicio técnico

### Problema: No se empareja Bluetooth
**Causas posibles**: Dispositivo no en modo de emparejamiento, interferencia
**Solución**:
1. Presiona el botón durante 3 segundos (debe estar azul parpadeante)
2. Aleja la impresora de otros dispositivos Bluetooth
3. Reinicia el Bluetooth en tu dispositivo
4. Intenta emparejar nuevamente

### Problema: Papel atascado
**Causas posibles**: Papel colocado incorrectamente, papel rasgado
**Solución**:
1. Apaga la impresora
2. Abre la cubierta de acceso
3. Retira cuidadosamente el papel atascado
4. Limpia la guía de papel
5. Inserta papel nuevo

### Problema: Impresión deficiente
**Causas posibles**: Cabeza térmica sucia, temperatura baja, papel de mala calidad
**Solución**:
1. Limpia la cabeza térmica con un paño seco
2. Verifica que la temperatura ambiental sea superior a 15°C
3. Usa papel térmico de calidad (80 gsm recomendado)
4. Ajusta la densidad de impresión en la aplicación

---

# INTERFAZ DE CONTROL

## 1. Interfaz Física

### Botón Principal (Multifunción)
```
┌─────────────────────────────────────────┐
│          PANEL DE CONTROL               │
├─────────────────────────────────────────┤
│  BOTÓN PRINCIPAL (Parte Superior)       │
│  ┌─────────────────────────────────┐   │
│  │         [POWER BUTTON]          │   │
│  └─────────────────────────────────┘   │
│                                         │
│  Acciones:                              │
│  • Presión única: Encender/Apagar     │
│  • 3 seg: Modo emparejamiento         │
│  • 5 seg: Silenciar/Activar sonido    │
│  • 10 seg: Reinicio completo          │
└─────────────────────────────────────────┘
```

### Puerto USB Type-C
- Ubicación: Panel trasero inferior
- Función: Carga de batería y transferencia de datos
- Especificación: 5V/2A mínimo recomendado

### Compartimiento de Papel
- Ubicación: Panel superior
- Captura: Presionar botón de liberación lateral
- Capacidad: Rollo de 58×40 mm

### Indicador LED
- Ubicación: Panel frontal (encima del botón)
- Estados: Verde, Naranja, Rojo, Azul (parpadeante o sólido)
- Función: Indica estado de dispositivo y batería

---

## 2. Interfaz de Software - Aplicación MAUI Android

### Pantalla Principal

```
╔════════════════════════════════════════╗
║  IMPRESORA TÉRMICA BLUETOOTH           ║
║                                        ║
║  ┌────────────────────────────────┐   ║
║  │ Estado: Desconectado 🔴       │   ║
║  │ Batería: 100% 🔋              │   ║
║  └────────────────────────────────┘   ║
║                                        ║
║  ╔════════════════════════════════╗   ║
║  ║ 1. BUSCAR DISPOSITIVOS         ║   ║
║  ║ [Escanear Dispositivos Bluetooth] ║   ║
║  ║ Dispositivos encontrados:      ║   ║
║  ║ └─ 58HB6-THERMAL-PRINTER       ║   ║
║  ╚════════════════════════════════╝   ║
║                                        ║
║  ╔════════════════════════════════╗   ║
║  ║ 2. CONECTAR                    ║   ║
║  ║ [Conectar] [Desconectar]       ║   ║
║  ╚════════════════════════════════╝   ║
╚════════════════════════════════════════╝
```

### Pantalla de Pruebas de Impresión

```
╔════════════════════════════════════════╗
║  3. PRUEBAS DE IMPRESIÓN               ║
║                                        ║
║  [Imprimir Texto de Prueba]            ║
║  [Imprimir Ticket de Ejemplo]          ║
║  [Imprimir Código QR]                  ║
║  [Imprimir Código de Barras]           ║
║                                        ║
║  ┌────────────────────────────────┐   ║
║  │ Mensaje: Esperando acción...   │   ║
║  └────────────────────────────────┘   ║
╚════════════════════════════════════════╝
```

### Pantalla de Texto Personalizado

```
╔════════════════════════════════════════╗
║  4. TEXTO PERSONALIZADO                ║
║                                        ║
║  ┌────────────────────────────────┐   ║
║  │ Ingresa el texto a imprimir    │   ║
║  │ [__________________________]    │   ║
║  └────────────────────────────────┘   ║
║                                        ║
║  Tamaño:  [Normal   ▼]                ║
║           [Grande (2x)]                ║
║           [Muy Grande (3x)]            ║
║                                        ║
║  ☐ Negrita  ☐ Centrado                ║
║                                        ║
║  [Imprimir Texto Personalizado]        ║
╚════════════════════════════════════════╝
```

---

## 3. Flujos de Interacción del Usuario

### Flujo 1: Primer Emparejamiento
```
Inicio
  │
  ├─→ Abrir App MAUI
  │
  ├─→ Pantalla: "Buscar Dispositivos"
  │
  ├─→ Presionar botón "Escanear Dispositivos Bluetooth"
  │
  ├─→ (Esperar 3-5 segundos)
  │
  ├─→ Aparece: "58HB6-THERMAL-PRINTER"
  │
  ├─→ Seleccionar dispositivo
  │
  ├─→ Pantalla: "Conectando..."
  │
  ├─→ LED en impresora cambia a verde
  │
  ├─→ Pantalla: "Conectado ✓"
  │
  └─→ Listo para usar
```

### Flujo 2: Impresión de Ticket
```
Usuario presiona "Imprimir Ticket"
  │
  ├─→ Validar conexión ✓
  │
  ├─→ Preparar datos del ticket
  │
  ├─→ Enviar comandos ESC/POS
  │
  ├─→ Impresora recibe datos
  │
  ├─→ LED parpadea verde (procesando)
  │
  ├─→ Comienza impresión
  │
  ├─→ (Tiempo: 3-5 segundos)
  │
  ├─→ Motores avanzan papel
  │
  ├─→ Corte automático
  │
  ├─→ LED vuelve a verde sólido
  │
  ├─→ Mostrar mensaje: "Impresión exitosa"
  │
  └─→ Listo para siguiente impresión
```

### Flujo 3: Manejo de Errores
```
Usuario intenta imprimir
  │
  ├─→ ¿Está conectado?
  │   NO → "Error: Dispositivo no conectado"
  │   SÍ → Continuar
  │
  ├─→ ¿Hay papel?
  │   NO → "Error: Papel agotado"
  │   SÍ → Continuar
  │
  ├─→ ¿Batería suficiente?
  │   NO → "Advertencia: Batería baja"
  │   SÍ → Continuar
  │
  ├─→ Procesar impresión
  │
  ├─→ ¿Éxito?
  │   SÍ → "Impresión completada"
  │   NO → "Error en impresión. Reintenta"
  │
  └─→ Fin
```

---

# GUÍA DEL DESARROLLADOR

## 1. Protocolo ESC/POS

### Introducción
El protocolo ESC/POS (Escape/Point of Sale) es un estándar de comunicación para impresoras térmicas desarrollado por Epson. La impresora 58HB6 utiliza este protocolo para recibir comandos de impresión.

### Estructura Básica de Comandos
```
[ESC][COMANDO][PARÁMETROS]
```

### Tabla de Comandos Principales

| Comando | Descripción | Sintaxis |
|---------|-------------|----------|
| **Inicializar** | Reinicia la impresora a valores por defecto | `ESC @ (1B 40)` |
| **Alinear** | Configura alineación del texto | `ESC a [0-2]` |
| **Seleccionar Fuente** | Cambia tipo de fuente | `ESC M [0-1]` |
| **Tamaño de Carácter** | Define ancho y alto | `GS ! [00-77]` |
| **Negrita** | Activa/desactiva negrita | `ESC E [0-1]` |
| **Subrayado** | Activa/desactiva subrayado | `ESC - [0-2]` |
| **Inversión** | Invierte colores (negro/blanco) | `GS B [0-1]` |
| **Avance de Papel** | Mueve el papel hacia adelante | `ESC J [00-FF]` |
| **Corte de Papel** | Corta el papel | `GS V [0-1]` |
| **Código de Barras** | Imprime código de barras | `GS k [parámetros]` |
| **Código QR** | Imprime código QR | `GS ( k [parámetros]` |

---

## 2. Librería ESCPOS_NET - Ejemplos Prácticos

### Instalación
```bash
dotnet add package ESCPOS_NET --version 2.2.1
```

### Inicialización Básica

#### Ejemplo 1: Conectar y Verificar Estado
```csharp
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Ejemplo_ThermalPrinter.Services;

// Inyectar el servicio
private readonly IThermalPrinterService _printerService;

// Conectar a la impresora
public async Task ConectarImpresora()
{
    try
    {
        // Obtener dispositivos Bluetooth disponibles
        var devices = await _printerService.ScanDevicesAsync();
        
        if (!devices.Any())
        {
            MessageBox.Show("No se encontraron dispositivos");
            return;
        }
        
        // Conectar con el primer dispositivo encontrado
        bool success = await _printerService.ConnectAsync(devices[0].Address);
        
        if (success)
        {
            MessageBox.Show("Conectado exitosamente");
            // Estado ahora es: _printerService.IsConnected = true
        }
        else
        {
            MessageBox.Show("Falló la conexión");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
}
```

---

### Operaciones de Impresión

#### Ejemplo 2: Imprimir Texto Simple
```csharp
public async Task ImprimirTextoSimple()
{
    try
    {
        // Texto izquierdo, tamaño normal
        await _printerService.PrintTextAsync(
            text: "Bienvenido a mi negocio",
            fontSize: 1,
            bold: false,
            centered: false
        );
        
        // Texto centrado en grande y negrita
        await _printerService.PrintTextAsync(
            text: "ANUNCIO IMPORTANTE",
            fontSize: 2,
            bold: true,
            centered: true
        );
        
        // Espacio en blanco
        await _printerService.PrintTextAsync(
            text: "",
            centered: true
        );
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error de impresión: {ex.Message}");
    }
}
```

**Parámetros:**
- `text`: Contenido a imprimir (string)
- `fontSize`: 1-8 (1=normal, 8=máximo)
- `bold`: true/false (negrita)
- `centered`: true/false (centrado)

---

#### Ejemplo 3: Imprimir Ticket Completo
```csharp
public async Task ImprimirTicket()
{
    try
    {
        // Preparar datos del ticket
        var items = new List<(string name, int qty, decimal price)>
        {
            ("Café Americano", 2, 3.50m),
            ("Croissant de jamón", 1, 4.75m),
            ("Agua mineral 500ml", 3, 1.25m),
            ("Jugo de naranja", 1, 2.50m)
        };
        
        decimal subtotal = 20.95m;
        decimal tax = 3.34m;      // 15.96% IVA
        decimal total = 24.29m;
        
        // Imprimir ticket
        await _printerService.PrintReceiptAsync(
            storeName: "CAFÉ ESPRESSO",
            items: items,
            subtotal: subtotal,
            tax: tax,
            total: total,
            footer: "¡Gracias por su compra!\nVuelva pronto"
        );
        
        MessageBox.Show("Ticket impreso exitosamente");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al imprimir ticket: {ex.Message}");
    }
}
```

---

#### Ejemplo 4: Imprimir Código de Barras
```csharp
public async Task ImprimirCodigoBarras()
{
    try
    {
        // Imprimir título
        await _printerService.PrintTextAsync(
            "CÓDIGO DE BARRAS",
            centered: true,
            bold: true
        );
        
        // Imprimir código de barras (CODE128)
        await _printerService.PrintBarcodeAsync(
            data: "1234567890128",
            barcodeType: BarcodeType.CODE128
        );
        
        // Imprimir el número debajo
        await _printerService.PrintTextAsync(
            text: "Producto: ITEM-12345",
            centered: true
        );
        
        await _printerService.FeedLinesAsync(3);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
}
```

**Tipos de Código Soportados:**
```csharp
public enum BarcodeType
{
    UPC_A,      // 11-12 dígitos
    UPC_E,      // 6-8 dígitos
    EAN13,      // 13 dígitos
    EAN8,       // 8 dígitos
    CODE39,     // Alfanumérico
    ITF,        // Interleaved 2 of 5
    CODABAR,    // Numérico
    CODE93,     // Alfanumérico comprimido
    CODE128     // Máxima densidad (recomendado)
}
```

---

#### Ejemplo 5: Imprimir Código QR
```csharp
public async Task ImprimirCodigoQR()
{
    try
    {
        // Información para el QR
        string urlProducto = "https://mitienda.com/productos/123456";
        
        // Imprimir encabezado
        await _printerService.PrintTextAsync(
            "ESCANEA PARA MÁS INFO",
            centered: true,
            bold: true
        );
        
        // Imprimir código QR
        await _printerService.PrintQRCodeAsync(
            data: urlProducto,
            size: 8  // Tamaño recomendado (1-16)
        );
        
        // Texto informativo
        await _printerService.PrintTextAsync(
            "Visita nuestra tienda en línea",
            centered: true
        );
        
        await _printerService.FeedLinesAsync(3);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
}
```

---

#### Ejemplo 6: Desconectar Impresora
```csharp
public async Task DesconectarImpresora()
{
    try
    {
        await _printerService.DisconnectAsync();
        
        if (!_printerService.IsConnected)
        {
            MessageBox.Show("Desconectado correctamente");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al desconectar: {ex.Message}");
    }
}
```

---

## 3. Referencia Rápida de Métodos

```csharp
// Conexión
await _printerService.ScanDevicesAsync()                    // Obtener dispositivos
await _printerService.ConnectAsync(address)                 // Conectar
await _printerService.DisconnectAsync()                     // Desconectar

// Impresión
await _printerService.PrintTextAsync(text, fontSize, bold, centered)
await _printerService.PrintReceiptAsync(name, items, subtotal, tax, total, footer)
await _printerService.PrintBarcodeAsync(data, type)
await _printerService.PrintQRCodeAsync(data, size)

// Control
await _printerService.FeedLinesAsync(lines)                // Avance de papel
await _printerService.CutPaperAsync()                       // Corte de papel

// Estado
_printerService.IsConnected                                 // Booleano de conexión
```

---

## 4. Soporte Técnico y Referencias

### Contacto
- **Email**: soporte@ejemplo.com
- **Teléfono**: 0343-4123456
- **Sitio Web**: www.ejemplo.com/soporte

### Documentación
- **ESCPOS_NET**: https://github.com/lukevp/ESCPOS_NET
- **Protocolo ESC/POS**: https://www.epson.com
- **.NET MAUI**: https://learn.microsoft.com/es-es/dotnet/maui/

---

**Versión del Manual**: 1.0  
**Fecha de Actualización**: Enero 2025  
**Modelo Compatible**: 58HB6-THERMAL-PRINTER  
**Plataforma**: .NET MAUI 10 (Android)
