using Ejemplo_Photo_MiMediaPicker_Callback.Pages;

namespace Ejemplo_Photo_MiMediaPicker_Callback;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute( nameof( MyMediaPickerPage ), typeof( MyMediaPickerPage ) );
    }
}
