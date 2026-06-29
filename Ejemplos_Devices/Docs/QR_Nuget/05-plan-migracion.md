# 5. Plan de migración: `BarcodeScanner.Mobile.Maui` → `BarcodeScanning.Native.Maui`

> Migración paso a paso de `QRLectorPage` (idéntico en ambos proyectos) hacia la opción recomendada
> en [04-recomendacion.md](04-recomendacion.md). **Documento de planificación: no se ha modificado
> código de los proyectos.** Los bloques `diff` muestran el cambio exacto a aplicar.

[⬅ Volver al índice](README.md) · [⬅ Anterior: 4. Recomendación](04-recomendacion.md)

---

## 5.0 Resumen del cambio

- **Quitar:** `BarcodeScanner.Mobile.Maui` 9.0.1 + el workaround `AdamE.Google.iOS.GoogleUtilities` (Rosetta).
- **Añadir:** `BarcodeScanning.Native.Maui` 3.0.4 (ML Kit en Android, **Apple Vision en iOS**).
- **Superficie afectada:** `*.csproj`, `MauiProgram.cs`, `Pages/QRLectorPage.xaml`, `Pages/QRLectorPage.xaml.cs` (×2 proyectos).
- **Sin cambios:** `MainPage`, `QrCommandHandler`, modelo `QRContent`, el patrón `TaskCompletionSource` y (en Híbrida) `OnQrCallback`/`QueryProperty`.

