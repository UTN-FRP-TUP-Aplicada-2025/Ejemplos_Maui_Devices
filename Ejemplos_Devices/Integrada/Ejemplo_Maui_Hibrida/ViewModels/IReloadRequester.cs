namespace Ejemplo_Maui_Hibrida.ViewModels;

// Contrato que escucha la behavior. Lo implementa cualquier VM que pida recarga.
public interface IReloadRequester
{
    event EventHandler? ReloadRequested;
}
