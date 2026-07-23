# Changelog

Cambios notables de los ejemplos de dispositivos MAUI (`Ejemplos_Maui_Devices`).
Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

## [2026-07-23] — Panel con ambos modos de GPS y normalización de namespaces en `LibApp`

Alcance: `Ejemplo_ws_Blazor/Components/Pages/Panel.razor` + namespaces/usings de
`Ejemplo_Maui_Hibrida/LibApp/**` y `MauiProgram.cs`. Complemento del Plan 1: al haber pasado el
camino web de `Injection` a `Substitution`, el panel se había quedado sin forma de ejercitar la
inyección en el DOM.

### Agregado

- **Tarjeta «Solicitar GeoPosicion» en `Panel.razor` (modo `Injection`).** Navega a
  `/panel?coordenadas=coordenadas&param=contenidoCoordenada`: la app cancela la navegación, toma el
  GPS e inyecta el resultado en `#contenidoCoordenada` sin recargar. Convive con la tarjeta
  existente «Tomar Coordenadas», que sigue usando `Substitution` contra `/geolocalizacion`.

### Cambiado

- **`ApiRelayService` y `PrintCommandHandler` pasan de `Ejemplo_Maui_Hibrida.LibApp.*` a `LibApp.*`.**
  Eran los dos últimos tipos de `LibApp` que colgaban del namespace de la app; ahora todo el paquete
  usa el prefijo `LibApp`, alineado con el resto de los handlers y con el link por comodín del
  `.csproj` de tests. Se actualizaron los `using` de `SendApiCommandHandler` y `MauiProgram`.
- **Usings agrupados por origen en `MauiProgram.cs`** (BCL / CommunityToolkit / `Microsoft.*` /
  `LibApp.*` / `MotorDsl.*` / app) y limpieza de `using` muertos en los handlers de GPS, QR e
  impresión. Sin cambios de comportamiento.

## [2026-07-23] — Puente de comandos por URL: clasificación separada de ejecución (Plan 1)

Alcance: `Ejemplo_Maui_Hibrida/LibApp/UrlCommands/*` (contrato, dispatcher, handler de GPS) +
`Ejemplo_ws_Blazor/Components/Pages/*` (Panel + nueva página de geolocalización) + tests.
Motivado por dos defectos del modelo anterior: (1) la cancelación era «es comando ⇒ cancelo»,
sin poder expresar un comando que *no* cancela; (2) en el camino de re-navegación del GPS, un
fallo del dispositivo devolvía `(cancel=true, navigateTo=null)` → navegación cancelada y sin
re-navegar = **página congelada**.

### Agregado

- **`CommandDelivery` (enum) — cómo un comando devuelve su resultado a la web.** `None` (la
  respuesta es la UI nativa: llamada, impresión), `Injection` (se inyecta en el DOM vía
  `IWebViewBridge.RunScript`, requiere navegación cancelada) y `Substitution` (se re-navega la
  misma URL con el query param de comando sustituido por query params de valor, patrón APPGDA).
  Es propiedad del **comando concreto**, no del handler: se consulta con `DeliveryFor(url)`.
- **`UrlPlan` (record) — resultado de clasificar una URL, calculado una sola vez y de forma
  síncrona.** Evita evaluar `CanHandle` dos veces por navegación (antes en `IsCommand` y de nuevo
  en `DispatchAsync`), lo que dejaba de ser inocuo apenas un handler consultara/mutara estado.
- **`Ejemplo_ws_Blazor/Components/Pages/GeoLocalizacion.razor` — nueva página `/geolocalizacion`.**
  Muestra la coordenada recibida por query (`Latitud`/`Longitud`); si viene vacía usa el centinela
  `0.0/0.0`. Es el destino del modo `Substitution` del GPS.
- **Tests.** `UrlCommandDispatcherTests` (los dos ejes del Plan 1: OR de cancelación y gancho
  síncrono para todos los matches) y tres casos nuevos en `GpsCommandHandlerTests`
  (re-navegación con centinela ante fallo en `Substitution`, no-inyección ante fallo en
  `Injection`, y `DeliveryFor` distinguiendo ambos modos).

### Corregido

- **El GPS en modo `Substitution` re-navega SIEMPRE, haya o no señal.** Ante un fallo del
  dispositivo ahora sustituye por el centinela `0.0/0.0` (igual que APPGDA) en vez de devolver
  `NavigateTo=null`. Se acabó la página congelada tras un GPS sin señal por el botón nativo.
