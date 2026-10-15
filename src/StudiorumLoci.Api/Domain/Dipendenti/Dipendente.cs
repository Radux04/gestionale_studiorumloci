using StudiorumLoci.Api.Domain.Accessi;
using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Sedi;

namespace StudiorumLoci.Api.Domain.Dipendenti;

public class Dipendente : Entity
{
    public required string Nome { get; set; }
    public required string Cognome { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? CodiceFiscale { get; set; }
    public RuoloDipendente Ruolo { get; set; }
    public TipoContratto Contratto { get; set; }
    public DateOnly DataAssunzione { get; set; }
    public DateOnly? DataCessazione { get; set; }
    public decimal CompensoOrario { get; set; }

    /// <summary>Materie insegnate, separate da virgola (es. "Matematica, Fisica").</summary>
    public string? Materie { get; set; }
    public bool Attivo { get; set; } = true;

    public List<Turno> Turni { get; set; } = [];

    /// <summary>Sedi in cui lavora (anche più d'una).</summary>
    public List<Sede> Sedi { get; set; } = [];

    /// <summary>Accesso al gestionale (null per i dipendenti creati prima del login).</summary>
    public Utente? Utente { get; set; }
}
