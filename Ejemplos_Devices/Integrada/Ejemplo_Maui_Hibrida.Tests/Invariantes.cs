using System.Reflection;
using LibApp.Devices.Common.ViewModels;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

/// <summary>
/// Los cinco invariantes del patrón de overlays, ejecutables.
/// Ver `Ejemplos_Maui_Devices.Documentos/Analisis/Plan-Armonizacion-Overlays.md` §2.
/// Cada aserción de acá nació de un defecto real encontrado en el código.
/// </summary>
public static class Invariantes
{
    /// <summary>
    /// I-1 · Toda variante no-Success produce exactamente una pantalla.
    /// <para>
    /// Un `case` que sólo hace `Hide()` deja al usuario sin ninguna respuesta. Es el defecto que
    /// tenía impresión ante un fallo de red (tocar «Imprimir» y que no pasara nada) y el que
    /// tiene GPS hoy en 5 de sus 8 variantes.
    /// </para>
    /// </summary>
    public static void MuestraUnaPantalla(StatusOverlayViewModel vm, string caso)
    {
        Assert.True(vm.Mode == OverlayMode.Error,
            $"I-1 · «{caso}» no muestra ninguna pantalla: Mode quedó en {vm.Mode}. " +
            $"El usuario no recibe ninguna respuesta del sistema.");

        Assert.True(vm.Actions.Count > 0,
            $"I-1 · «{caso}» muestra una pantalla sin ninguna salida: el usuario queda atrapado.");

        Assert.False(string.IsNullOrWhiteSpace(vm.Title), $"I-1 · «{caso}» no tiene título.");
        Assert.False(string.IsNullOrWhiteSpace(vm.Message), $"I-1 · «{caso}» no tiene mensaje.");
    }

    /// <summary>
    /// I-3 · Ningún mensaje crudo del sistema llega al usuario.
    /// <para>
    /// El usuario leía literalmente `Print failed after 1 attempt(s): paper out`. El texto
    /// técnico se conserva para el log, pero no se muestra.
    /// </para>
    /// </summary>
    public static void NoFiltraTextoTecnico(StatusOverlayViewModel vm, string textoTecnico, string caso)
    {
        Assert.False(vm.Message.Contains(textoTecnico, StringComparison.OrdinalIgnoreCase),
            $"I-3 · «{caso}» le muestra al usuario el texto técnico «{textoTecnico}». " +
            $"Mensaje mostrado: «{vm.Message}»");
    }

    /// <summary>
    /// I-4 · Toda pantalla de error tiene exactamente un botón primario.
    /// <para>
    /// `Primary` es «el DataTrigger de Secondary no disparó», así que omitirlo no da error de
    /// compilación: simplemente ninguna acción destaca y la UI no guía. Pasa hoy en la pantalla
    /// de permiso denegado de GPS y de Telefonía, donde la variable se llama `primary` pero se
    /// construye `Secondary`.
    /// </para>
    /// </summary>
    public static void TieneUnSoloPrimario(StatusOverlayViewModel vm, string caso)
    {
        var primarios = vm.Actions.Count(a => a.Style == OverlayActionStyle.Primary);

        Assert.True(primarios == 1,
            $"I-4 · «{caso}» tiene {primarios} botones primarios (debe tener 1). " +
            $"Botonera: [{string.Join(" · ", vm.Actions.Select(a => $"{a.Text}:{a.Style}"))}]");
    }

    /// <summary>
    /// I-2 · Toda variante del resultado tipado tiene su pantalla.
    /// <para>
    /// Devuelve las variantes selladas anidadas de un `abstract record` de resultado. Es la
    /// única defensa automatizable contra el defecto más caro del patrón: modelar una variante
    /// y no producirla nunca. C# no lo verifica — un `switch` sin un caso compila igual, y un
    /// tipo cerrado da ilusión de exhaustividad.
    /// </para>
    /// </summary>
    public static IEnumerable<Type> VariantesDe<TResultado>()
        => typeof(TResultado)
            .GetNestedTypes(BindingFlags.Public)
            .Where(t => t.IsSealed && typeof(TResultado).IsAssignableFrom(t))
            .OrderBy(t => t.Name);
}
