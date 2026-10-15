using System.ComponentModel.DataAnnotations;

namespace StudiorumLoci.Shared;

public record SedeDto(
    int Id, string Nome, string? Indirizzo, string? Cap, string Comune, string Provincia, string Regione,
    string? Telefono, string? Email, string? Note, bool Attiva, int NumeroStudenti)
{
    /// <summary>Es. "Pescara (PE)".</summary>
    public string Localita => $"{Comune} ({Provincia})";
}

public class SedeInput
{
    [Required(ErrorMessage = "Nome obbligatorio"), MaxLength(150)] public string Nome { get; set; } = "";
    [MaxLength(200)] public string? Indirizzo { get; set; }
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Il CAP deve avere 5 cifre")] public string? Cap { get; set; }
    [Required(ErrorMessage = "Comune obbligatorio"), MaxLength(100)] public string Comune { get; set; } = "";
    [Required(ErrorMessage = "Provincia obbligatoria"), RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Usa la sigla di 2 lettere (es. PE)")]
    public string Provincia { get; set; } = "";
    [Required(ErrorMessage = "Regione obbligatoria")] public string Regione { get; set; } = "";
    public string? Telefono { get; set; }
    [EmailAddress(ErrorMessage = "Email non valida")] public string? Email { get; set; }
    public string? Note { get; set; }
    public bool Attiva { get; set; } = true;

    public static SedeInput Da(SedeDto s) => new()
    {
        Nome = s.Nome, Indirizzo = s.Indirizzo, Cap = s.Cap, Comune = s.Comune, Provincia = s.Provincia,
        Regione = s.Regione, Telefono = s.Telefono, Email = s.Email, Note = s.Note, Attiva = s.Attiva,
    };
}

public static class Regioni
{
    public static readonly string[] Elenco =
    [
        "Abruzzo", "Basilicata", "Calabria", "Campania", "Emilia-Romagna", "Friuli-Venezia Giulia",
        "Lazio", "Liguria", "Lombardia", "Marche", "Molise", "Piemonte", "Puglia", "Sardegna",
        "Sicilia", "Toscana", "Trentino-Alto Adige", "Umbria", "Valle d'Aosta", "Veneto",
    ];
}
