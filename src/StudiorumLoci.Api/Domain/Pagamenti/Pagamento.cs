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

    public bool Scaduto(DateOnly oggi) => Stato == StatoPagamento.DaPagare && Scadenza < oggi;
}

public enum StatoPagamento { DaPagare, Pagato, Annullato }

public enum MetodoPagamento { Contanti, Bonifico, Carta, Pos, Satispay, Altro }
