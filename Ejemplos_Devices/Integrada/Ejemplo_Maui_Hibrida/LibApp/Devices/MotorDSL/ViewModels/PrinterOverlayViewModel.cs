using CommunityToolkit.Mvvm.Input;
using LibApp.Devices.Common.ViewModels;
using LibApp.Devices.MotorDSL.Models;
using LibApp.Devices.MotorDSL.Services;
using MotorDsl.Core.Models;
using MotorDsl.Printing;

namespace LibApp.Devices.MotorDSL.ViewModels;

/// <summary>
/// Orquesta el flujo de impresión térmica Bluetooth sobre la capa de overlay
/// (espejo de CallOverlayViewModel). Recibe un <see cref="RenderResult"/> ya
/// generado y maneja permisos, descubrimiento, selección, conexión e impresión,
/// reusando la impresora predeterminada cuando está en la lista detectada.
/// Guarda <c>_bytes</c>, <c>_lastDevice</c> y <c>_lastRender</c> para reintentos.
/// </summary>
public partial class PrinterOverlayViewModel : StatusOverlayViewModel
{
    private readonly IPrinterService _service;
    private byte[]? _bytes;
    private PrinterDevice? _lastDevice;
    private RenderResult? _lastRender;

    // Si _lastDevice se reusó como predeterminada. Lo necesita "Reintentar": sin esto, el
    // segundo fallo se diagnosticaría como una conexión común y el usuario perdería la salida
    // de "Olvidar y emparejar otra", que es justamente la que necesita si la impresora cambió.
    private bool _lastEraPredeterminada;

    // Última lista descubierta: decide si ofrecer "Elegir otra impresora" (sólo tiene sentido
    // si hay más de una emparejada) y alimenta la desambiguación de homónimas.
    private IReadOnlyList<PrinterDevice> _lastDevices = Array.Empty<PrinterDevice>();

    // Rehace la obtención del documento desde la red. Lo provee quien tenga el documento
    // remoto (PrintCommandHandler); nulo cuando el documento es local.
    private Func<Task>? _reintentarDocumento;

    public PrinterOverlayViewModel(IPrinterService service)
    {
        _service = service;
        Hide();
    }

    /// <summary>
    /// Capa de espera mientras se obtiene el comprobante. Sin esto el usuario espera hasta el
    /// timeout del GET sin ninguna señal en pantalla.
    /// </summary>
    public void MostrarObteniendoDocumento()
        => ShowBusy("Preparando impresión", "Obteniendo el comprobante…", "timer.gif");

    /// <summary>
    /// Muestra un fallo anterior al render (no se pudo traer el documento).
    /// </summary>
    /// <param name="reintentar">
    /// Rehace el GET. Es el único reintento del flujo que vuelve a la red: reusar el render
    /// anterior no serviría, porque justamente no hay documento. Nulo ⇒ no se ofrece reintento.
    /// </param>
    public void MostrarFalloDocumento(PrintFailure failure, Func<Task>? reintentar = null)
    {
        _reintentarDocumento = reintentar;

        var acciones = new List<OverlayAction>();
        if (reintentar is not null)
            acciones.Add(new OverlayAction("Reintentar", ReintentarDocumentoCommand));

        acciones.Add(Cerrar(unico: acciones.Count == 0));

        ShowError("error", failure.Title, failure.DisplayMessage, acciones.ToArray());
    }

    public async Task ImprimirAsync(RenderResult render)
    {
        _lastRender = render;

        if (!render.IsSuccessful || render.Output is not byte[] bytes)
        {
            var detalle = render.Errors.FirstOrDefault() ?? "Error de render";
            var fallo = PrinterErrorCatalog.RenderFallido(detalle);
            ShowError("error", fallo.Title, fallo.DisplayMessage, Cerrar(unico: true));
            return;
        }
        _bytes = bytes;

        if (!_service.IsSupported)
        {
            ShowError("print_disabled", "Impresión no disponible",
                "Este dispositivo no puede imprimir por Bluetooth.",
                Cerrar(unico: true));
            return;
        }

        ShowBusy("Preparando impresión", "Verificando permisos…", "timer.gif");
        var perm = await _service.EnsurePermissionsAsync();
        if (perm != BluetoothPermissionResult.Granted) { MostrarPermiso(perm); return; }

        await BuscarYImprimirAsync();
    }

