namespace StudiorumLoci.Api.Domain.Pagamenti;

/// <summary>Contenuto del file di ricevuta di un pagamento (uno per pagamento).</summary>
public class RicevutaFile
{
    public int PagamentoId { get; set; }
    public required byte[] Contenuto { get; set; }
}
