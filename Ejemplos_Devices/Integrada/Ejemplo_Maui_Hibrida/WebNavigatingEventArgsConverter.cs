
using System.Globalization;

namespace Ejemplo_Maui_Hibrida.Converters;

public class WebNavigatingEventArgsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        => value as WebNavigatingEventArgs;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        => throw new NotSupportedException();
}

public class WebNavigatedEventArgsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        => value as WebNavigatedEventArgs;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        => throw new NotSupportedException();
}