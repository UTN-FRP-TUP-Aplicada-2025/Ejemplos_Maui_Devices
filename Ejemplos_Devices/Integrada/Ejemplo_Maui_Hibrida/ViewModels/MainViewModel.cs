using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Ejemplo_Maui_Hibrida.UrlCommands;

namespace Ejemplo_Maui_Hibrida.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string url = string.Empty;

    [ObservableProperty]
    GpsOverlayViewModel gpsOverlayViewModel;

    [ObservableProperty]
    NetworkOverlayViewModel networkOverlayViewModel;

    [ObservableProperty]
    CallOverlayViewModel callOverlayViewModel;

    private readonly UrlCommandDispatcher _dispatcher;

    public MainViewModel(NetworkOverlayViewModel network, GpsOverlayViewModel gps, CallOverlayViewModel call, UrlCommandDispatcher dispatcher)
    {
        gpsOverlayViewModel = gps;
        networkOverlayViewModel = network;
        callOverlayViewModel = call;
        _dispatcher = dispatcher;
    }

    // Botón manual: reusa el CallCommandHandler a través del dispatcher.
    [RelayCommand]
    private async Task TakePhone()
    {
        await _dispatcher.DispatchAsync("photo=2");
    }

    // Botón manual: reusa el GpsCommandHandler a través del dispatcher. Garantiza el
    // marcador geo=1 sobre la URL actual y aplica la reescritura con coordenadas que
    // hace el handler (sin duplicar esa lógica acá).
    [RelayCommand]
    async private Task TakeGPS()
    {
        var url = Url.Contains("geo=1", StringComparison.OrdinalIgnoreCase)
            ? Url
            : $"{Url}{(Url.Contains('?') ? "&" : "?")}geo=1";

        var outcome = await _dispatcher.DispatchAsync(url);

        if (outcome.NavigateTo is not null)
            Url = outcome.NavigateTo;
    }

    [RelayCommand]
    private async Task Navigating(WebNavigatingEventArgs e)
    {
        var outcome = await _dispatcher.DispatchAsync(e.Url);

        e.Cancel = outcome.CancelNavigation;

        if (outcome.NavigateTo is not null)
            Url = outcome.NavigateTo;

        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task Navigated(WebNavigatedEventArgs e)
    {
        IsRefreshing = false;

        if (e.Result == WebNavigationResult.Success)
            NetworkOverlayViewModel.NotifyNavigationSucceeded(e.Url);
        else
            await NetworkOverlayViewModel.NotifyNavigationFailedAsync(e.Url, e.Result);
    }
}