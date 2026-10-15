using System.ComponentModel.DataAnnotations;

namespace StudiorumLoci.Shared;

// ---- Studenti ----

public record StudenteDto(
    int Id, string Nome, string Cognome, DateOnly? DataNascita,
    string? Scuola, string? Classe, string? Note, bool Attivo, int[] GenitoriIds)
{
    public string NomeCompleto => $"{Cognome} {Nome}";
}

public class StudenteInput
{
    [Required(ErrorMessage = "Nome obbligatorio"), MaxLength(100)] public string Nome { get; set; } = "";
    [Required(ErrorMessage = "Cognome obbligatorio"), MaxLength(100)] public string Cognome { get; set; } = "";
    public DateOnly? DataNascita { get; set; }
    public string? Scuola { get; set; }
    public string? Classe { get; set; }
    public string? Note { get; set; }
    public bool Attivo { get; set; } = true;
    public List<int> GenitoriIds { get; set; } = [];

    public static StudenteInput Da(StudenteDto s) => new()
    {
        Nome = s.Nome, Cognome = s.Cognome, DataNascita = s.DataNascita, Scuola = s.Scuola,
        Classe = s.Classe, Note = s.Note, Attivo = s.Attivo, GenitoriIds = [.. s.GenitoriIds],
    };
}

// ---- Genitori ----

public record GenitoreDto(
    int Id, string Nome, string Cognome, string? Email, string? Telefono,
    string? CodiceFiscale, string? Indirizzo, string? Note, int[] StudentiIds)
{
    public string NomeCompleto => $"{Cognome} {Nome}";
}

public class GenitoreInput
{
    [Required(ErrorMessage = "Nome obbligatorio"), MaxLength(100)] public string Nome { get; set; } = "";
    [Required(ErrorMessage = "Cognome obbligatorio"), MaxLength(100)] public string Cognome { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email non valida")] public string? Email { get; set; }
    public string? Telefono { get; set; }
    [MaxLength(16)] public string? CodiceFiscale { get; set; }
    public string? Indirizzo { get; set; }
    public string? Note { get; set; }

    public static GenitoreInput Da(GenitoreDto g) => new()
    {
        Nome = g.Nome, Cognome = g.Cognome, Email = g.Email, Telefono = g.Telefono,
        CodiceFiscale = g.CodiceFiscale, Indirizzo = g.Indirizzo, Note = g.Note,
    };
}

// ---- Lead ----

public record LeadDto(
    int Id, string Nome, string? Email, string? Telefono, string? Messaggio,
    FonteLead Fonte, StatoLead Stato, int? GenitoreId, DateTime CreatoIl);

public class LeadInput
{
    [Required(ErrorMessage = "Nome obbligatorio"), MaxLength(200)] public string Nome { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email non valida")] public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Messaggio { get; set; }
    public FonteLead Fonte { get; set; } = FonteLead.Telefono;
}

/// <summary>Payload del form contatti del sito pubblico.</summary>
public class RichiestaContattoInput
{
    [Required, MaxLength(200)] public string Nome { get; set; } = "";
    [EmailAddress] public string? Email { get; set; }
    [MaxLength(30)] public string? Telefono { get; set; }
    [MaxLength(2000)] public string? Messaggio { get; set; }
}

public record CambiaStatoLeadInput(StatoLead Stato);

// ---- Interazioni ----

public record InterazioneDto(int Id, DateTime Data, TipoInterazione Tipo, string Descrizione, int? LeadId, int? GenitoreId);

public class InterazioneInput
{
    public TipoInterazione Tipo { get; set; } = TipoInterazione.Telefonata;
    [Required(ErrorMessage = "Descrizione obbligatoria")] public string Descrizione { get; set; } = "";
    public DateTime? Data { get; set; }
}