    /// <param name="forzarSelector">
    /// Saltea la impresora predeterminada y muestra siempre el selector. Es la salida del caso
    /// en que la predeterminada está emparejada pero no responde: sin esto, "Elegir otra"
    /// volvía a descubrir, volvía a encontrar la predeterminada en BondedDevices y volvía a
    /// conectar con la misma — un bucle sin salida por UI.
    /// </param>
    private async Task BuscarYImprimirAsync(bool forzarSelector = false)
    {
        ShowBusy("Buscando impresoras", "Aguardá unos segundos…", "timer.gif");
        switch (await _service.DiscoverAsync())
        {
            case DiscoverResult.Found f:
                _lastDevices = f.Devices;

                if (!forzarSelector)
                {
                    var def = _service.GetDefaultIfPresent(f.Devices);
                    if (def is not null) { await ConectarEImprimirAsync(def, esPredeterminada: true); return; }
                    if (f.Devices.Count == 1) { await ConectarEImprimirAsync(f.Devices[0], esPredeterminada: false); return; }
                }

                MostrarSelector(f.Devices);
                break;

            case DiscoverResult.Empty:
                var sinImpresoras = PrinterErrorCatalog.SinImpresoras();
                ShowError("print_disabled", sinImpresoras.Title, sinImpresoras.DisplayMessage,
                    new OverlayAction("Reintentar", ReintentarBuscarCommand),
                    new OverlayAction("Emparejar impresora", AbrirAjustesBluetoothCommand, OverlayActionStyle.Secondary),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case DiscoverResult.BluetoothOff off:
                ShowError("bluetooth_disabled", off.Error.Title, off.Error.DisplayMessage,
                    new OverlayAction("Activar Bluetooth", AbrirAjustesBluetoothCommand),
                    new OverlayAction("Reintentar", ReintentarBuscarCommand, OverlayActionStyle.Secondary),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case DiscoverResult.PermissionRevoked rev:
                ShowError("bluetooth_disabled", rev.Error.Title, rev.Error.DisplayMessage,
                    new OverlayAction("Abrir configuración", AbrirAjustesCommand),
                    new OverlayAction("Reintentar", ReintentarBuscarCommand, OverlayActionStyle.Secondary),
                    new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
                break;

            case DiscoverResult.NotSupported:
                ShowError("print_disabled", "Impresión no disponible",
                    "Este dispositivo no puede imprimir por Bluetooth.",
                    Cerrar(unico: true));
                break;
        }
    }

    /// <summary>
    /// «Cerrar». Cuando es la única acción de la pantalla es <b>primaria</b>: un botón único en
    /// gris comunica «esto no es importante» siendo lo único que el usuario puede hacer.
    /// Recordar que Primary es «el DataTrigger de Secondary no disparó», así que omitirlo no da
    /// ningún error y la pantalla queda sin nada que destaque.
    /// </summary>
    private OverlayAction Cerrar(bool unico = false)
        => new("Cerrar", CerrarOverlayCommand,
               unico ? OverlayActionStyle.Primary : OverlayActionStyle.Secondary);

    private void MostrarSelector(IReadOnlyList<PrinterDevice> devices)
    {
        // Los nombres BT de las térmicas genéricas son el modelo, no una identidad: dos 58HB6
        // producen dos botones idénticos. Sólo se agrega el sufijo de MAC cuando el nombre se
        // repite, para no ensuciar el caso habitual de impresoras con nombres distintos.
        var repetidos = devices.GroupBy(d => d.Name)
                               .Where(g => g.Count() > 1)
                               .Select(g => g.Key)
                               .ToHashSet();

        var actions = devices
            .Select(d => new OverlayAction(Etiqueta(d, repetidos.Contains(d.Name)),
                new RelayCommand(() => _ = ConectarEImprimirAsync(d, esPredeterminada: false))))
            .Append(new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary))
            .ToArray();

        ShowError("print", "Elegí una impresora",
            "Se detectaron varias impresoras disponibles.", actions);
    }

    /// <param name="esPredeterminada">
    /// Cambia el diagnóstico ante un fallo de conexión. Con una impresora que el usuario acaba
    /// de elegir, el fallo es simplemente "no se pudo conectar". Con la predeterminada —que se
    /// reusa sin preguntar— el fallo puede además significar que ya no es la impresora que
    /// tiene delante, y esa es la causa que hay que nombrar.
    /// </param>
    private async Task ConectarEImprimirAsync(PrinterDevice device, bool esPredeterminada)
    {
        _lastDevice = device;
        _lastEraPredeterminada = esPredeterminada;
        ShowBusy("Conectando", $"Conectando a {Etiqueta(device)}…", "timer.gif");

        if (!await _service.ConnectAsync(device))
        {
            if (esPredeterminada) MostrarPredeterminadaAusente(device);
            else MostrarNoSePudoConectar(device);
            return;
        }

        await EnviarAsync();
    }

    /// <summary>
    /// Fallo de la impresora predeterminada. El stack no puede distinguir "está apagada" de
    /// "no es la que se emparejó": las dos son el mismo fallo de socket. Se ofrecen las tres
    /// salidas y decide el usuario, que es quien ve el hardware.
    /// </summary>
    private void MostrarPredeterminadaAusente(PrinterDevice device)
    {
        var fallo = PrinterErrorCatalog.PredeterminadaAusente(Etiqueta(device));

        var acciones = new List<OverlayAction> { new("Reintentar", ReintentarConectarCommand) };

        // Sólo tiene sentido si hay otra emparejada a la que cambiar.
        if (_lastDevices.Count > 1)
            acciones.Add(new OverlayAction("Elegir otra impresora", ElegirOtraCommand, OverlayActionStyle.Secondary));

        acciones.Add(new OverlayAction("Olvidar y emparejar otra", OlvidarYEmparejarCommand, OverlayActionStyle.Secondary));
        acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));

        ShowError("print_disabled", fallo.Title, fallo.DisplayMessage, acciones.ToArray());
    }

