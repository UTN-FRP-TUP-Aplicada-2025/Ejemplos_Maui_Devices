namespace Ejemplo_Photo_MediaPicker.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    async private void OnAbrirCamaraClicked(object? sender, EventArgs e)
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                using Stream sourceStream = await photo.OpenReadAsync();
                var image = new Image { Source = ImageSource.FromStream(() => sourceStream) };
                ImgPhoto.Source = image.Source;
            }
        }
    }
}
