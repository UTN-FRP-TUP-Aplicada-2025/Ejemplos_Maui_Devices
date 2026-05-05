# `AsyncRelayCommand` — qué es, qué resuelve y cómo usarlo

Este documento describe la clase `AsyncRelayCommand` que aparece en
[GPS/Ejemplo_Maui_GPS/ViewModels/AsyncRelayCommand.cs](../GPS/Ejemplo_Maui_GPS/ViewModels/AsyncRelayCommand.cs)
y [Phone/Ejemplo_Maui_DirectCall/ViewModels/AsyncRelayCommand.cs](../Phone/Ejemplo_Maui_DirectCall/ViewModels/AsyncRelayCommand.cs).
Es una implementación didáctica, **sin dependencias externas**, pensada para
mostrar MVVM en .NET MAUI sin incorporar `CommunityToolkit.Mvvm`.

---

## 1. El problema que resuelve

`ICommand` (la interfaz nativa que MAUI consume desde XAML para los `Command`
de botones, gestos, etc.) tiene esta forma:

```csharp
public interface ICommand
{
    bool CanExecute(object? parameter);
    void Execute(object? parameter);          // <-- void, no Task
    event EventHandler? CanExecuteChanged;
}
```

El método `Execute` es **`void`**. Esto provoca tres problemas cuando el
trabajo real es asincrónico (red, GPS, cámara, archivo):

1. **No espera la finalización** — el binding "termina" inmediatamente
   y la UI no sabe cuándo concluye la operación.
2. **`async void` traga excepciones** — si la `Task` tira, no hay manera
   de capturarla afuera; en muchos casos tumba la app.
3. **No hay anti-reentrancia** — si el usuario toca el botón dos veces,
   se disparan dos ejecuciones en paralelo (doble llamada GPS, doble
   request HTTP, doble llamada telefónica, etc.).

`AsyncRelayCommand` envuelve un `Func<Task>` en un `ICommand` y resuelve
los puntos 1 y 3. El punto 2 queda a cargo del ViewModel (con `try/catch`).

---

## 2. Anatomía de la clase

```csharp
public class AsyncRelayCommand : ICommand
{
    private readonly Func<CancellationToken, Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute) { }

    public AsyncRelayCommand(Func<CancellationToken, Task> execute,
                             Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value) return;
            _isRunning = value;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
        => !_isRunning && (_canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;

        IsRunning = true;
        try { await _execute(CancellationToken.None); }
        finally { IsRunning = false; }
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

### Qué aporta cada bloque

| Bloque | Qué resuelve |
|--------|--------------|
| Dos constructores (`Func<Task>` y `Func<CancellationToken, Task>`) | Permite bindear métodos async a un `ICommand` sin escribir wrappers `async void` por cada botón. |
| `IsRunning` + chequeo en `CanExecute` | **Anti-reentrancia**: el botón queda deshabilitado mientras la tarea está en curso. |
| `RaiseCanExecuteChanged()` al cambiar `IsRunning` | La UI re-evalúa `CanExecute` y deshabilita/habilita el botón visualmente, sin código extra en el ViewModel. |
| `try/finally` con `IsRunning = false` en el `finally` | Aunque el `Task` falle, el botón se rehabilita (no queda "trabado"). |
| `_canExecute` opcional | Permite condiciones extra (ej. "solo si hay número cargado"). Se combina con `!_isRunning`. |
| Validación `null` en el constructor | Falla temprano si alguien arma el comando sin lambda; mejor que un `NullReferenceException` runtime. |

---

## 3. Limitaciones conocidas

Al ser una versión minimalista, hay cosas que **no** hace:

- **No captura excepciones internamente**. Como `Execute` es `async void`,
  una excepción no manejada se propaga al `SynchronizationContext` y puede
  tirar la app. Por eso, en los ViewModels reales del repo verás `try/catch`
  *adentro* del método (ver
  [Phone/Ejemplo_Maui_DirectCall/ViewModels/MainPageViewModel.cs:47-58](../Phone/Ejemplo_Maui_DirectCall/ViewModels/MainPageViewModel.cs#L47-L58)).
- **No expone un `ExecuteAsync` awaitable**. No podés `await` el comando
  desde otro código (por ejemplo, un test unitario): solo se ejecuta vía
  `ICommand.Execute`, que es fire-and-forget.
- **El `CancellationToken` no está cableado al exterior**. La sobrecarga
  acepta `Func<CancellationToken, Task>`, pero `Execute` siempre pasa
  `CancellationToken.None`. La cancelación, si se necesita, debe hacerse
  con un mecanismo aparte (por ejemplo, el patrón `GpsCoordinator` en este
  repo, que tiene su propio `CancellationTokenSource`).
- **`CanExecuteChanged` no se dispara automáticamente** cuando cambian
  las variables que usa `_canExecute`. Si querés refrescar el estado
  habilitado/deshabilitado por cambios externos, hay que llamar
  `RaiseCanExecuteChanged()` a mano.

---

## 4. Ejemplos puntuales

### 4.1. Comando simple sin `canExecute` — botón que dispara una tarea async

```csharp
public class MainPageViewModel : ViewModelBase
{
    public ICommand SaludarCommand { get; }

