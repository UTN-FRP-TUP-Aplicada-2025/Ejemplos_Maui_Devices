# Changelog

Cambios notables de los ejemplos de dispositivos MAUI (`Ejemplos_Maui_Devices`).
Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

## [2026-07-16] — UX de impresión: errores con código, estados alcanzables y salida al cambiar de impresora

Aplica los hallazgos de `PrintThermal_Motor_Maui.Documentacion/Analisis/Analisis-UX-UI.md`.
Alcance: `Ejemplo_Maui_Hibrida` (copia `Integrada`) y `Ejemplo_MotorDSL_Dialog`.
**La librería `MotorDsl.*` no se modificó** (sigue en v1.0.13).

### Agregado

- **Catálogo de errores con código** (`Models/PrintFailure.cs`, `Models/PrinterErrorCatalog.cs`):
  14 códigos estables (`PRN-DOC-NET`, `PRN-HW-PAPER`, `PRN-DEV-ABSENT`…), uno por **acción
  distinta del usuario**. La UI muestra un mensaje accionable en español más el código; el
  mensaje original y la excepción se conservan en `TechnicalMessage`/`Exception` para log y
  reporte automático, y nunca se muestran crudos.
- **`DocumentResult` tipado** (`Ok` · `NetworkError` · `InvalidContract`) en `Ejemplo_Maui_Hibrida`:
  distingue «no llegó el comprobante» (reintentable, rehace el GET) de «el comprobante está mal»
  (va a soporte). Antes ambos eran un string vacío.
- **`DiscoverResult.PermissionRevoked`**: cubre el permiso `BLUETOOTH_CONNECT` revocado desde
  Ajustes con la app corriendo.
- **`PrinterService.OpenBluetoothSettings()`**: abre los ajustes de Bluetooth del SO, distintos de
  los de la aplicación. No enciende el adaptador (`BluetoothAdapter.Enable()` está deprecado desde
  Android 13); sólo lleva al panel donde el usuario puede hacerlo.
- **`ClearDefault`, `GetAlias`, `SetAlias`** en `PrinterService`: olvidar la predeterminada y
  ponerle un alias legible a una impresora, indexado por MAC.
- **Capa Busy durante el GET del comprobante**: antes el usuario esperaba hasta 30 s sin ninguna
  señal en pantalla.

### Corregido

- **El botón «Imprimir» no hacía nada si fallaba la red** (H-1). `PrintCommandHandler` retornaba
  antes de mostrar el overlay cuando el render fallaba, contradiciendo su propio comentario. Ante
  timeout, caída de red o backend no disponible, la pantalla simplemente no cambiaba. Ahora se
  delega **siempre** en el overlay, que es el único componente que puede comunicarle algo al usuario.
- **`BluetoothOff` era código inalcanzable** (H-2). El transport distingue el Bluetooth apagado y
  lanza, pero `ThermalPrinterService` captura esa excepción y devuelve lista vacía (por diseño: un
  transport caído no debe abortar el barrido de los demás), así que el flujo siempre caía en
  `Empty` → *«No se encontraron impresoras — Encendé la impresora»*, mandando al usuario a revisar
  la impresora cuando el problema estaba en el teléfono. `PrinterService.DiscoverAsync` ahora
  chequea adaptador y permiso **antes** de llamar a la librería.
- **Errores de impresión crudos y en inglés** (H-3). El usuario leía
  `Print failed after 1 attempt(s): paper out`. Ahora lee *«La impresora se quedó sin papel. Cargá
  un rollo nuevo y reintentá»* con el botón «Ya cargué papel — Reintentar», que refleja el gesto
  físico en lugar de una repetición ciega. La clasificación **reusa `PrintError.FromException`**
  (público en `MotorDsl.Core.Models`) en vez de reimplementarla.
- **Impresoras homónimas indistinguibles** (H-4). Dos `58HB6` producían dos botones idénticos. Ahora
  el selector usa el alias del usuario si existe, y si no agrega el sufijo de MAC (últimos 2
  octetos) **sólo cuando el nombre se repite**.
- **«Elegir otra» no elegía otra** (H-6). El descubrimiento sólo lista `BondedDevices`, así que una
  predeterminada emparejada pero ausente igual aparecía, se reusaba, fallaba al conectar, y «Elegir
  otra» volvía a descubrirla y reconectar a la misma: un bucle sin salida por UI. `BuscarYImprimirAsync`
  acepta ahora `forzarSelector`. El fallo de la predeterminada da `PRN-DEV-ABSENT`, que nombra las
  dos causas que el stack **no puede distinguir** (impresora apagada vs. impresora distinta a la
  emparejada: ambas son el mismo fallo de socket RFCOMM) y ofrece «Olvidar y emparejar otra».
- **Sustitución silenciosa de impresora** (H-7). La capa «Conectando» nombra la impresora.
- **Bloque de fallo de envío duplicado** entre `ConectarEImprimirAsync` y `ReintentarImprimir`:
  unificado en `EnviarAsync`.

### Notas

