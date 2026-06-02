# Eventos en .NET MAUI: `WebView` + `EventToCommandBehavior`

Apunte sobre un problema típico al enlazar eventos de un control (en este caso un `WebView`) a comandos de un ViewModel mediante `EventToCommandBehavior` del **CommunityToolkit.Maui** cuando se usan **compiled bindings** (`x:DataType`).

---

## 1. Síntoma

- La aplicación inicia y el `WebView` muestra correctamente la URL (por ejemplo `https://www.google.com`).
- **Pero los eventos `Navigating` / `Navigated` nunca se disparan** sobre el ViewModel.
- No aparece ninguna excepción en la ventana de salida.

---

## 2. Causa: compiled bindings + Behaviors

Cuando se declara el tipo del `BindingContext` a nivel página:

```xml
<ContentPage ...
             xmlns:pageModels="clr-namespace:Ejemplo_Maui_Hibrida.ViewModels"
             x:DataType="pageModels:MainViewModel">
```

Y los bindings de los comandos viven **dentro de un `Behavior`**:

```xml
<WebView.Behaviors>
    <toolkit:EventToCommandBehavior EventName="Navigating"
                                    Command="{Binding NavigatingCommand}" />
    <toolkit:EventToCommandBehavior EventName="Navigated"
                                    Command="{Binding NavigatedCommand}" />
</WebView.Behaviors>
```

Sucede lo siguiente:

- Los `Behavior` **no heredan de forma fiable el `BindingContext`** del control al que se adjuntan.
- Con **compiled bindings**, el compilador XAML resuelve el binding usando el `x:DataType` del padre más cercano, pero **no entra dentro del `BehaviorCollection`**.
- Resultado: el binding se compila contra un `DataType` equivocado (o `null`) y el `Command` queda en `null`.
- El evento llega al `Behavior`, pero como el `Command` es `null`, **no invoca nada y no falla**.

---

## 3. Cómo confirmarlo en 30 segundos

Poner un breakpoint y un log en la primera línea del comando:

```csharp
[RelayCommand]
private void Navigating(WebNavigatingEventArgs e)
{
    System.Diagnostics.Debug.WriteLine("⚡ Navigating disparado: " + e.Url);
    // ...
}
```

- Si **no** se imprime nada y **no** se detiene el breakpoint → es el problema de binding del `Command` (ver soluciones).
- Si **sí** se imprime → el evento sí llega y el problema es otro (por ejemplo, un deadlock por `GetAwaiter().GetResult()` dentro de un handler sincrónico que llama a un método `async`).

---

## 4. Solución 1 (recomendada): bindear con `Source={x:Reference page}`

Le damos un `x:Name` a la página y bindeamos el `Command` relativo a su `BindingContext`. De esta forma el binding se resuelve siempre contra el ViewModel real.

```xml
<ContentPage ...
             x:Name="page"
             x:Class="Ejemplo_Maui_Hibrida.MainPage"
             x:DataType="pageModels:MainViewModel">

    <Grid>
        <RefreshView IsRefreshing="{Binding IsRefreshing}"
                     IsVisible="{Binding MostrarNavegador}">
            <WebView x:Name="webView" Source="{Binding Url}">
                <WebView.Behaviors>
                    <toolkit:EventToCommandBehavior
                        EventName="Navigating"
                        Command="{Binding BindingContext.NavigatingCommand,
                                          Source={x:Reference page}}" />
                    <toolkit:EventToCommandBehavior
                        EventName="Navigated"
                        Command="{Binding BindingContext.NavigatedCommand,
                                          Source={x:Reference page}}" />
                </WebView.Behaviors>
            </WebView>
        </RefreshView>
    </Grid>
</ContentPage>
```

Funciona incluso con compiled bindings activadas.

---

## 5. Solución 2: declarar `x:DataType` en el propio `Behavior`

Si se quiere mantener el binding simple `{Binding NavigatingCommand}`, se puede indicar al compilador qué tipo esperar:

```xml
<toolkit:EventToCommandBehavior
    x:DataType="pageModels:MainViewModel"
    EventName="Navigating"
    Command="{Binding NavigatingCommand}" />
```

