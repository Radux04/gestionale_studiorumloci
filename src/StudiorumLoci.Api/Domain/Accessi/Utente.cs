using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Dipendenti;

namespace StudiorumLoci.Api.Domain.Accessi;

/// <summary>Account di accesso al gestionale.</summary>
public class Utente : Entity
{
    /// <summary>Email di accesso, sempre in minuscolo.</summary>
    public required string Email { get; set; }
    public string PasswordHash { get; set; } = "";
    public RuoloAccesso Ruolo { get; set; }
    public bool Attivo { get; set; } = true;
    public bool DeveCambiarePassword { get; set; } = true;
    public DateTime? UltimoAccesso { get; set; }

    /// <summary>Nome mostrato per gli account senza dipendente (admin, superadmin).</summary>
    public string? Nome { get; set; }

    /// <summary>
    /// Incrementata a ogni cambio di password, ruolo o stato: i token emessi prima
    /// smettono di essere validi.
    /// </summary>
    public int VersioneToken { get; set; }

    /// <summary>Dipendente collegato (obbligatorio per manager e professori).</summary>
    public int? DipendenteId { get; set; }
    public Dipendente? Dipendente { get; set; }

    public string NomeVisualizzato => Dipendente is { } d ? $"{d.Nome} {d.Cognome}" : Nome ?? Email;

    public void InvalidaToken() => VersioneToken++;
}