- **`UrlCommandDispatcher` separa `Plan(url)` (100 % síncrono) de `ExecuteAsync(plan, url)`.** La
  cancelación pasa a ser un **OR** sobre los handlers que matchean: alcanza con uno cancelable.
  Se conserva first-match-wins para la ejecución. En `DEBUG`, dos aserciones (`Debug.Fail`)
  atrapan URLs mal formadas (cancela por un handler pero ejecuta otro que no cancela) y comandos
  `Substitution` que devuelven `NavigateTo=null` — falla ruidoso en vez de un WebView colgado.
- **`MainViewModel.Navigating` decide `e.Cancel` en la fase síncrona** (antes de cualquier
  `await`) a partir del `UrlPlan`, y limita el guard de reentrada a los planes que cancelan.

### Cambiado

- **`IUrlCommandHandler` gana tres miembros con *default interface members*** — `CancelsNavigation`
  (default `true`), `DeliveryFor` (default `None`) y `OnMatchedSync` (default no-op). Los 7 handlers
  existentes **no requieren ninguna edición** y conservan su comportamiento.
- **El camino web «Tomar Coordenadas» pasa de `Injection` a `Substitution`.** `Panel.razor` ya no
  inyecta en `#contenidoCoordenada`: navega a `/geolocalizacion?coordenadas=coordenadas`. Se
  renombró el `NavigationManager` inyectado a `_navigationManager` en todos los handlers de la
  página.
- **El `.csproj` de tests linkea `LibApp/UrlCommands/*.cs` por comodín** (todo el paquete raíz es
  platform-free), en vez de enumerar archivo por archivo. `Handlers/` sigue explícito: sólo
  `GpsCommandHandler` entra.

## [2026-07-18] — El video end2end ahora captura el recorrido completo (pre-warm + espera activa)

Alcance: `Utilities/simular_ui.sh` + `Utilities/end2end/com.ejemplos.devices.integrada.hibrida.yaml`.
Motivado por un run donde el simulador booteó y grabó (1:50) pero **el video terminaba sobre el
arranque de la app y no registraba las interacciones**: el arranque en frío (Release + WebView
remoto) se comía la grabación y el flujo Maestro fallaba en el primer `tapOn` porque tocaba antes
de que la UI estuviera lista. No se tocó código de la app.

### Corregido

- **El flujo Maestro espera de forma activa a que la UI esté lista.** Se reemplazó el
  `waitForAnimationToEnd: { timeout: 8000 }` fijo posterior a `launchApp` por
  `extendedWaitUntil: { visible: "Geo Pos", timeout: 120000 }`. Maestro ya no toca nada hasta que
  el botón nativo existe → las interacciones ocurren (y quedan en el video) aunque el cold start
  sea lento.
- **La grabación ya no gasta el arranque en frío.** `simular_ui.sh` hace **pre-warm**: lanza la
  app y espera su carga (`sleep 45`) **antes** de arrancar `recordVideo`. El flujo usa
  `launchApp: { stopApp: false }`, así que Maestro **no reinicia** la app pre-cargada (un solo
  arranque en frío, no grabado). El video queda enfocado en el recorrido.

## [2026-07-18] — Robustez del arranque del simulador iOS en el CI (boot por GUI + precalentado + timeout/retry)

Alcance: la técnica de simulación end2end (`Utilities/simular_ui.sh` + workflow de la híbrida).
Motivado por un run donde el step de grabación se colgó **30 min** en el arranque del simulador
(«Waiting on BackBoard») y terminó en timeout. Un primer intento con solo timeout/retry acotó el
cuelgue a ~9 min pero **el simulador seguía sin bootear** (ni tras `erase`): la causa real es que
el boot **headless** por `simctl` no levanta el stack gráfico. No se tocó código de la app.

### Corregido

- **`Utilities/simular_ui.sh` — el simulador ahora sí bootea, levantando `Simulator.app`.** En un
  runner headless con Xcode instalado a mano, `xcrun simctl boot`/`bootstatus -b` se cuelga en
  «Waiting on BackBoard» porque BackBoard/SpringBoard no arrancan solos. Ahora el boot **abre la GUI
  del Simulador** (`open -a "$(xcode-select -p)/Applications/Simulator.app" --args -CurrentDeviceUDID`),
  que fuerza ese stack. Los runners macOS de GitHub corren sesión GUI (Aqua), así que es automatizado;
  Maestro sigue manejando la app por `simctl`.
