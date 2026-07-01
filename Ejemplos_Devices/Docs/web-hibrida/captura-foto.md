# Captura de foto y selfie en la arquitectura Web ↔ MAUI Híbrida

> Documenta la **secuencia de llamadas** y la **lógica asociada** cuando la página Blazor
> (`Ejemplo_ws_Blazor`) pide **tomar una foto** o **una selfie** y la app contenedora
> (`Ejemplo_Maui_Hibrida`) intercepta la URL, abre la cámara nativa, **normaliza** la imagen y la
> devuelve al DOM **sin recargar** la página.
>
> Alcance: los comandos `photo=photo` (cámara trasera) y `selfie=selfie` (cámara frontal). Ambos
> comparten handler casi idéntico y el mismo patrón de callback. El mecanismo general del puente
> (Canal B) está en [`maui-hibrido.md`](./maui-hibrido.md); el caso QR en
> [`lectura-qr.md`](./lectura-qr.md).

---

## 1. Panorama: rol dentro del Canal B

Foto y selfie son **dos comandos** del puente nativo por URL. Ambos siguen el patrón de "abrir una
página modal, esperar un resultado con `TaskCompletionSource`, inyectar JS y quedarse en la página"
(rama `NavigateTo == null` de `BridgeOutcome`). La diferencia con QR es **qué** se abre (la cámara de
captura en vez del lector de códigos) y **qué** se inyecta (una imagen base64 en vez de texto).

Son **gemelos**: `CameraCommandHandler` + `MyMediaPickerPage` (trasera) y
`SelfieCommandHandler` + `MyMediaSelfiePickerPage` (frontal, con máscara). El servicio de
normalización de imagen (`IImageService`) es **compartido**.

```
Panel.razor (OnTomarFoto / OnTomarSelfie, forceLoad)
   └─► WebView.Navigating ─► MainViewModel.Navigating
            ├─ IsCommand → e.Cancel = true
            └─ Dispatcher.DispatchAsync
                   └─► Camera/SelfieCommandHandler.HandleAsync
                          ├─ GoToAsync(MyMedia[Selfie]PickerPage) ──► cámara captura
                          │                                              └─ Pop + callback(tempPath)
                          ├─ await tempPath
                          ├─ IImageService.ProcesarPhotoAsync (EXIF + resize + JPEG)
                          ├─ base64 → dataURI
                          └─ bridge.RunScript(JS) ─► img.src / input.value
```

---

## 2. Componentes que participan

| # | Componente | Archivo | Rol |
|---|---|---|---|
| 1 | `Panel.razor` | `Ejemplo_ws_Blazor/Components/Pages/Panel.razor` | Dispara la URL (`OnTomarFoto`/`OnTomarSelfie`) y aloja `<img>`/`<input>` destino |
| 2 | `CameraCommandHandler` | `UrlCommands/Handlers/CameraCommandHandler.cs` | Orquesta la foto (cámara trasera) |
| 3 | `SelfieCommandHandler` | `UrlCommands/Handlers/SelfieCommandHandler.cs` | Orquesta la selfie (cámara frontal) |
| 4 | `MyMediaPickerPage` | `Pages/MyMediaPickerPage.xaml(.cs)` | Página de captura (cámara trasera) |
| 5 | `MyMediaSelfiePickerPage` | `Pages/MyMediaSelfiePickerPage.xaml(.cs)` | Página de captura (cámara frontal + máscara) |
| 6 | `IImageService` / `ImageDeviceAutoRotateService` | `Services/*` | Normaliza: corrige orientación EXIF, redimensiona, recomprime a JPEG |
| 7 | `IWebViewBridge` + `WebViewBridgeBehavior` | `Behaviors/*` | Inyectan el JS en el DOM vivo (`EvaluateJavaScriptAsync`) |
| 8 | `BridgeOutcome` | `UrlCommands/BridgeOutcome.cs` | Resultado: cancelar navegación, sin re-navegar |

Registro DI: handlers en `MauiProgram.cs:92-93`; `IImageService` en `MauiProgram.cs:77`; páginas
`Transient` en `MauiProgram.cs:78-79`. Rutas: `AppShell.xaml.cs:11` (foto).
La selfie **no** aparece en `AppShell.xaml.cs`; se resuelve por su nombre de tipo vía `GoToAsync`.

