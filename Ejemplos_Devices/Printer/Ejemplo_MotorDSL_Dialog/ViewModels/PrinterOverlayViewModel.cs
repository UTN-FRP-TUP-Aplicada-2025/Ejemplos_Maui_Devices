using CommunityToolkit.Mvvm.Input;

using Ejemplo_MotorDSL_Dialog.Models;
using Ejemplo_MotorDSL_Dialog.Services;

using MotorDsl.Core.Models;
using MotorDsl.Printing;

namespace Ejemplo_MotorDSL_Dialog.ViewModels;

/// <summary>
/// Orquesta el flujo de impresión térmica Bluetooth sobre la capa de overlay
/// (espejo de CallOverlayViewModel). Recibe un <see cref="RenderResult"/> ya
/// generado y maneja permisos, descubrimiento, selección, conexión e impresión,
/// reusando la impresora predeterminada cuando está en la lista detectada.
/// Guarda <c>_bytes</c>, <c>_lastDevice</c> y <c>_lastRender</c> para reintentos.
/// </summary>
public partial class PrinterOverlayViewModel : StatusOverlayViewModel
{
    private readonly PrinterService _service;
    private byte[]? _bytes;
    private PrinterDevice? _lastDevice;
    private RenderResult? _lastRender;

    public PrinterOverlayViewModel(PrinterService service)
    {
        _service = service;
        Hide();
    }

    public async Task ImprimirAsync(RenderResult render)
    {
        _lastRender = render;

        if (!render.IsSuccessful || render.Output is not byte[] bytes)
        {
            ShowError("error", "No se pudo generar el documento",
                render.Errors.FirstOrDefault() ?? "Error de render",
                new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
            return;
        }
        _bytes = bytes;

        if (!_service.IsSupported)
        {
            ShowError("print_disabled", "Impresión no disponible",
                "Este dispositivo no puede imprimir por Bluetooth.",
                new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
            return;
        }

        ShowBusy("Preparando impresión", "Verificando permisos…", "timer.gif");
        var perm = await _service.EnsurePermissionsAsync();
        if (perm != BluetoothPermissionResult.Granted) { MostrarPermiso(perm); return; }

        await BuscarYImprimirAsync();
    }

    private async Task BuscarYImprimirAsync()
    {
        ShowBusy("Buscando impresoras", "Aguardá unos segundos…", "timer.gif");
        switch (await _service.DiscoverAsync())
        {
            case DiscoverResult.Found f:
                var def = _service.GetDefaultIfPresent(f.Devices);
                if (def is not null) { await ConectarEImprimirAsync(def); return; }
                if (f.Devices.Count == 1) { await ConectarEImprimirAsync(f.Devices[0]); return; }
                MostrarSelector(f.Devices);
                break;

            case DiscoverResult.Empty:
                ShowError("print_disabled", "No se encontraron impresoras",
                    "Encendé la impresora y volvé a intentar.",
                    new OverlayAction("Reintentar", ReintentarBuscarCommand),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case DiscoverResult.BluetoothOff:
                ShowError("bluetooth_disabled", "Bluetooth desactivado",
                    "Activá el Bluetooth para buscar impresoras.",
                    new OverlayAction("Reintentar", ReintentarBuscarCommand),
                    new OverlayAction("Abrir configuración", AbrirAjustesCommand, OverlayActionStyle.Secondary),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case DiscoverResult.NotSupported:
                ShowError("print_disabled", "Impresión no disponible",
                    "Este dispositivo no puede imprimir por Bluetooth.",
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;
        }
    }

    private void MostrarSelector(IReadOnlyList<PrinterDevice> devices)
    {
        var actions = devices
            .Select(d => new OverlayAction(d.Name ?? d.Id,
                new RelayCommand(() => _ = ConectarEImprimirAsync(d))))
            .Append(new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary))
            .ToArray();
        ShowError("print", "Elegí una impresora",
            "Se detectaron varias impresoras disponibles.", actions);
    }

    private async Task ConectarEImprimirAsync(PrinterDevice device)
    {
        _lastDevice = device;
        ShowBusy("Conectando", $"Conectando a {device.Name}…", "timer.gif");
        if (!await _service.ConnectAsync(device))
        {
            ShowError("print_disabled", "No se pudo conectar",
                $"No fue posible conectar a {device.Name}.",
                new OverlayAction("Reintentar", ReintentarConectarCommand),
                new OverlayAction("Elegir otra", ReintentarBuscarCommand, OverlayActionStyle.Secondary),
                new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
            return;
        }

        ShowBusy("Imprimiendo", "Enviando el documento…", "timer.gif");
        var res = await _service.SendAsync(_bytes!);
        if (res is PrintResult.Success) { Hide(); return; }

        var msg = res is PrintResult.Failure f ? f.Message : "Error desconocido";
        ShowError("error", "Falló la impresión", msg,
            new OverlayAction("Reintentar", ReintentarImprimirCommand),
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
    }

    private void MostrarPermiso(BluetoothPermissionResult perm)
    {
        switch (perm)
        {
            case BluetoothPermissionResult.DeniedCanRetry:
                ShowError("bluetooth_disabled", "Permiso Bluetooth necesario",
                    "Para imprimir necesitamos acceso al Bluetooth. Podés intentar concederlo.",
                    new OverlayAction("Pedir permiso", PedirPermisoCommand),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case BluetoothPermissionResult.Denied:
                ShowError("bluetooth_disabled", "Acceso Bluetooth denegado",
                    "Habilitá el permiso de Bluetooth desde los ajustes de la aplicación.",
                    new OverlayAction("Abrir configuración", AbrirAjustesCommand),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case BluetoothPermissionResult.Restricted:
                ShowError("block", "Acceso restringido",
                    "El Bluetooth está restringido por una política del dispositivo.",
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;
        }
    }

    [RelayCommand] private void CerrarOverlay() => Hide();

    [RelayCommand] private void AbrirAjustes() => _service.OpenAppSettings();

    // Reintenta el permiso sin rehacer el render: reusa el último RenderResult.
    [RelayCommand] private Task PedirPermiso() => ImprimirAsync(_lastRender!);

    [RelayCommand] private Task ReintentarBuscar() => BuscarYImprimirAsync();

    [RelayCommand] private Task ReintentarConectar() => ConectarEImprimirAsync(_lastDevice!);

    [RelayCommand]
    private async Task ReintentarImprimir()
    {
        ShowBusy("Imprimiendo", "Enviando el documento…", "timer.gif");
        var res = await _service.SendAsync(_bytes!);
        if (res is PrintResult.Success) { Hide(); return; }

        var msg = res is PrintResult.Failure f ? f.Message : "Error desconocido";
        ShowError("error", "Falló la impresión", msg,
            new OverlayAction("Reintentar", ReintentarImprimirCommand),
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
    }
}