- **Sigue acotado con timeout + reintento limpio.** Short-circuit si ya está `Booted`; si no,
  `bootstatus -b` con **240 s** → si se cuelga, `shutdown` + `erase` + **reabrir GUI** + **300 s**.
  Peor caso ~9 min en vez de 30. Si aun así falla, vuelca las últimas 120 líneas de
  `~/Library/Logs/CoreSimulator/CoreSimulator.log` para diagnóstico.

### Cambiado

- **`.github/workflows/cd-ios-Integrada.Ejemplo_Maui_Hibrida.yml` — precalentado con GUI.**
  El step «Verificando simulador instalado» ahora **abre `Simulator.app`** y bootea (fire-and-forget):
  el simulador arranca en segundo plano **durante el build** y llega caliente al step de grabación,
  evitando pagar el arranque en frío.

## [2026-07-18] — Flujo end2end de la híbrida rehecho y activación del CI de simulación

Alcance: la técnica de prueba **end2end sobre la UI real** de `Ejemplo_Maui_Hibrida`
(Maestro «dedo virtual» + grabación de video en el simulador iOS). No se tocó código de la app.

### Cambiado

- **`Utilities/end2end/com.ejemplos.devices.integrada.hibrida.yaml` — flujo rehecho desde la UI
  real** (Estrategia B: grabación de navegación → normalización a las convenciones del repo).
  Textos **verificados contra dispositivo real** (Motorola Moto G42, Android, 1080px, vía
  `adb uiautomator dump`): la barra inferior de `MainPage` declara cuatro botones nativos
  `Volver` · `Geo Pos` · `Llamar` · `Leer QR`.
  - **Corrige el texto del botón de GPS: «Geo Pos», no «Geo Posicionar»** — el texto viejo no
    matcheaba con ningún control y hacía fallar el `tapOn`.
  - Agrega el recorrido completo: `Geo Pos` → `Llamar` → `Leer QR` (abre `QRLectorPage`) →
    `Volver` (botón sólo-ícono `arrow_back` al pie de la página QR, por coordenada) →
    `Volver` de la barra de `MainPage` (resetea el `WebView` a la home).
  - Documenta el límite verificado en Android 1080px (la fila de 4 botones no entra y `Leer QR`
    queda recortado a la derecha) y deja un **fallback por coordenada comentado**; el target del
    CI es el simulador iOS (iPhone 17 Pro Max, más ancho), donde la botonera entra.

### Activado

- **`.github/workflows/cd-ios-Integrada.Ejemplo_Maui_Hibrida.yml` — se reactiva el disparador
  `push`** sobre `main`, filtrado a `Ejemplos_Devices/Integrada/Ejemplo_Maui_Hibrida/**`
  (excluyendo `*.md`, `.gitignore`, `.gitattributes`). Estaba comentado; ahora el flujo de
  simulación corre automáticamente ante cambios de la app híbrida.

## [2026-07-17] — Armonización de los overlays de dispositivo y primer proyecto de tests

Aplica `Ejemplos_Maui_Devices.Documentos/Analisis/Plan-Armonizacion-Overlays.md`.
Alcance: los cuatro overlays de `Ejemplo_Maui_Hibrida` (GPS, Red, Telefonía, Impresión).
**La librería `MotorDsl.*` no se modificó** (sigue en 1.0.13).

### Agregado

- **`Ejemplo_Maui_Hibrida.Tests` — el primer proyecto de tests de la solución**: 116 tests xUnit
  sobre `net10.0`, que corren en el runner de escritorio **sin emulador ni dispositivo**. Es
  viable porque los ViewModels no tocan la plataforma y los servicios quedan detrás de
  interfaces. Accede al código por **linkeo de fuentes**, no por `ProjectReference`: la app es
  `net10.0-android` y un proyecto `net10.0` no puede referenciarla.
- **Los cinco invariantes del patrón, ejecutables** (`Invariantes.cs`): toda variante no-`Success`
  muestra exactamente una pantalla (I-1); toda variante es alcanzable (I-2, por reflexión sobre
  la jerarquía de records); ningún mensaje crudo llega al usuario (I-3); toda pantalla tiene un
  único botón primario (I-4); el resultado tipado no se colapsa (I-5). **Agregar una variante sin
  pantalla ahora rompe la suite** — es lo que C# no verifica y lo que dejó a `BluetoothOff`
  inalcanzable durante toda la vida del PoC.
- **`IGpsService`, `ICallService`, `INetworkService`, `IPrinterService`**: costuras entre los
  ViewModels y la plataforma. Los VMs dependían de los tipos concretos, que usan estáticos de
  MAUI (`Preferences`, `Permissions`, `AppInfo`) imposibles de ejercitar fuera de un dispositivo.
