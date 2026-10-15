using System.Text.RegularExpressions;

namespace StudiorumLoci.Api;

/// <summary>
/// Origini CORS da configurazione (Cors:Origini). Accetta anche il carattere jolly "*",
/// utile per gli indirizzi dei singoli deploy Vercel (es. "https://gestionale-*.vercel.app").
/// </summary>
public class OriginiConsentite(IEnumerable<string> origini)
{
    private readonly Regex[] modelli = origini
        .Select(o => o.Trim().TrimEnd('/'))
        .Where(o => o.Length > 0)
        .Select(o => new Regex("^" + Regex.Escape(o).Replace(@"\*", "[a-z0-9-]*") + "$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        .ToArray();

    public bool Consente(string origine) => modelli.Any(m => m.IsMatch(origine));
}
