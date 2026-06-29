# 1. Análisis del uso actual de `BarcodeScanner.Mobile.Maui`

> Cubre **Tareas 1 y 2**: lógica/secuencia de llamadas (foco iOS) y análisis del uso de la librería.
> Ambos proyectos comparten un `QRLectorPage` **casi idéntico**; la única diferencia es el patrón de consumo.

[⬅ Volver al índice](README.md)

---

## 1.1 Versión del NuGet y TFMs

| Aspecto | Ejemplo_Maui_Hibrida | Ejemplo_LectorQR_Dialog |
|---|---|---|
| `BarcodeScanner.Mobile.Maui` | **9.0.1** (`.csproj:85`) | **9.0.1** (`.csproj:83`) |
| TargetFrameworks | `net10.0-android`; `net10.0-ios` solo en macOS | Igual |
| `SupportedOSPlatformVersion` iOS | **15.0** | **15.0** |
| `SupportedOSPlatformVersion` Android | 25.0 | 25.0 |
| RuntimeIdentifiers Android | `android-arm;android-arm64;android-x86;android-x64` | Igual |
| Config iOS Release | `UseInterpreter=true`, `PublishTrimmed=true`, `MtouchLink=SdkOnly`, AOT solo si RID=`ios-arm64` | Idéntico |
| Paquete condicional simulador | `AdamE.Google.iOS.GoogleUtilities 8.1.0.3` **solo si RID=`iossimulator-x64`** | Idéntico |

**Claves para iOS:**
- El paquete `BarcodeScanner.Mobile.Maui` 9.0.1 arrastra transitivamente toda la cadena nativa de
  **Google ML Kit** (`Xamarin.Google.MLKit.BarcodeScanning`, `MLKit.Common`, `MLKit.Vision.*`,
  `Google.GoogleDataTransport`, `GTMSessionFetcher`, `Nanopb`, `PromisesObjC`).
- El paquete condicional `AdamE.Google.iOS.GoogleUtilities` para `iossimulator-x64` es el **workaround Rosetta**
  (build de simulador x86_64), origen del problema documentado en
  [`propuesta-rosetta.md`](../propuesta-rosetta.md).

Archivos: `Ejemplos_Devices/Integrada/Ejemplo_Maui_Hibrida/Ejemplo_Maui_Hibrida.csproj` ·
`Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Ejemplo_LectorQR_Dialog.csproj`

---

## 1.2 Registro / Inicialización

Ambos proyectos inicializan la librería **de forma idéntica**, vía el handler de MAUI en `MauiProgram.cs`:

```csharp
using BarcodeScanner.Mobile;

.ConfigureMauiHandlers(handlers =>
{
    handlers.AddBarcodeScannerHandler();
})
```

- Híbrida: `MauiProgram.cs:44-47` · Dialog: `MauiProgram.cs:21-24`.
- **No** hay `.UseBarcodeReader()` ni inicializador equivalente: el único punto de arranque es `AddBarcodeScannerHandler()`.
- **No** hay inicialización específica de iOS: `AppDelegate.cs` / `Program.cs` son los estándar de MAUI.
- La configuración de **formatos** se hace en el constructor de la página, no en el arranque (ver 1.3).

---

## 1.3 Superficie de API consumida

> Es **lo que hay que reemplazar** en una migración. Está concentrada en `QRLectorPage`.

**Builder / DI**
- `BarcodeScanner.Mobile.AddBarcodeScannerHandler()`

**Control XAML** (`xmlns:gv="clr-namespace:BarcodeScanner.Mobile;assembly=BarcodeScanner.Mobile.Maui"`)
- `<gv:CameraView>` con: `OnDetected`, `TorchOn`, `VibrationOnDetected`, `ScanInterval="50"`, `x:Name="Camera"`.

**Métodos / tipos en code-behind**
- `Methods.SetSupportBarcodeFormat(BarcodeFormats.QRCode | BarcodeFormats.Code39)` — en el constructor.
- `Methods.AskForRequiredPermission()` → `Task<bool>` — solicitud de permiso.
- Evento `OnCameraViewOnDetecte(object, OnDetectedEventArg e)`:
  - `e.BarcodeResults` → `List<BarcodeResult>`
  - `BarcodeResult.BarcodeType` (enum `BarcodeTypes`)
  - `BarcodeResult.DisplayValue` (string)
- `Camera.IsScanning = false` — detener escaneo tras detección.
- `Camera.TorchOn` — toggle linterna.

> No se usan `Start()/Stop()` explícitos: el arranque/paro de cámara lo gestiona el handler con el
> ciclo de vida de la página + `IsScanning`.

---

## 1.4 Secuencia de llamadas en iOS (foco principal)

1. **Navegación** a `QRLectorPage` (`Navigation.PushAsync`).
2. **Constructor:** `InitializeComponent()` instancia `CameraView`; `Methods.SetSupportBarcodeFormat(QRCode|Code39)`; `BindingContext = this`.
3. **Permiso:** `OnAppearing()` → `Methods.AskForRequiredPermission()` → dispara el prompt nativo iOS,
   cuyo texto sale de **`NSCameraUsageDescription`**. ⚠️ El `bool` resultante **se ignora** para
   bloquear la cámara (solo se reutiliza en el botón de linterna).
