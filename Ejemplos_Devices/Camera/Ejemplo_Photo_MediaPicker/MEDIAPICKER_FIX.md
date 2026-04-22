# MediaPicker — Bug de Stream y Estrategias de Carga de Imagen

## El problema encontrado

Al capturar una foto con `MediaPicker.Default.CapturePhotoAsync()` y mostrarla en un `Image`, la app se resetea en modo **Debug** pero no en **Release**.

### Causa raíz confirmada por logcat

```
E GlideExecutor: android.runtime.JavaProxyThrowable:
[System.ObjectDisposedException]: Cannot access a closed file.
  at System.IO.FileStream.ValidateReadWriteArgs
  at System.IO.FileStream.Read
  at com.microsoft.maui.glide.stream.GlideInputStreamModelLoader
```

El código original tiene una **race condition** entre el `using` y el motor de imágenes de MAUI (Glide en Android):

```csharp
// CÓDIGO ORIGINAL — BUGGY
using Stream sourceStream = await photo.OpenReadAsync();
var image = new Image { Source = ImageSource.FromStream(() => sourceStream) };
ImgPhoto.Source = image.Source;
// ← aquí el using cierra el stream
// Glide intenta leerlo en un hilo separado DESPUÉS → ObjectDisposedException
```

`ImageSource.FromStream` es **lazy**: no lee el stream en el momento de la asignación, sino que le pasa el delegate a Glide para que lo consuma asincrónicamente. Para ese momento, el bloque `using` ya cerró el `FileStream`.

En Release el crash no siempre se manifiesta porque el timing entre el GC y Glide varía, pero el código es incorrecto en ambas configuraciones.

---

## Opción A — `ImageSource.FromFile(photo.FullPath)`

### Fundamentos

`CapturePhotoAsync()` guarda la foto en un archivo temporal en el almacenamiento privado de la app (`cache/` o `files/`). `FileResult.FullPath` expone la ruta absoluta a ese archivo.

`ImageSource.FromFile` le entrega directamente esa ruta a Glide, que abre y cierra su propio `FileStream` internamente cuando lo necesita. No hay stream que administrar manualmente ni posibilidad de que sea dispuesto prematuramente.

**Ventajas:**
- Código mínimo, sin gestión de streams.
- Glide puede cachear la imagen por ruta y aplicar sampling automático según el tamaño del `Image`.
- Correcto en todos los escenarios (Debug, Release, baja memoria).

**Desventajas:**
- El archivo temporal puede ser eliminado por el sistema si la app pasa mucho tiempo en background. Para uso de larga duración es necesario copiar el archivo a un lugar permanente.

### Código

```csharp
async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
{
    if (MediaPicker.Default.IsCaptureSupported)
    {
        FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

        if (photo != null)
        {
            ImgPhoto.Source = ImageSource.FromFile(photo.FullPath);
        }
    }
}
```

---

## Opción B — Copiar a `MemoryStream` antes de cerrar el stream

### Fundamentos

Cuando se necesita `ImageSource.FromStream` (por ejemplo, para aplicar transformaciones en memoria antes de mostrar la imagen), la solución correcta es **leer todos los bytes mientras el stream está abierto** y crear un nuevo `MemoryStream` a partir de ellos. El delegate que se pasa a `FromStream` devuelve siempre un nuevo `MemoryStream` sobre el mismo array de bytes, por lo que Glide puede invocarlo múltiples veces sin problema.

**Ventajas:**
- Útil si se necesita transformar o inspeccionar los bytes de la imagen antes de mostrarla (redimensionar, rotar, marca de agua, etc.).
- No depende de que el archivo temporal siga en disco.
- El stream original se cierra correctamente con el `using`.

**Desventajas:**
- Carga toda la imagen en memoria RAM. En fotos de cámara de alta resolución (10–50 MB) esto puede causar `OutOfMemoryException`.
- Mayor overhead que `FromFile` para el caso simple.

### Código

```csharp
async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
{
    if (MediaPicker.Default.IsCaptureSupported)
    {
        FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

        if (photo != null)
        {
            using Stream sourceStream = await photo.OpenReadAsync();
            var buffer = new MemoryStream();
            await sourceStream.CopyToAsync(buffer);
            byte[] imageBytes = buffer.ToArray();
            ImgPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }
    }
}
```

---

## Qué hace la industria: WhatsApp, Instagram y las fotos en la galería

### Por qué las fotos aparecen en una carpeta separada en la galería

Aplicaciones como WhatsApp, Instagram o cualquier app de cámara seria **no muestran la foto únicamente dentro de la app**. Siguen este flujo:

1. Capturan la foto con la cámara del sistema.
2. **Guardan una copia permanente** en el almacenamiento externo (o en `MediaStore`, la base de datos de medios de Android).
3. Notifican al sistema operativo que existe una nueva imagen mediante `MediaScannerConnection` o insertando directamente en `MediaStore.Images`.
4. El sistema la incorpora al catálogo de fotos del teléfono, donde aparece en la galería, a veces agrupada en una carpeta con el nombre de la app (ej.: `WhatsApp Images`, `Instagram`).

### ¿Por qué lo hacen así?

- **Persistencia**: el almacenamiento privado de la app (donde `CapturePhotoAsync` guarda el temporal) puede borrarse si el usuario limpia la caché o desinstala la app. El `MediaStore` es gestionado por el SO.
- **Accesibilidad**: el usuario puede acceder a las fotos desde cualquier app de galería, compartirlas o hacer backup automático con Google Fotos.
- **Permisos**: desde Android 10 (API 29) en adelante, las apps pueden escribir en `MediaStore.Images` **sin necesitar** `WRITE_EXTERNAL_STORAGE` (solo necesitan `READ_MEDIA_IMAGES` para leer fotos de otras apps). Esto simplifica el modelo de permisos.

### Cómo implementarlo en .NET MAUI (referencia)

Para guardar la foto capturada en la galería del dispositivo, se usaría la API nativa de Android a través de `MediaStore`:

```csharp
// Guardar en la galería (Android) — requiere código nativo en Platforms/Android
// o usar un plugin como Plugin.Maui.Gallery

// Con FullPath disponible tras CapturePhotoAsync:
// 1. Insertar en MediaStore.Images.Media
// 2. Copiar el archivo al Content URI resultante
// 3. El sistema agrega la imagen al catálogo de fotos
```

Para una implementación multiplataforma se suelen usar plugins como:
- `Plugin.Maui.Gallery` — guarda archivos en la galería en iOS y Android
- `CommunityToolkit.Maui.MediaElement` — reproducción/captura de medios enriquecidos

En el contexto de este ejemplo de aprendizaje, **Opción A** es suficiente y correcta para mostrar la foto capturada dentro de la app sin guardarla en la galería.
