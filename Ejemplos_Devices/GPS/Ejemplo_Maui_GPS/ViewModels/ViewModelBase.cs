using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ejemplo_Maui_GPS.ViewModels;

// Base reutilizable para ViewModels: implementa INotifyPropertyChanged
// y expone SetProperty<T> para reducir boilerplate sin librerías externas.
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
