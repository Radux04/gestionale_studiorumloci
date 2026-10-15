using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using StudiorumLoci.Shared;

namespace StudiorumLoci.Web.Services;

/// <summary>
/// Sessione dell'utente collegato: token e dati dell'utente, salvati nel localStorage del browser
/// così restano dopo un ricaricamento della pagina (fino alla scadenza del token).
/// Fa anche da AuthenticationStateProvider per [Authorize] e &lt;AuthorizeView&gt;.
/// </summary>
public class Sessione(IJSRuntime js) : AuthenticationStateProvider
{
    private const string Chiave = "studiorumloci.sessione";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private LoginDto? corrente;
    private bool caricata;

    public string? Token => Valida ? corrente!.Token : null;
    public UtenteCorrenteDto? Utente => Valida ? corrente!.Utente : null;
    public bool Valida => corrente is not null && corrente.Scadenza > DateTime.UtcNow.AddMinutes(1);

    public bool PuoGestire => Utente?.Ruolo is RuoloAccesso.SuperAdmin or RuoloAccesso.Admin or RuoloAccesso.Manager;
    public bool PuoAmministrare => Utente?.Ruolo is RuoloAccesso.SuperAdmin or RuoloAccesso.Admin;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!caricata)
        {
            caricata = true;
            try
            {
                var salvata = await js.InvokeAsync<string?>("localStorage.getItem", Chiave);
                if (salvata is not null) corrente = JsonSerializer.Deserialize<LoginDto>(salvata, Json);
            }
            catch (JsonException) { corrente = null; }
            catch (JSException) { corrente = null; }
        }
        return new AuthenticationState(Principal());
    }

    public async Task Accedi(LoginDto login)
    {
        corrente = login;
        caricata = true;
        await js.InvokeVoidAsync("localStorage.setItem", Chiave, JsonSerializer.Serialize(login, Json));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Principal())));
    }

    public async Task Esci()
    {
        corrente = null;
        caricata = true;
        await js.InvokeVoidAsync("localStorage.removeItem", Chiave);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Principal())));
    }

    private ClaimsPrincipal Principal()
    {
        if (Utente is not { } u) return new ClaimsPrincipal(new ClaimsIdentity());

        var identita = new ClaimsIdentity(
        [
            new Claim("sub", u.Id.ToString()),
            new Claim("name", u.Nome),
            new Claim("email", u.Email),
            new Claim("role", u.Ruolo.ToString()),
        ], authenticationType: "token", nameType: "name", roleType: "role");
        return new ClaimsPrincipal(identita);
    }
}

/// <summary>Aggiunge il token alle chiamate all'API; se l'API risponde 401 la sessione è scaduta.</summary>
public class AggiungiToken(Sessione sessione, Microsoft.AspNetCore.Components.NavigationManager navigazione) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = sessione.Token;
        if (token is not null) request.Headers.Authorization = new("Bearer", token);

        var risposta = await base.SendAsync(request, ct);
        if (risposta.StatusCode == System.Net.HttpStatusCode.Unauthorized && token is not null)
        {
            await sessione.Esci();
            navigazione.NavigateTo("login?scaduta=1");
        }
        return risposta;
    }
}
