using Microsoft.AspNetCore.Components;
using StudiorumLoci.Web.Services;

namespace StudiorumLoci.Web.Components;

/// <summary>Base per le pagine: client API, stato di caricamento e messaggi di errore/conferma.</summary>
public abstract class PaginaBase : ComponentBase
{
    [Inject] protected ApiClient Api { get; set; } = null!;

    protected bool Caricamento { get; private set; }
    protected string? Errore { get; set; }
    protected string? Conferma { get; set; }

    /// <summary>Esegue una chiamata all'API mostrando l'errore all'utente. Restituisce true se è andata a buon fine.</summary>
    protected async Task<bool> Esegui(Func<Task> azione, string? messaggioOk = null)
    {
        Caricamento = true;
        Errore = null;
        Conferma = null;
        try
        {
            await azione();
            Conferma = messaggioOk;
            return true;
        }
        catch (ApiException ex)
        {
            Errore = ex.Message;
            return false;
        }
        finally
        {
            Caricamento = false;
            StateHasChanged();
        }
    }
}
