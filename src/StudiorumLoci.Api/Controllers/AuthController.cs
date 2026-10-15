using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Auth;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;

namespace StudiorumLoci.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, ServizioAccessi accessi, TimeProvider clock) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<ActionResult<LoginDto>> Login(LoginInput input)
    {
        var email = input.Email.Trim().ToLowerInvariant();
        var utente = await db.Utenti.Include(u => u.Dipendente).FirstOrDefaultAsync(u => u.Email == email);

        // Stesso messaggio per email inesistente, password errata o account bloccato.
        if (!accessi.VerificaPassword(utente, input.Password) || !utente!.Attivo)
            return Problem("Email o password non corretti.", statusCode: StatusCodes.Status401Unauthorized);

        utente.UltimoAccesso = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();

        var (token, scadenza) = accessi.CreaToken(utente);
        return new LoginDto(token, scadenza, utente.ToCorrenteDto());
    }

    /// <summary>Dati dell'utente collegato (usato dal gestionale all'avvio).</summary>
    [HttpGet("io")]
    public async Task<ActionResult<UtenteCorrenteDto>> Io()
    {
        var utente = await db.Utenti.Include(u => u.Dipendente).AsNoTracking().FirstOrDefaultAsync(u => u.Id == User.Id());
        return utente is null ? Unauthorized() : utente.ToCorrenteDto();
    }

    /// <summary>Cambia la password e restituisce un nuovo token (quello vecchio non vale più).</summary>
    [HttpPost("cambia-password")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<ActionResult<LoginDto>> CambiaPassword(CambiaPasswordInput input)
    {
        var utente = await db.Utenti.Include(u => u.Dipendente).FirstOrDefaultAsync(u => u.Id == User.Id());
        if (utente is null) return Unauthorized();

        if (!accessi.VerificaPassword(utente, input.PasswordAttuale))
            return ValidationProblem("La password attuale non è corretta.");
        if (input.NuovaPassword == input.PasswordAttuale)
            return ValidationProblem("La nuova password deve essere diversa da quella attuale.");

        accessi.ImpostaPassword(utente, input.NuovaPassword);
        utente.DeveCambiarePassword = false;
        await db.SaveChangesAsync();

        var (token, scadenza) = accessi.CreaToken(utente);
        return new LoginDto(token, scadenza, utente.ToCorrenteDto());
    }
}
