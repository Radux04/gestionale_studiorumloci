using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Crm;

namespace StudiorumLoci.Api.Domain.Sedi;

/// <summary>Sede del doposcuola (punto fisico in cui si svolgono le attività).</summary>
public class Sede : Entity
{
    public required string Nome { get; set; }
    public string? Indirizzo { get; set; }
    public string? Cap { get; set; }
    public required string Comune { get; set; }
    /// <summary>Sigla della provincia (es. "PE").</summary>
    public required string Provincia { get; set; }
    public required string Regione { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Note { get; set; }
    public bool Attiva { get; set; } = true;

    public List<Studente> Studenti { get; set; } = [];
}
