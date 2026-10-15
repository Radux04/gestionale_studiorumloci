using StudiorumLoci.Api.Domain.Common;

namespace StudiorumLoci.Api.Domain.Dipendenti;

/// <summary>Turno di lavoro: base per presenze e calcolo dei compensi.</summary>
public class Turno : Entity
{
    public int DipendenteId { get; set; }
    public Dipendente Dipendente { get; set; } = null!;

    public DateTime Inizio { get; set; }
    public DateTime Fine { get; set; }
    public string? Note { get; set; }

    public decimal Ore => (decimal)(Fine - Inizio).TotalHours;
}
