



using Ejemplo_Maui_DirectCall.ViewModels;

namespace Ejemplo_Maui_DirectCall.Pages;


public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;

    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

   

}
