using StudiorumLoci.Api.Domain.Common;

namespace StudiorumLoci.Api.Domain.Crm;

/// <summary>Genitore o tutore: il contatto di riferimento (e di fatturazione) per uno o più studenti.</summary>
public class Genitore : Entity
{
    public required string Nome { get; set; }
    public required string Cognome { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? CodiceFiscale { get; set; }
    public string? Indirizzo { get; set; }
    public string? Note { get; set; }

    public List<Studente> Studenti { get; set; } = [];
    public List<Interazione> Interazioni { get; set; } = [];
}