- **`IUiDispatcher`** (`Common/Services/`): abstrae `MainThread`. Lo necesita sólo el overlay de
  Red, el único reactivo.
- **Catálogos de errores con código en GPS (`GPS-*`) y Telefonía (`TEL-*`)**, espejo del de
  impresión: mensaje accionable en español + código dictable a soporte, con el original
  preservado para log.
- **`GpsService.OpenLocationSettings()`**: los ajustes de ubicación del SO son distintos de los
  de la app. El permiso lo concede la app; el GPS se enciende desde el sistema.

### Corregido

- **GPS no decía nada cuando fallaba.** Cinco de sus ocho variantes (`GpsDisabled`,
  `NotSupported`, `NoSignal`, `Cancelled`, `Failure`) escribían el mensaje en la propiedad
  `Coordenadas` —que **no estaba bindeada a ningún control**— y ocultaban el overlay. Apagabas el
  GPS y la app no decía que el GPS estaba apagado. Ahora cada una tiene su pantalla con su salida.
- **`Coordenadas` (GPS) y `Estado` (Telefonía) eliminadas**: verificado que no tenían **ningún**
  consumidor — cero bindings, cero lectores. Las dos asignaciones de `Estado` estaban además en
  ramas inalcanzables.
- **El `case Success` del switch era código muerto** en GPS y Telefonía: un guard previo retornaba
  antes, y con él moría la única asignación real de la propiedad de salida. Eliminado el guard.
- **GPS colapsaba las 7 variantes no-`Success` en `Failure("")`** con mensaje vacío: eran
  indistinguibles desde fuera del ViewModel. Ahora devuelve la variante que recibió.
- **Telefonía mostraba `f.Message` crudo**: el texto de una excepción de Android, en inglés y sin
  acción posible. Ahora se clasifica.
- **Pantallas sin botón primario** en los cuatro dominios. `Primary` es «el DataTrigger de
  Secondary no disparó», así que omitirlo no da error: la pantalla queda sin nada que destaque.
  Afectaba al permiso denegado de GPS/Telefonía y a **toda pantalla cuya única acción es
  «Cerrar»**, incluidas las de impresión.
- **El mensaje de fallo de DNS nombraba un host que el usuario nunca visitó.**
  `NetworkService.CheckUrlAsync(url, …)` ignoraba el parámetro `url` y reportaba el de la sonda:
  el usuario leía «No fue posible encontrar www.msftconnecttest.com». La sonda sigue yendo a un
  endpoint fijo —detectar portal cautivo lo exige— pero ahora se reporta el host del sitio.
- **`NetworkOverlayViewModel` era el único que tocaba la plataforma directamente**
  (`MainThread`, `AppInfo`); ahora delega, como los otros tres.
- **`GpsService.CheckAsync()` y `Map()` eliminados**: sin llamadores.
- **`GpsCommandHandler`**: el comentario afirmaba «el overlay ya muestra el error», falso para 5
  de 8 variantes.

### Cambiado

- **El workflow CI de la app híbrida se recategorizó** de `gps` a una categoría propia
  `Integrada`: `.github/workflows/cd-ios-gps.Ejemplo_Maui_Hibrida.yml` →
  `.github/workflows/cd-ios-Integrada.Ejemplo_Maui_Hibrida.yml`. El contenido del workflow es
  idéntico; solo cambia el nombre de archivo/categoría. Siguen siendo **18 workflows** en total y
  la categoría `gps` queda con un único ejemplo (`Ejemplo_GPS`).

### Notas

- **La suite arrancó en 34 rojos** (GPS 21, Telefonía 7, Impresión 3, Red 3) y terminó en 116/116.
  Los rojos iniciales son el entregable: convirtieron los defectos documentados en fallas
  ejecutables. Los verdes de impresión son la red de no-regresión del único dominio ya validado
  en dispositivo.
- **La suite encontró tres afirmaciones falsas del propio plan**, entre ellas que los ViewModels
  eran platform-free: no tener `#if` no equivale a no tocar la plataforma.
- **Pendiente**: la prueba en dispositivo real. La suite verifica que el ViewModel decide bien;
  que eso llegue a la pantalla —y que los glyphs existan en la fuente— sigue necesitando un
  teléfono. `ActionLocationSourceSettings` y `ActionBluetoothSettings` siguen sin verificar.
- La capa Busy sigue sin admitir botones: **toda espera sigue siendo no-cancelable**. Es la deuda
  estructural del patrón y no se atacó en este lote.

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
