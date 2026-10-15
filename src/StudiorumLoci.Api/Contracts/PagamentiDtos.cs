using System.ComponentModel.DataAnnotations;
using StudiorumLoci.Api.Domain.Pagamenti;

namespace StudiorumLoci.Api.Contracts;

public record PagamentoDto(
    int Id, int StudenteId, string Causale, decimal Importo, DateOnly Scadenza,
    StatoPagamento Stato, bool Scaduto, DateOnly? DataPagamento, MetodoPagamento? Metodo, string? Note);

public record PagamentoInput(
    int StudenteId,
    [Required, MaxLength(200)] string Causale,
    [Range(0.01, 100_000)] decimal Importo,
    DateOnly Scadenza,
    string? Note);

public record RegistraIncassoInput(MetodoPagamento Metodo, DateOnly? DataPagamento = null);

public record RiepilogoPagamentiDto(int Anno, int Mese, decimal Atteso, decimal Incassato, decimal DaIncassare, int NumeroScaduti);

public static class PagamentiMapping
{
    public static PagamentoDto ToDto(this Pagamento p, DateOnly oggi) => new(
        p.Id, p.StudenteId, p.Causale, p.Importo, p.Scadenza, p.Stato, p.Scaduto(oggi),
        p.DataPagamento, p.Metodo, p.Note);
}
