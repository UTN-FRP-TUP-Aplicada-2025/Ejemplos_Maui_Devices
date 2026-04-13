namespace Ejemplo_Maui_Dialer.Pages;

public partial class MainPage : ContentPage
{

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

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    async private void btnAbrirMarcador_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Telefono))
        {
            await DisplayAlertAsync("Atención", "Ingresá un número de teléfono", "OK");
            return;
        }

        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(Telefono);
            }
            else
            {
                await DisplayAlertAsync("Error", "Este dispositivo no puede hacer llamadas", "OK");
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlertAsync("Error", "Este dispositivo no puede hacer llamadas.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo abrir el marcador: {ex.Message}", "OK");
        }
    }
}