    public MainPageViewModel()
    {
        SaludarCommand = new AsyncRelayCommand(SaludarAsync);
    }

    private async Task SaludarAsync()
    {
        await Task.Delay(2000);          // simula trabajo lento
        Console.WriteLine("¡Hola!");
    }
}
```

```xml
<Button Text="Saludar" Command="{Binding SaludarCommand}" />
```

**Qué pasa**: al tocar el botón, `IsRunning` pasa a `true`, el botón se
deshabilita visualmente, se ejecuta la tarea y al terminar se vuelve a
habilitar. Si el usuario toca durante esos 2 segundos, no pasa nada
(anti-reentrancia).

---

### 4.2. Comando con `canExecute` dependiente de una propiedad

Caso típico: deshabilitar el botón "Llamar" si no hay número cargado.

```csharp
private string _telefono = "";
public string Telefono
{
    get => _telefono;
    set
    {
        if (SetProperty(ref _telefono, value))
            ((AsyncRelayCommand)LlamarCommand).RaiseCanExecuteChanged();
    }
}

public ICommand LlamarCommand { get; }

public MainPageViewModel()
{
    LlamarCommand = new AsyncRelayCommand(
        execute:    LlamarAsync,
        canExecute: () => !string.IsNullOrWhiteSpace(Telefono));
}

private async Task LlamarAsync()
{
    await PhoneDialer.Default.OpenAsync(Telefono);
}
```

**Detalle clave**: el `canExecute` se evalúa cuando MAUI lo pide, pero
solo se *re-evalúa* cuando se dispara `CanExecuteChanged`. Por eso, en
el setter de `Telefono` hay que llamar `RaiseCanExecuteChanged()`
manualmente — si no, el botón no cambia de estado al escribir.

---

### 4.3. Comando con manejo de errores en el ViewModel

Como la clase no captura excepciones, el `try/catch` va dentro del método.

```csharp
public ICommand DescargarDatosCommand { get; }

public MainPageViewModel()
{
    DescargarDatosCommand = new AsyncRelayCommand(DescargarDatosAsync);
}

private async Task DescargarDatosAsync()
{
    try
    {
        Estado = "Descargando...";
        var data = await _httpClient.GetStringAsync("https://api.ejemplo.com/datos");
        Estado = $"Recibidos {data.Length} caracteres";
    }
    catch (HttpRequestException ex)
    {
        Estado = $"Error de red: {ex.Message}";
    }
    catch (Exception ex)
    {
        Estado = $"Error inesperado: {ex.Message}";
    }
}
```

**Por qué importa**: si NO ponés el `try/catch`, una excepción en
`GetStringAsync` se va al `SynchronizationContext` y puede crashear la
app. La regla práctica con esta clase es: **toda lambda async que pases
al comando debe tener su propio `try/catch`**.

---

### 4.4. Comando con `CancellationToken` (cooperativo, vía coordinador externo)

El comando acepta el token, pero como internamente pasa `None`, hay que
inyectar la cancelación desde otro lado. El patrón usado en este repo es
un *coordinador singleton* que maneja el `CancellationTokenSource`:

```csharp
public class GpsCoordinator
{
    private CancellationTokenSource? _cts;

    public async Task<GpsResult> CapturarAsync(CancellationToken external = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(external);
        try
        {
            return await ObtenerUbicacionInternaAsync(_cts.Token);
        }
        finally { _cts = null; }
    }

    public void Cancelar() => _cts?.Cancel();
}

// En el ViewModel:
ObtenerUbicacionCommand = new AsyncRelayCommand(() => _coord.CapturarAsync());
CancelarCommand        = new RelayCommand(() => _coord.Cancelar());
```

**Lección**: con esta implementación, la cancelación no la maneja el
comando; la maneja el servicio que está abajo. Si necesitás cancelación
fina por comando, conviene migrar a `CommunityToolkit.Mvvm`, que ya
provee un `IRelayCommand` cancelable.

---

### 4.5. `RelayCommand` (variante sincrónica) para acciones rápidas

La misma clase trae `RelayCommand` (sin `Async`) para acciones que no
son `Task`:

```csharp
public ICommand AbrirAjustesCommand { get; }
public ICommand CerrarOverlayCommand { get; }