---

## 3. La convención de URL (el "protocolo")

```
/panel?photo=photo&param=imgFoto1          (foto: cámara trasera)
/panel?selfie=selfie&param=Selfie1         (selfie: cámara frontal)
        │            │
        │            └── param: id del elemento del DOM donde inyectar la imagen
        └── comando que la app reconoce e intercepta
```

- **`photo=photo`** → `CameraCommandHandler.CanHandle` (`CameraCommandHandler.cs:21-22`).
- **`selfie=selfie`** → `SelfieCommandHandler.CanHandle` (`SelfieCommandHandler.cs:21-22`).
- **`param`** → `id` del elemento destino. Si falta, el handler cancela sin hacer nada
  (`CameraCommandHandler.cs:26-28`).

Origen en la web:

```csharp
// Panel.razor:191-194
public void OnTomarFoto()
    => Navigation.NavigateTo("/panel?photo=photo&param=imgFoto1", forceLoad: true);

// Panel.razor:215-218
public void OnTomarSelfie()
    => Navigation.NavigateTo("/panel?selfie=selfie&param=Selfie1", forceLoad: true);
```

Elementos destino en la web:

```razor
@* foto — Panel.razor:48-50 *@
<img id="imgFoto1" src="assets/images/no_foto.png" ... />
<input id="inputFoto1" type="hidden" name="inputFoto1" />

@* selfie — Panel.razor:66-67 *@
<img id="imgSelfie1" src="assets/images/no_foto.png" ... />
<input id="inputSelfie1" type="hidden" name="inputSelfie1" />
```

> `forceLoad: true` es obligatorio para que el `WebView` dispare `Navigating` y la app intercepte
> (igual que en QR). Ver [`maui-hibrido.md`](./maui-hibrido.md) §5.

---

## 4. Secuencia de llamadas (camino feliz)

### 4.1 Diagrama de secuencia

```mermaid
sequenceDiagram
    participant Web as Panel.razor (Blazor)
    participant WV as WebView
    participant VM as MainViewModel
    participant H as CameraCommandHandler
    participant Page as MyMediaPickerPage (cámara)
    participant Img as ImageService
    participant Bridge as WebViewBridge + Behavior

    Web->>WV: NavigateTo("/panel?photo=photo&param=imgFoto1", forceLoad:true)
    WV->>VM: Navigating(e)
    VM->>WV: e.Cancel = true  (IsCommand → síncrono)
    VM->>H: DispatchAsync → HandleAsync(url)
    H->>Page: Shell.GoToAsync(MyMediaPickerPage, {OnPhotoCallback})
    Note over Page: OnAppearing → permiso cámara → visor
    Page->>Page: OnTomarFotoClicked → CaptureImage → OnMediaCaptured
    Page->>Page: guarda Stream en CacheDirectory (tempPath)
    Page-->>H: callback.Invoke(tempPath) + GoToAsync("..")
    H->>Img: ProcesarPhotoAsync(tempPath)  (EXIF + resize + JPEG q75)
    Img-->>H: byte[]
    H->>H: File.Delete(tempPath) · base64 → dataURI
    H->>Bridge: RunScript("img.src = dataURI; img.value = dataURI")
    Bridge->>WV: EvaluateJavaScriptAsync(js)  (UI thread)
    WV->>Web: <img> muestra la foto (sin recargar)
    H-->>VM: BridgeOutcome(CancelNavigation:true, NavigateTo:null)
```

### 4.2 Intercepción y dispatch

Idéntico a QR: `MainPage.xaml:30-32` (EventToCommandBehavior) → `MainViewModel.Navigating`
(`MainViewModel.cs:69-81`) cancela síncronamente y delega en `UrlCommandDispatcher`, que aplica
*first-match-wins* sobre el orden **Gps → Call → Camera → Selfie → Qr → SendApi**
(`MauiProgram.cs:90-95`). `photo=photo` gana en Camera; `selfie=selfie` en Selfie.