> ℹ️ En algunas versiones de MAUI, los behaviors no reciben el `BindingContext` heredado automáticamente. Si esta variante no funciona, usar la **Solución 1**.

---

## 6. Solución 3: manejar los eventos en el code-behind

La opción más simple de depurar (con breakpoints e hot-reload) es no usar `EventToCommandBehavior` y suscribirse al evento desde el code-behind, delegando al ViewModel:

```xml
<WebView x:Name="webView"
         Source="{Binding Url}"
         Navigating="WebView_Navigating"
         Navigated="WebView_Navigated" />
```

```csharp
private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
{
    if (BindingContext is MainViewModel vm &&
        vm.NavigatingCommand.CanExecute(e))
    {
        vm.NavigatingCommand.Execute(e);
    }
}

private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
{
    if (BindingContext is MainViewModel vm &&
        vm.NavigatedCommand.CanExecute(e))
    {
        vm.NavigatedCommand.Execute(e);
    }
}
```

---

## 7. Segundo problema: el comando se invoca, pero `e` llega `null`

Una vez que el binding del `Command` se resuelve correctamente (Soluciones 1, 2 o 3), aparece un **segundo problema** muy común con `EventToCommandBehavior` del CommunityToolkit.Maui:

```csharp
[RelayCommand]
private void Navigating(WebNavigatingEventArgs e)
{
    // e == null  😱
}
```

### 7.1. Causa

A diferencia del viejo `EventToCommandBehavior` de Xamarin/Xamarin Community Toolkit (que pasaba los `EventArgs` automáticamente), en **CommunityToolkit.Maui** el contrato cambió:

> *Si `CommandParameter` no está seteado, se utiliza `EventArgsConverter` para convertir los `EventArgs` del evento al tipo que el `Command` espera. Si **ninguno** de los dos está seteado, se pasa `null`.*

Es decir: **por defecto el evento NO viaja al comando**. Hay que decirle explícitamente cómo convertir/pasar los `EventArgs`.

Además, el generador de `[RelayCommand]` crea internamente un `RelayCommand<T>` cuyo `Execute(object? parameter)` hace algo así:

```csharp
if (parameter is T t) action(t);
else                  action(null);   // ← acá te llega null
```

Si lo que llega no es exactamente del tipo `T`, **te entra `null` sin error**.

### 7.2. Diagnóstico rápido

Cambiá temporalmente la firma del método a `object` para ver qué te está llegando realmente:

```csharp
[RelayCommand]
private void Navigating(object e)
{
    System.Diagnostics.Debug.WriteLine(
        $"[Navigating] tipo={e?.GetType().FullName ?? "NULL"}");
}
```

- Si imprime `NULL` → no hay `CommandParameter` ni `EventArgsConverter` configurados (caso típico).
- Si imprime otro tipo → un `CommandParameter` o un `EventArgsConverter` mal armado está pisando los args.
- Si imprime `Microsoft.Maui.Controls.WebNavigatingEventArgs` → el problema está en otro lado (revisá el tipo del parámetro de tu método).

### 7.3. Solución A (recomendada): manejar el evento en code-behind

Es lo más simple y elimina toda la complejidad del converter. Coincide con la Solución 3 de la sección 6:

```xml
<WebView x:Name="webView"
         Source="{Binding Url}"
         Navigating="WebView_Navigating"
         Navigated="WebView_Navigated" />
```

```csharp
private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
{
    if (BindingContext is MainViewModel vm && vm.NavigatingCommand.CanExecute(e))
        vm.NavigatingCommand.Execute(e);
}

private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
{
    if (BindingContext is MainViewModel vm && vm.NavigatedCommand.CanExecute(e))
        vm.NavigatedCommand.Execute(e);
}
```

`e` siempre llega con el valor correcto.

### 7.4. Solución B: crear un `EventArgsConverter`

Si querés mantener MVVM puro con `EventToCommandBehavior`, hay que crear un converter por cada tipo de `EventArgs`.

#### 1) Crear los converters

