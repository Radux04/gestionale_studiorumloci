using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Crm;

namespace StudiorumLoci.Api.Domain.Pagamenti;

/// <summary>Quota dovuta da uno studente (retta mensile, iscrizione, ecc.) e relativo incasso.</summary>
public class Pagamento : Entity
{
    public int StudenteId { get; set; }
    public Studente Studente { get; set; } = null!;

    public required string Causale { get; set; }
    public decimal Importo { get; set; }
    public DateOnly Scadenza { get; set; }

    public StatoPagamento Stato { get; set; } = StatoPagamento.DaPagare;
    public DateOnly? DataPagamento { get; set; }
    public MetodoPagamento? Metodo { get; set; }
    public string? Note { get; set; }

    // Ricevuta allegata (es. foto/PDF del bonifico): qui solo i metadati,
    // il contenuto sta in RicevutaFile per non caricarlo negli elenchi.
    public string? RicevutaNome { get; set; }
    public string? RicevutaTipo { get; set; }
    public long? RicevutaDimensione { get; set; }

    public bool Scaduto(DateOnly oggi) => Stato == StatoPagamento.DaPagare && Scadenza < oggi;
}
