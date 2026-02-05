using System.Diagnostics;

namespace Ejemplo_Imagen_Normalizacion.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private bool _isNavigating = false;

    async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
    {
        var destinoPage = new MyMediaPickerPage();

        await Navigation.PushAsync(destinoPage);

        var imagen = await destinoPage.ResultadoTask.Task;

        if (imagen != null)
        {
            ImgPhoto.Source = imagen.Source;
        }
        else
        {
            await DisplayAlertAsync("Cancelado", "No se recibió ningún dato", "OK");
        }

        //if (_isNavigating) return;
        //_isNavigating = true;

        //try
        //{
        //    // En MAUI Shell, lo ideal es pasar el TaskCompletionSource o usar mensajería
        //    var destinoPage = new MyMediaPickerPage();

        //    // Si usas Shell con rutas registradas:
        //    // await Shell.Current.GoToAsync(nameof(MyMediaPickerPage));

        //    // Pero si sigues pasando la instancia manual:
        //    await Navigation.PushAsync(destinoPage);

        //    var imagen = await destinoPage.ResultadoTask.Task;
        //    if (imagen != null)
        //    {
        //        ImgPhoto.Source = imagen.Source;
        //    }
        //    else
        //    {
        //        await DisplayAlertAsync("Cancelado", "No se recibió ningún dato", "OK");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Debug.WriteLine($"Error al navegar: {ex.Message}");
        //    await DisplayAlertAsync("Error", "Ocurrió un error al abrir la cámara.", "OK");
        //}
        //finally
        //{
        //    _isNavigating = false; // IMPORTANTE: Liberar siempre en el finally
        //}
    }
}
