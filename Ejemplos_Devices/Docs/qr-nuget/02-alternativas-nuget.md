# 2. Alternativas NuGet (fichas detalladas)

> Cubre **Tareas 3 y 4**. Datos verificados en nuget.org / GitHub a **junio 2026**.
> Criterios: (3.a) **.NET 9+**, (3.b) **APIs nativas Android e iOS**, (3.c) **confianza del autor**.
>
> Clasificación iOS: 🟢 decodificación nativa (Apple Vision) · 🟡 preview nativo + decodificación *managed* · 🔵 motor propietario nativo.

[⬅ Volver al índice](README.md) · [⬅ Anterior: 1. Análisis](01-analisis-uso-actual.md)

---

## 2.1 🟢 BarcodeScanning.Native.Maui  *(recomendado)*

| Campo | Detalle |
|---|---|
| **NuGet** | `BarcodeScanning.Native.Maui` v**3.0.4** (29/05/2026) |
| **Autor / licencia** | afriscic (Ante Friscic) · **MIT** |
| **Confianza** | ~856K descargas · 339★ · 4 releases en 2026 → **activo** |
| **TFM** | **net10-only** (`net10.0-android36`, `-ios26`, `-maccatalyst26`, `-windows`). Para net9 puro: fijar serie 2.2.x. |
| **Nativo Android** | **Google ML Kit** sobre **CameraX** |
| **Nativo iOS** | **Apple Vision** (no ML Kit → **sin Rosetta**) |
| **Windows** | zxing-cpp (nativo C++) |
| **RID** | Android arm64-v8a/armeabi-v7a/x86_64 (**sin x86**) · iOS device arm64 + **simulador arm64** · Mac Catalyst ✅ |
| **Permisos** | Android `CAMERA` + `VIBRATE` · iOS `NSCameraUsageDescription` + `await Methods.AskForRequiredPermissionAsync()` |

**Integración mínima**

```csharp
// MauiProgram.cs
builder.UseMauiApp<App>().UseBarcodeScanning();
```
```xml
<!-- página XAML -->
xmlns:scanner="clr-namespace:BarcodeScanning;assembly=BarcodeScanning.Native.Maui"
<scanner:CameraView OnDetectionFinished="CameraView_OnDetectionFinished" />
```
```csharp
void CameraView_OnDetectionFinished(object s, OnDetectionFinishedEventArg e)
{
    if (e.BarcodeResults.Count > 0)
    {
        var b = e.BarcodeResults.First();   // b.DisplayValue, b.BarcodeType
    }
}
```

**Pros:** 100% nativo (ML Kit + Apple Vision), MIT, muy mantenido, multiplataforma real, API simple, features extra (ViewfinderMode, AimMode, zoom, captura).
**Contras:** 3.0.4 es net10-only; Android sin x86 (no corre en emuladores x86); requiere device físico para pruebas reales; issues iOS reportados (rotación/foco/crash al denegar permiso).

**Fuentes:** <https://www.nuget.org/packages/BarcodeScanning.Native.Maui/> · <https://github.com/afriscic/BarcodeScanning.Native.Maui>

---

## 2.2 🟢 CameraScanner.Maui

| Campo | Detalle |
|---|---|
| **NuGet** | `CameraScanner.Maui` v**1.8.31** (16/05/2026) |
| **Autor / licencia** | Thomas Galliker (superdev GmbH) · **MIT** |
| **Confianza** | ~47K descargas · autor MAUI muy reconocido y activo |
| **TFM** | `net9.0` **y** `net10.0` (android35/36, ios18/26) |
| **Nativo Android** | **Google ML Kit** + AndroidX.Camera (CameraX) |
| **Nativo iOS** | **Apple VisionKit / Vision** |
| **RID** | Igual perfil que 2.1 (Android sin x86; iOS device + simulador arm64) |
| **Permisos** | Android `CAMERA` (+`VIBRATE` opc.) · iOS `NSCameraUsageDescription` |

**Integración mínima**

```csharp
// MauiProgram.cs
builder.UseMauiApp<App>().UseCameraScanner();
```
```xml
xmlns:c="http://camerascanner.maui"
<c:CameraView CameraEnabled="True" BarcodeDetected="OnBarcodeDetected" />
```

**Pros:** gratis y MIT, nativo en ambas plataformas, **soporta net9 y net10**, autor confiable, API moderna bindable.
**Contras:** sin soporte comercial; mismas limitaciones de ML Kit (x86/simulador Intel) en Android; comunidad menor que 2.1.

**Fuentes:** <https://www.nuget.org/packages/CameraScanner.Maui> · <https://github.com/thomasgalliker/CameraScanner.Maui>

---

## 2.3 🟡 ZXing.Net.Maui

| Campo | Detalle |
|---|---|
| **NuGet** | `ZXing.Net.Maui` + `ZXing.Net.Maui.Controls` v**0.10.1 estable** (19/06/2026) |
| **Autor / licencia** | Redth (Jonathan Dick) + jfversluis (Gerald Versluis), **Microsoft** · **MIT** |
| **Confianza** | ~1.7M descargas · 570★ · commits jun-2026 → **muy activo** (máxima confianza de autor) |
| **TFM** | 0.10.1 = **net10**; para net9 usar serie 0.7.x (ene-2026) |
| **Nativo Android** | preview **CameraX nativo**; **decodificación managed ZXing** (no ML Kit) |
| **Nativo iOS** | preview **AVFoundation nativo**; **decodificación managed ZXing** |
| **RID** | **Cobertura total** (Android incl. x86; iOS device + simulador arm64/x64). ⚠️ Windows: solo generación, sin escaneo por cámara |
| **Permisos** | Android `CAMERA` · iOS `NSCameraUsageDescription` |

