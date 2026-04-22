using System.Windows.Input;

namespace Ejemplo_Maui_GPS.ViewModels;

// Estado + comandos del overlay reutilizable (espera + permisos denegados).
// Recibe callbacks del VM padre para mantener el ContentView autónomo
// (sus bindings no necesitan cruzar al VM padre).
public class GpsOverlayViewModel : ViewModelBase
{
    public GpsOverlayViewModel(Func<Task> onRetry, Action onOpenSettings)
    {
        PedirPermisoCommand = new AsyncRelayCommand(onRetry);
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
        private set
        {
            if (SetProperty(ref _canRetry, value))
                OnPropertyChanged(nameof(MustOpenSettings));
        }
    }

    public bool MustOpenSettings => !_canRetry;

    // Overlay visible si está esperando GPS o mostrando panel de permisos denegados.
    public bool IsVisible => _isBusy || _isDenied;

    // ── Comandos enlazados desde el ContentView ───────────

    public ICommand PedirPermisoCommand { get; }
    public ICommand AbrirAjustesCommand { get; }
    public ICommand CerrarOverlayCommand { get; }

    // ── API que usa el VM ─────────────────────────────────

    public void ShowBusy()
    {
        IsDenied = false;
        IsBusy = true;
    }

    public void ShowPermissionDenied(bool canRetry)
    {
        Title = canRetry
            ? "Permiso de ubicación necesario"
            : "Acceso a la ubicación denegado";
        Message = canRetry
            ? "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Podés intentar conceder el permiso."
            : "Para obtener coordenadas GPS necesitamos acceso a la ubicación. Habilitalo desde los ajustes de la aplicación.";
        CanRetry = canRetry;
        IsBusy = false;
        IsDenied = true;
    }

    public void ShowRestricted()
    {
        Title = "Acceso restringido";
        Message = "El acceso a la ubicación está restringido por una política del dispositivo. Consultá con el administrador.";
        CanRetry = false;
        IsBusy = false;
        IsDenied = true;
    }

    public void Hide()
    {
        IsBusy = false;
        IsDenied = false;
    }
}
