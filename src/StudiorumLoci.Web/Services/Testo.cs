using System.Text.RegularExpressions;
using StudiorumLoci.Shared;

namespace StudiorumLoci.Web.Services;

/// <summary>Formattazione per la UI.</summary>
public static partial class Testo
{
    /// <summary>"PrestazioneOccasionale" → "Prestazione occasionale".</summary>
    public static string Di(Enum valore)
    {
        var parole = SpazioPrimaMaiuscola().Replace(valore.ToString(), " $1");
        return parole[..1] + parole[1..].ToLowerInvariant();
    }

    public static string Data(DateOnly? d) => d?.ToString("dd/MM/yyyy") ?? "—";

    public static string Data(DateTime d) => d.ToString("dd/MM/yyyy");

    /// <summary>Data e ora; gli istanti UTC vengono convertiti in ora locale.</summary>
    public static string DataOra(DateTime d) =>
        (d.Kind == DateTimeKind.Utc ? d.ToLocalTime() : d).ToString("dd/MM/yyyy HH:mm");

    /// <summary>Classe CSS del colore di una sede nel calendario (c0…c5, definite in app.css).</summary>
    public static string ColoreSede(int sedeId) => $"c{sedeId % 6}";

    /// <summary>"mercoledì 14 ottobre" → "Mercoledì 14 ottobre": solo la prima lettera maiuscola.</summary>
    public static string Maiuscola(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];

    public static string Euro(decimal importo) => importo.ToString("C");

    public static string Dimensione(long byte_) => byte_ switch
    {
        < 1024 => $"{byte_} B",
        < 1024 * 1024 => $"{byte_ / 1024.0:0} KB",
        _ => $"{byte_ / 1024.0 / 1024.0:0.0} MB",
    };

    public static string Ore(decimal ore) => ore.ToString("0.##") + " h";

    public static string BadgeLead(StatoLead stato) => stato switch
    {
        StatoLead.Nuovo => "text-bg-primary",
        StatoLead.Contattato => "text-bg-warning",
        StatoLead.Convertito => "text-bg-success",
        _ => "text-bg-secondary",
    };

    public static string BadgePagamento(PagamentoDto p) => p switch
    {
        { Scaduto: true } => "text-bg-danger",
        { Stato: StatoPagamento.Pagato } => "text-bg-success",
        { Stato: StatoPagamento.Annullato } => "text-bg-secondary",
        _ => "text-bg-warning",
    };

    public static string Stato(PagamentoDto p) => p.Scaduto ? "Scaduto" : Di(p.Stato);

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex SpazioPrimaMaiuscola();
}
