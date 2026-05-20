using Ejemplo_Photo_MiMediaPicker_Callback_Normalizacion.Pages;

namespace Ejemplo_Photo_MiMediaSelfie_Callback_Normalizacion
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MyMediaSelfiePickerPage), typeof(MyMediaSelfiePickerPage));
        }
    }
}
