using System.ComponentModel.DataAnnotations;

namespace StudiorumLoci.Shared;

public record LezioneDto(
    int Id, string Titolo, DateTime Inizio, DateTime Fine,
    int SedeId, string Sede, int? DipendenteId, string? Docente, string? Note, Guid? SerieId,
    PartecipanteDto[] Studenti)
{
    public TimeSpan Durata => Fine - Inizio;
}

public record PartecipanteDto(int StudenteId, string Nome, bool? Presente);

public class LezioneInput
{
    [Required(ErrorMessage = "Indica la materia o un titolo"), MaxLength(150)] public string Titolo { get; set; } = "";
    public DateTime Inizio { get; set; } = DateTime.Today.AddHours(15);
    public DateTime Fine { get; set; } = DateTime.Today.AddHours(16);
    [Range(1, int.MaxValue, ErrorMessage = "Seleziona la sede")] public int SedeId { get; set; }
    public int? DipendenteId { get; set; }
    public string? Note { get; set; }
    public List<int> StudentiIds { get; set; } = [];

    /// <summary>Solo in creazione: ripete la lezione ogni settimana fino a questa data (inclusa).</summary>
    public DateOnly? RipetiFinoAl { get; set; }

    public static LezioneInput Da(LezioneDto l) => new()
    {
        Titolo = l.Titolo, Inizio = l.Inizio, Fine = l.Fine, SedeId = l.SedeId, DipendenteId = l.DipendenteId,
        Note = l.Note, StudentiIds = [.. l.Studenti.Select(s => s.StudenteId)],
    };
}

public record PresenzaInput(int StudenteId, bool? Presente);

/// <summary>Quante lezioni ci sono in un giorno (vista annuale).</summary>
public record ConteggioGiornoDto(DateOnly Giorno, int Lezioni);
