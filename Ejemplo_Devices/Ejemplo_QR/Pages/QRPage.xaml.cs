namespace Ejemplo_QR.Pages;

[QueryProperty(nameof(Contenido), "Contenido")]
public partial class QRPage : ContentPage
{
	public TaskCompletionSource<string>? Contenido { get; set; }

    public QRPage()
	{
		InitializeComponent();
	}
}