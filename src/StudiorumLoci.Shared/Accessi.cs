using System.ComponentModel.DataAnnotations;

namespace StudiorumLoci.Shared;

/// <summary>Ruolo di accesso al gestionale (diverso dalla mansione del dipendente).</summary>
public enum RuoloAccesso
{
    /// <summary>Informatici che gestiscono la piattaforma.</summary>
    SuperAdmin,
    /// <summary>Proprietario: tutto, compresa la creazione dei dipendenti.</summary>
    Admin,
    /// <summary>Come l'admin, ma non crea/elimina dipendenti né ne gestisce gli accessi.</summary>
    Manager,
    /// <summary>Vede la propria scheda e gli studenti delle sue sedi.</summary>
    Professore,
}

/// <summary>Nomi dei ruoli e gruppi usati per le autorizzazioni (API e gestionale).</summary>
public static class Ruoli
{
    public const string SuperAdmin = nameof(RuoloAccesso.SuperAdmin);
    public const string Admin = nameof(RuoloAccesso.Admin);
    public const string Manager = nameof(RuoloAccesso.Manager);
    public const string Professore = nameof(RuoloAccesso.Professore);

    /// <summary>Chi gestisce l'attività quotidiana (CRM, sedi, pagamenti, turni).</summary>
    public const string Gestione = $"{SuperAdmin},{Admin},{Manager}";
    /// <summary>Chi crea/elimina dipendenti e ne gestisce gli accessi.</summary>
    public const string Amministrazione = $"{SuperAdmin},{Admin}";

    /// <summary>Ruoli che chi ha il ruolo indicato può assegnare ad altri.</summary>
    public static RuoloAccesso[] Assegnabili(RuoloAccesso chi) => chi switch
    {
        RuoloAccesso.SuperAdmin => [RuoloAccesso.SuperAdmin, RuoloAccesso.Admin, RuoloAccesso.Manager, RuoloAccesso.Professore],
        RuoloAccesso.Admin => [RuoloAccesso.Manager, RuoloAccesso.Professore],
        _ => [],
    };

    public const int LunghezzaMinimaPassword = 10;
}

// ---- Login ----

public class LoginInput
{
    [Required(ErrorMessage = "Inserisci l'email"), EmailAddress(ErrorMessage = "Email non valida")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Inserisci la password")] public string Password { get; set; } = "";
}

public record LoginDto(string Token, DateTime Scadenza, UtenteCorrenteDto Utente);

public record UtenteCorrenteDto(int Id, string Email, string Nome, RuoloAccesso Ruolo, int? DipendenteId, bool DeveCambiarePassword);

public class CambiaPasswordInput
{
    [Required(ErrorMessage = "Inserisci la password attuale")] public string PasswordAttuale { get; set; } = "";

    [Required(ErrorMessage = "Inserisci la nuova password")]
    [MinLength(Ruoli.LunghezzaMinimaPassword, ErrorMessage = "La password deve avere almeno 10 caratteri")]
    public string NuovaPassword { get; set; } = "";

    [Compare(nameof(NuovaPassword), ErrorMessage = "Le due password non coincidono")]
    public string ConfermaPassword { get; set; } = "";
}

// ---- Gestione degli account ----

public record UtenteDto(
    int Id, string Email, string Nome, RuoloAccesso Ruolo, bool Attivo, bool DeveCambiarePassword,
    DateTime? UltimoAccesso, int? DipendenteId);

/// <summary>Accesso di un dipendente, incluso nella sua scheda.</summary>
public record AccessoDto(int UtenteId, string Email, RuoloAccesso Ruolo, bool Attivo, bool DeveCambiarePassword, DateTime? UltimoAccesso);

public class UtenteInput
{
    [Required(ErrorMessage = "Inserisci l'email di accesso"), EmailAddress(ErrorMessage = "Email non valida")] public string Email { get; set; } = "";
    /// <summary>Nome mostrato (solo per gli account non legati a un dipendente, es. admin).</summary>
    [MaxLength(200)] public string? Nome { get; set; }
    public RuoloAccesso Ruolo { get; set; } = RuoloAccesso.Professore;
    public bool Attivo { get; set; } = true;
    /// <summary>Dipendente a cui collegare l'account (obbligatorio per manager e professori).</summary>
    public int? DipendenteId { get; set; }
}

/// <summary>Password generata dal sistema, mostrata una sola volta a chi crea/reimposta l'account.</summary>
public record PasswordTemporaneaDto(int UtenteId, string Email, string PasswordTemporanea);

/// <summary>Risposta alla creazione di un dipendente con il suo accesso.</summary>
public record DipendenteCreatoDto(DipendenteDto Dipendente, PasswordTemporaneaDto Accesso);
