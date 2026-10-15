using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Pagamenti;

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

    public List<Genitore> Genitori { get; set; } = [];
    public List<Pagamento> Pagamenti { get; set; } = [];
}
