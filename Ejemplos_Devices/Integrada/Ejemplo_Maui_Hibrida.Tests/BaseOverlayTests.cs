using LibApp.Devices.Common.ViewModels;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

/// <summary>
/// La máquina de tres estados de `StatusOverlayViewModel`, que sostiene los cuatro overlays.
/// </summary>
public class BaseOverlayTests
{
    // Sonda mínima: expone los métodos protegidos para poder ejercitarlos.
    private sealed class Sonda : StatusOverlayViewModel
    {
        public void Busy(string t = "t", string s = "s", string i = "gif") => ShowBusy(t, s, i);
        public void Error(params OverlayAction[] a) => ShowError("error", "Título", "Mensaje", a);
    }

    private static OverlayAction Accion(string texto = "Botón", OverlayActionStyle estilo = OverlayActionStyle.Primary)
        => new(texto, new FakeCommand(), estilo);

    private sealed class FakeCommand : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? p) => true;
        public void Execute(object? p) { }
    }

    [Fact]
    public void Arranca_oculto()
    {
        var vm = new Sonda();
        Assert.Equal(OverlayMode.None, vm.Mode);
        Assert.False(vm.IsVisible);
    }

    [Fact]
    public void ShowBusy_deja_la_capa_de_espera_visible()
    {
        var vm = new Sonda();
        vm.Busy("Buscando", "Aguardá", "timer.gif");

        Assert.Equal(OverlayMode.Busy, vm.Mode);
        Assert.True(vm.IsBusy);
        Assert.False(vm.IsError);
        Assert.True(vm.IsVisible);
        Assert.Equal("Buscando", vm.BusyTitle);
    }

    [Fact]
    public void ShowError_puebla_la_botonera()
    {
        var vm = new Sonda();
        vm.Error(Accion("Reintentar"), Accion("Cerrar", OverlayActionStyle.Secondary));

        Assert.Equal(OverlayMode.Error, vm.Mode);
        Assert.Equal(2, vm.Actions.Count);
        Assert.Equal("Reintentar", vm.Actions[0].Text);
    }

    // La capa Busy no admite botones: ShowBusy limpia Actions incondicionalmente. No es un
    // descuido — sin el Clear, los botones del ShowError anterior sobrevivirían y reaparecerían
    // mezclados con los nuevos. El costo es que toda espera es no-cancelable.
    [Fact]
    public void ShowBusy_limpia_la_botonera_anterior()
    {
        var vm = new Sonda();
        vm.Error(Accion("Reintentar"), Accion("Cerrar"));
        Assert.Equal(2, vm.Actions.Count);

        vm.Busy();

        Assert.Empty(vm.Actions);
    }

    [Fact]
    public void ShowError_no_acumula_botones_entre_llamadas()
    {
        var vm = new Sonda();
        vm.Error(Accion("A"), Accion("B"));
        vm.Error(Accion("C"));

        Assert.Single(vm.Actions);
        Assert.Equal("C", vm.Actions[0].Text);
    }

    [Fact]
    public void Hide_oculta_desde_cualquier_estado()
    {
        var vm = new Sonda();
        vm.Error(Accion());
        vm.Hide();

        Assert.Equal(OverlayMode.None, vm.Mode);
        Assert.False(vm.IsVisible);
    }

    // Primary es el default del record: por eso los llamadores escriben Secondary explícito y
    // omiten el primario. Si esto cambiara, la mitad de las botoneras del proyecto se
    // degradarían en silencio.
    [Fact]
    public void OverlayAction_es_Primary_por_defecto()
    {
        var a = new OverlayAction("X", new FakeCommand());
        Assert.Equal(OverlayActionStyle.Primary, a.Style);
    }
}
