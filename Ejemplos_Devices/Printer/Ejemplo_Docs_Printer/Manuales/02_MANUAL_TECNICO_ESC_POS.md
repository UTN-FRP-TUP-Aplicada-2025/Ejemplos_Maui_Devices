# Manual Técnico del Dispositivo - ESC/POS Programming Guide
## Modelo: 58HB6-THERMAL-PRINTER

---

## Introducción

Este documento especifica los detalles técnicos del protocolo ESC/POS implementado por la impresora térmica 58HB6-THERMAL-PRINTER. Está orientado a ingenieros y desarrolladores que requieren información de bajo nivel para integración y programación de dispositivos.

---

## Arquitectura del Dispositivo

### Componentes Principales

```
┌─────────────────────────────────────────────────────┐
│                 ARQUITECTURA GENERAL                │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌────────────────┐        ┌─────────────────┐    │
│  │  Controlador   │        │  Motor Stepper  │    │
│  │   Principal    │────────│  (Alimentación  │    │
│  │  (MCU ARM 32)  │        │    de papel)    │    │
│  └────────────────┘        └─────────────────┘    │
│        │                                            │
│        ├─────────────────────┬────────────────┐   │
│        │                     │                │   │
│  ┌─────┴─────┐      ┌──────┴───┐      ┌──────┴──┐ │
│  │ Bluetooth  │      │ Cabeza   │      │ Batería│ │
│  │ 5.0 BLE    │      │ Térmica  │      │ Li-ion │ │
│  │ (UART)     │      │ 203 DPI  │      │ 2000mAh│ │
│  └────────────┘      └──────────┘      └────────┘ │
│        │                                            │
│  ┌─────┴───────────┐                              │
│  │  Puerto USB     │                              │
│  │  Type-C         │                              │
│  │  (Dual role)    │                              │
│  └─────────────────┘                              │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Memoria del Dispositivo

| Tipo | Capacidad | Uso |
|------|-----------|-----|
| **RAM** | 256 KB | Buffer de comandos, variables de estado |
| **ROM/Flash** | 2 MB | Firmware ESC/POS, tablas de fuentes |
| **EEPROM** | 64 KB | Configuración, contadores de líneas impresas |

---

## Interfaces de Comunicación

### Bluetooth 5.0 (BLE - Bluetooth Low Energy)

**Especificación Técnica:**
- **Versión**: Bluetooth 5.0
- **Perfil**: SPP (Serial Port Profile) emulado
- **Rango Efectivo**: 10 metros (sin obstáculos)
- **Velocidad de Datos**: 2 Mbps teórico, ~100 Kbps efectivo
- **Frecuencia**: 2.4 GHz
- **Canales de Salto**: 37 canales de frecuencia

**Parámetros de Conexión:**
- **Timeout de conexión**: 30 segundos
- **Período de transmisión**: 20-100 ms
- **MTU (Maximum Transmission Unit)**: 20 bytes
- **Reintentos automáticos**: Sí
- **Reconexión automática**: No (requiere iniciar desde aplicación)

**Emparejamiento:**
- **Método**: PIN numérico (0000)
- **Duración**: Persistente
- **Dispositivos simultáneos emparejados**: Hasta 8
- **Dispositivos conectados simultáneamente**: 1

### USB Type-C

**Especificación Técnica:**
- **Estándar**: USB 2.0 Full-Speed
- **Velocidad de Transferencia**: 12 Mbps
- **Voltage**: 5V ± 10%
- **Corriente Máxima**: 2A (carga), 500 mA (transferencia datos)
- **Función**: Dual (carga de batería + comunicación de datos)

**Parámetros de Comunicación Serial:**
- **Baudrate**: 115200 bps (estándar), soporta también 9600, 19200, 38400
- **Data Bits**: 8
- **Stop Bits**: 1
- **Parity**: Ninguno
- **Flow Control**: RTS/CTS opcional

---

## Parámetros de Comunicación General

### Protocolo de Transporte

**Estructura de Comando General:**
```
[ESC/POS COMMAND] = [PREFIX] [COMMAND_BYTE] [PARAMETERS...]
```

**Ejemplo:**
```
Hexadecimal: 1B 40 (Inicializar)
Binario:     00011011 01000000
```

### Formato de Datos

| Aspecto | Especificación |
|--------|---|
| **Conjunto de Caracteres** | ASCII (0x20-0x7E), extendido (0x80-0xFF) |
| **Orden de Bytes** | Big-endian para comandos, little-endian para datos de imagen |
| **Encoding de Strings** | UTF-8 (con conversión a código de página seleccionado) |
| **Timeout de Comando** | 5 segundos |
| **Buffer de Entrada** | 2 KB (aproximadamente 40-50 comandos) |

---

## Compatibilidad ESC/POS

### Nivel de Conformidad

La impresora 58HB6-THERMAL-PRINTER implementa un subconjunto estándar de ESC/POS definido por Epson:

- **Versión de Base**: Epson ESC/POS 2.0
- **Comandos Implementados**: ~95% de comandos estándar
- **Extensiones Propietarias**: Ninguna

### Funcionalidades Soportadas

| Función | Soportada | Notas |
|---------|-----------|-------|
| Texto simple | ✓ | ASCII y caracteres extendidos |
| Fuentes múltiples | ✓ | Fuente A (12×24), Fuente B (9×17) |
| Estilos de texto | ✓ | Negrita, subrayado, inversión, doble altura |
| Alineación | ✓ | Izquierda, centro, derecha |
| Códigos de barras | ✓ | 9 formatos diferentes |
| Código QR | ✓ | Versiones hasta 40 |
| Imágenes raster | ⚠ | Por confirmar (posible en versión firmware 2.0+) |
| Corte de papel | ✓ | Corte completo y parcial |
| Gestión de papel | ✓ | Avance, inicio de línea, reset de posición |

---

## Tabla de Comandos ESC/POS Soportados

### Comandos de Control General

| Comando | Hex | Parámetros | Descripción | Ejemplo |
|---------|-----|-----------|-------------|---------|
| Inicializar | `1B 40` | Ninguno | Reinicia impresora a estado por defecto | `ESC @` |
| Salto de línea | `0A` | Ninguno | Avanza una línea | `LF` |
| Retorno carro | `0D` | Ninguno | Vuelve al inicio de línea | `CR` |
| Campana | `07` | Ninguno | Emite beep de confirmación | `BEL` |

### Comandos de Alineación

| Comando | Hex | Parámetro | Efecto |
|---------|-----|-----------|--------|
| Alineación | `1B 61` | 0=Izq, 1=Cent, 2=Der | Configura justificación de texto |
| Centering on | `1B 63 05 FF` | Ninguno | Activa centrado |
| Centering off | `1B 63 05 00` | Ninguno | Desactiva centrado |

**Ejemplo de uso:**
```
1B 61 01  → Alinear al centro
```

### Comandos de Estilos de Fuente

| Comando | Hex | Parámetro | Efecto | Rango |
|---------|-----|-----------|--------|-------|
| Negrita | `1B 45` | 0=Off, 1=On | Activar/desactivar negrita | 1 byte |
| Subrayado | `1B 2D` | 0=Off, 1=Simple, 2=Doble | Subrayado de texto | 1 byte |
| Inversión | `1B 42` | 0=Off, 1=On | Invierte negro/blanco | 1 byte |
| Fuente | `1B 4D` | 0=Fuente A, 1=Fuente B | Selecciona tipo de fuente | 1 byte |
| Doble altura | `1B 77 01` | Ninguno | Duplica altura de caracteres | - |
| Doble ancho | `1B 77 02` | Ninguno | Duplica ancho de caracteres | - |
| Tamaño personalizado | `1D 21` | Parámetro combinado | Define alto/ancho personalizado | 1 byte |

**Tabla de Parámetro para Tamaño Personalizado (GS !):**
```
Formato: [Alto (bits 0-3)] [Ancho (bits 4-7)]

