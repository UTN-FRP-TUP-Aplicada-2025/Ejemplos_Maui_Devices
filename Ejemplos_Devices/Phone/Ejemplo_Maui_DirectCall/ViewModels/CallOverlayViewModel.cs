using System.Windows.Input;

namespace Ejemplo_Maui_DirectCall.ViewModels;

// Estado + comandos del overlay reutilizable de llamada (espera + permisos
// denegados + dispositivo sin soporte). Espejo del GpsOverlayViewModel.
//
// Recibe callbacks del coordinador (no del VM padre) para mantener al
// ContentView autónomo: sus bindings no necesitan cruzar al VM padre.
public class CallOverlayViewModel : ViewModelBase
{
    public CallOverlayViewModel(Func<Task> onRetry, Action onOpenSettings)
    {
        // PedirPermiso y Reintentar comparten el mismo callback (volver a invocar
        // CallAsync con el último número), pero se exponen como botones distintos
        // para que el texto coincida con la situación que el usuario está viendo.
        PedirPermisoCommand = new AsyncRelayCommand(onRetry);
        ReintentarCommand = new AsyncRelayCommand(onRetry);
        AbrirAjustesCommand = new RelayCommand(onOpenSettings);
        CerrarOverlayCommand = new RelayCommand(Hide);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
                OnPropertyChanged(nameof(IsVisible));
        }
    }

    private bool _isDenied;
    public bool IsDenied
    {
        get => _isDenied;
        private set
        {
            if (SetProperty(ref _isDenied, value))
                OnPropertyChanged(nameof(IsVisible));
        }
    }

    private string _title = "";
    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    private string _message = "";
    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    private bool _canRetry;
    public bool CanRetry
    {
        get => _canRetry;
        private set => SetProperty(ref _canRetry, value);
    }

    // Si el estado denegado tiene relación con permisos del SO, mostramos
    // el botón "Abrir configuración" como atajo a ajustes — incluso cuando
    // el usuario también puede reintentar. En NotSupported / Failure es false
    // porque ir a ajustes no resuelve el problema.
    private bool _canOpenSettings;
    public bool CanOpenSettings
    {
        get => _canOpenSettings;
        private set => SetProperty(ref _canOpenSettings, value);
    }

    // "Reintentar": visible cuando "Pedir permiso" no aplica pero un retry
    // de la operación todavía tiene sentido (post-ajustes, error transitorio).
    // Falso en NotSupported (sin hardware) y en PermissionDenied(canRetry)
    // donde el botón "Pedir permiso" ya hace la misma acción.
    private bool _canRetryAction;
    public bool CanRetryAction
    {
        get => _canRetryAction;
        private set => SetProperty(ref _canRetryAction, value);
    }

    public bool IsVisible => _isBusy || _isDenied;

    public ICommand PedirPermisoCommand { get; }
    public ICommand ReintentarCommand { get; }
    public ICommand AbrirAjustesCommand { get; }
    public ICommand CerrarOverlayCommand { get; }

    public void ShowBusy(string mensaje = "Iniciando llamada...")
    {
        Title = mensaje;
        Message = "";
        IsDenied = false;
        IsBusy = true;
    }

    public void ShowPermissionDenied(bool canRetry)
    {
        Title = canRetry
            ? "Permiso de llamadas necesario"
            : "Acceso a llamadas denegado";
        Message = canRetry
            ? "Para llamar directamente necesitamos permiso de teléfono. Podés intentar concederlo o habilitarlo desde los ajustes."
            : "Para llamar directamente necesitamos permiso de teléfono. Habilitalo desde los ajustes y volvé a intentar.";
        CanRetry = canRetry;
        // "Reintentar" tiene sentido cuando "Pedir permiso" NO aplica
        // (post "no volver a preguntar"): el usuario va a ajustes, vuelve y reintenta.
        CanRetryAction = !canRetry;
        CanOpenSettings = true;
        IsBusy = false;
        IsDenied = true;
    }

    public void ShowRestricted()
    {
        Title = "Acceso restringido";
        Message = "El acceso al teléfono está restringido por una política del dispositivo. Consultá con el administrador o revisá los ajustes.";
        CanRetry = false;
        CanRetryAction = true;
        CanOpenSettings = true;
        IsBusy = false;
        IsDenied = true;
    }

    public void ShowNotSupported()
    {
        Title = "Función no disponible";
        Message = "Este dispositivo no puede realizar llamadas telefónicas.";
        CanRetry = false;
        CanRetryAction = false;
        CanOpenSettings = false;
        IsBusy = false;
        IsDenied = true;
    }

    public void ShowFailure(string mensaje)
    {
        Title = "Error al llamar";
        Message = mensaje;
        CanRetry = false;
        CanRetryAction = true;
        CanOpenSettings = false;
        IsBusy = false;
        IsDenied = true;
    }

    public void Hide()
    {
        IsBusy = false;
        IsDenied = false;
    }
}
