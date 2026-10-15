using System.ComponentModel.DataAnnotations;

namespace StudiorumLoci.Shared;

public record DipendenteDto(
    int Id, string Nome, string Cognome, string? Email, string? Telefono, string? CodiceFiscale,
    RuoloDipendente Ruolo, TipoContratto Contratto, DateOnly DataAssunzione, DateOnly? DataCessazione,
    decimal CompensoOrario, string? Materie, bool Attivo, int[] SediIds, AccessoDto? Accesso)
{
    public string NomeCompleto => $"{Cognome} {Nome}";
}

public class DipendenteInput
{
    [Required(ErrorMessage = "Nome obbligatorio"), MaxLength(100)] public string Nome { get; set; } = "";
    [Required(ErrorMessage = "Cognome obbligatorio"), MaxLength(100)] public string Cognome { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email non valida")] public string? Email { get; set; }
    public string? Telefono { get; set; }
    [MaxLength(16)] public string? CodiceFiscale { get; set; }
    public RuoloDipendente Ruolo { get; set; }
    public TipoContratto Contratto { get; set; } = TipoContratto.Collaborazione;
    public DateOnly DataAssunzione { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? DataCessazione { get; set; }
    [Range(0, 1000, ErrorMessage = "Compenso non valido")] public decimal CompensoOrario { get; set; }
    public string? Materie { get; set; }
    public bool Attivo { get; set; } = true;
    public List<int> SediIds { get; set; } = [];

    public static DipendenteInput Da(DipendenteDto d) => new()
    {
        Nome = d.Nome, Cognome = d.Cognome, Email = d.Email, Telefono = d.Telefono, CodiceFiscale = d.CodiceFiscale,
        Ruolo = d.Ruolo, Contratto = d.Contratto, DataAssunzione = d.DataAssunzione, DataCessazione = d.DataCessazione,
        CompensoOrario = d.CompensoOrario, Materie = d.Materie, Attivo = d.Attivo, SediIds = [.. d.SediIds],
    };
}

/// <summary>Creazione di un dipendente: oltre all'anagrafica si decide subito il suo accesso al gestionale.</summary>
public class NuovoDipendenteInput : DipendenteInput
{
    [Required(ErrorMessage = "Inserisci l'email con cui accederà"), EmailAddress(ErrorMessage = "Email non valida")]
    public string EmailAccesso { get; set; } = "";

    /// <summary>Manager o Professore.</summary>
    public RuoloAccesso RuoloAccesso { get; set; } = RuoloAccesso.Professore;
}

public record TurnoDto(int Id, int DipendenteId, DateTime Inizio, DateTime Fine, decimal Ore, string? Note);

public class TurnoInput
{
    public DateTime Inizio { get; set; } = DateTime.Today.AddHours(14);
    public DateTime Fine { get; set; } = DateTime.Today.AddHours(18);
    public string? Note { get; set; }

    public static TurnoInput Da(TurnoDto t) => new() { Inizio = t.Inizio, Fine = t.Fine, Note = t.Note };
}

public record CompensoDto(int DipendenteId, DateOnly Da, DateOnly A, decimal OreTotali, decimal CompensoOrario, decimal Totale);