Ejemplos:
  00H → Normal (1×1)
  11H → 1×2 (doble ancho)
  12H → 2×1 (doble alto)
  11H → 2×2 (doble ancho y alto)
  77H → 8×8 (máximo)
```

### Comandos de Gestión de Papel

| Comando | Hex | Parámetros | Descripción |
|---------|-----|-----------|-------------|
| Avance de línea | `1B 4A` | n (1 byte) | Avanza n puntos (aprox. n/203 mm) |
| Avance líneas | `1B 64` | n (1 byte) | Avanza n líneas completas |
| Avance a inicio | `0C` | Ninguno | Mueve a inicio de línea siguiente |
| Reset posición | `1B 2A` | Ninguno | Reinicia contador de posición horizontal |
| Página nueva | `0C` | Ninguno | Avanza página (50 líneas) |

**Ejemplo de avance de 100 puntos:**
```
1B 4A 64  (1B 4A = comando, 64H = 100 decimal)
```

### Comandos de Corte de Papel

| Comando | Hex | Parámetro | Tipo de Corte | Resultado |
|---------|-----|-----------|--------------|-----------|
| Corte | `1D 56` | 0 | Corte parcial | Deja ~3 mm |
| Corte | `1D 56` | 1 | Corte completo | Corta completamente |
| Corte full | `1D 56 41` | 1 | Corte con retracción | Óptimo para pegado |

**Ejemplo de corte completo:**
```
1D 56 01
```

### Comandos de Códigos de Barras (GS k)

**Estructura General:**
```
GS k [tipo] [longitud] [datos...]
1D 6B [01-09] [LL] [datos]
```

| Tipo | Hex | Formato | Dígitos Válidos | Caracteres/Línea |
|------|-----|---------|-----------------|------------------|
| UPC-A | 00 | Numérico | 11-12 | 12 |
| UPC-E | 01 | Numérico | 6-8 | 8 |
| EAN13 | 02 | Numérico | 13 | 13 |
| EAN8 | 03 | Numérico | 8 | 8 |
| CODE39 | 04 | Alfanumérico | Variable | 43 |
| ITF | 05 | Numérico | Variable | N/A |
| CODABAR | 06 | Numérico + ABC | Variable | 20 |
| CODE93 | 07 | ASCII | Variable | 47 |
| CODE128 | 08 | ASCII | Variable | 48 |

**Ejemplo de CODE128:**
```
1D 6B 08 0D 313233343536373839303132 33
         ↑  ↑                      ↑
      tipo lon                   datos "123456789012 3"