    private void MostrarNoSePudoConectar(PrinterDevice device)
    {
        var fallo = PrinterErrorCatalog.NoSePudoConectar(Etiqueta(device));
        ShowError("print_disabled", fallo.Title, fallo.DisplayMessage,
            new OverlayAction("Reintentar", ReintentarConectarCommand),
            new OverlayAction("Elegir otra", ElegirOtraCommand, OverlayActionStyle.Secondary),
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
    }

    // Envío + manejo del fallo, en un solo lugar: lo usan tanto la primera impresión como el
    // reintento (antes eran dos copias del mismo bloque).
    private async Task EnviarAsync()
    {
        ShowBusy("Imprimiendo", "Enviando el documento…", "timer.gif");
        var res = await _service.SendAsync(_bytes!);
        if (res is PrintResult.Success) { Hide(); return; }

        MostrarFalloImpresion(res is PrintResult.Failure f
            ? f.Error
            : PrinterErrorCatalog.Describe(new Exception("Resultado de impresión no reconocido."),
                                           "SendAsync devolvió un PrintResult inesperado."));
    }

    /// <summary>
    /// Botonera según la causa: con el fallo identificado, el botón primario puede ser el que
    /// refleja la acción física en lugar de un "Reintentar" que va a fallar idéntico. El fraseo
    /// "Ya cargué papel" convierte el botón en la confirmación de un gesto, no en una
    /// repetición ciega.
    /// </summary>
    private void MostrarFalloImpresion(PrintFailure fallo)
    {
        var acciones = new List<OverlayAction>();

        switch (fallo.Code)
        {
            case PrinterErrorCatalog.HwPaper:
                acciones.Add(new OverlayAction("Ya cargué papel — Reintentar", ReintentarImprimirCommand));
                break;

            case PrinterErrorCatalog.HwCover:
                acciones.Add(new OverlayAction("Ya la cerré — Reintentar", ReintentarImprimirCommand));
                break;

            case PrinterErrorCatalog.LinkLost:
            case PrinterErrorCatalog.Timeout:
                acciones.Add(new OverlayAction("Reintentar", ReintentarImprimirCommand));
                if (_lastDevices.Count > 1)
                    acciones.Add(new OverlayAction("Elegir otra", ElegirOtraCommand, OverlayActionStyle.Secondary));
                break;

            default:
                acciones.Add(new OverlayAction("Reintentar", ReintentarImprimirCommand));
                break;
        }

        acciones.Add(new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
        ShowError("error", fallo.Title, fallo.DisplayMessage, acciones.ToArray());
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
                    Cerrar(unico: true));
                break;
        }
    }

