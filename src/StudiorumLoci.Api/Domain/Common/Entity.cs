namespace StudiorumLoci.Api.Domain.Common;

/// <summary>Base per tutte le entità persistite.</summary>
public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CreatoIl { get; set; }
    public DateTime? ModificatoIl { get; set; }
}
