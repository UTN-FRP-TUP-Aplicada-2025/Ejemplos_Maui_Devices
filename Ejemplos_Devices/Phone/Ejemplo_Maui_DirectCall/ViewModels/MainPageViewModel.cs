using Ejemplo_Maui_DirectCall.Services;
using System.Windows.Input;

namespace Ejemplo_Maui_DirectCall.ViewModels;

public class MainPageViewModel : ViewModelBase
{
    private readonly CallCoordinator _calls;

    // El overlay vive en el coordinador (singleton).
    // La página lo enlaza con BindingContext="{Binding CallOverlay}".
    public CallOverlayViewModel CallOverlay => _calls.Overlay;

    public ICommand RealizarLLamadaCommand { get; }

    public MainPageViewModel(CallCoordinator calls)
    {
        _calls = calls;
        RealizarLLamadaCommand = new AsyncRelayCommand(RealizarLLamadaAsync);
    }

    private string _telefono = "";
    public string Telefono
    {
        get => _telefono;
        set
        {
            if (_telefono != value)
            {
                _telefono = value;
                OnPropertyChanged();
            }
        }
    }

    private async Task RealizarLLamadaAsync()
    {
        // Toda la lógica (validación, permisos, overlay, errores) la maneja
        // el coordinador. El VM solo dispara la operación; el resultado se
        // refleja en el overlay.
        await _calls.CallAsync(Telefono);
    }
}
