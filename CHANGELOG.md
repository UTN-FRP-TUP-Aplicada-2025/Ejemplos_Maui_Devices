# Changelog

Cambios notables de los ejemplos de dispositivos MAUI (`Ejemplos_Maui_Devices`).
Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

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
