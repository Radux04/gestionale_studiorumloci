using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Pagamenti;
using StudiorumLoci.Api.Domain.Sedi;

namespace StudiorumLoci.Api.Domain.Crm;

public class Studente : Entity
{
    public required string Nome { get; set; }
    public required string Cognome { get; set; }
    public DateOnly? DataNascita { get; set; }
    public string? Scuola { get; set; }
    public string? Classe { get; set; }
    public string? Note { get; set; }
    public bool Attivo { get; set; } = true;

    /// <summary>Sede di appartenenza. Nullable solo per gli studenti creati prima dell'introduzione delle sedi.</summary>
    public int? SedeId { get; set; }
    public Sede? Sede { get; set; }

    public List<Genitore> Genitori { get; set; } = [];
    public List<Pagamento> Pagamenti { get; set; } = [];
}