**Integración mínima**

```csharp
// MauiProgram.cs
using ZXing.Net.Maui.Controls;
builder.UseMauiApp<App>().UseBarcodeReader();
```
```xml
xmlns:zxing="clr-namespace:ZXing.Net.Maui.Controls;assembly=ZXing.Net.MAUI.Controls"
<zxing:CameraBarcodeReaderView x:Name="reader" BarcodesDetected="BarcodesDetected" />
```
```csharp
void BarcodesDetected(object s, BarcodeDetectionEventArgs e)
{
    foreach (var b in e.Results) { /* b.Format, b.Value */ }
    // El evento llega en hilo de fondo → usar MainThread.BeginInvokeOnMainThread para UI.
}
```

**Pros:** sucesor oficial de ZXing.Net.Mobile; autores de Microsoft; estable y al día (net10); MIT; preview nativo; **sin dependencia de ML Kit/Google Play Services** → máxima portabilidad de CI/simuladores.
**Contras:** decodificación *managed* (algo menos robusta en baja luz/ángulos que ML Kit/Vision); sin escaneo por cámara en Windows; versionado 0.x (API puede cambiar).

**Fuentes:** <https://www.nuget.org/packages/ZXing.Net.Maui> · <https://github.com/Redth/ZXing.Net.Maui>

---

## 2.4 🟡 Camera.MAUI → fork `CameraMaui` (janusw)

| Campo | Detalle |
|---|---|
| **Original** | `Camera.MAUI` (hjam40) v1.5.1 (29/02/2024) · MIT · 507★ · ~705K desc. → **abandonado (net7/8, sin net9/10)** ❌ |
| **Fork mantenido** | `CameraMaui` (janusw) v**1.4.14** (07/02/2026): `net9.0-android35`, `net9.0-ios18` + net10 |
| **Nativo Android** | **Camera2 nativo**; decodificación **managed ZXing** (`ZXingBarcodeDecoder`) |
| **Nativo iOS** | **AVFoundation**; decodificación **managed ZXing** |
| **Confianza** | fork activo (releases hasta feb-2026), pero comunidad pequeña (~34K desc.) |
| **Permisos** | Android `CAMERA`/`RECORD_AUDIO`/`RECORD_VIDEO` · iOS `NSCameraUsageDescription` + `NSMicrophoneUsageDescription` |

**Integración mínima**

```csharp
// MauiProgram.cs
builder.UseMauiApp<App>().UseMauiCameraView();
```
```xml
xmlns:cv="clr-namespace:Camera.MAUI;assembly=Camera.MAUI"
<cv:CameraView x:Name="cameraView" BarCodeDetectionEnabled="True" />
```
```csharp
cameraView.BarCodeDecoder = new ZXingBarcodeDecoder();
cameraView.BarcodeDetected += (s, args) => { var text = args.Result[0].Text; };
```

**Pros:** un solo paquete para preview + foto + vídeo + QR; Camera2/AVFoundation nativos; MIT.
**Contras:** el **original está abandonado** (usar el fork con otro ID de NuGet); decodificación managed; pide permiso de micrófono aunque no se grabe.
**Cuándo usarlo:** solo si necesitas **cámara genérica además** del escaneo.

**Fuentes:** <https://github.com/hjam40/Camera.MAUI> · <https://github.com/janusw/CameraMaui/releases>

---

## 2.5 🔵 Comerciales nativos (Dynamsoft / Scandit / Scanbot)

| Paquete | Versión | Nativo | Autor / modelo | Notas clave |
|---|---|---|---|---|
| **Dynamsoft.BarcodeReaderBundle.Maui** | 11.4.1300 (20/05/2026) | motor propio | Dynamsoft · de pago (licencia por dispositivo) | **Único que añade x86 en Android**; cubre **Windows** (paquete hermano desktop); máxima robustez en códigos dañados/baja luz |
| **Scandit.DataCapture.Barcode.Maui** | 8.4.1 (23/06/2026) | motor propio | Scandit AG · de pago (por volumen) | Enterprise, MatrixScan/AR; requiere también `Scandit.DataCapture.Core.Maui` |
| **ScanbotBarcodeSDK.MAUI** | 8.0.0 (06/02/2026) | motor propio | Scanbot SDK GmbH · de pago (tarifa plana anual) | Offline, UI lista, coste predecible |

**Inicialización típica:** requieren clave de licencia, p. ej. Dynamsoft `await LicenseManager.InitLicenseAsync("LICENSE-KEY");`.
**Permisos:** Android `CAMERA` · iOS `NSCameraUsageDescription`.
**Cuándo usarlos:** precisión enterprise, soporte de **Android x86** (Dynamsoft) o **Windows desktop**, o necesidad de soporte comercial/SLA.

**Fuentes:** <https://www.nuget.org/packages/Dynamsoft.BarcodeReaderBundle.Maui> · <https://www.nuget.org/packages/Scandit.DataCapture.Barcode.Maui> · <https://www.nuget.org/packages/ScanbotBarcodeSDK.MAUI>

---

[⬅ Volver al índice](README.md) · [Siguiente: 3. Matriz RID ➡](03-matriz-rid.md)
