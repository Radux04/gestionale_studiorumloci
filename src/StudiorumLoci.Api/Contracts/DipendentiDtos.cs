using System.ComponentModel.DataAnnotations;
using StudiorumLoci.Api.Domain.Dipendenti;

namespace StudiorumLoci.Api.Contracts;

public record DipendenteDto(
    int Id, string Nome, string Cognome, string? Email, string? Telefono, string? CodiceFiscale,
    RuoloDipendente Ruolo, TipoContratto Contratto, DateOnly DataAssunzione, DateOnly? DataCessazione,
    decimal CompensoOrario, string? Materie, bool Attivo);

public record DipendenteInput(
    [Required, MaxLength(100)] string Nome,
    [Required, MaxLength(100)] string Cognome,
    [EmailAddress] string? Email,
    string? Telefono,
    [MaxLength(16)] string? CodiceFiscale,
    RuoloDipendente Ruolo,
    TipoContratto Contratto,
    DateOnly DataAssunzione,
    DateOnly? DataCessazione,
    [Range(0, 1000)] decimal CompensoOrario,
    string? Materie,
    bool Attivo = true);

public record TurnoDto(int Id, int DipendenteId, DateTime Inizio, DateTime Fine, decimal Ore, string? Note);

public record TurnoInput(DateTime Inizio, DateTime Fine, string? Note);

public record CompensoDto(int DipendenteId, DateOnly Da, DateOnly A, decimal OreTotali, decimal CompensoOrario, decimal Totale);

public static class DipendentiMapping
{
    public static DipendenteDto ToDto(this Dipendente d) => new(
        d.Id, d.Nome, d.Cognome, d.Email, d.Telefono, d.CodiceFiscale, d.Ruolo, d.Contratto,
        d.DataAssunzione, d.DataCessazione, d.CompensoOrario, d.Materie, d.Attivo);

    public static TurnoDto ToDto(this Turno t) => new(t.Id, t.DipendenteId, t.Inizio, t.Fine, t.Ore, t.Note);
}