    /// <summary>Etiqueta de un botón de impresora, desambiguando sólo si hace falta.</summary>
    private string Etiqueta(PrinterDevice d, bool ambiguo = false)
    {
        var alias = _service.GetAlias(d.Id);
        if (!string.IsNullOrEmpty(alias)) return alias;
        return ambiguo ? $"{d.Name} ({SufijoMac(d.Id)})" : d.Name ?? d.Id;
    }

    /// <summary>
    /// Últimos 2 octetos de la MAC: alcanza para desambiguar y no expone el identificador de
    /// hardware entero, en línea con el enmascarado que la librería ya aplica en diagnóstico.
    /// </summary>
    private static string SufijoMac(string id)
    {
        var partes = id.Split(':');
        return partes.Length >= 2 ? string.Join(":", partes[^2..]) : id;
    }

    [RelayCommand] private void CerrarOverlay() => Hide();

    [RelayCommand] private void AbrirAjustes() => _service.OpenAppSettings();

    [RelayCommand] private void AbrirAjustesBluetooth() => _service.OpenBluetoothSettings();

    // Reintenta el permiso sin rehacer el render: reusa el último RenderResult.
    [RelayCommand] private Task PedirPermiso() => ImprimirAsync(_lastRender!);

    [RelayCommand] private Task ReintentarBuscar() => BuscarYImprimirAsync();

    [RelayCommand] private Task ElegirOtra() => BuscarYImprimirAsync(forzarSelector: true);

    // Conserva el rol de la impresora: reintentar con la predeterminada tiene que volver a
    // ofrecer la misma salida, no degradar a un fallo de conexión común.
    [RelayCommand] private Task ReintentarConectar() => ConectarEImprimirAsync(_lastDevice!, _lastEraPredeterminada);

    [RelayCommand] private Task ReintentarImprimir() => EnviarAsync();

    [RelayCommand]
    private Task ReintentarDocumento()
        => _reintentarDocumento?.Invoke() ?? Task.CompletedTask;

    /// <summary>
    /// Olvida la predeterminada y lleva al usuario a emparejar la nueva. Es la acción del caso
    /// en que la impresora que tiene delante no es la que está emparejada con este teléfono.
    /// </summary>
    [RelayCommand]
    private void OlvidarYEmparejar()
    {
        _service.ClearDefault();
        _service.OpenBluetoothSettings();

        ShowError("print_disabled", "Emparejá la impresora",
            "Olvidamos la impresora anterior. Emparejá la nueva desde los ajustes de Bluetooth y volvé para imprimir.",
            new OverlayAction("Buscar impresoras", ReintentarBuscarCommand),
            new OverlayAction("Cerrar", CerrarOverlayCommand, OverlayActionStyle.Secondary));
    }
}