public MainPageViewModel()
{
    AbrirAjustesCommand  = new RelayCommand(() => AppInfo.ShowSettingsUI());
    CerrarOverlayCommand = new RelayCommand(() => Overlay.IsVisible = false);
}
```

**Cuándo usar cuál**:
- `AsyncRelayCommand` → cuando el método **devuelve `Task`** o tarda
  más de un par de milisegundos.
- `RelayCommand` → para abrir ajustes, ocultar paneles, alternar flags,
  navegación instantánea, etc.

---

### 4.6. Anti-reentrancia en acción — caso "doble click"

```csharp
public ICommand ImprimirTicketCommand { get; }

public MainPageViewModel()
{
    ImprimirTicketCommand = new AsyncRelayCommand(ImprimirAsync);
}

private async Task ImprimirAsync()
{
    await _impresora.AbrirConexionAsync();
    await _impresora.EnviarBytesAsync(ticketBytes);
    await _impresora.CerrarConexionAsync();
}
```

**Qué garantiza el comando**: aunque el usuario toque el botón 5 veces
seguidas mientras se imprime, **se imprime un solo ticket**. La segunda,
tercera, etc., llamadas a `Execute` salen por `CanExecute() == false` en
[línea 43](../Phone/Ejemplo_Maui_DirectCall/ViewModels/AsyncRelayCommand.cs#L43)
sin disparar nada.

Sin esta clase, escrito como `async void OnClick(...)`, los 5 toques
producirían 5 conexiones concurrentes a la impresora.

---

### 4.7. Combinando `IsRunning` con un indicador visual

La propiedad `IsRunning` es pública (read-only), así que se puede
bindear a la UI:

```csharp
ObtenerUbicacionCommand = new AsyncRelayCommand(ObtenerUbicacionAsync);
```

```xml
<Grid>
    <Button Text="Obtener ubicación"
            Command="{Binding ObtenerUbicacionCommand}" />

    <ActivityIndicator
        IsRunning="{Binding ObtenerUbicacionCommand.IsRunning}"
        IsVisible="{Binding ObtenerUbicacionCommand.IsRunning}" />
</Grid>
```

**Truco útil**: como `IsRunning` no implementa `INotifyPropertyChanged`
en esta versión minimalista, el binding sobre la propiedad puede no
refrescarse en algunos escenarios. Si necesitás refresco confiable,
exponé una propiedad equivalente en el ViewModel y movéla a mano dentro
del método:

```csharp
private bool _cargando;
public bool Cargando
{
    get => _cargando;
    set => SetProperty(ref _cargando, value);
}

private async Task ObtenerUbicacionAsync()
{
    Cargando = true;
    try { /* ... */ }
    finally { Cargando = false; }
}
```

---

## 5. Comparación rápida con `CommunityToolkit.Mvvm.Input.AsyncRelayCommand`

| Característica | Esta versión | CommunityToolkit |
|----------------|--------------|------------------|
| Anti-reentrancia | ✅ | ✅ |
| `CanExecute` opcional | ✅ | ✅ |
| `ExecuteAsync` awaitable | ❌ | ✅ |
| Captura de excepciones configurable | ❌ | ✅ (`AsyncRelayCommandOptions`) |
| Cancelación cableada | ❌ | ✅ (con `IRelayCommand` cancelable) |
| Source generators (`[RelayCommand]`) | ❌ | ✅ |
| Dependencias externas | Ninguna | Paquete NuGet |
| Tamaño | ~50 líneas | Biblioteca completa |

**Cuándo conviene cada una**:
- **Esta versión** → proyectos didácticos, demos, ejemplos de cátedra
  donde se quiere mostrar MVVM "desde cero" sin paquetes.
- **CommunityToolkit.Mvvm** → producción, apps reales con tests, donde
  cancelación, awaitable y manejo de errores estructurado importan.

---

## 6. Reglas prácticas al usarlo

1. **Siempre poné `try/catch` dentro de tu método async.**
   La clase no captura excepciones; un fallo no manejado puede tumbar la app.
2. **Si tu `canExecute` depende de propiedades, llamá
   `RaiseCanExecuteChanged()`** desde el setter de esas propiedades.
3. **No uses `AsyncRelayCommand` para acciones triviales** (cambiar un
   booleano, navegar). Para eso está `RelayCommand`.
4. **Para cancelación real, no confíes en el `CancellationToken` del
   comando** — está, pero siempre vale `None`. Usá un coordinador o
   migrá a `CommunityToolkit.Mvvm`.
5. **Si necesitás bindear `IsRunning` a la UI**, considerá replicar el
   estado en una propiedad del ViewModel con `INotifyPropertyChanged`,
   por las razones explicadas en 4.7.

---

## 7. Resumen en una línea

`AsyncRelayCommand` convierte métodos `async Task` en `ICommand` con
**anti-reentrancia gratis**, a cambio de delegar el manejo de excepciones
y la cancelación al ViewModel. Es la pieza mínima para hacer MVVM async
en MAUI sin paquetes.
