using System.ComponentModel.DataAnnotations;

namespace StudiorumLoci.Shared;

public record PagamentoDto(
    int Id, int StudenteId, string Studente, string Causale, decimal Importo, DateOnly Scadenza,
    StatoPagamento Stato, bool Scaduto, DateOnly? DataPagamento, MetodoPagamento? Metodo, string? Note,
    RicevutaDto? Ricevuta);

/// <summary>File allegato a un pagamento (es. foto o PDF del bonifico).</summary>
public record RicevutaDto(string Nome, string Tipo, long Dimensione);

public class PagamentoInput
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleziona uno studente")] public int StudenteId { get; set; }
    [Required(ErrorMessage = "Causale obbligatoria"), MaxLength(200)] public string Causale { get; set; } = "";
    [Range(0.01, 100_000, ErrorMessage = "Importo non valido")] public decimal Importo { get; set; }
    public DateOnly Scadenza { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? Note { get; set; }

    /// <summary>
    /// In creazione: se valorizzato, il pagamento nasce già incassato con questo metodo.
    /// In modifica: considerati solo per i pagamenti già incassati (correzione di metodo/data).
    /// </summary>
    public MetodoPagamento? Metodo { get; set; }
    public DateOnly? DataPagamento { get; set; }

    public static PagamentoInput Da(PagamentoDto p) => new()
    {
        StudenteId = p.StudenteId, Causale = p.Causale, Importo = p.Importo, Scadenza = p.Scadenza, Note = p.Note,
        Metodo = p.Metodo, DataPagamento = p.DataPagamento,
    };
}

public static class Ricevute
{
    public const long DimensioneMassima = 10 * 1024 * 1024;

    public static readonly string[] TipiAmmessi =
        ["image/jpeg", "image/png", "image/webp", "image/heic", "image/heif", "application/pdf"];

    /// <summary>Valore per l'attributo accept di &lt;input type="file"&gt;.</summary>
    public const string Accept = "image/jpeg,image/png,image/webp,image/heic,image/heif,application/pdf,.heic,.heif";
}

public class RegistraIncassoInput
{
    public MetodoPagamento Metodo { get; set; } = MetodoPagamento.Bonifico;
    public DateOnly? DataPagamento { get; set; }
}

public record RiepilogoPagamentiDto(int Anno, int Mese, decimal Atteso, decimal Incassato, decimal DaIncassare, int NumeroScaduti);