4. **Arranque de cámara:** lo hace el handler iOS del `CameraView` al renderizar (sin llamada explícita).
   Internamente usa **AVFoundation + ML Kit BarcodeScanning**; escanea cada `ScanInterval=50 ms`.
5. **Detección:** ML Kit dispara `OnDetected` → `OnCameraViewOnDetecte`. Mapea `e.BarcodeResults`
   (`BarcodeType`, `DisplayValue`) a `List<QRContent>`.
6. **Retorno + cierre:** dentro de `Dispatcher.Dispatch`: `Camera.IsScanning = false`,
   `CompletarResultado(QRs)`, `await Navigation.PopAsync()`.
7. **Liberación:** el teardown de `AVCaptureSession` lo hace el handler al destruir el control
   (no hay `Dispose`/`Stop` explícito).
8. **Cancelación:** botón "Volver" → `CompletarResultado(lista vacía)` + `PopAsync()`.

### Diferencias dialog vs. integrado

| | Dialog (`Ejemplo_LectorQR_Dialog`) | Integrado (`Ejemplo_Maui_Hibrida`) |
|---|---|---|
| Obtención del resultado | Solo `TaskCompletionSource` (`await ResultadoTask.Task`) | `TaskCompletionSource` **+** `Action<List<QRContent>?> OnQrCallback` (`QueryProperty`) |
| Quién la invoca | `MainPage` con botón "Leer QR" | `QrCommandHandler` (comando de URL desde el WebView; DI en `MauiProgram.cs:98`) |
| Resto de la página | Idéntico | Idéntico |

---

## 1.5 Permisos declarados

**Android**

| Permiso | Híbrida | Dialog |
|---|---|---|
| `CAMERA` | ✅ | ✅ (declarado 2 veces) |
| `VIBRATE` | ❌ | ✅ |
| `FLASHLIGHT` | ❌ | ✅ |
| `<queries>` IMAGE_CAPTURE | ✅ | ✅ |

**iOS (`Info.plist`)**

| Clave | Híbrida | Dialog |
|---|---|---|
| `NSCameraUsageDescription` | ✅ ("...escanear QR y tomar fotos.") | ✅ ("...escanear códigos QR.") |
| `MinimumOSVersion` | 15.0 | 15.0 |
| `UIRequiredDeviceCapabilities` | `arm64` | `arm64` |
| Entitlements | ninguno | ninguno |

> La única clave iOS imprescindible para el escáner es **`NSCameraUsageDescription`** (la cámara no requiere entitlements).

---

## 1.6 Acoplamiento / esfuerzo de migración

Para cambiar de librería habría que reemplazar:

1. **Paquete + handler:** quitar `BarcodeScanner.Mobile.Maui` (y el condicional `AdamE.Google.iOS.GoogleUtilities`); cambiar `AddBarcodeScannerHandler()` por el init de la nueva lib.
2. **Control XAML:** `<gv:CameraView ...>` → control equivalente + su xmlns.
3. **Formatos:** `Methods.SetSupportBarcodeFormat(...)` → propiedad de opciones del nuevo control.
4. **Permisos:** `Methods.AskForRequiredPermission()` → `Permissions.RequestAsync<Permissions.Camera>()` (MAUI Essentials).
5. **Evento + resultado:** `OnDetectedEventArg.BarcodeResults` / `BarcodeResult.{BarcodeType, DisplayValue}` → tipo de resultado de la nueva lib; re-mapear a `QRContent {Type, Value}`.
6. **Runtime:** `Camera.IsScanning` y `Camera.TorchOn` → propiedades equivalentes.
7. **Manifest/plist:** se mantienen (`CAMERA` Android, `NSCameraUsageDescription` iOS).

> **Buena noticia:** el acoplamiento está **concentrado** en `QRLectorPage.xaml(.cs)` (idéntico en
> ambos proyectos) + 1 línea de `MauiProgram.cs`. `MainPage`, `QrCommandHandler` y el modelo
> `QRContent` **no dependen** de la API de la librería → migración localizada y de bajo riesgo.

### Riesgos específicos de iOS (situación actual)

- **ML Kit no tiene slice de simulador arm64** → obliga al workaround `iossimulator-x64` + Rosetta para CI.
- **Rosetta en retirada** por Apple → la solución tiene fecha de caducidad.
- **Cadena nativa pesada** (ML Kit + GoogleDataTransport + GTMSessionFetcher + Nanopb + PromisesObjC) → IPA mayor y más superficie de linkeo/trimming/AOT.
- **Permiso no bloqueante:** si el usuario deniega el permiso, la cámara queda en negro sin feedback claro.

> Migrar a una librería con **Apple Vision/AVFoundation** elimina el problema de simulador/Rosetta.

---

[⬅ Volver al índice](README.md) · [Siguiente: 2. Alternativas ➡](02-alternativas-nuget.md)
