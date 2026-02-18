using Ejemplo_Photo_MiMediaPicker_Task.Pages;

namespace Ejemplo_Photo_MiMediaPicker_Task;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(MyMediaPickerPage), typeof(MyMediaPickerPage));
    }
}
