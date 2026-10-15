using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    // ---- CRM: studenti ----
    public Task<List<StudenteDto>> Studenti(string? cerca = null, bool soloAttivi = true) =>
        Get<List<StudenteDto>>($"api/crm/studenti?soloAttivi={soloAttivi}&cerca={Uri.EscapeDataString(cerca ?? "")}");
    public Task<StudenteDto> CreaStudente(StudenteInput s) => Send<StudenteDto>(HttpMethod.Post, "api/crm/studenti", s);
    public Task<StudenteDto> ModificaStudente(int id, StudenteInput s) => Send<StudenteDto>(HttpMethod.Put, $"api/crm/studenti/{id}", s);
    public Task DisattivaStudente(int id) => Send(HttpMethod.Delete, $"api/crm/studenti/{id}");

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
    public Task<LeadDto> CambiaStatoLead(int id, StatoLead stato) =>
        Send<LeadDto>(HttpMethod.Patch, $"api/crm/lead/{id}/stato", new CambiaStatoLeadInput(stato));
    public Task<GenitoreDto> ConvertiLead(int id, GenitoreInput g) => Send<GenitoreDto>(HttpMethod.Post, $"api/crm/lead/{id}/converti", g);
    public Task<List<InterazioneDto>> InterazioniLead(int id) => Get<List<InterazioneDto>>($"api/crm/lead/{id}/interazioni");
    public Task<InterazioneDto> AggiungiInterazioneLead(int id, InterazioneInput i) =>
        Send<InterazioneDto>(HttpMethod.Post, $"api/crm/lead/{id}/interazioni", i);

    // ---- Dipendenti ----
    public Task<List<DipendenteDto>> Dipendenti(RuoloDipendente? ruolo = null, bool soloAttivi = true) =>
        Get<List<DipendenteDto>>($"api/dipendenti?soloAttivi={soloAttivi}&ruolo={ruolo}");
    public Task<DipendenteDto> Dipendente(int id) => Get<DipendenteDto>($"api/dipendenti/{id}");
    public Task<DipendenteDto> CreaDipendente(DipendenteInput d) => Send<DipendenteDto>(HttpMethod.Post, "api/dipendenti", d);
    public Task<DipendenteDto> ModificaDipendente(int id, DipendenteInput d) => Send<DipendenteDto>(HttpMethod.Put, $"api/dipendenti/{id}", d);
    public Task DisattivaDipendente(int id) => Send(HttpMethod.Delete, $"api/dipendenti/{id}");
    public Task<List<TurnoDto>> Turni(int id, DateOnly da, DateOnly a) => Get<List<TurnoDto>>($"api/dipendenti/{id}/turni?{Periodo(da, a)}");
    public Task<TurnoDto> AggiungiTurno(int id, TurnoInput t) => Send<TurnoDto>(HttpMethod.Post, $"api/dipendenti/{id}/turni", t);
    public Task EliminaTurno(int id, int turnoId) => Send(HttpMethod.Delete, $"api/dipendenti/{id}/turni/{turnoId}");
    public Task<CompensoDto> Compenso(int id, DateOnly da, DateOnly a) => Get<CompensoDto>($"api/dipendenti/{id}/compenso?{Periodo(da, a)}");

    // ---- Pagamenti ----
    public Task<List<PagamentoDto>> Pagamenti(int? studenteId = null, StatoPagamento? stato = null, bool soloScaduti = false) =>
        Get<List<PagamentoDto>>($"api/pagamenti?studenteId={studenteId}&stato={stato}&soloScaduti={soloScaduti}");
    public Task<PagamentoDto> CreaPagamento(PagamentoInput p) => Send<PagamentoDto>(HttpMethod.Post, "api/pagamenti", p);
    public Task<PagamentoDto> ModificaPagamento(int id, PagamentoInput p) => Send<PagamentoDto>(HttpMethod.Put, $"api/pagamenti/{id}", p);
    public Task<PagamentoDto> RegistraIncasso(int id, RegistraIncassoInput i) => Send<PagamentoDto>(HttpMethod.Post, $"api/pagamenti/{id}/incasso", i);
    public Task<PagamentoDto> AnnullaPagamento(int id) => Send<PagamentoDto>(HttpMethod.Post, $"api/pagamenti/{id}/annulla");
    public Task<RiepilogoPagamentiDto> Riepilogo(int anno, int mese) => Get<RiepilogoPagamentiDto>($"api/pagamenti/riepilogo?anno={anno}&mese={mese}");

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
        if (body is not null) request.Content = JsonContent.Create(body, options: Json);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Impossibile contattare il server. Verifica che l'API sia avviata.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var messaggio = await LeggiErrore(response);
            response.Dispose();
            throw new ApiException(messaggio);
        }
        return response;
    }

    /// <summary>Estrae un messaggio da ProblemDetails / ValidationProblemDetails o da una stringa JSON.</summary>
    private static async Task<string> LeggiErrore(HttpResponseMessage response)
    {
        var predefinito = $"Errore {(int)response.StatusCode} ({response.ReasonPhrase}).";
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.String) return root.GetString()!;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var dettagli = errors.EnumerateObject().SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString()));
                return string.Join(" ", dettagli);
            }
            if (root.TryGetProperty("detail", out var detail) && detail.GetString() is { Length: > 0 } d) return d;
            if (root.TryGetProperty("title", out var title) && title.GetString() is { Length: > 0 } t) return t;
        }
        catch (JsonException) { }
        return predefinito;
    }
}