### 4.3 Orquestación (`CameraCommandHandler.HandleAsync`)

`CameraCommandHandler.cs:24-62` (el de selfie es idéntico salvo la página destino):

```csharp
var targetId = GetQueryValue(url, "param");             // "imgFoto1"
if (string.IsNullOrEmpty(targetId)) return new BridgeOutcome(true, null);

var tcs = new TaskCompletionSource<string?>();
Action<string?> callback = p => tcs.TrySetResult(p);

await Shell.Current.GoToAsync(nameof(MyMediaPickerPage),
    new ShellNavigationQueryParameters { { "OnPhotoCallback", callback } });

var tempPath = await tcs.Task;                          // null = canceló
if (string.IsNullOrEmpty(tempPath)) return new BridgeOutcome(true, null);

byte[]? bytes;
using (var fs = File.OpenRead(tempPath))
    bytes = await _img.ProcesarPhotoAsync(fs);          // normalización
try { File.Delete(tempPath); } catch { }

if (bytes is null) return new BridgeOutcome(true, null);

var dataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}";

string scriptjs = $@"document.getElementById('{targetId}').src = '{dataUri}';
document.getElementById('{targetId}').value = '{dataUri}';";
_bridge.RunScript(scriptjs);

return new BridgeOutcome(true, null);                   // se queda en la página
```

> **Clave — callback vía `[QueryProperty]`:** a diferencia de QR, aquí **sí** está completo el
> cableado. `MyMediaPickerPage` declara `[QueryProperty(nameof(OnPhotoCallback), "OnPhotoCallback")]`
> (`MyMediaPickerPage.xaml.cs:8`) y expone `public Action<string?>? OnPhotoCallback`
> (`:15`). Por eso el `callback` del handler **sí** se invoca y `tcs.Task` se completa. (En QR falta
> ese `[QueryProperty]`; ver [`lectura-qr.md`](./lectura-qr.md) §7.)

### 4.4 Captura en la página (`MyMediaPickerPage`)

- `OnAppearing` → `EvaluarYMostrarEstadoPermisoAsync` (`:87-138`): resuelve el permiso de cámara y,
  si está concedido y `MediaPicker.IsCaptureSupported`, muestra el visor. Guardia iOS: en simulador
  (`DeviceType.Virtual`) muestra overlay de "cámara no disponible" (`:90-98`).
- `OnTomarFotoClicked` (`:226-258`) → `CameraView.CaptureImage`.
- `OnMediaCaptured` (`:260-306`): materializa el `Stream` a un archivo temporal en
  `FileSystem.CacheDirectory` **fuera del UI thread**, luego salta al UI thread para
  `callback?.Invoke(tempPath)` + `GoToAsync("..")` (`:301-304`).
- Cancelar: `OnVolverClicked` (`:197-202`) → `OnPhotoCallback?.Invoke(null)` (`:200`).

> **Clave — el archivo temporal, no el `Stream`:** el `Stream` del toolkit se copia a un `.jpg` en
> `CacheDirectory` (`:269-276`) antes de cruzar de página; así el stream "nace y muere en su hilo
> natural". El handler recibe un **path**, procesa el archivo y lo borra (`CameraCommandHandler.cs:46`).

### 4.5 Normalización de la imagen (`ImageDeviceAutoRotateService`)

`ProcesarPhotoAsync(Stream)` (`ImageDeviceAutoRotateService.cs:15-67`):

1. Lee la **orientación EXIF** (`MetadataExtractor`) y rota el bitmap con SkiaSharp para dejarlo
   derecho (`AplicarOrientation`, `:93-138`; cubre los 8 casos EXIF).
2. **Redimensiona**: al `CustomPhotoSize` = 50 % o, si algún lado supera `MaxWidthHeight` = 1000 px,
   escala para que el lado mayor sea 1000 px (`:40-51`).
3. **Recomprime** a JPEG `CompressionQuality` = 75 (`:57-58`) → `byte[]`.

Defaults en `:11-13`. El handler convierte ese `byte[]` a `data:image/jpeg;base64,…`.

### 4.6 Inyección y cierre

