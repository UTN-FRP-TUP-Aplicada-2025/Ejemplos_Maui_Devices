using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.KotlinX.Coroutines;

namespace Ejemplo_MotorDSL_Dialog.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    // Botón manual: abre el lector de QR usando el protocolo real de la web.
    [RelayCommand]
    private async Task ImprimirEjemplo()
    {
        
    }
}