- El flujo normal **no cambia**: la impresora predeterminada se sigue reusando sin preguntar y sin
  interrupción. La salida para cambiarla aparece sólo cuando falla.
- El atasco de papel y la impresión desvanecida siguen **sin código y sin detección**, deliberadamente:
  `DLE EOT` no los reporta. La verificación final es visual.
- **Pendiente**: verificación en dispositivo Android real, en particular `ActionBluetoothSettings`.

## [2026-07-15] — Fix navegación WebView, overlay de impresión y permisos Bluetooth

### Agregado

- **`PrinterOverlayViewModel` integrado a `MainViewModel`/`MainPage.xaml`** en
  `Ejemplo_Maui_Hibrida`: se registra e inyecta junto a los overlays de GPS, Red
  y Llamada, y se agrega al layout con máxima prioridad visual.
- **Permisos Bluetooth en `AndroidManifest.xml`** (`BLUETOOTH`, `BLUETOOTH_ADMIN`,
  `BLUETOOTH_SCAN` con `neverForLocation`, `BLUETOOTH_CONNECT`) y query del intent
  `android.bluetooth.adapter.action.REQUEST_ENABLE`, necesarios para que
  `EnsurePermissionsAsync` no falle con `PermissionException`.

### Corregido

- **Doble navegación en el `WebView` al hacer pull-to-refresh.** `UrlCommandDispatcher`
  devolvía `url` como `NavigateTo` en cualquier navegación normal, provocando que
  `MainViewModel.Navigating` reasignara `Url`/`WebView.Source` y disparara una
  segunda navegación superpuesta que impedía cerrar el `RefreshView`
  (`IsRefreshing` no volvía a `false`). Ahora `NavigateTo` queda en `null` salvo
  para el caso GPS.

### Cambiado

- **`MotorDsl.*` 1.0.12 → 1.0.13** en `Ejemplo_MotorDSL_Dialog.csproj` (los 7
  paquetes), alineado con la versión ya usada en `Ejemplo_Maui_Hibrida`.
- Comentarios de depuración eliminados/reescritos en `PrintCommandHandler.cs`.

### Eliminado

- **Soporte Windows removido de `Ejemplo_Maui_Hibrida`**: `Platforms/Windows/`
  (`App.xaml`, `App.xaml.cs`, `Package.appxmanifest`, `app.manifest`) y las
  propiedades `SupportedOSPlatformVersion`/`TargetPlatformMinVersion` para
  `windows` en el `.csproj`.

## [2026-07-13] — Impresión térmica MotorDSL + reorganización a `LibApp/`

### Agregado

- **Impresión térmica Bluetooth (MotorDSL) en `Ejemplo_Maui_Hibrida`.** Integración
  del motor de documentos vía comando de URL desde el WebView:
  - `LibApp/UrlCommands/Handlers/PrintCommandHandler.cs` — handler `action=print`
    que trae el comprobante desde la API, lo renderiza con `IDocumentEngine` y
    delega el flujo de impresión al overlay.
  - `LibApp/Devices/MotorDSL/` — `PrinterService`, `PrinterOverlayViewModel`,
    `OverlayBlueToothThermalPrintPage`, `BluetoothPermissions`, y modelos
    (`BluetoothPermissionResult`, `DiscoverResult`, `PrintResult`).
  - DTOs del documento imprimible: `DTOs/Print/{PrintDocument,PrintNode,PrintStyle,N}`
    y `DTOs/LocationDto`.
- **Backend de ejemplo `Ejemplo_ws_Blazor`.** `TikectsController` (endpoint
  `comprobante`), DTOs `Print/*` y assets en `wwwroot` (`qr.ejemplo.png`,
  `pago-fake.html`) para servir el comprobante que consume la app.
- **READMEs** por área: `MotorDSL`, `Camera`, `Images`, `UrlCommands/Handlers`,
  `Devices`, y del ejemplo `Ejemplo_MotorDSL_Dialog`.
- **Utilidades**: `Utilities/flows/recorrido.yaml` y `Utilities/simular_ui.sh`.

### Cambiado

- **Reorganización de `Ejemplo_Maui_Hibrida` a estructura `LibApp/`** (~40 archivos
  movidos), agrupando por dominio: `CustomWebView/`, `UrlCommands/` y
  `Devices/{Camera,Common,GPS,Images,Networks,Phone,QRLector,MotorDSL}`.
- **`MotorDsl.*` actualizado de 1.0.12 → 1.0.13** en `Ejemplo_Maui_Hibrida.csproj`
  (los 7 paquetes). Incorpora el fix del render térmico (imágenes bitmap ya no
  devuelven el ticket en 0 bytes) y la propagación de `bold`.
- `MauiProgram.cs`, `AppShell.xaml.cs`, `Pages/MainPage.xaml` y
  `ViewModels/MainViewModel.cs` adaptados a la nueva estructura y al registro de
  los servicios de impresión.
- Ejemplo Blazor: `NavMenu.razor`, `Panel.razor`, `Redirigir.razor`, `Program.cs`.
- CI: workflow `cd-ios-gps.Ejemplo_Maui_Hibrida.yml`.
