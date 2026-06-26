using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

using Ejemplo_MotorDSL.Templates;

using MotorDsl.Core.Contracts;
using MotorDsl.Core.Models;

namespace Ejemplo_MotorDSL_Dialog.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDocumentEngine _engine;

    [ObservableProperty]
    private PrinterOverlayViewModel printerOverlayViewModel;

    public MainViewModel(IDocumentEngine engine, PrinterOverlayViewModel printerOverlay)
    {
        _engine = engine;
        printerOverlayViewModel = printerOverlay;
    }

    // Botón manual: renderiza el acta (diagnóstico independiente) y delega el
    // flujo de impresión Bluetooth en el overlay.
    [RelayCommand]
    private async Task ImprimirEjemplo()
    {
        // 1. Render SIEMPRE primero, antes de tocar la impresora.
        var profile = new DeviceProfile("58HB6", 32, "escpos-bitmap");
        profile.SetCapability("supports_bitmap", true);
        profile.SetCapability("bitmap_max_width_px", 320);
        profile.SetCapability("bitmap_binarization_threshold", 128);

        var render = _engine.Render(MultaIntegratedDsl.Document, profile);

        // 2. El overlay maneja permisos, descubrimiento, selección, conexión e impresión.
        await PrinterOverlayViewModel.ImprimirAsync(render);
    }
}
