using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ejemplo_Maui_GPS.ViewModels;

class MainPageViewModel : INotifyPropertyChanged, IDisposable
{

    string coordenadas = "";
    public string Coordenadas
    {
        get => coordenadas;
        set
        {
            if (coordenadas != value)
            {
                coordenadas = value;
                OnPropertyChanged();
            }
        }
    }

    #region esperandoGPS
    private bool _esperandoGPS = false;
    public bool EsperandoGPS
    {
        get => _esperandoGPS;
        set
        {
            if (SetProperty(ref _esperandoGPS, value))
                OnPropertyChanged(nameof(MostrandoContenido));
        }
    }

    // MostrandoContenido = !EsperandoGPS
    // Evita depender de InverseBoolConverter en el XAML.
    public bool MostrandoContenido => !_esperandoGPS;

    private bool _deniedGPS = false;
    public bool DeniedGPS
    {
        get => _deniedGPS;
        set
        {
            if (SetProperty(ref _deniedGPS, value))
                OnPropertyChanged(nameof(MostrandoPanelEspera));
        }
    }

    // MostrandoPanelEspera = !DeniedGPS
    // Evita depender de InverseBoolConverter en el XAML.
    public bool MostrandoPanelEspera => !_deniedGPS;
    #endregion

    #region changed event
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion

    public void Dispose()
    {
        PropertyChanged = null;
        GC.SuppressFinalize(this);
    }
}