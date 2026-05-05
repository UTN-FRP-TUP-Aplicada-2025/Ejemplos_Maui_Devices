using Ejemplo_Maui_DirectCall.Services;
using Ejemplo_Maui_GPS.ViewModels;
using System.Windows.Input;



namespace Ejemplo_Maui_DirectCall.ViewModels;

public class MainPageViewModel: ViewModelBase
{

    PhoneDialerDevice _phoneDialerDevice;

    public ICommand RealizarLLamadaCommand { get; }

    public MainPageViewModel(PhoneDialerDevice phoneDialerDevice)
    {
        RealizarLLamadaCommand = new AsyncRelayCommand(RealizarLLamadaAsync);

        _phoneDialerDevice = phoneDialerDevice;
    }

    string telefono;
    public string Telefono
    {
        get => telefono;
        set
        {
            if (telefono != value) //importante! evita que entre en un bucle
            {
                telefono = value;
                OnPropertyChanged(); //para que funcione la bidireccionalidad
            }
        }
    }

    async private Task RealizarLLamadaAsync()
    {
        if (string.IsNullOrWhiteSpace(Telefono))
        {
            await Shell.Current.DisplayAlertAsync("Atención", "Ingresá un número de teléfono", "OK");
            return;
        }

        try
        {
            await _phoneDialerDevice.CallPhoneAsync(telefono);
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlertAsync("Error", "Este dispositivo no puede hacer llamadas.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo abrir el marcador: {ex.Message}", "OK");
        }
    }

}
