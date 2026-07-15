# Changelog

Cambios notables de los ejemplos de dispositivos MAUI (`Ejemplos_Maui_Devices`).
Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

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