```

### Comandos de Código QR (GS ( k)

**Estructura General:**
```
GS ( k [2 bytes tamaño] [función] [parámetros]
1D 28 6B [LL] [LH] [fn] [datos]
```

| Función | Descripción | Parámetros |
|---------|-------------|-----------|
| 1 | Especificar tamaño | [tamaño: 1-8] |
| 2 | Especificar nivel error | [nivel: 0-3 (L,M,Q,H)] |
| 3 | Almacenar datos | [datos a codificar] |
| 4 | Imprimir | Ninguno |

**Ejemplo simplificado (tamaño 8, datos "EXAMPLE"):**
```
1D 28 6B 10 00 31 01 08  (especificar tamaño)
1D 28 6B 0E 00 32 02 03  (especificar nivel error M)
1D 28 6B 0C 00 33 30 45 58 414D 504C 45  (almacenar "EXAMPLE")
1D 28 6B 03 00 34 (imprimir)
```

---

## Fuentes Internas Disponibles

### Fuente A (Estándar)

| Propiedad | Valor |
|-----------|-------|
| **Nombre** | Fuente A |
| **Dimensión Normal** | 12×24 puntos |
| **Dimensión Doble Altura** | 12×48 puntos |
| **Dimensión Doble Ancho** | 24×24 puntos |
| **Dimensión Doble (A×A)** | 24×48 puntos |
| **Caracteres por Línea** | 32 (normal), 16 (doble ancho) |
| **Tipo** | Monoespaciado |
| **Código de Comando** | Predeterminado (ESC M 00) |

### Fuente B

| Propiedad | Valor |
|-----------|-------|
| **Nombre** | Fuente B |
| **Dimensión Normal** | 9×17 puntos |
| **Dimensión Doble Altura** | 9×34 puntos |
| **Caracteres por Línea** | 42 (normal), 21 (doble ancho) |
| **Tipo** | Monoespaciado condensado |
| **Código de Comando** | ESC M 01 |

**Selección de Fuente:**
```
Fuente A: 1B 4D 00
Fuente B: 1B 4D 01
```

---

## Code Pages (Páginas de Código)

La impresora soporta múltiples páginas de código para diferentes regiones y idiomas.

### Code Pages Disponibles

| ID | Nombre | Idiomas | Caracteres Especiales |
|----|--------|---------|----------------------|
| 0 | PC437 | Inglés | Symbols (gráficos) |
| 1 | Katakana | Japonés | Silabario japonés |
| 2 | PC850 | Multi-idioma | Acentos europeos |
| 3 | PC860 | Portugués | Caracteres portugueses |
| 4 | PC863 | Francés | Caracteres franceses |
| 5 | PC865 | Nórdico | Caracteres nórdicos |
| 12 | PC858 (Euro) | Multi-idioma + Euro | € + acentos completos |

**Comando de Selección:**
```
1B 74 [n]  (donde n = ID de página)

Ejemplos:
1B 74 0C   → PC858 Euro (recomendado para América Latina)
1B 74 02   → PC850 (multi-idioma europeo)
```

---

## Soporte de Códigos de Barras (Detalles)

### Compatibilidad por Tipo

| Tipo | Soporte | Algoritmo de Check Digit | Dígitos Requeridos |
|------|---------|--------------------------|-------------------|
| UPC-A | ✓ | Modulo 10 | 11 (genera el 12°) |
| UPC-E | ✓ | Modulo 10 | 6 (genera el 8°) |
| EAN-13 | ✓ | Modulo 10 | 12 (genera el 13°) |
| EAN-8 | ✓ | Modulo 10 | 7 (genera el 8°) |
| CODE39 | ✓ | Modulo 43 (opcional) | Variable |
| ITF | ✓ | Modulo 10 | Pares de dígitos |
| CODABAR | ✓ | Modulo 11 | Variable |
| CODE93 | ✓ | 2 dígitos check | Variable |
| CODE128 | ✓ | Modulo 103 | Variable |

### Altura de Código de Barras

**Comando:**
```
1D 68 [n]  (donde n = altura en puntos)

Rango: 1-255 puntos
Recomendado: 50-100 puntos (aprox. 2.5-5 mm)
```

---

## Soporte de QR Code

### Especificaciones

| Parámetro | Valor |
|-----------|-------|
| **Versión Máxima** | 40 (177×177 módulos) |
| **Nivel de Corrección de Error** | L (7%), M (15%), Q (25%), H (30%) |
| **Tamaño de Módulo** | 1-8 puntos (configurables) |
| **Capacidad Máxima** | ~2953 bytes (Versión 40, nivel L) |
| **Formato de Datos** | Alfanumérico, Numérico, Byte, Kanji |

### Tamaño Recomendado para Papel 58mm

```
Tamaño 6: Mínimo escaneable (~20×20 mm)
Tamaño 8: Óptimo (~27×27 mm)
Tamaño 10: Máximo legible (~34×34 mm)
```

---

## Manejo de Buffer

### Características del Buffer

| Aspecto | Especificación |
|--------|---|
| **Tamaño Total** | 2 KB (2048 bytes) |
| **Tamaño de Línea de Impresión** | 384 puntos (48 bytes por línea) |
| **Líneas en Buffer** | ~85 líneas (aproximado) |
| **Modo de Operación** | FIFO (First-In, First-Out) |
| **Timeout de Vaciado** | Automático después de 5 segundos sin datos |

### Administración de Buffer

**Situaciones Críticas:**

1. **Buffer Lleno**: Se rechaza el próximo comando hasta que se procese.
2. **Buffer Casi Lleno**: Recomendable enviar datos más lentamente.
3. **Vaciado Automático**: Después de fin de línea o timeout.

**Comando de Vaciado Manual:**
```
1B 76  (Flush buffer)
```

---

## Estados de la Impresora

### Registro de Estado (GS r)

**Comando:**
```
1D 72 [n]  (donde n = tipo de estado)
```

| n | Estado | Bits Significativos |
|---|--------|-------------------|
| 1 | Operativo | Bit 0: Listo, Bit 2: Papel bajo, Bit 5: Error |
| 2 | Papel | Bit 1: Papel presente |
| 3 | Temperatura | Bit 0: Temperatura OK, Bit 1: Advertencia |

### Estados Comunes

| Estado | LED | Significado | Acción |
|--------|-----|-----------|--------|
| **Listo** | Verde sólido | Impresora lista para comandos | Continuar operación |
| **Imprimiendo** | Verde parpadeante | Procesando comando | Esperar finalización |
| **Papel bajo** | Naranja | <10% de papel | Cambiar papel pronto |
| **Papel agotado** | Rojo parpadeante | No hay papel | Cambiar papel inmediatamente |
| **Temperatura alta** | Rojo sólido | Cabeza térmica sobre 70°C | Esperar enfriamiento |
| **Error general** | Rojo | Error desconocido | Reiniciar dispositivo |

---

## Secuencias de Inicialización Recomendadas

### Secuencia de Inicio Mínima

```
1. 1B 40          (Inicializar impresora)
2. 1B 74 0C       (Seleccionar code page PC858 Euro)
3. 1B 4D 00       (Seleccionar Fuente A)
4. 1B 61 00       (Alinear a la izquierda)
```

### Secuencia de Inicio Completa (Recomendada)

```
1. 1B 40          (Inicializar)
2. 1B 74 0C       (Code page Euro)
3. 1B 4D 00       (Fuente A)
4. 1B 61 00       (Alinear izquierda)
5. 1D 21 00       (Tamaño normal 1×1)
6. 1B 45 00       (Negrita OFF)
7. 1B 2D 00       (Subrayado OFF)
8. 1B 42 00       (Inversión OFF)
9. 0A 0A          (2 líneas en blanco)
```

### Secuencia de Reset de Seguridad

```
1. 1B 40          (Inicializar)
2. 1D 56 01       (Cortar papel - limpiar buffer)
3. [Esperar 2 segundos]
4. 1B 40          (Re-inicializar)
```

---

## Limitaciones Conocidas

### Limitaciones Técnicas

| Limitación | Detalle | Workaround |
|-----------|---------|-----------|
| **Ancho máximo** | 384 puntos (48 mm) para 58 mm | No existe, inherente al hardware |
| **Líneas por impresión** | ~85 líneas en buffer | Enviar en lotes si es más largo |
| **Tiempo de procesamiento** | 5 segundos máximo por comando | Usar secuencias de inicialización cortas |
| **Imágenes raster** | No completamente soportadas (por confirmar) | Usar códigos QR/barras en su lugar |
| **Múltiples fuentes** | Solo Fuente A y B | Limitación de firmware |

### Limitaciones de Comunicación

| Limitación | Detalle |
|-----------|---------|
| **Distancia Bluetooth** | Máximo 10 metros (reduce con obstáculos) |
| **Dispositivos simultáneos** | Solo 1 conexión activa a la vez |
| **Velocidad USB** | USB 2.0 Full-Speed (12 Mbps teórico) |
| **Tiempo de reconexión** | 30 segundos timeout |

### Limitaciones de Códigos

| Código | Limitación |
|--------|-----------|
| **Códigos de barras** | Máximo 48 caracteres por línea |
| **Código QR** | Versión máxima 40 (~177×177 módulos) |
| **Caracteres especiales** | Limitados a code page seleccionada |

---

## Compatibilidad Conocida

### Sistemas Operativos Probados

- Android 8.0+ (con Bluetooth 5.0)
- iOS 14+ (con Bluetooth 5.0)
- Windows 10+ (con puerto COM USB)
- Linux (con controladores USB ACM)

### Aplicaciones Compatibles

- ESCPOS_NET v2.2.1+ (.NET)
- Escpos-PHP (PHP)
- Epson ePOS SDK (JavaScript)
- Printer ESC/POS (Python)

---

## Información de Diagnóstico

### Comando de Diagnóstico (GS I)

```
1D 49 [n]  (donde n = tipo de información)

n=0: Dispositivo
n=1: Posición de cabeza
n=2: Contadores internos
n=3: Información de firmware
```

### Logs de Evento

| Evento | Código |
|--------|--------|
| Encendido | 0xAA |
| Corte papel | 0xBB |
| Papel agotado | 0xCC |
| Error temperatura | 0xDD |

---

## Anexo: Tabla Rápida de Caracteres Especiales (PC858 Euro)

| Carácter | Código Decimal | Código Hex |
|---------|---|---|
| € (Euro) | 213 | 0xD5 |
| ñ | 164 | 0xA4 |
| Ñ | 165 | 0xA5 |
| á | 160 | 0xA0 |
| é | 130 | 0x82 |
| í | 161 | 0xA1 |
| ó | 162 | 0xA2 |
| ú | 163 | 0xA3 |

---

**Versión del Manual**: 1.0  
**Fecha de Publicación**: Enero 2025  
**Modelo Aplicable**: 58HB6-THERMAL-PRINTER  
**Estándar Base**: Epson ESC/POS 2.0
