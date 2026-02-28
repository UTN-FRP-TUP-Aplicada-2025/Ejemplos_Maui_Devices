# Guía de Instalación y Configuración
## Impresión Térmica Bluetooth en .NET MAUI

### Paso 1: Crear el Proyecto

```bash
dotnet new maui -n ThermalPrinterMAUI
cd ThermalPrinterMAUI
```

### Paso 2: Instalar Paquetes NuGet

Abre el proyecto en Visual Studio o VS Code y ejecuta:

```bash
dotnet add package ESCPOS_NET --version 4.5.0
dotnet add package Plugin.BLE --version 3.1.0
```

O edita manualmente el archivo `.csproj` para incluir:

```xml
<PackageReference Include="ESCPOS_NET" Version="4.5.0" />
<PackageReference Include="Plugin.BLE" Version="3.1.0" />
```

### Paso 3: Estructura de Carpetas

Crea la siguiente estructura:

```
ThermalPrinterMAUI/
├── Services/
│   ├── IThermalPrinterService.cs
│   └── ThermalPrinterService.cs
├── Platforms/
│   └── Android/
│       └── AndroidManifest.xml
├── MainPage.xaml
├── MainPage.xaml.cs
└── MauiProgram.cs
```

### Paso 4: Copiar los Archivos

1. **Services/IThermalPrinterService.cs** - Interface del servicio
2. **Services/ThermalPrinterService.cs** - Implementación del servicio
3. **MainPage.xaml** - UI de ejemplo
4. **MainPage.xaml.cs** - Código behind
5. **MauiProgram.cs** - Configuración de dependencias
6. **Platforms/Android/AndroidManifest.xml** - Permisos Android

### Paso 5: Configurar AndroidManifest.xml

Ubicación: `Platforms/Android/AndroidManifest.xml`

Asegúrate de incluir todos los permisos necesarios:

```xml
<!-- Bluetooth -->
<uses-permission android:name="android.permission.BLUETOOTH" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
<uses-permission android:name="android.permission.BLUETOOTH_SCAN" />
<uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />

<!-- Ubicación (necesario para escaneo Bluetooth) -->
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

### Paso 6: Modificar MauiProgram.cs

Registra el servicio de impresión:

```csharp
builder.Services.AddSingleton<IThermalPrinterService, ThermalPrinterService>();
builder.Services.AddTransient<MainPage>();
```

### Paso 7: Configurar App.xaml.cs

Si no existe, modifica para inyectar MainPage:

```csharp
public partial class App : Application
{
    public App(MainPage mainPage)
    {
        InitializeComponent();
        MainPage = new NavigationPage(mainPage);
    }
}
```

### Paso 8: Compilar y Probar

#### En Android

1. Conecta tu dispositivo Android o inicia un emulador
2. Asegúrate de que Bluetooth esté activado
3. Empareja tu impresora térmica con el dispositivo Android
4. Compila y ejecuta:

```bash
dotnet build -t:Run -f net8.0-android
```

O desde Visual Studio: **Run > Start Debugging (F5)**

### Paso 9: Probar la Aplicación

1. **Escanear**: Presiona "Escanear Dispositivos Bluetooth"
2. **Seleccionar**: Elige tu impresora de la lista
3. **Conectar**: Presiona "Conectar"
4. **Imprimir**: Prueba cualquiera de las opciones de impresión

### Solución de Problemas Comunes

#### Error: "Bluetooth no está disponible"
- Verifica que el dispositivo tenga Bluetooth
- Activa Bluetooth en configuración

#### Error: "No se encontraron dispositivos"
- Empareja la impresora primero en Configuración > Bluetooth de Android
- Verifica que la impresora esté encendida
- Dale permisos de ubicación a la app

#### Error: "No se pudo conectar"
- Cierra otras apps que puedan estar usando la impresora
- Desempareja y vuelve a emparejar la impresora
- Reinicia la impresora

#### Error de compilación: "ESCPOS_NET no encontrado"
```bash
dotnet restore
dotnet clean
dotnet build
```

#### Imprime caracteres extraños
- Verifica la codificación en ThermalPrinterService.cs
- Algunas impresoras usan diferentes code pages
- Prueba cambiar de `PC858_EURO` a `PC437_USA_STANDARD_EUROPE_DEFAULT`

### Personalización

#### Cambiar el ancho de papel

En `ThermalPrinterService.cs`, modifica la constante:

```csharp
private const int PAPER_WIDTH_58MM = 384; // Para 58mm
// O
private const int PAPER_WIDTH_80MM = 576; // Para 80mm
```

#### Agregar más funciones

Puedes extender `IThermalPrinterService` para agregar:
- Impresión de imágenes
- Diferentes fuentes
- Subrayado
- Inversión de colores
- etc.

### Recursos Útiles

- **ESCPOS_NET GitHub**: https://github.com/lukevp/ESC-POS-.NET
- **Plugin.BLE**: https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le
- **Documentación MAUI**: https://learn.microsoft.com/dotnet/maui/

### Prueba de Impresión Exitosa

Si todo está configurado correctamente, deberías poder:
✅ Escanear dispositivos Bluetooth
✅ Conectarte a la impresora
✅ Imprimir texto de prueba
✅ Imprimir tickets
✅ Imprimir códigos QR
✅ Imprimir códigos de barras

### Próximos Pasos

1. Implementar impresión de imágenes
2. Agregar soporte para iOS (usando Plugin.BLE)
3. Crear plantillas de tickets personalizables
4. Implementar cola de impresión
5. Agregar reconexión automática

---

## Notas Importantes

⚠️ **Solo Android**: Esta implementación solo funciona en Android por ahora
⚠️ **Emparejamiento previo**: La impresora debe estar emparejada antes de usarla
⚠️ **Permisos**: Los permisos son críticos, especialmente en Android 12+
⚠️ **Compatibilidad**: Funciona con impresoras que soportan comandos ESC/POS

---

Si tienes problemas o preguntas, revisa la sección de solución de problemas o consulta la documentación de las bibliotecas utilizadas.