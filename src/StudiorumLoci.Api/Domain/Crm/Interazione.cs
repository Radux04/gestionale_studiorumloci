using StudiorumLoci.Api.Domain.Common;

namespace StudiorumLoci.Api.Domain.Crm;

/// <summary>Storico dei contatti (chiamate, email, colloqui) con un lead o un genitore.</summary>
public class Interazione : Entity
{
    public DateTime Data { get; set; }
    public TipoInterazione Tipo { get; set; }
    public required string Descrizione { get; set; }

    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public int? GenitoreId { get; set; }
    public Genitore? Genitore { get; set; }
}
