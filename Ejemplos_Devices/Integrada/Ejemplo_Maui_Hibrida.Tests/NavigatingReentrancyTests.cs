using Ejemplo_Maui_Hibrida.Tests.Fakes;
using Ejemplo_Maui_Hibrida.ViewModels;
using LibApp.Devices.Common.Services;
using LibApp.Devices.GPS.ViewModels;
using LibApp.Devices.Networks.ViewModels;
using LibApp.Devices.Phone.ViewModels;
using LibApp.Devices.MotorDSL.ViewModels;
using LibApp.UrlCommands;
using Microsoft.Maui.Controls;
using Xunit;

namespace Ejemplo_Maui_Hibrida.Tests;

// ADR-0009 · D1: ante clicks repetidos sobre Foto/Selfie/QR la página se recargaba. Causa: el
// AsyncRelayCommand de Navigating no permite ejecución concurrente por defecto → mientras el 1er
// comando espera al dispositivo, el 2º evento no ejecuta el comando y `e.Cancel` NO se fija → el
// WebView hace la navegación real (recarga). El fix: AllowConcurrentExecutions=true + guard de
// reentrada `comandoEnCurso`, que SIEMPRE cancela y descarta el duplicado.
public class NavigatingReentrancyTests
{
    // Handler que se queda "en vuelo" hasta que el test lo libera: simula la cámara/QR abiertos.
    private sealed class HandlerLento : IUrlCommandHandler
    {
        private readonly TaskCompletionSource _liberar = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Invocaciones { get; private set; }

        public bool CanHandle(string url) => url.Contains("photo=photo", StringComparison.OrdinalIgnoreCase);

        public async Task<BridgeOutcome> HandleAsync(string url)
        {
            Invocaciones++;
            await _liberar.Task;                 // permanece en vuelo (dispositivo abierto)
            return new BridgeOutcome(true, null);
        }

        public void Completar() => _liberar.TrySetResult();
    }

    private static MainViewModel CrearVm(IUrlCommandHandler handler)
    {
        var bridge = new FakeWebViewBridge();
        var dispatcher = new UrlCommandDispatcher(new[] { handler });
        var net = new NetworkOverlayViewModel(new FakeNetworkService(), bridge, new FakeUiDispatcher());
        var gps = new GpsOverlayViewModel(new FakeGpsService());
        var call = new CallOverlayViewModel(new FakeCallService());
        var printer = new PrinterOverlayViewModel(new FakePrinterService());
        return new MainViewModel(net, gps, call, printer, dispatcher, bridge);
    }

    private static WebNavigatingEventArgs Evento(string url) =>
        new(WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url);

    [Fact]
    public async Task Segundo_click_durante_un_comando_en_curso_se_cancela_y_no_re_despacha()
    {
        var handler = new HandlerLento();
        var vm = CrearVm(handler);

        // 1er click: arranca el comando y queda en vuelo (handler bloqueado).
        var e1 = Evento("/panel?photo=photo&param=imgFoto1");
        var t1 = vm.NavigatingCommand.ExecuteAsync(e1);

        Assert.True(e1.Cancel);                 // cancela la navegación-comando (no recarga)
        Assert.Equal(1, handler.Invocaciones);
        Assert.False(t1.IsCompleted);           // sigue esperando al "dispositivo"

        // 2º click mientras el 1º sigue en vuelo: DEBE cancelar y NO disparar un 2º despacho.
        var e2 = Evento("/panel?photo=photo&param=imgFoto1");
        await vm.NavigatingCommand.ExecuteAsync(e2);

        Assert.True(e2.Cancel);                 // ← lo que el bug NO hacía: dejaba recargar
        Assert.Equal(1, handler.Invocaciones);  // guard de reentrada: no se re-despachó

        // Se libera el 1º y se completa limpio.
        handler.Completar();
        await t1;
    }

    [Fact]
    public async Task Tras_completar_el_comando_un_nuevo_click_vuelve_a_despachar()
    {
        var handler = new HandlerLento();
        var vm = CrearVm(handler);

        var e1 = Evento("/panel?photo=photo&param=imgFoto1");
        var t1 = vm.NavigatingCommand.ExecuteAsync(e1);
        handler.Completar();
        await t1;

        // Nuevo click una vez cerrado el comando anterior: el guard ya se liberó.
        var e2 = Evento("/panel?photo=photo&param=imgFoto1");
        await vm.NavigatingCommand.ExecuteAsync(e2);

        Assert.True(e2.Cancel);
        Assert.Equal(2, handler.Invocaciones);
    }

    [Fact]
    public async Task Una_url_que_no_es_comando_no_se_cancela()
    {
        var vm = CrearVm(new HandlerLento());

        var e = Evento("/panel?algo=otracosa");
        await vm.NavigatingCommand.ExecuteAsync(e);

        Assert.False(e.Cancel);                 // navegación normal: no se toca
    }
}