```csharp Integrada\Ejemplo_Maui_Hibrida\Converters\WebNavigatingEventArgsConverter.cs
using CommunityToolkit.Maui.Converters;

namespace Ejemplo_Maui_Hibrida.Converters;

public class WebNavigatingEventArgsConverter
    : BaseConverterOneWay<WebNavigatingEventArgs, WebNavigatingEventArgs>
{
    public override WebNavigatingEventArgs DefaultConvertReturnValue { get; set; } = null!;

    public override WebNavigatingEventArgs ConvertFrom(
        WebNavigatingEventArgs value, System.Globalization.CultureInfo? culture)
        => value;
}

public class WebNavigatedEventArgsConverter
    : BaseConverterOneWay<WebNavigatedEventArgs, WebNavigatedEventArgs>
{
    public override WebNavigatedEventArgs DefaultConvertReturnValue { get; set; } = null!;

    public override WebNavigatedEventArgs ConvertFrom(
        WebNavigatedEventArgs value, System.Globalization.CultureInfo? culture)
        => value;
}
```

> El converter parece "no hacer nada", pero su rol es **autorizar el paso del `EventArgs`** como parámetro del comando. Sin él, el behavior pasa `null`.

#### 2) Registrarlos como recursos en el XAML

```xml
<ContentPage ...
             xmlns:conv="clr-namespace:Ejemplo_Maui_Hibrida.Converters"
             x:Name="page"
             x:DataType="pageModels:MainViewModel">

    <ContentPage.Resources>
        <conv:WebNavigatingEventArgsConverter x:Key="NavigatingConverter" />
        <conv:WebNavigatedEventArgsConverter  x:Key="NavigatedConverter" />
    </ContentPage.Resources>

    <Grid>
        <RefreshView IsRefreshing="{Binding IsRefreshing}"
                     IsVisible="{Binding MostrarNavegador}">
            <WebView x:Name="webView" Source="{Binding Url}">
                <WebView.Behaviors>
                    <toolkit:EventToCommandBehavior
                        EventName="Navigating"
                        EventArgsConverter="{StaticResource NavigatingConverter}"
                        Command="{Binding BindingContext.NavigatingCommand,
                                          Source={x:Reference page}}" />

                    <toolkit:EventToCommandBehavior
                        EventName="Navigated"
                        EventArgsConverter="{StaticResource NavigatedConverter}"
                        Command="{Binding BindingContext.NavigatedCommand,
                                          Source={x:Reference page}}" />
                </WebView.Behaviors>
            </WebView>
        </RefreshView>
    </Grid>
</ContentPage>
```

Ahora el comando recibirá el `WebNavigatingEventArgs` real y `e` deja de venir `null`.

### 7.5. Errores comunes a evitar

- **Confundir el evento con el comando**: enganchar `Navigated` al `NavigatingCommand` en lugar de a `NavigatedCommand`. Es un copy/paste muy fácil de cometer.
- **Setear `CommandParameter` "por las dudas"**: si lo bindeás a algo que resuelve a `null` o a otro tipo, pisa los `EventArgs` y vuelve a llegar `null` al comando.
- **Olvidarse de registrar el converter como recurso** (`<ContentPage.Resources>`).

---

## 8. Resumen / checklist final

1. Verificar con un `Debug.WriteLine` si el comando se llega a invocar.
2. Si **no** se invoca → el binding del `Command` no se resolvió:
   - Usar `Source={x:Reference page}` (Solución 1, recomendada).
   - O declarar `x:DataType` en el `Behavior` (Solución 2).
   - O manejar el evento en code-behind (Solución 3).
3. Si **sí** se invoca pero `e == null` → falta `EventArgsConverter` (sección 7):
   - Crear un converter por tipo de `EventArgs` (Solución B).
   - O pasarse a code-behind (Solución A, recomendada por simplicidad).
4. Si **sí** se invoca y se cuelga → revisar deadlocks (`async void` en handlers, `GetAwaiter().GetResult()` en el hilo de UI, etc.).

> Reglas prácticas:
> - Con **compiled bindings** + `EventToCommandBehavior`, **siempre** especificar explícitamente el origen del binding del `Command` (vía `x:Reference` a la página o `x:DataType` en el behavior).
> - En **CommunityToolkit.Maui**, `EventToCommandBehavior` **no pasa los `EventArgs` por defecto**: hay que configurar un `EventArgsConverter` o usar code-behind.
