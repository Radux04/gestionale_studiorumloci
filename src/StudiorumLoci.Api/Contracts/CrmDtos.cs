using System.ComponentModel.DataAnnotations;
using StudiorumLoci.Api.Domain.Crm;

namespace StudiorumLoci.Api.Contracts;

// ---- Studenti ----

public record StudenteDto(
    int Id, string Nome, string Cognome, DateOnly? DataNascita,
    string? Scuola, string? Classe, string? Note, bool Attivo, int[] GenitoriIds);

public record StudenteInput(
    [Required, MaxLength(100)] string Nome,
    [Required, MaxLength(100)] string Cognome,
    DateOnly? DataNascita,
    string? Scuola,
    string? Classe,
    string? Note,
    bool Attivo = true,
    int[]? GenitoriIds = null);

// ---- Genitori ----

public record GenitoreDto(
    int Id, string Nome, string Cognome, string? Email, string? Telefono,
    string? CodiceFiscale, string? Indirizzo, string? Note, int[] StudentiIds);

public record GenitoreInput(
    [Required, MaxLength(100)] string Nome,
    [Required, MaxLength(100)] string Cognome,
    [EmailAddress] string? Email,
    string? Telefono,
    [MaxLength(16)] string? CodiceFiscale,
    string? Indirizzo,
    string? Note);

// ---- Lead ----

public record LeadDto(
    int Id, string Nome, string? Email, string? Telefono, string? Messaggio,
    FonteLead Fonte, StatoLead Stato, int? GenitoreId, DateTime CreatoIl);

public record LeadInput(
    [Required, MaxLength(200)] string Nome,
    [EmailAddress] string? Email,
    string? Telefono,
    string? Messaggio,
    FonteLead Fonte = FonteLead.Altro);

/// <summary>Payload del form contatti del sito pubblico.</summary>
public record RichiestaContattoInput(
    [Required, MaxLength(200)] string Nome,
    [EmailAddress] string? Email,
    [MaxLength(30)] string? Telefono,
    [MaxLength(2000)] string? Messaggio);

public record CambiaStatoLeadInput([Required] StatoLead Stato);

// ---- Interazioni ----

public record InterazioneDto(int Id, DateTime Data, TipoInterazione Tipo, string Descrizione, int? LeadId, int? GenitoreId);

public record InterazioneInput(TipoInterazione Tipo, [Required] string Descrizione, DateTime? Data = null);

public static class CrmMapping
{
    public static StudenteDto ToDto(this Studente s) => new(
        s.Id, s.Nome, s.Cognome, s.DataNascita, s.Scuola, s.Classe, s.Note, s.Attivo,
        s.Genitori.Select(g => g.Id).ToArray());

    public static GenitoreDto ToDto(this Genitore g) => new(
        g.Id, g.Nome, g.Cognome, g.Email, g.Telefono, g.CodiceFiscale, g.Indirizzo, g.Note,
        g.Studenti.Select(s => s.Id).ToArray());

    public static LeadDto ToDto(this Lead l) => new(
        l.Id, l.Nome, l.Email, l.Telefono, l.Messaggio, l.Fonte, l.Stato, l.GenitoreId, l.CreatoIl);

    public static InterazioneDto ToDto(this Interazione i) => new(
        i.Id, i.Data, i.Tipo, i.Descrizione, i.LeadId, i.GenitoreId);
}