> ✅ **Nombres de API verificados** contra un **volcado offline** del `BarcodeScanning.Native.Maui.dll`
> **3.0.4** (misma técnica documentada para los DLL de MotorDsl en `E:\.nuget\packages`). Correcciones
> respecto de borradores previos de este plan, ya aplicadas más abajo:
> - La propiedad XAML del control es **`BarcodeSymbologies`** (NO `Symbologies`).
> - El valor del enum es **`BarcodeFormats.QRCode`** (NO `QrCode`).
> - La extensión del builder es **`UseBarcodeScanning()`** (NO `UseCameraScanner()`, que pertenece a *CameraScanner.Maui*).
> - **NO** importar `using Xamarin.Google.MLKit.Vision.BarCode;` (rompe la compilación — ver [5.8](#58-notas-y-riesgos)).

---

## 5.1 Pre-requisitos

| Requisito | Detalle |
|---|---|
| **TFM** | Ambos proyectos ya son **net10** (cumple: 3.0.4 es net10-only). |
| **MauiVersion** | La 3.0.4 exige `Microsoft.Maui.Controls >= 10.0.20`. Verificar `$(MauiVersion)`. |
| **Workloads** | El paquete apunta a `net10.0-ios26.0` / `net10.0-android36.0`: tener instaladas las workloads MAUI de .NET 10 con esos SDK de plataforma (`dotnet workload update`). `SupportedOSPlatformVersion` (iOS 15.0 / Android 25.0) **no cambia**. |
| **Permisos** | Android `CAMERA`+`VIBRATE`; iOS `NSCameraUsageDescription`. Ver [5.6](#56-permisos). |

---

## 5.2 Paso 1 — `*.csproj`

Aplicar en **ambos** `.csproj` (`Ejemplo_LectorQR_Dialog.csproj` y `Ejemplo_Maui_Hibrida.csproj`).

```diff
- <ItemGroup Condition="'$(RuntimeIdentifier)' == 'iossimulator-x64'">
- 	<PackageReference Include="AdamE.Google.iOS.GoogleUtilities" Version="8.1.0.3" />
- </ItemGroup>
-
  <ItemGroup>
- 	<PackageReference Include="BarcodeScanner.Mobile.Maui" Version="9.0.1" />
+ 	<PackageReference Include="BarcodeScanning.Native.Maui" Version="3.0.4" />
  </ItemGroup>
```

- Se **elimina** el `ItemGroup` condicional de `AdamE.Google.iOS.GoogleUtilities`: ya no hace falta porque iOS deja de usar ML Kit → **adiós al workaround Rosetta/`iossimulator-x64`**.
- El resto del CSPROJ (Android `max-page-size=16384`, bloque iOS Release `UseInterpreter`/`PublishTrimmed`/`MtouchLink=SdkOnly`) puede quedarse igual. *Opcional a futuro:* sin ML Kit podrías reevaluar el trimming/AOT, pero no es necesario para migrar.

---

## 5.3 Paso 2 — `MauiProgram.cs`

```diff
- using BarcodeScanner.Mobile;
+ using BarcodeScanning;
  using Microsoft.Extensions.Logging;
  ...
  builder
      .UseMauiApp<App>()
      .ConfigureFonts(fonts =>
      {
          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
          fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
-     })
-     #region barcode scanner
-     .ConfigureMauiHandlers(handlers =>
-     {
-         handlers.AddBarcodeScannerHandler();
-     });
-     #endregion
+     })
+     .UseBarcodeScanning();
```

> En Híbrida, `.UseBarcodeScanning()` se encadena igual; mantener intacto el resto del registro
> (CommunityToolkit, MotorDsl, los handlers propios). Si `ConfigureMauiHandlers` se usa solo para el
> barcode, se elimina por completo; si registra otros handlers, conservar el bloque y quitar solo la
> línea `handlers.AddBarcodeScannerHandler();`.

---

## 5.4 Paso 3 — `Pages/QRLectorPage.xaml`

```diff
  <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
               xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
-              xmlns:gv="clr-namespace:BarcodeScanner.Mobile;assembly=BarcodeScanner.Mobile.Maui"
+              xmlns:scanner="clr-namespace:BarcodeScanning;assembly=BarcodeScanning.Native.Maui"
               ...>
  ...
-     <gv:CameraView x:Name="Camera" Grid.Row="1" Grid.Column="0"
-                    OnDetected="OnCameraViewOnDetecte"
-                    HorizontalOptions="Fill" VerticalOptions="Fill"
-                    TorchOn="False" VibrationOnDetected="False" ScanInterval="50"/>
+     <scanner:CameraView x:Name="Camera" Grid.Row="1" Grid.Column="0"
+                    OnDetectionFinished="OnCameraViewOnDetecte"
+                    HorizontalOptions="Fill" VerticalOptions="Fill"
+                    CameraEnabled="True"
+                    TorchOn="False" VibrationOnDetected="False"
+                    BarcodeSymbologies="QRCode,Code39" />
```

Cambios clave:
- `OnDetected` → **`OnDetectionFinished`** (mismo handler `OnCameraViewOnDetecte`).
- **`CameraEnabled="True"`**: la nueva cámara **no arranca sola**; hay que habilitarla (la vieja la iniciaba el handler).
- **`BarcodeSymbologies="QRCode,Code39"`** reemplaza a `Methods.SetSupportBarcodeFormat(...)` (ahora declarativo en el control). ⚠️ La propiedad es `BarcodeSymbologies` (no `Symbologies`) y el valor del enum es `QRCode` (no `QrCode`).
- `ScanInterval="50"` **no tiene equivalente directo**; se elimina. Si se quiere agrupar detecciones por intervalo, usar `PoolingInterval` (ms) — opcional.
- `TorchOn` y `VibrationOnDetected` se conservan (mismos nombres).

---

## 5.5 Paso 4 — `Pages/QRLectorPage.xaml.cs`

**(a) `using` y constructor**

```diff
- using BarcodeScanner.Mobile;
+ using BarcodeScanning;   // ⚠️ NO agregar `using Xamarin.Google.MLKit.Vision.BarCode;` (ver 5.8)
  ...
  public QRLectorPage()
  {
      InitializeComponent();
-     BarcodeScanner.Mobile.Methods.SetSupportBarcodeFormat(
-         BarcodeScanner.Mobile.BarcodeFormats.QRCode | BarcodeScanner.Mobile.BarcodeFormats.Code39);
+     // Formatos ahora se declaran en XAML (BarcodeSymbologies="QRCode,Code39").
+     // Alternativa por código: Camera.BarcodeSymbologies = BarcodeFormats.QRCode | BarcodeFormats.Code39;
      BindingContext = this;
  }
```

**(b) Permiso de cámara**

```diff
  async public Task<bool> RequestCameraPermission()
  {
-     bool allowed = await BarcodeScanner.Mobile.Methods.AskForRequiredPermission();
+     bool allowed = await BarcodeScanning.Methods.AskForRequiredPermissionAsync();
      return allowed;
  }
```

**(c) Handler de detección** (cambia el tipo del `EventArg`, la colección pasa a `foreach`, y el stop de cámara)

```diff
- async private void OnCameraViewOnDetecte(object sender, BarcodeScanner.Mobile.OnDetectedEventArg e)
+ private void OnCameraViewOnDetecte(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
  {
-     List<BarcodeResult> obj = e.BarcodeResults;
-     List<QRContent> QRs = new List<QRContent>();
-     for (int i = 0; i < obj.Count; i++)
-     {
-         string type = obj[i].BarcodeType == BarcodeTypes.Unknown ? "Text" : obj[i].BarcodeType.ToString();
-         var qr = new QRContent { Type = type, Value = obj[i].DisplayValue };
-         QRs.Add(qr);
-     }
+     var QRs = new List<QRContent>();
+     foreach (var b in e.BarcodeResults)
+     {
+         string type = b.BarcodeType == BarcodeTypes.Unknown ? "Text" : b.BarcodeType.ToString();
+         QRs.Add(new QRContent { Type = type, Value = b.DisplayValue });
+     }

      this.Dispatcher.Dispatch(async () =>
      {
-         Camera.IsScanning = false;
+         Camera.CameraEnabled = false;   // detener cámara (antes: IsScanning = false)
          CompletarResultado(QRs);
          await Navigation.PopAsync();
      });
  }
```

> - Se quita `async` del método (ya no hay `await` directo; el `await` vive dentro del lambda de `Dispatcher.Dispatch`) para evitar el warning CS1998.
> - `e.BarcodeResults` en la nueva librería es un conjunto (`IReadOnlySet<BarcodeResult>`), por eso se itera con `foreach` en vez de índice.
> - `BarcodeTypes.Unknown`, `BarcodeType` y `DisplayValue` mantienen el mismo nombre → el mapeo a `QRContent` no cambia.

**(d) Linterna** — `OnActiveFlashClicked`, `PaintFlashStatus` usan `Camera.TorchOn`, que **se conserva igual**: no requieren cambios.

---

## 5.6 Permisos

| Plataforma | Estado actual | Acción |
|---|---|---|
| Android — Dialog | `CAMERA` + `VIBRATE` ya declarados | ✅ Sin cambios |
| Android — Híbrida | `CAMERA` declarado, **falta `VIBRATE`** | Añadir `<uses-permission android:name="android.permission.VIBRATE" />` **solo si** activarás `VibrationOnDetected="True"` (hoy está en `False` → opcional) |
| iOS — ambos | `NSCameraUsageDescription` ya presente | ✅ Sin cambios |

> La cadena nativa de ML Kit en iOS desaparece, por lo que **no se necesitan entitlements nuevos** ni el paquete de simulador.

---

## 5.7 Tabla de equivalencias de API

| `BarcodeScanner.Mobile` (actual) | `BarcodeScanning.Native.Maui` (nuevo) |
|---|---|
| `using BarcodeScanner.Mobile;` | `using BarcodeScanning;` |
| `handlers.AddBarcodeScannerHandler()` | `.UseBarcodeScanning()` (en el builder) |
| xmlns `BarcodeScanner.Mobile;assembly=BarcodeScanner.Mobile.Maui` | xmlns `BarcodeScanning;assembly=BarcodeScanning.Native.Maui` |
| `<CameraView OnDetected="...">` | `<CameraView OnDetectionFinished="...">` |
| `Methods.SetSupportBarcodeFormat(BarcodeFormats.QRCode\|Code39)` | `BarcodeSymbologies="QRCode,Code39"` (XAML) o `Camera.BarcodeSymbologies = BarcodeFormats.QRCode \| BarcodeFormats.Code39` |
| `Methods.AskForRequiredPermission()` | `Methods.AskForRequiredPermissionAsync()` |
| `OnDetectedEventArg` | `OnDetectionFinishedEventArg` |
| `e.BarcodeResults` → `List<BarcodeResult>` | `e.BarcodeResults` → `IReadOnlySet<BarcodeResult>` (usar `foreach`) |
| `BarcodeResult.BarcodeType` / `BarcodeTypes` | igual — `BarcodeTypes` lo provee la **propia** librería (`BarcodeScanning.BarcodeTypes`); **no** importar el de ML Kit |
| `CameraFacing` (si se usa) | `BarcodeScanning.CameraFacing` (`Back`/`Front`) — **no** existe `FromArrayMobile.CameraFacing` |
| `BarcodeResult.DisplayValue` | igual |
| `Camera.IsScanning = false` | `Camera.CameraEnabled = false` (o `PauseScanning = true`) |
| `Camera.TorchOn` | igual |
| `VibrationOnDetected` | igual |
| `ScanInterval` | sin equivalente directo (≈ `PoolingInterval`, opcional) |
| arranque automático | requiere `CameraEnabled="True"` |

---

## 5.8 Notas y riesgos

- ✅ **Casing/nombres exactos (verificados en el DLL 3.0.4):** el valor del enum es **`BarcodeFormats.QRCode`** (NO `QrCode`); la propiedad del control es **`BarcodeSymbologies`** (NO `Symbologies`); la extensión del builder es **`UseBarcodeScanning()`** (NO `UseCameraScanner()`, que es de *CameraScanner.Maui*); el enum de cámara es **`BarcodeScanning.CameraFacing`** (`Back`/`Front`). `e.BarcodeResults` es `IReadOnlySet<BarcodeResult>` (iterar con `foreach`). Volcado offline del DLL en `E:\.nuget\packages\barcodescanning.native.maui\3.0.4\lib\net10.0-android36.0\`.
- ⚠️ **Colisión de namespace con ML Kit (Android):** **NO** agregar `using Xamarin.Google.MLKit.Vision.BarCode;`. Ese namespace define un **tipo** llamado `BarcodeScanning` que **eclipsa al namespace** `BarcodeScanning` de la librería; entonces `BarcodeScanning.OnDetectionFinishedEventArg` deja de resolverse y la compilación falla con `CS0426: El nombre de tipo 'OnDetectionFinishedEventArg' no existe en el tipo 'BarcodeScanning'`. Con `using BarcodeScanning;` ya tenés `BarcodeTypes`, `BarcodeResult`, `OnDetectionFinishedEventArg`, etc.
- **Simulador iOS:** tras migrar, compila/corre en **simulador arm64 (Apple Silicon)** sin Rosetta; pero el simulador **no tiene cámara real**, así que el escaneo en vivo se valida igualmente en **iPhone físico**.
- **Android x86:** ML Kit no da x86 (igual que el paquete actual): el escaneo no corre en emuladores x86/x86_64. Probar en **Moto g42 físico (arm64)**.
- **Permiso no bloqueante:** el código actual ignora el `bool` de permiso en `OnAppearing`. Buen momento para, si se deniega, no habilitar `CameraEnabled` y mostrar aviso (mejora opcional).

---

## 5.9 Orden de ejecución y verificación

1. Migrar primero **`Ejemplo_LectorQR_Dialog`** (minimalista, menor riesgo).
2. `dotnet restore` y compilar `net10.0-android`.
3. **Desplegar y probar en Moto g42** (escaneo QR + Code39 + botón linterna + botón volver).
4. Compilar `net10.0-ios` y correr en **simulador arm64** (verifica build sin Rosetta) y luego en **iPhone físico** (escaneo real).
5. Repetir los mismos cambios en **`Ejemplo_Maui_Hibrida`** (el `QRLectorPage` es idéntico; `OnQrCallback`/`QueryProperty` y `QrCommandHandler` no se tocan). Validar el flujo desde el WebView.

### Checklist

- [ ] `.csproj`: quitado `BarcodeScanner.Mobile.Maui` + `AdamE.Google.iOS.GoogleUtilities`; añadido `BarcodeScanning.Native.Maui` 3.0.4 (×2)
- [ ] `MauiProgram.cs`: `using` + `.UseBarcodeScanning()` (×2)
- [ ] `QRLectorPage.xaml`: xmlns + `OnDetectionFinished` + `CameraEnabled` + `BarcodeSymbologies="QRCode,..."` (×2)
- [ ] `QRLectorPage.xaml.cs`: `using`, formatos, `AskForRequiredPermissionAsync`, handler `foreach`, `CameraEnabled=false` (×2)
- [ ] Permisos revisados (VIBRATE en Híbrida solo si aplica)
- [ ] Probado: Moto g42 (Android) + simulador iOS arm64 + iPhone físico

### Rollback

`git revert`/`git checkout` de los 4 archivos por proyecto. No hay migraciones de datos ni cambios de esquema → reversión limpia.

---

[⬅ Volver al índice](README.md)
