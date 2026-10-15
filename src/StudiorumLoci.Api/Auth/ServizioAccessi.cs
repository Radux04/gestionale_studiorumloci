using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StudiorumLoci.Api.Domain.Accessi;

namespace StudiorumLoci.Api.Auth;

/// <summary>Configurazione dei token (sezione "Jwt").</summary>
public class ImpostazioniJwt
{
    /// <summary>Chiave di firma, almeno 32 caratteri. Da user-secrets o variabile d'ambiente Jwt__Chiave.</summary>
    public string Chiave { get; set; } = "";
    public string Emittente { get; set; } = "studiorumloci-api";
    public int DurataOre { get; set; } = 10;

    public SymmetricSecurityKey ChiaveFirma()
    {
        if (Chiave.Length < 32)
            throw new InvalidOperationException("Jwt:Chiave mancante o troppo corta (servono almeno 32 caratteri).");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Chiave));
    }
}

/// <summary>Nomi delle claim nel token.</summary>
public static class NomiClaim
{
    public const string Id = "sub";
    public const string Email = "email";
    public const string Nome = "name";
    public const string Ruolo = "role";
    public const string Dipendente = "dip";
    public const string Versione = "ver";
}

public class ServizioAccessi(ImpostazioniJwt jwt, TimeProvider clock)
{
    private readonly PasswordHasher<Utente> hasher = new();

    /// <summary>Hash di una password qualsiasi, per rendere uguali i tempi di risposta quando l'utente non esiste.</summary>
    private static readonly string HashFittizio = new PasswordHasher<Utente>().HashPassword(null!, "password-fittizia-per-i-tempi");

    public void ImpostaPassword(Utente utente, string password)
    {
        utente.PasswordHash = hasher.HashPassword(utente, password);
        utente.InvalidaToken();
    }

    public bool VerificaPassword(Utente? utente, string password)
    {
        if (utente is null)
        {
            hasher.VerifyHashedPassword(null!, HashFittizio, password);
            return false;
        }
        return hasher.VerifyHashedPassword(utente, utente.PasswordHash, password) != PasswordVerificationResult.Failed;
    }

    /// <summary>Password temporanea leggibile (senza caratteri ambigui come 0/O, 1/l).</summary>
    public static string GeneraPasswordTemporanea() =>
        RandomNumberGenerator.GetString("abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789", 12);

    public (string Token, DateTime Scadenza) CreaToken(Utente utente)
    {
        var scadenza = clock.GetUtcNow().UtcDateTime.AddHours(jwt.DurataOre);
        var claims = new List<System.Security.Claims.Claim>
        {
            new(NomiClaim.Id, utente.Id.ToString()),
            new(NomiClaim.Email, utente.Email),
            new(NomiClaim.Nome, utente.NomeVisualizzato),
            new(NomiClaim.Ruolo, utente.Ruolo.ToString()),
            new(NomiClaim.Versione, utente.VersioneToken.ToString()),
        };
        if (utente.DipendenteId is { } dip) claims.Add(new(NomiClaim.Dipendente, dip.ToString()));

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Emittente,
            Audience = jwt.Emittente,
            Subject = new ClaimsIdentity(claims),
            Expires = scadenza,
            SigningCredentials = new SigningCredentials(jwt.ChiaveFirma(), SecurityAlgorithms.HmacSha256),
        });
        return (token, scadenza);
    }
}

public static class UtenteCorrente
{
    public static int Id(this ClaimsPrincipal u) => int.Parse(u.FindFirstValue(NomiClaim.Id)!);

    public static RuoloAccesso Ruolo(this ClaimsPrincipal u) => Enum.Parse<RuoloAccesso>(u.FindFirstValue(NomiClaim.Ruolo)!);

    public static int? DipendenteId(this ClaimsPrincipal u) =>
        int.TryParse(u.FindFirstValue(NomiClaim.Dipendente), out var id) ? id : null;

    public static bool PuoGestire(this ClaimsPrincipal u) => u.Ruolo() is RuoloAccesso.SuperAdmin or RuoloAccesso.Admin or RuoloAccesso.Manager;
}
