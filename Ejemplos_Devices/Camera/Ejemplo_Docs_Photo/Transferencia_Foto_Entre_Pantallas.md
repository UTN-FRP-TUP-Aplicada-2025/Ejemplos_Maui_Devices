# Transferencia de una foto recién capturada entre pantallas

> Guía conceptual para .NET MAUI con ejemplos de patrones reales usados por
> WhatsApp, Instagram, Telegram y Signal.
>
> Audiencia: alumnos de la cátedra. Foco: **estabilidad en Android**, claridad
> conceptual y criterio para elegir entre técnicas.

---

## Índice

1. [El problema](#1-el-problema)
2. [Conceptos previos](#2-conceptos-previos)
3. [Técnicas de transferencia entre pantallas](#3-técnicas-de-transferencia-entre-pantallas)
   - 3.1 Pasar `Stream` directo
   - 3.2 Pasar `byte[]` en memoria
   - 3.3 Pasar un `string` con la ruta a un archivo temporal
   - 3.4 Servicio singleton / Repository de "foto pendiente"
   - 3.5 Mensajería desacoplada (`WeakReferenceMessenger`)
   - 3.6 Content URI / `FileProvider` (Android nativo)
4. [Cómo lo hacen las apps reales](#4-cómo-lo-hacen-las-apps-reales)
5. [Procesamiento previo a transferir](#5-procesamiento-previo-a-transferir)
6. [Sobrecargas y pipeline de procesamiento por archivos](#6-sobrecargas-y-pipeline-de-procesamiento-por-archivos)
7. [Acceso al UI thread: `Dispatcher` y `MainThread`](#7-acceso-al-ui-thread-dispatcher-y-mainthread)
8. [Subida al servidor: ¿base64 o multipart?](#8-subida-al-servidor-base64-o-multipart)
9. [Anti-patterns frecuentes](#9-anti-patterns-frecuentes)
10. [Tabla comparativa de técnicas](#10-tabla-comparativa-de-técnicas)
11. [Recomendación para el ejemplo de la cátedra](#11-recomendación-para-el-ejemplo-de-la-cátedra)
12. [Referencias](#12-referencias)

---

## 1. El problema

Cuando capturás una foto con `CameraView` (CommunityToolkit) en una página
secundaria y querés mostrarla en la página anterior, tenés que **mover el
resultado entre dos `ContentPage`**. Suena trivial, pero involucra:

- **Dos hilos**: el evento `MediaCaptured` viene del hilo de la cámara, no del UI.
- **Dos ciclos de vida**: la página de captura se destruye al volver atrás.
- **Carga diferida**: `ImageSource.FromStream` y `FromFile` leen el recurso
  *después* de asignarse a la `Image`.
- **Restricciones de Android**: scoped storage, low memory killer, back stack
  reciclando páginas.

Elegir mal cómo "mover" la foto produce bugs intermitentes que aparecen solo
en dispositivos físicos: imagen en blanco, `ObjectDisposedException`,
`FileNotFoundException`, o crash silencioso al rotar la pantalla.

---

## 2. Conceptos previos

Antes de comparar técnicas, fijemos vocabulario.

### 2.1 `Stream`

Una secuencia de bytes con **estado** (posición de lectura) y **dueño** (quien
lo crea suele ser responsable de cerrarlo). No es thread-safe y, una vez
cerrado o leído hasta el final, no se puede reusar sin reposicionarlo. En
Android, suele estar respaldado por un archivo temporal del cache.

### 2.2 `byte[]`

Un arreglo **inmutable en estructura** (su largo no cambia) y **thread-safe
para lectura**. Una vez que tenés los bytes, no dependen del archivo origen
ni del hilo que los produjo. Es la unidad ideal para pasar datos binarios
entre componentes.

### 2.3 Archivo temporal en `CacheDirectory`

`FileSystem.CacheDirectory` apunta a un directorio privado de la app que el
sistema operativo puede limpiar cuando hay poco espacio. Es el lugar correcto
para guardar resultados intermedios (fotos antes de subir, conversiones,
thumbnails). Persiste entre páginas pero **no es confiable a largo plazo**.

### 2.4 Content URI y `FileProvider` (Android)

A partir de Android 7, una app no puede compartir rutas `file://` con **otras
apps** (el sistema rechaza el `Intent` con `FileUriExposedException`). Dentro
de la misma app, `file://` sigue funcionando — por eso el patrón
"path en `CacheDirectory`" de la sección 3.3 es válido sin tocar `FileProvider`.
Pero apenas necesitás que **otra app** lea el archivo (compartir, abrir en
galería, mandar por mail), tenés que exponerlo con `FileProvider`, que devuelve
un `content://` URI con permisos temporales. .NET MAUI usa este mecanismo por
debajo cuando llamás a `Share`, `Launcher` o `MediaPicker`.

### 2.5 Scoped Storage (Android 11+)

Cada app accede solo a su propio directorio externo
(`/Android/data/<package>/files/...`). Para escribir en la galería pública
hay que usar `MediaStore`. Esto cambia profundamente las decisiones de
arquitectura: **lo que vos manejás en `CacheDirectory` es invisible para
otras apps y eso está bien**.

### 2.6 EXIF y rotación

Las cámaras casi nunca rotan el JPEG físicamente. Guardan los píxeles "como
salieron del sensor" y anotan en el header EXIF la orientación deseada
(`Orientation = 6` → rotar 90°). Si tu visor o tu backend ignora EXIF, vas
a ver la foto acostada.

### 2.7 Carga diferida en MAUI

`ImageSource.FromStream(factory)` y `FromFile(path)` **no leen el recurso
inmediatamente**. MAUI lo hace cuando el handler nativo necesita pintar.
Si el archivo o stream subyacente desapareció en el medio → imagen vacía.

---

## 3. Técnicas de transferencia entre pantallas

Las ordeno de menos a más robustas para el caso de un ejemplo MAUI.

### 3.1 Pasar `Stream` directo

**En qué consiste**: el productor invoca un callback con el `Stream` que le
entregó el toolkit (`e.Media`) y el consumidor lo lee.

**Por qué es frágil**:

- **Ownership ambiguo**: el `Stream` lo creó la cámara; nadie definió quién
  lo cierra ni cuándo. Si la página de captura se destruye al navegar, el
  stream puede cerrarse o el archivo temporal borrarse antes de que la
  `Image` lo lea (`FromStream` es diferido).
- **Hilos**: el evento llega del hilo de la cámara. Aunque saltes a UI,
  el stream sigue siendo un recurso no thread-safe.
- **Una sola lectura**: si MAUI re-evalúa el `ImageSource` (rotación,
  reciclado en `CollectionView`), el stream ya está consumido.

**Cuándo usarlo**: nunca para cruzar páginas. Solo *dentro* del mismo
método donde se generó.

### 3.2 Pasar `byte[]` en memoria

**En qué consiste**: el productor copia el `Stream` a un `MemoryStream`,
extrae `ToArray()` y entrega ese `byte[]` al consumidor.

**Ventajas**:

- Inmutable y thread-safe para lectura.
- `ImageSource.FromStream(() => new MemoryStream(bytes))` se puede invocar
  **N veces**: cada llamada devuelve un stream nuevo posicionado en 0.
- Listo para `Convert.ToBase64String(bytes)` si lo necesitás (con la
  reserva de la sección 6).
- No depende del ciclo de vida de la página productora.

**Desventajas**:

- La foto entera vive en RAM mientras el `byte[]` esté referenciado.
  Para una foto de 12 MP sin comprimir podés estar hablando de varios MB.
- En batch (galerías, carruseles) se acumula y dispara OOM en gama baja.

**Cuándo usarlo**: fotos sueltas, ya comprimidas a JPEG, que se muestran
una sola vez y/o se mandan al backend en base64.

### 3.3 Pasar un `string` con la ruta a un archivo temporal

**En qué consiste**: el productor vuelca el `Stream` a un archivo en
`FileSystem.CacheDirectory` y entrega el `path`. El consumidor lo lee
cuando lo necesita y, si corresponde, lo borra.

**Ventajas**:

- El archivo es **la unidad estable de transferencia**: sobrevive a la
  navegación, a rotaciones y al GC de la página productora.
- No carga la imagen en RAM mientras no haga falta.
- Si querés borrarlo apenas mostrás la foto, podés:
  1. Leer los bytes (`File.ReadAllBytes(path)`).
  2. Borrar el archivo en un `finally`.
  3. Mostrar con `ImageSource.FromStream(() => new MemoryStream(bytes))`.
  Así la imagen vive en memoria pero el cache queda limpio
  (patrón usado en el ejemplo `Ejemplo_Photo_MiMediaPicker_Callback`).

**Desventajas**:

- I/O en disco (despreciable en Flash moderna, pero existe).
- Tenés que gestionar la limpieza vos (no confíes en el SO).

**Cuándo usarlo**: por defecto en cualquier app real. Es el patrón que usan
WhatsApp, Instagram y Telegram para sus pantallas intermedias de preview.

### 3.4 Servicio singleton / Repository de "foto pendiente"

**En qué consiste**: registrás un servicio en `MauiProgram` con scope
singleton que mantiene la "foto en curso" (path o bytes). Cualquier página
lo inyecta por DI y lee el estado actual.

```csharp
public interface IPendingPhotoStore
{
    string? CurrentPath { get; set; }
}
```

**Ventajas**:

- Desacopla las páginas: no hay que pasar parámetros por `Shell` ni callbacks.
- Funciona naturalmente con MVVM (el `ViewModel` recibe el servicio).
- Sobrevive a rotación y a navegación.

**Desventajas**:

- Es **estado global mutable**: si dos flujos lo usan a la vez, se pisan.
  Esto es problema solo si tu dominio admite varias fotos en proceso
  (galería de edición, carrusel). Para el caso típico de "una foto pendiente
  a la vez" es perfecto.
- Requiere disciplina de limpieza (`Reset()` al consumirlo).

**Cuándo usarlo**: apps con MVVM y flujos donde la "foto pendiente" es un
concepto del dominio (por ejemplo: alta de producto, perfil de usuario).

### 3.5 Mensajería desacoplada (`WeakReferenceMessenger`)

**En qué consiste**: el productor publica un mensaje (`PhotoTakenMessage`)
con la foto y los suscriptores reaccionan.

```csharp
WeakReferenceMessenger.Default.Send(new PhotoTakenMessage(bytes));
```

**Ventajas**:

- Productor y consumidor no se conocen entre sí.
- Las referencias son débiles → no genera leaks si el suscriptor se destruye.
- Encaja con el patrón MVVM Toolkit.

**Desventajas**:

- Más indirecto: el flujo es más difícil de seguir leyendo el código.
- Si publicás antes de que el consumidor se suscriba, se pierde el mensaje
  (hay que coordinar con `IsActive`).

**Cuándo usarlo**: cuando querés evitar pasar `Action<>` por
`ShellNavigationQueryParameters` y mantener arquitectura limpia.

### 3.6 Content URI / `FileProvider` (Android nativo)

**En qué consiste**: en lugar de pasar un `string` con la ruta, pasás un
`content://` URI que apunta al archivo expuesto vía `FileProvider`. Es la
forma estándar de compartir archivos con otras apps en Android.

**Ventajas**:

- Permisos temporales y revocables (`FLAG_GRANT_READ_URI_PERMISSION`).
- Compatible con Scoped Storage.
- Es lo que el SO espera cuando hacés `Share`, `Intent.ACTION_VIEW`, etc.

**Desventajas**:

- Es específico de Android. Para mantener portabilidad, en MAUI conviene
  envolverlo detrás de una interfaz (`IPhotoSharing`).

**Cuándo usarlo**: cuando además de mostrar, vas a compartir la foto con
otra app (galería, cliente de mail, WhatsApp). MAUI ya lo hace por debajo
en `Share.Default.RequestAsync(new ShareFileRequest(...))`.

---

## 4. Cómo lo hacen las apps reales

Esta sección no es prescriptiva: documenta patrones observables y
declaraciones públicas de los equipos. El objetivo es entender **por qué**
cada decisión tiene sentido en su contexto.

> **Disclaimer importante**: las cifras concretas que aparecen abajo
> (dimensiones, calidad JPEG, tamaños de chunk) están basadas en mediciones
> públicas de la comunidad y reportes de ingeniería inversa. Sirven para
> entender los *trade-offs*, no como valores oficiales. Las apps cambian
> estos parámetros entre versiones.

### 4.1 WhatsApp

- **Captura → preview → envío** pasa siempre por archivos en
  `cacheDir/Camera/`. Nunca pasa la foto entera entre Activities por memoria.
- **Compresión agresiva**: por defecto reduce a aprox. 1600 px del lado
  mayor con calidad JPEG ~70-80%. Una foto de 4 MB del sensor termina en
  ~200 KB para envío.
- **Limpieza**: el archivo temporal se borra después de confirmar el envío
  exitoso. Si la red falla, queda en cache para reintentar.
- **Justificación**: optimiza para conexiones lentas en mercados emergentes.
  La calidad "perfecta" no compensa el costo en datos y batería.

### 4.2 Instagram

- **Pipeline multi-pantalla**: Captura → editor → filtros → caption →
  publicación. Entre cada paso pasa **URI a archivo**, no bytes.
- **Thumbnail diferido**: muestra una versión 720 px mientras procesa
  filtros sobre la versión 1440 px en background.
- **Sin EXIF en upload**: rota físicamente los píxeles antes de subir y
  elimina metadatos por privacidad.
- **Justificación**: el editor necesita repintar muchas veces; trabajar
  contra un archivo permite cargar/descartar resoluciones a demanda sin
  duplicar memoria.

### 4.3 Telegram

- **Chunked upload**: divide el archivo en bloques de 256 KB y los sube
  independientemente. Si falla un chunk, reintenta solo ese.
- **Cache estricto**: documenta el directorio de cache y permite al usuario
  limpiarlo desde Ajustes (transparencia).
- **Sin EXIF**: lo elimina por defecto (privacidad de geolocalización).
- **Justificación**: red móvil inestable + archivos grandes (videos
  incluidos) hacen que reanudar parcial sea esencial.

### 4.4 Signal

- **Encripta el archivo de cache**: aún antes de subirlo, la foto en
  `cacheDir` está cifrada. Si el dispositivo es comprometido, el atacante
  ve un blob inútil.
- **Pasa siempre por URI** (nunca bytes en intent extras).
- **Justificación**: modelo de amenaza distinto. Asumen que el SO no es
  confiable; las otras apps confían en el sandbox de Android.

### 4.5 X / Twitter

- **Upload chunked + multipart** vía API `INIT / APPEND / FINALIZE`.
- **Compresión cliente**: 2048 px lado mayor, calidad ~85%.
- **Justificación**: balance entre calidad visual aceptable y costo de
  almacenamiento global.

### 4.6 Síntesis

De los cinco casos surge un patrón consistente: **ninguna app hace pasar la
foto entera entre pantallas como `Stream` o `byte[]` en parámetros de
navegación**. Todas usan archivo (con URI o path) como soporte físico
interno y **multipart** hacia el servidor. **Ninguna usa base64 para subir**.

| App        | Transferencia interna | Subida          | EXIF      |
| ---------- | --------------------- | --------------- | --------- |
| WhatsApp   | Archivo cache         | Multipart       | Elimina   |
| Instagram  | URI / archivo         | Multipart       | Elimina   |
| Telegram   | Archivo cache         | Multipart chunked | Elimina |
| Signal     | URI cifrada           | Multipart cifrado | Preserva (cifrado) |
| X/Twitter  | URI / archivo         | Multipart chunked | Elimina |

Patrón común: **archivo + URI internamente, multipart hacia el servidor,
nunca base64**.

---

## 5. Procesamiento previo a transferir

Antes de mostrar o subir, casi todas las apps aplican uno o varios pasos.

### 5.1 Downsampling

Reducir las dimensiones a las realmente necesarias.

- **Preview en pantalla**: 720-1080 px del lado mayor.
- **Thumbnail**: 150-300 px.
- **Upload**: 1440-2048 px.

Una foto de 12 MP (4000 × 3000) sin downsample ocupa ~48 MB descomprimida
en RAM (RGBA, 4 bytes por píxel). Con downsample a 1440 px del lado mayor
→ ~6 MB. Con compresión JPEG calidad 85 → ~600 KB.

### 5.2 Compresión JPEG

Calidad ~80-85% es prácticamente indistinguible visualmente del 100% y
ahorra el 60-70% del tamaño. WhatsApp baja hasta 70% sin quejas masivas.

### 5.3 Corrección EXIF

Aplicar la rotación a los píxeles y eliminar el tag, o decidir
explícitamente preservarlo. Lo importante es **decidir**: dejar EXIF "por
si acaso" causa fotos acostadas en visores que no lo respetan.

### 5.4 Limpieza de metadatos

EXIF puede contener GPS, modelo de cámara, fecha/hora exacta. Si la foto se
publica, esto es información sensible.

---

## 6. Sobrecargas y pipeline de procesamiento por archivos

Una vez que adoptaste "path en `CacheDirectory`" como unidad de transferencia
(sección 3.3), aparece naturalmente otra pregunta: **¿dónde meto el
procesamiento (rotación EXIF, resize, recompresión) descrito en la sección 5?**

La respuesta consistente con todo el resto del documento es: **encadená
funciones que reciben un path y devuelven un path nuevo**. Cada paso del
pipeline es una etapa independiente que opera sobre un archivo y produce
otro archivo. Esto es exactamente lo que hace Instagram entre Captura →
Editor → Filtros → Caption → Publicación.

### 6.1 Patrón: sobrecarga `Stream → byte[]` + `path → path`

Una utilidad de procesamiento (por ejemplo, una clase tipo `ImageProcessor`
que rota según EXIF, redimensiona y recomprime JPEG) suele exponer dos
firmas complementarias:

```csharp
// Sobrecarga "baja": trabaja en memoria. Útil cuando ya tenés el stream
// y no querés pasar por disco (por ejemplo, una foto que viene de la red).
public Task<byte[]?> ProcesarPhotoAsync(Stream input);

// Sobrecarga "alta": trabaja en archivos. Útil cuando el origen y el
// destino del pipeline son archivos en cache.
public Task<string?> ProcesarPhotoAsync(string inputPath, string? outputPath = null);
```

La sobrecarga `path → path` se implementa **reusando** la de `Stream`:

```csharp
public async Task<string?> ProcesarPhotoAsync(string inputPath, string? outputPath = null)
{
    if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
        return null;

    byte[]? processed;
    using (var fs = File.OpenRead(inputPath))
    {
        processed = await ProcesarPhotoAsync(fs);
    }

    if (processed is null) return null;

    outputPath ??= Path.Combine(
        FileSystem.CacheDirectory,
        $"photo_norm_{Guid.NewGuid():N}.jpg");

    await File.WriteAllBytesAsync(outputPath, processed);
    return outputPath;
}
```

### 6.2 Por qué conviene esta forma

- **Composabilidad**: cada paso recibe un path y devuelve otro. Podés
  encadenarlos sin pegamento intermedio:

  ```csharp
  var rotado     = await new ExifRotator().ProcesarPhotoAsync(originalPath);
  var resizeado  = await new Resizer(1080).ProcesarPhotoAsync(rotado);
  var watermark  = await new Watermarker().ProcesarPhotoAsync(resizeado);
  ```

- **Reuso**: la lógica está en la sobrecarga `Stream → byte[]`. La de
  `path → path` solo se ocupa de I/O y nombres de archivo. No duplica
  código de procesamiento.

- **Testabilidad**: la versión `Stream → byte[]` se prueba con archivos
  embebidos como recursos del proyecto de tests, sin tocar disco real.

- **Memoria acotada**: nunca se mantienen dos copias en RAM a la vez.
  Cuando termina un paso, su `byte[]` queda elegible para GC; el siguiente
  paso vuelve a leer del disco.

- **Punto natural de cancelación**: si el usuario navega antes de que
  termine el pipeline, los pasos pendientes no arrancan; los archivos
  intermedios quedan en cache para limpiar después.

### 6.3 Gestión del ciclo de vida de los archivos intermedios

Cada etapa produce un archivo nuevo. Hay que decidir **quién borra qué**.
Las políticas posibles, ordenadas de más simple a más sólida:

1. **Borrar el input al producir el output**: cada etapa borra el archivo
   que recibió una vez que escribió el siguiente. Solo sobrevive el
   último del pipeline. Fácil de razonar; no necesita coordinación externa.

2. **Borrar todos los intermedios al final**: el orquestador (la página o
   servicio que arma el pipeline) lleva una lista de paths producidos y
   los borra en un `finally`. Permite inspección/debug porque los
   intermedios viven hasta el final.

3. **Cache con TTL**: dejar todo en cache y limpiar al iniciar la app los
   archivos `photo_*.jpg` con `LastWriteTime` mayor a N días. Es lo que
   hace Telegram. Útil cuando hay reuso (la misma foto puede aparecer en
   varias pantallas).

Para un ejemplo didáctico, la política 1 es la más clara.

### 6.4 Variante: pipeline en background

El pipeline puede ser pesado (decodificar JPEG + rotar + resize +
recomprimir tarda cientos de ms en gama media). Si lo hacés sincrónicamente
en el callback de la página consumidora, congelás el UI thread.

Dos opciones:

- **`await` explícito** con un indicador de progreso. Útil cuando el
  resultado bloquea la siguiente acción del usuario (por ejemplo, un botón
  "Subir" deshabilitado hasta que el pipeline termina).
- **Fire-and-forget** (`_ = Task.Run(async () => { ... })`). Útil cuando
  sólo querés mostrar la imagen procesada apenas esté lista, sin bloquear
  el flujo principal.

En ambos casos, **el pipeline debe correr fuera del UI thread**. La
librería de procesamiento (SkiaSharp, ImageSharp, etc.) hace trabajo
intensivo de CPU; mantenerlo en el hilo de UI tira los frames.

### 6.5 Cuándo sí pasar `byte[]` (y no path)

La sobrecarga `Stream → byte[]` (y por extensión el patrón de la sección
3.2) sigue siendo útil cuando:

- La fuente no es un archivo (descarga HTTP, generación procedural).
- Es un único paso (no hay pipeline que componer).
- Vas a serializar a base64 inmediatamente después.

La regla general: **si hay más de un paso de procesamiento, usá paths**.

---

## 7. Acceso al UI thread: `Dispatcher` y `MainThread`

Todo lo discutido hasta acá — capturar, materializar a archivo, normalizar,
leer bytes — ocurre **fuera del UI thread**. Pero el resultado final
(`ImgPhoto.Source = ...`) toca un control de UI, y eso sí debe pasar en el
thread correcto. Esta sección explica por qué y cómo se hace en MAUI.

### 7.1 Definiciones

- **UI thread (o "main thread")**: el único hilo del proceso autorizado a
  leer y modificar elementos visuales. En Android es el thread donde corre
  el `Looper` principal de la actividad; en iOS es el main thread del
  proceso. MAUI los expone uniformemente a través de `MainThread`.
- **Thread pool**: conjunto de hilos administrados por .NET donde corren
  las continuaciones de `await` cuando no hay un `SynchronizationContext`
  que las ate al UI. Tareas de I/O y CPU intensivo viven acá.
- **`SynchronizationContext`**: mecanismo de .NET que captura "el contexto
  donde estaba" para volver a él después de un `await`. En aplicaciones
  con UI, hay un contexto especial que reanuda en el UI thread.
- **`Dispatcher`** (MAUI / WPF / WinUI): cola de mensajes asociada a un
  thread (típicamente el UI). Encolar una acción al `Dispatcher` significa
  "que esto se ejecute en ese thread cuando la cola lo procese".
- **`MainThread`** (MAUI): API estática que abstrae el dispatcher principal
  de la app. Es el camino recomendado en código cross-platform.

### 7.2 Por qué los controles solo se tocan en el UI thread

Los toolkits gráficos no son thread-safe por diseño. El UI framework
(Android Views, UIKit, WinUI) asume **un solo escritor**: si dos hilos
modifican un control simultáneamente, el estado interno (medición,
layout, dirty flags) queda inconsistente. Las consecuencias típicas:

- Excepción inmediata (`CalledFromWrongThreadException` en Android,
  `Only the original thread that created a view hierarchy can touch its views`).
- Crash diferido al próximo layout pass.
- Render parcial / pantalla negra sin error visible (el caso más
  engañoso).

Por eso cualquier asignación a una propiedad bindeable (`Text`,
`IsVisible`, `Source`, etc.) debe ocurrir en el UI thread.

### 7.3 ¿En qué thread está el callback?

Después de un `await` no hay garantía de en qué thread reanuda la
ejecución. Reglas prácticas para un callback como el del ejemplo de la
cátedra:

- Si el `await` anterior usaba I/O (`File.ReadAllBytesAsync`,
  `CopyToAsync`, `Task.Run`) → la continuación cae en un thread del
  pool. **No** estás en UI.
- Si la cadena arrancó en un evento de UI y nunca cruzó por `Task.Run` →
  tenés chances de seguir en UI, pero no es seguro asumirlo.
- Si el evento viene de una librería nativa (la cámara, sensores) →
  casi seguro estás en un thread propio de esa librería.

Conclusión: **antes de tocar UI, saltá explícitamente al UI thread**. No
adivines.

### 7.4 APIs disponibles en MAUI

Las tres formas equivalentes de "saltar al UI thread":

```csharp
// (a) MAUI - desde una Page o Element. Postea y retorna enseguida.
Dispatcher.Dispatch(() =>
{
    ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
});

// (b) MAUI - API estática equivalente a (a). Funciona desde cualquier lado
//     (servicios, ViewModels) sin necesidad de tener una Page a mano.
MainThread.BeginInvokeOnMainThread(() =>
{
    ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
});

// (c) Variante awaitable: espera a que la acción termine antes de seguir.
await MainThread.InvokeOnMainThreadAsync(() =>
{
    ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
});

// (d) Atajo: solo postear si NO estoy ya en UI.
if (MainThread.IsMainThread)
    ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
else
    Dispatcher.Dispatch(() =>
        ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes)));
```

Diferencias clave:

| API                                    | Espera result. | Devuelve `Task` | Origen recomendado          |
| -------------------------------------- | :------------: | :-------------: | --------------------------- |
| `Dispatcher.Dispatch`                  | No             | No              | Dentro de una `Page`/`Element` |
| `MainThread.BeginInvokeOnMainThread`   | No             | No              | Cualquier capa              |
| `MainThread.InvokeOnMainThreadAsync`   | Sí             | Sí              | Cuando lo siguiente depende |

### 7.5 "Postear" vs "esperar"

Las variantes `BeginInvoke` / `Dispatch` son **fire-and-forget**: encolan
la acción y retornan al instante. La asignación al control ocurre
*después*, cuando el UI thread procesa la cola.

Consecuencia importante para el patrón "leer bytes, borrar archivo, mostrar
imagen":

```csharp
byte[] bytes;
try { bytes = File.ReadAllBytes(path); }
finally { File.Delete(path); }     // (1) borra el archivo

Dispatcher.Dispatch(() =>          // (2) postea el seteo de UI
{
    ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
});
// (3) el método retorna; el lambda corre más tarde.
```

¿Hay race? **No**, porque el lambda **captura `bytes`**, no `path`.
Los bytes viven en memoria mientras el `ImageSource` los referencie; el
archivo ya no se necesita. Si el lambda capturara `path` y usara
`FromFile(path)`, ahí sí habría race — y es justamente uno de los
anti-patterns de la sección 9.

Si necesitás saber **cuándo** se aplicó el set (por ejemplo, para hacer
una animación encadenada), usá la variante `await
InvokeOnMainThreadAsync(...)`.

### 7.6 Reglas prácticas

- **Saltar al UI thread "justo a tiempo"**: lo más tarde posible, solo para
  la línea que toca un control. No envuelvas todo el método en
  `MainThread.InvokeOnMainThreadAsync`: bloqueás la UI con trabajo
  innecesario.
- **Trabajo intensivo → fuera de UI**: decodificar JPEG, redimensionar,
  comprimir, leer/escribir archivos → `Task.Run` o `await` sobre APIs
  async. Solo el set de propiedades vuelve a UI.
- **No mezcles**: `Dispatcher.Dispatch` y `MainThread.BeginInvokeOnMainThread`
  son intercambiables; elegí uno y mantenelo en todo el proyecto.
- **`async void` solo en handlers de eventos**. En el resto, devolvé
  `Task` y `await`-elo. Un `async void` que tira no se puede atrapar.

### 7.7 Conexión con el resto del documento

- **Pipeline de archivos (§6)**: corre fuera del UI thread; al terminar,
  salta a UI solo para mostrar.
- **Carga diferida (§2.7)**: `ImageSource.FromStream` con factory se
  ejecuta cuando MAUI necesita pintar, *en el UI thread*. El `byte[]`
  capturado por el lambda debe seguir vivo en ese momento.
- **Anti-patterns (§9)**: tocar controles desde el thread de la cámara
  o desde una continuación de `Task.Run` sin volver a UI → crash.

---

## 8. Subida al servidor: ¿base64 o multipart?

Una pregunta recurrente. Resumen:

### 8.1 Multipart (`multipart/form-data`)

- Envía los bytes **tal cual**.
- Permite streaming: no hay que cargar el archivo entero en RAM.
- Headers HTTP estándar; el backend usa el parser nativo del framework.

### 8.2 Base64

- Cada 3 bytes binarios se codifican en 4 ASCII → **+33% de tamaño**.
- Hay que tener el `string` entero en RAM para armar el JSON.
- Útil solo cuando el endpoint **exige JSON** y no acepta multipart.

### 8.3 Conclusión

Para subir fotos al backend: **multipart**. Reservá base64 para casos
puntuales (incrustar en un JSON existente, almacenar en una columna de
base de datos, mandar por un protocolo de solo texto).

---

## 9. Anti-patterns frecuentes

| Anti-pattern                           | Por qué falla                                                             |
| -------------------------------------- | ------------------------------------------------------------------------- |
| Pasar `Stream` entre páginas           | Ownership y ciclo de vida indefinidos; muere al navegar atrás.            |
| Guardar `byte[]` enorme en `Preferences` | `Preferences` está pensado para escalares; rompe en bytes grandes.        |
| Pasar la foto en el `extras` de un Intent (Android) | Límite ~1 MB por transacción Binder → `TransactionTooLargeException`. |
| `ImageSource.FromFile(path)` y borrar el archivo enseguida | Carga diferida → cuando MAUI lee, el archivo ya no existe.       |
| Subir base64 en JSON                   | Inflación 33% + RAM duplicada + timeouts en redes lentas.                 |
| No corregir EXIF                       | Fotos acostadas en backends y visores que ignoran el tag.                 |
| Guardar resultados intermedios en `Pictures/` o galería | Llena la galería del usuario con basura; mala UX.               |
| Confiar en que el SO limpia `CacheDirectory` | Lo hace, pero solo bajo presión de espacio. Limpiá vos.            |
| Capturar `this` en un callback diferido | Page no se libera → leak en flujos repetidos.                            |
| Tocar un control fuera del UI thread   | `CalledFromWrongThreadException` o render inconsistente. Ver §7.          |

---

## 10. Tabla comparativa de técnicas

| Técnica                       | Estabilidad | Memoria | Complejidad | Sobrevive navegación | Ideal para                                    |
| ----------------------------- | ----------- | ------- | ----------- | -------------------- | --------------------------------------------- |
| `Stream` directo              | Baja        | Baja    | Baja        | No                   | Nada que cruce páginas.                       |
| `byte[]`                      | Alta        | Media   | Baja        | Sí                   | Foto chica/comprimida; preparar base64.       |
| `string` path + cache         | Alta        | Baja    | Media       | Sí                   | Caso general; previews, edición.              |
| Singleton Repository          | Alta        | Media   | Media       | Sí                   | MVVM, dominio con "foto pendiente".           |
| `WeakReferenceMessenger`      | Alta        | Media   | Media       | Sí                   | Desacoplamiento productor/consumidor.         |
| Content URI / FileProvider    | Alta        | Baja    | Alta        | Sí                   | Compartir con otras apps (Share, Intents).    |

---

## 11. Recomendación para el ejemplo de la cátedra

Para el ejemplo `Ejemplo_Photo_MiMediaPicker_Callback`:

1. **Materializar la foto a archivo en el productor** (`MyMediaPickerPage`),
   en el hilo de la cámara, antes de navegar. El `Stream` del toolkit se
   consume y se cierra en su hilo natural y nunca cruza páginas.
2. **Pasar el `string path`** al callback registrado por la página origen.
3. En el consumidor (`MainPage`):
   - Leer los bytes con `File.ReadAllBytes`.
   - **Borrar el archivo en el `finally`** del bloque de lectura.
   - Mostrar la imagen con `ImageSource.FromStream(() => new MemoryStream(bytes))`,
     que ya no depende del archivo.
4. Si en el futuro este flujo crece (varias pantallas: editor, caption,
   envío), migrar a un **Repository singleton** o `WeakReferenceMessenger`
   manteniendo el archivo como soporte físico.

Este patrón refleja, en versión didáctica, lo que hacen WhatsApp y
Telegram en su flujo de captura → preview → envío.

> Implementación concreta: ver `Ejemplo_Photo_MiMediaPicker_Callback` en
> este mismo repositorio.

---

## 12. Referencias

- Microsoft Learn — [Media picker for photos and videos](https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/picker)
- Microsoft Learn — [.NET MAUI Community Toolkit `CameraView`](https://learn.microsoft.com/dotnet/communitytoolkit/maui/views/camera-view)
- Microsoft Learn — [`FileSystem.CacheDirectory`](https://learn.microsoft.com/dotnet/maui/platform-integration/storage/file-system-helpers)
- Microsoft Learn — [`MainThread`](https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/main-thread)
- Microsoft Learn — [`IDispatcher`](https://learn.microsoft.com/dotnet/api/microsoft.maui.dispatching.idispatcher)
- Android Developers — [`FileProvider`](https://developer.android.com/reference/androidx/core/content/FileProvider)
- Android Developers — [Scoped storage](https://developer.android.com/about/versions/11/privacy/storage)
- Android Developers — [Threading on Android](https://developer.android.com/guide/components/processes-and-threads)
- MDN — [`multipart/form-data`](https://developer.mozilla.org/docs/Web/HTTP/Headers/Content-Type)
- WhatsApp FAQ — [About media auto-download](https://faq.whatsapp.com/) (referencia general a su política de compresión).
- Telegram — [MTProto upload (`upload.saveFilePart`)](https://core.telegram.org/method/upload.saveFilePart) (subida chunked).
- Signal — [Blog técnico sobre cifrado en disco](https://signal.org/blog/) (referencia general a su modelo de amenaza).
- CommunityToolkit MVVM — [`WeakReferenceMessenger`](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/messenger)

> Las URLs de WhatsApp, Telegram y Signal son entradas a sus sitios técnicos;
> las cifras de compresión y dimensiones citadas en la sección 4 son
> aproximaciones basadas en mediciones públicas y reportes de la comunidad,
> no en documentación oficial detallada.
