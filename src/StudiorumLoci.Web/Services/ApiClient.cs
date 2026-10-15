using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Forms;
using StudiorumLoci.Shared;

namespace StudiorumLoci.Web.Services;

/// <summary>Errore restituito dall'API, con un messaggio già leggibile dall'utente.</summary>
public class ApiException(string message) : Exception(message);

/// <summary>Client tipizzato per StudiorumLoci.Api.</summary>
public class ApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    // ---- Accesso ----
    public Task<LoginDto> Login(LoginInput l) => Send<LoginDto>(HttpMethod.Post, "api/auth/login", l);
    public Task<LoginDto> CambiaPassword(CambiaPasswordInput c) => Send<LoginDto>(HttpMethod.Post, "api/auth/cambia-password", c);

    // ---- Account (amministrazione) ----
    public Task<List<UtenteDto>> Utenti() => Get<List<UtenteDto>>("api/utenti");
    public Task<PasswordTemporaneaDto> CreaUtente(UtenteInput u) => Send<PasswordTemporaneaDto>(HttpMethod.Post, "api/utenti", u);
    public Task<UtenteDto> ModificaUtente(int id, UtenteInput u) => Send<UtenteDto>(HttpMethod.Put, $"api/utenti/{id}", u);
    public Task<PasswordTemporaneaDto> ReimpostaPassword(int id) => Send<PasswordTemporaneaDto>(HttpMethod.Post, $"api/utenti/{id}/reimposta-password");
    public Task EliminaUtente(int id) => Send(HttpMethod.Delete, $"api/utenti/{id}");

    // ---- Sedi ----
    public Task<List<SedeDto>> Sedi(string? regione = null, bool soloAttive = false) =>
        Get<List<SedeDto>>($"api/sedi?soloAttive={soloAttive}&regione={Uri.EscapeDataString(regione ?? "")}");
    public Task<SedeDto> CreaSede(SedeInput s) => Send<SedeDto>(HttpMethod.Post, "api/sedi", s);
    public Task<SedeDto> ModificaSede(int id, SedeInput s) => Send<SedeDto>(HttpMethod.Put, $"api/sedi/{id}", s);
    public Task EliminaSede(int id) => Send(HttpMethod.Delete, $"api/sedi/{id}");

    // ---- CRM: studenti ----
    /// <param name="sedeId">0 = solo studenti senza sede.</param>
    public Task<List<StudenteDto>> Studenti(string? cerca = null, bool soloAttivi = true, int? sedeId = null) =>
        Get<List<StudenteDto>>($"api/crm/studenti?soloAttivi={soloAttivi}&sedeId={sedeId}&cerca={Uri.EscapeDataString(cerca ?? "")}");
    public Task<StudenteDto> CreaStudente(StudenteInput s) => Send<StudenteDto>(HttpMethod.Post, "api/crm/studenti", s);
    public Task<StudenteDto> ModificaStudente(int id, StudenteInput s) => Send<StudenteDto>(HttpMethod.Put, $"api/crm/studenti/{id}", s);
    public Task DisattivaStudente(int id) => Send(HttpMethod.Post, $"api/crm/studenti/{id}/disattiva");
    public Task EliminaStudente(int id) => Send(HttpMethod.Delete, $"api/crm/studenti/{id}");

    // ---- CRM: genitori ----
    public Task<List<GenitoreDto>> Genitori(string? cerca = null) =>
        Get<List<GenitoreDto>>($"api/crm/genitori?cerca={Uri.EscapeDataString(cerca ?? "")}");
    public Task<GenitoreDto> CreaGenitore(GenitoreInput g) => Send<GenitoreDto>(HttpMethod.Post, "api/crm/genitori", g);
    public Task<GenitoreDto> ModificaGenitore(int id, GenitoreInput g) => Send<GenitoreDto>(HttpMethod.Put, $"api/crm/genitori/{id}", g);
    public Task EliminaGenitore(int id) => Send(HttpMethod.Delete, $"api/crm/genitori/{id}");
    public Task<List<InterazioneDto>> InterazioniGenitore(int id) => Get<List<InterazioneDto>>($"api/crm/genitori/{id}/interazioni");
    public Task<InterazioneDto> AggiungiInterazioneGenitore(int id, InterazioneInput i) =>
        Send<InterazioneDto>(HttpMethod.Post, $"api/crm/genitori/{id}/interazioni", i);

    // ---- CRM: lead ----
    public Task<List<LeadDto>> Lead(StatoLead? stato = null) => Get<List<LeadDto>>($"api/crm/lead?stato={stato}");
    public Task<LeadDto> CreaLead(LeadInput l) => Send<LeadDto>(HttpMethod.Post, "api/crm/lead", l);
    public Task<LeadDto> ModificaLead(int id, LeadInput l) => Send<LeadDto>(HttpMethod.Put, $"api/crm/lead/{id}", l);
    public Task EliminaLead(int id) => Send(HttpMethod.Delete, $"api/crm/lead/{id}");
    public Task<LeadDto> CambiaStatoLead(int id, StatoLead stato) =>
        Send<LeadDto>(HttpMethod.Patch, $"api/crm/lead/{id}/stato", new CambiaStatoLeadInput(stato));
    public Task<GenitoreDto> ConvertiLead(int id, GenitoreInput g) => Send<GenitoreDto>(HttpMethod.Post, $"api/crm/lead/{id}/converti", g);
    public Task<List<InterazioneDto>> InterazioniLead(int id) => Get<List<InterazioneDto>>($"api/crm/lead/{id}/interazioni");
    public Task<InterazioneDto> AggiungiInterazioneLead(int id, InterazioneInput i) =>
        Send<InterazioneDto>(HttpMethod.Post, $"api/crm/lead/{id}/interazioni", i);

    // ---- CRM: storico contatti ----
    public Task<InterazioneDto> ModificaInterazione(int id, InterazioneInput i) =>
        Send<InterazioneDto>(HttpMethod.Put, $"api/crm/interazioni/{id}", i);
    public Task EliminaInterazione(int id) => Send(HttpMethod.Delete, $"api/crm/interazioni/{id}");

    // ---- Dipendenti ----
    /// <param name="sedeId">0 = solo dipendenti senza sede.</param>
    public Task<List<DipendenteDto>> Dipendenti(RuoloDipendente? ruolo = null, bool soloAttivi = true, int? sedeId = null) =>
        Get<List<DipendenteDto>>($"api/dipendenti?soloAttivi={soloAttivi}&ruolo={ruolo}&sedeId={sedeId}");
    public Task<DipendenteDto> Dipendente(int id) => Get<DipendenteDto>($"api/dipendenti/{id}");
    public Task<DipendenteCreatoDto> CreaDipendente(NuovoDipendenteInput d) => Send<DipendenteCreatoDto>(HttpMethod.Post, "api/dipendenti", d);
    public Task<DipendenteDto> ModificaDipendente(int id, DipendenteInput d) => Send<DipendenteDto>(HttpMethod.Put, $"api/dipendenti/{id}", d);
    public Task CessaDipendente(int id) => Send(HttpMethod.Post, $"api/dipendenti/{id}/cessa");
    public Task EliminaDipendente(int id) => Send(HttpMethod.Delete, $"api/dipendenti/{id}");
    public Task<List<TurnoDto>> Turni(int id, DateOnly da, DateOnly a) => Get<List<TurnoDto>>($"api/dipendenti/{id}/turni?{Periodo(da, a)}");
    public Task<TurnoDto> AggiungiTurno(int id, TurnoInput t) => Send<TurnoDto>(HttpMethod.Post, $"api/dipendenti/{id}/turni", t);
    public Task<TurnoDto> ModificaTurno(int id, int turnoId, TurnoInput t) =>
        Send<TurnoDto>(HttpMethod.Put, $"api/dipendenti/{id}/turni/{turnoId}", t);
    public Task EliminaTurno(int id, int turnoId) => Send(HttpMethod.Delete, $"api/dipendenti/{id}/turni/{turnoId}");
    public Task<CompensoDto> Compenso(int id, DateOnly da, DateOnly a) => Get<CompensoDto>($"api/dipendenti/{id}/compenso?{Periodo(da, a)}");

    // ---- Pagamenti ----
    /// <param name="sedeId">Sede dello studente; 0 = studenti senza sede.</param>
    public Task<List<PagamentoDto>> Pagamenti(int? studenteId = null, StatoPagamento? stato = null, bool soloScaduti = false, int? sedeId = null) =>
        Get<List<PagamentoDto>>($"api/pagamenti?studenteId={studenteId}&stato={stato}&soloScaduti={soloScaduti}&sedeId={sedeId}");
    public Task<PagamentoDto> CreaPagamento(PagamentoInput p) => Send<PagamentoDto>(HttpMethod.Post, "api/pagamenti", p);
    public Task<PagamentoDto> ModificaPagamento(int id, PagamentoInput p) => Send<PagamentoDto>(HttpMethod.Put, $"api/pagamenti/{id}", p);
    public Task<PagamentoDto> RegistraIncasso(int id, RegistraIncassoInput i) => Send<PagamentoDto>(HttpMethod.Post, $"api/pagamenti/{id}/incasso", i);
    public Task<PagamentoDto> AnnullaPagamento(int id) => Send<PagamentoDto>(HttpMethod.Post, $"api/pagamenti/{id}/annulla");
    public Task<PagamentoDto> RipristinaPagamento(int id) => Send<PagamentoDto>(HttpMethod.Post, $"api/pagamenti/{id}/ripristina");
    public Task EliminaPagamento(int id) => Send(HttpMethod.Delete, $"api/pagamenti/{id}");

    public async Task<PagamentoDto> CaricaRicevuta(int id, Stream contenuto, string nomeFile, string tipo)
    {
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(contenuto);
        file.Headers.ContentType = new(string.IsNullOrEmpty(tipo) ? "application/octet-stream" : tipo);
        form.Add(file, "file", nomeFile);
        return await Send<PagamentoDto>(HttpMethod.Post, $"api/pagamenti/{id}/ricevuta", form);
    }
    public async Task<PagamentoDto> CaricaRicevuta(int id, IBrowserFile file)
    {
        if (file.Size > Ricevute.DimensioneMassima) throw new ApiException("Il file supera i 10 MB.");
        await using var contenuto = file.OpenReadStream(Ricevute.DimensioneMassima);
        return await CaricaRicevuta(id, contenuto, file.Name, file.ContentType);
    }
    public Task<PagamentoDto> EliminaRicevuta(int id) => Send<PagamentoDto>(HttpMethod.Delete, $"api/pagamenti/{id}/ricevuta");
    /// <summary>Contenuto della ricevuta (la richiesta porta il token, quindi non si può usare un semplice link).</summary>
    public async Task<byte[]> ScaricaRicevuta(int id)
    {
        using var risposta = await Invia(HttpMethod.Get, $"api/pagamenti/{id}/ricevuta", null);
        return await risposta.Content.ReadAsByteArrayAsync();
    }
    public Task<RiepilogoPagamentiDto> Riepilogo(int anno, int mese, int? sedeId = null) =>
        Get<RiepilogoPagamentiDto>($"api/pagamenti/riepilogo?anno={anno}&mese={mese}&sedeId={sedeId}");

    // ---- Infrastruttura ----

    private static string Periodo(DateOnly da, DateOnly a) => $"da={da:yyyy-MM-dd}&a={a:yyyy-MM-dd}";

    private Task<T> Get<T>(string url) => Send<T>(HttpMethod.Get, url);

    private async Task<T> Send<T>(HttpMethod method, string url, object? body = null)
    {
        using var response = await Invia(method, url, body);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private async Task Send(HttpMethod method, string url, object? body = null)
    {
        using var response = await Invia(method, url, body);
    }

    private async Task<HttpResponseMessage> Invia(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);
        request.Content = body switch
        {
            null => null,
            HttpContent contenuto => contenuto,
            _ => JsonContent.Create(body, options: Json),
        };

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException($"Impossibile contattare il server ({http.BaseAddress}). " +
                "Verifica che l'API sia avviata e che questo indirizzo sia tra le origini CORS consentite.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var messaggio = await LeggiErrore(response);
            response.Dispose();
            throw new ApiException(messaggio);
        }
        return response;
    }

    /// <summary>Estrae un messaggio da ProblemDetails / ValidationProblemDetails, da una stringa JSON o da testo semplice.</summary>
    private static async Task<string> LeggiErrore(HttpResponseMessage response)
    {
        var predefinito = $"Errore {(int)response.StatusCode} ({response.ReasonPhrase}).";
        var contenuto = await response.Content.ReadAsStringAsync();
        if (response.Content.Headers.ContentType?.MediaType == "text/plain")
            return string.IsNullOrWhiteSpace(contenuto) ? predefinito : contenuto;
        try
        {
            using var doc = JsonDocument.Parse(contenuto);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.String) return root.GetString()!;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var dettagli = string.Join(" ", errors.EnumerateObject().SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString())));
                if (dettagli.Length > 0) return dettagli;
            }
            if (root.TryGetProperty("detail", out var detail) && detail.GetString() is { Length: > 0 } d) return d;
            if (root.TryGetProperty("title", out var title) && title.GetString() is { Length: > 0 } t) return t;
        }
        catch (JsonException) { }
        return predefinito;
    }
}
