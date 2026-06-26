using Ejemplo_MotorDSL_Dialog.ViewModels;

namespace Ejemplo_MotorDSL_Dialog.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
