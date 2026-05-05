# Alternativas para que `TestService` use el GPS y el overlay reaccione

Primero, lo que conviene **evitar**: inyectar `MainPageViewModel` dentro de `TestService`. Eso invierte la dependencia natural (la UI depende de los servicios, no al revés), genera ciclos en el contenedor DI (`MainPageViewModel` ya depende de servicios), acopla el servicio a una pantalla concreta y rompe si hubiera más de una página/instancia del VM.

A continuación las opciones, ordenadas de menos a más acopladas.

---

## 1. `TestService` solo devuelve `GpsResult` y el VM lo aplica (recomendada)

`TestService` reutiliza `GpsService` y delega el manejo de UI al VM, igual que ya hace [MainPageViewModel.cs](../Ejemplo_Maui_GPS/ViewModels/MainPageViewModel.cs#L60-L82).

```csharp
internal class TestService
{
    private readonly GpsService _gps;
    public TestService(GpsService gps) => _gps = gps;

    public Task<GpsResult> GetLocationAsync(CancellationToken ct = default)
        => _gps.ObtenerUbicacionAsync(ct);
}
```

En el VM:

```csharp
Overlay.ShowBusy();
var result = await _test.GetLocationAsync(_cts.Token);
AplicarResultado(result);
```

**Pros:** sin acoplamiento UI↔servicio, testeable, la lógica del overlay queda en un único lugar (`AplicarResultado`).
**Contras:** cada caller tiene que "saber" mostrar el overlay. Se mitiga extrayendo `AplicarResultado` a un helper del VM.

---

## 2. Patrón **progress reporter** (`IProgress<GpsStatus>`)

`TestService` (o `GpsService`) reporta estados intermedios; el VM los traduce al overlay.

```csharp
public enum GpsStatus { Starting, RequestingPermission, Locating, Done }

public async Task<GpsResult> GetLocationAsync(IProgress<GpsStatus>? progress, CancellationToken ct)
{
    progress?.Report(GpsStatus.RequestingPermission);
    ...
}
```

VM:

```csharp
var progress = new Progress<GpsStatus>(s => {
    if (s != GpsStatus.Done) Overlay.ShowBusy();
});
var r = await _test.GetLocationAsync(progress, _cts.Token);
AplicarResultado(r);
```

**Pros:** sigue sin conocer el VM, permite estados granulares.
**Contras:** un poco más de ceremonia.

---

## 3. Servicio de **UI abstracto** inyectable (`IGpsUiNotifier`)

Definís una interfaz mínima que represente "la UI que necesita el GPS" y la implementa el VM (o un mediador). El servicio depende de la **abstracción**, no del VM.

```csharp
public interface IGpsUiNotifier
{
    void ShowBusy();
    void ShowPermissionDenied(bool canRetry);
    void ShowRestricted();
    void Hide();
}
```

`MainPageViewModel` (o un wrapper sobre `Overlay`) implementa la interfaz y se registra/asigna al servicio cuando la página está activa:

```csharp
public class TestService
{
    private readonly GpsService _gps;
    public IGpsUiNotifier? Ui { get; set; }   // o un Register/Unregister

    public async Task<GpsResult> GetLocationAsync(CancellationToken ct = default)
    {
        Ui?.ShowBusy();
        var r = await _gps.ObtenerUbicacionAsync(ct);
        // aplicar al notifier según r
        return r;
    }
}
```

**Pros:** permite que el servicio orqueste también el overlay sin conocer al VM concreto.
**Contras:** hay que gestionar el ciclo de vida del notifier (suscribir/desuscribir al aparecer/desaparecer la página) para evitar fugas y notificar a una página equivocada.

---

## 4. **Event aggregator / MessagingCenter / WeakReferenceMessenger**

`TestService` publica mensajes (`GpsBusy`, `GpsDenied`, `GpsResultReady`); el VM se suscribe en `OnAppearing` y se desuscribe en `OnDisappearing`.

**Pros:** desacoplamiento total, escalable a varias pantallas.
**Contras:** flujo más difícil de seguir y de testear; `MessagingCenter` está obsoleto en .NET MAUI moderno (usar `WeakReferenceMessenger` de CommunityToolkit.Mvvm).

---

## 5. Estado **observable compartido** en un servicio singleton

Mover `GpsOverlayViewModel` (o un `GpsStateService` con las mismas propiedades) al contenedor DI como singleton, e inyectarlo tanto en el VM como en `TestService`. La página bindea al estado del singleton vía el VM.

```csharp
builder.Services.AddSingleton<GpsOverlayViewModel>();
```

`TestService` actualiza el estado directamente; el overlay reacciona por bindings.

**Pros:** fuente única de verdad, varias pantallas pueden compartirlo.
**Contras:** hay que cuidar el hilo de UI (`MainThread.BeginInvokeOnMainThread`) y el ciclo de vida (singleton vivo toda la app).

---

## Recomendación

Para este proyecto, lo más limpio es la **opción 1**: `TestService` recibe `GpsService` por DI y devuelve `GpsResult`; el VM sigue siendo el único responsable del overlay (reusando `AplicarResultado`). Si en algún momento necesitás que el servicio dispare la UI por sí mismo desde varios lugares, escalá a la **opción 3** (`IGpsUiNotifier`) o **5** (estado observable compartido). Inyectar el VM en el servicio (opción descartada) no la consideraría.
