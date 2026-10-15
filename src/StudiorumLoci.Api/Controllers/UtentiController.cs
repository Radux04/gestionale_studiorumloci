using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Auth;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Accessi;

namespace StudiorumLoci.Api.Controllers;

/// <summary>
/// Gestione degli account. Il superadmin gestisce tutti; l'admin solo manager e professori.
/// Nessuno può cambiare ruolo, bloccare o eliminare il proprio account.
/// </summary>
[ApiController]
[Route("api/utenti")]
[Authorize(Roles = Ruoli.Amministrazione)]
public class UtentiController(AppDbContext db, ServizioAccessi accessi) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<UtenteDto>> Elenco()
    {
        var query = db.Utenti.Include(u => u.Dipendente).AsNoTracking();
        if (User.Ruolo() != RuoloAccesso.SuperAdmin) query = query.Where(u => u.Ruolo != RuoloAccesso.SuperAdmin);

        return (await query.OrderBy(u => u.Ruolo).ThenBy(u => u.Email).ToListAsync()).Select(u => u.ToDto());
    }

    /// <summary>Crea un account con password temporanea (mostrata una sola volta).</summary>
    [HttpPost]
    public async Task<ActionResult<PasswordTemporaneaDto>> Crea(UtenteInput input)
    {
        if (!Ruoli.Assegnabili(User.Ruolo()).Contains(input.Ruolo))
            return Problem($"Non puoi creare account con ruolo {input.Ruolo}.", statusCode: StatusCodes.Status403Forbidden);

        var email = input.Email.Trim().ToLowerInvariant();
        if (await db.Utenti.AnyAsync(u => u.Email == email)) return Conflict($"Esiste già un account con l'email {email}.");
        if (await VerificaDipendente(input, utenteId: null) is { } errore) return errore;

        var password = ServizioAccessi.GeneraPasswordTemporanea();
        var utente = new Utente
        {
            Email = email,
            Nome = input.Nome,
            Ruolo = input.Ruolo,
            Attivo = input.Attivo,
            DipendenteId = input.DipendenteId,
        };
        accessi.ImpostaPassword(utente, password);
        db.Utenti.Add(utente);
        await db.SaveChangesAsync();
        return new PasswordTemporaneaDto(utente.Id, utente.Email, password);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UtenteDto>> Modifica(int id, UtenteInput input)
    {
        var utente = await db.Utenti.Include(u => u.Dipendente).FirstOrDefaultAsync(u => u.Id == id);
        if (utente is null) return NotFound();
        if (VerificaPermesso(utente) is { } vietato) return vietato;

        if (utente.Id == User.Id() && (input.Ruolo != utente.Ruolo || !input.Attivo))
            return Problem("Non puoi cambiare ruolo o bloccare il tuo account.", statusCode: StatusCodes.Status403Forbidden);
        if (input.Ruolo != utente.Ruolo && !Ruoli.Assegnabili(User.Ruolo()).Contains(input.Ruolo))
            return Problem($"Non puoi assegnare il ruolo {input.Ruolo}.", statusCode: StatusCodes.Status403Forbidden);

        var email = input.Email.Trim().ToLowerInvariant();
        if (email != utente.Email && await db.Utenti.AnyAsync(u => u.Email == email))
            return Conflict($"Esiste già un account con l'email {email}.");
        if (await VerificaDipendente(input, utente.Id) is { } errore) return errore;

        // Ruolo, stato o email cambiati: le sessioni aperte vanno chiuse.
        if (input.Ruolo != utente.Ruolo || input.Attivo != utente.Attivo || email != utente.Email) utente.InvalidaToken();

        utente.Email = email;
        utente.Nome = input.Nome;
        utente.Ruolo = input.Ruolo;
        utente.Attivo = input.Attivo;
        utente.DipendenteId = input.DipendenteId;
        await db.SaveChangesAsync();
        await db.Entry(utente).Reference(u => u.Dipendente).LoadAsync();
        return utente.ToDto();
    }

    /// <summary>Genera una nuova password temporanea; l'utente dovrà cambiarla al prossimo accesso.</summary>
    [HttpPost("{id:int}/reimposta-password")]
    public async Task<ActionResult<PasswordTemporaneaDto>> ReimpostaPassword(int id)
    {
        var utente = await db.Utenti.FindAsync(id);
        if (utente is null) return NotFound();
        if (VerificaPermesso(utente) is { } vietato) return vietato;
        if (utente.Id == User.Id()) return Problem("Per la tua password usa «Cambia password».", statusCode: StatusCodes.Status400BadRequest);

        var password = ServizioAccessi.GeneraPasswordTemporanea();
        accessi.ImpostaPassword(utente, password);
        utente.DeveCambiarePassword = true;
        await db.SaveChangesAsync();
        return new PasswordTemporaneaDto(utente.Id, utente.Email, password);
    }

    /// <summary>Elimina l'account (il dipendente collegato resta).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id)
    {
        var utente = await db.Utenti.FindAsync(id);
        if (utente is null) return NotFound();
        if (VerificaPermesso(utente) is { } vietato) return vietato;
        if (utente.Id == User.Id()) return Problem("Non puoi eliminare il tuo account.", statusCode: StatusCodes.Status403Forbidden);

        db.Utenti.Remove(utente);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>L'admin può agire solo su manager e professori (e su se stesso, entro i limiti sopra).</summary>
    private ActionResult? VerificaPermesso(Utente bersaglio) =>
        bersaglio.Id == User.Id() || Ruoli.Assegnabili(User.Ruolo()).Contains(bersaglio.Ruolo)
            ? null
            : Problem("Non hai i permessi per gestire questo account.", statusCode: StatusCodes.Status403Forbidden);

    /// <summary>Manager e professori devono essere collegati a un dipendente che non abbia già un account.</summary>
    private async Task<ActionResult?> VerificaDipendente(UtenteInput input, int? utenteId)
    {
        if (input.Ruolo is RuoloAccesso.Manager or RuoloAccesso.Professore && input.DipendenteId is null)
            return ValidationProblem("Manager e professori devono essere collegati a un dipendente.");
        if (input.DipendenteId is not { } dip) return null;

        if (!await db.Dipendenti.AnyAsync(d => d.Id == dip)) return ValidationProblem("Dipendente inesistente.");
        if (await db.Utenti.AnyAsync(u => u.DipendenteId == dip && u.Id != utenteId))
            return Conflict("Questo dipendente ha già un account di accesso.");
        return null;
    }
}