`_bridge.RunScript(js)` → `WebViewBridgeBehavior.OnScriptRequested` (`WebViewBridgeBehavior.cs:65`) →
`EvaluateJavaScriptAsync` en UI thread → el `<img>` muestra la foto. `BridgeOutcome(true, null)` → se
queda en la página (sin re-navegar). Ver [`maui-hibrido.md`](./maui-hibrido.md) §5 para el detalle del
puente.

---

## 5. Variante selfie: diferencias con foto

`SelfieCommandHandler` y `MyMediaSelfiePickerPage` son el gemelo frontal. Diferencias reales:

| Aspecto | Foto (`MyMediaPickerPage`) | Selfie (`MyMediaSelfiePickerPage`) |
|---|---|---|
| Cámara | Trasera (`CameraPosition.Rear`, `:213`) | Frontal (`CameraPosition.Front`, `:178-182`) |
| Momento de seleccionar cámara | Tras `OnNavigatedTo` (`:76-83`) | **Pre-selección antes de adjuntar** al árbol visual (`:173-190`) para que el handler nativo nazca con la cámara ya elegida |
| Máscara | — | `SelfieMaskDrawable` (óvalo); se re-dibuja al cambiar el tema (`:50-53`) |
| `MediaCaptured` | callback + `GoToAsync("..")` (`:301-304`) | callback + `GoToAsync("..")` (`:372-383`) |

El handler (`SelfieCommandHandler.cs`) es **idéntico** al de foto salvo `CanHandle` (`selfie=selfie`)
y la página destino (`nameof(MyMediaSelfiePickerPage)`, `:34`). Reusa el **mismo** `IImageService` y el
**mismo** parámetro `OnPhotoCallback`.

---

## 6. Lógica y decisiones de diseño

| Decisión | Dónde | Por qué |
|---|---|---|
| Callback por `[QueryProperty]` + `TaskCompletionSource` | `MyMediaPickerPage.xaml.cs:8,15`; `CameraCommandHandler.cs:31-37` | Esperar la captura como un `await` lineal (patrón "modal async") |
| Pasar **path** temporal, no `Stream` | `MyMediaPickerPage.xaml.cs:260-306` | El stream del toolkit no debe cruzar páginas/hilos |
| Normalizar antes de inyectar (EXIF+resize+JPEG) | `ImageDeviceAutoRotateService.cs` | Corregir rotación del sensor y bajar el peso del base64 |
| Borrar el temporal tras procesar | `CameraCommandHandler.cs:46` | No dejar fotos sin usar en `CacheDirectory` |
| Handler gemelo por cámara | `Camera`/`SelfieCommandHandler` | Un comando = una clase (abierto/cerrado); reúsa servicio y callback |
| Pre-seleccionar cámara frontal antes del attach | `MyMediaSelfiePickerPage.xaml.cs:173-190` | Evitar el "parpadeo" a cámara trasera al crear el handler nativo |
| `BridgeOutcome(true, null)` | ambos handlers | Se queda en la página; el resultado llega por JS, no por navegación |

---

## 7. Variante: disparo desde botón nativo

No hay botón nativo específico de foto/selfie en `MainPage.xaml` (los botones del pie son GPS, Llamar y
QR, `MainPage.xaml:50-52`). Foto/selfie se disparan **sólo desde la web**. Aun así, como todo pasa por
el mismo dispatcher, agregar un botón nativo sería `_dispatcher.DispatchAsync("photo=photo&param=…")`
(igual que `TakeQR` en `MainViewModel.cs:47-51`).

---

## 8. ⚠️ Observaciones sobre el cableado `param` ↔ DOM

> Al momento de escribir este doc, hay dos desajustes entre el `param` que envía la web y los `id`
> reales del DOM. El flujo nativo (cámara, normalización) funciona; lo que falla es **dónde aterriza**
> la imagen.

### 8.1 Foto: la imagen se ve, pero "Mostrar contenido" queda vacío

`OnTomarFoto` manda `param=imgFoto1`. El handler inyecta sobre **ese único id**:

