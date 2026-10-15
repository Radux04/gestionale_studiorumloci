using System.ComponentModel.DataAnnotations;

namespace StudiorumLoci.Shared;

public record PagamentoDto(
    int Id, int StudenteId, string Studente, string Causale, decimal Importo, DateOnly Scadenza,
    StatoPagamento Stato, bool Scaduto, DateOnly? DataPagamento, MetodoPagamento? Metodo, string? Note);

public class PagamentoInput
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleziona uno studente")] public int StudenteId { get; set; }
    [Required(ErrorMessage = "Causale obbligatoria"), MaxLength(200)] public string Causale { get; set; } = "";
    [Range(0.01, 100_000, ErrorMessage = "Importo non valido")] public decimal Importo { get; set; }
    public DateOnly Scadenza { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? Note { get; set; }

    public static PagamentoInput Da(PagamentoDto p) => new()
    {
        StudenteId = p.StudenteId, Causale = p.Causale, Importo = p.Importo, Scadenza = p.Scadenza, Note = p.Note,
    };
}

public class RegistraIncassoInput
{
    public MetodoPagamento Metodo { get; set; } = MetodoPagamento.Bonifico;
    public DateOnly? DataPagamento { get; set; }
}

public record RiepilogoPagamentiDto(int Anno, int Mese, decimal Atteso, decimal Incassato, decimal DaIncassare, int NumeroScaduti);
