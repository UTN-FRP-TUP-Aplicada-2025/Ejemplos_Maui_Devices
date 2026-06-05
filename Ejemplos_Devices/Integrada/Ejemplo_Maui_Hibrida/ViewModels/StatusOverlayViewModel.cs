using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Ejemplo_Maui_Hibrida.ViewModels;

/// <summary>
/// Estado visual del overlay.
/// </summary>
public enum OverlayMode { None, Busy, Error }

/// <summary>
/// Estilo visual de un botón de acción del overlay.
/// </summary>
public enum OverlayActionStyle { Primary, Secondary }

/// <summary>
/// Acción (botón) que se muestra en la capa de error del overlay.
/// </summary>
public record OverlayAction(string Text, ICommand Command, OverlayActionStyle Style = OverlayActionStyle.Primary);

/// <summary>
/// Base común reutilizable para los overlays de estado (GPS, Red, etc.).
/// Modela una máquina de tres estados (None / Busy / Error) y expone una
/// capa de espera y una capa de error con botonera dinámica.
/// </summary>
public abstract partial class StatusOverlayViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    [NotifyPropertyChangedFor(nameof(IsError))]
    private OverlayMode mode = OverlayMode.None;

    // --- Capa de error ---
    [ObservableProperty] private string iconGlyph = "";
    [ObservableProperty] private string title = "";
    [ObservableProperty] private string message = "";

    // --- Capa de espera ---
    [ObservableProperty] private string busyTitle = "";
    [ObservableProperty] private string busySubtitle = "";
    [ObservableProperty] private string busyImage = "";

    /// <summary>Botones que se muestran en la capa de error.</summary>
    public ObservableCollection<OverlayAction> Actions { get; } = new();

    public bool IsVisible => Mode != OverlayMode.None;
    public bool IsBusy => Mode == OverlayMode.Busy;
    public bool IsError => Mode == OverlayMode.Error;

    /// <summary>Muestra la capa de espera animada.</summary>
    protected void ShowBusy(string title, string subtitle = "", string image = "")
    {
        Actions.Clear();
        BusyTitle = title;
        BusySubtitle = subtitle;
        BusyImage = image;
        Mode = OverlayMode.Busy;
    }

    /// <summary>Muestra la capa de error con un ícono, textos y la botonera indicada.</summary>
    protected void ShowError(string icon, string title, string message, params OverlayAction[] actions)
    {
        Actions.Clear();
        foreach (var action in actions)
            Actions.Add(action);

        IconGlyph = icon;
        Title = title;
        Message = message;
        Mode = OverlayMode.Error;
    }

    /// <summary>Oculta el overlay por completo.</summary>
    public void Hide() => Mode = OverlayMode.None;
}