```js
document.getElementById('imgFoto1').src   = 'data:image/jpeg;base64,…';  // ✔ el <img> muestra la foto
document.getElementById('imgFoto1').value = 'data:image/jpeg;base64,…';  // ✗ .value sobre un <img> = no-op
```

Pero `OnMostrarContenidoFoto` (`Panel.razor:195-198`) lee el **input oculto** `inputFoto1`, que el
handler **nunca escribe**:

```csharp
inputFoto1 = await JSRuntime.InvokeAsync<string>("eval",
    "document.getElementById('inputFoto1')?.value ?? ''");   // siempre ""
```

Resultado: la foto **se muestra**, pero el base64 no es recuperable por "Mostrar contenido". La
intención original (ver el código de prueba comentado en `Panel.razor:204-205`) era escribir
`imgFoto1.src` **y** `inputFoto1.value` (dos ids distintos), pero el handler recibe **un solo** `param`
y escribe `.src` y `.value` sobre el mismo elemento.

### 8.2 Selfie: `param` no coincide con ningún `id` → la imagen no se muestra

`OnTomarSelfie` manda `param=Selfie1`, pero en el DOM los elementos son `imgSelfie1` e `inputSelfie1`
(`Panel.razor:66-67`). El handler ejecuta:

```js
document.getElementById('Selfie1').src = '…';   // getElementById('Selfie1') === null → TypeError
```

Como no existe ningún elemento con id `Selfie1`, `getElementById` devuelve `null` y la asignación
**lanza** en el JS inyectado (que `EvaluateJavaScriptAsync` descarta silenciosamente). La selfie **no
se muestra** por este camino.

### 8.3 Forma del arreglo (a confirmar)

- Unificar el contrato: o el `param` referencia el `<img>` **y** el handler recibe un segundo id para
  el `<input>`, o el handler escribe `.src` en `img{param}` y `.value` en `input{param}` por convención
  de nombres.
- Para selfie, alinear `param` con un id existente (`imgSelfie1`) o renombrar el elemento.

> Este documento **describe**, no corrige.

---

## 9. Referencia rápida de archivos

| Archivo | Líneas clave |
|---|---|
| `Ejemplo_ws_Blazor/Components/Pages/Panel.razor` | `48-50` (foto DOM), `66-67` (selfie DOM), `191-198` (`OnTomarFoto`/`OnMostrarContenidoFoto`), `215-222` (`OnTomarSelfie`/`OnMostrarContenidoSelfie`) |
| `Ejemplo_Maui_Hibrida/UrlCommands/Handlers/CameraCommandHandler.cs` | `21-22` (`CanHandle`), `24-62` (`HandleAsync`), `56-57` (JS) |
| `Ejemplo_Maui_Hibrida/UrlCommands/Handlers/SelfieCommandHandler.cs` | `21-22`, `24-62`, `34` (página destino) |
| `Ejemplo_Maui_Hibrida/Pages/MyMediaPickerPage.xaml.cs` | `8`,`15` (`[QueryProperty]`/prop), `226-258` (capturar), `260-306` (`OnMediaCaptured`), `197-202` (cancelar) |
| `Ejemplo_Maui_Hibrida/Pages/MyMediaSelfiePickerPage.xaml.cs` | `173-190` (pre-select frontal), `341-384` (`OnMediaCaptured`), `269-274` (cancelar) |
| `Ejemplo_Maui_Hibrida/Services/ImageDeviceAutoRotateService.cs` | `11-13` (defaults), `15-67` (`ProcesarPhotoAsync`), `93-138` (EXIF) |
| `Ejemplo_Maui_Hibrida/Behaviors/WebViewBridgeBehavior.cs` | `65` (`EvaluateJavaScriptAsync`) |
| `Ejemplo_Maui_Hibrida/MauiProgram.cs` | `77` (`IImageService`), `78-79` (páginas), `92-93` (handlers) |
| `Ejemplo_Maui_Hibrida/AppShell.xaml.cs` | `11` (ruta foto) |

> Relacionado: [`maui-hibrido.md`](./maui-hibrido.md) (Canal B y GPS) · [`lectura-qr.md`](./lectura-qr.md) ·
> [`llamada.md`](./llamada.md) · [`envio-api.md`](./envio-api.md).
