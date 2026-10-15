using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Auth;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Accessi;
using StudiorumLoci.Api.Domain.Dipendenti;

namespace StudiorumLoci.Api.Controllers;

/// <summary>
/// Dipendenti e turni. Gestione (superadmin, admin, manager) vede e modifica tutto; solo
/// l'amministrazione (superadmin, admin) crea, cessa ed elimina. Il professore vede solo la propria scheda.
/// </summary>
[ApiController]
[Route("api/dipendenti")]
[Authorize]
public class DipendentiController(AppDbContext db, ServizioAccessi accessi) : ControllerBase
{
    /// <param name="sedeId">Filtra per sede di lavoro; 0 = dipendenti senza sede assegnata.</param>
    [HttpGet]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<IEnumerable<DipendenteDto>> Elenco(RuoloDipendente? ruolo, int? sedeId, bool soloAttivi = true)
    {
        var query = db.Dipendenti.Include(d => d.Sedi).Include(d => d.Utente).AsNoTracking();
        if (soloAttivi) query = query.Where(d => d.Attivo);
        if (ruolo is not null) query = query.Where(d => d.Ruolo == ruolo);
        if (sedeId == 0) query = query.Where(d => !d.Sedi.Any());
        else if (sedeId is not null) query = query.Where(d => d.Sedi.Any(s => s.Id == sedeId));

        return (await query.OrderBy(d => d.Cognome).ThenBy(d => d.Nome).ToListAsync()).Select(d => d.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DipendenteDto>> Dettaglio(int id)
    {
        if (!PuoVedere(id)) return Forbid();

        var dipendente = await Carica(id);
        return dipendente is null ? NotFound() : dipendente.ToDto();
    }

    /// <summary>Crea il dipendente e il suo accesso (manager o professore) con password temporanea.</summary>
    [HttpPost]
    [Authorize(Roles = Ruoli.Amministrazione)]
    public async Task<ActionResult<DipendenteCreatoDto>> Crea(NuovoDipendenteInput input)
    {
        if (input.RuoloAccesso is not (RuoloAccesso.Manager or RuoloAccesso.Professore))
            return ValidationProblem("Un nuovo dipendente può essere manager o professore.");

        var email = input.EmailAccesso.Trim().ToLowerInvariant();
        if (await db.Utenti.AnyAsync(u => u.Email == email)) return Conflict($"Esiste già un account con l'email {email}.");

        var dipendente = new Dipendente { Nome = input.Nome, Cognome = input.Cognome };
        if (await Applica(dipendente, input) is { } errore) return errore;

        var password = ServizioAccessi.GeneraPasswordTemporanea();
        dipendente.Utente = new Utente { Email = email, Ruolo = input.RuoloAccesso };
        accessi.ImpostaPassword(dipendente.Utente, password);

        db.Dipendenti.Add(dipendente);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = dipendente.Id },
            new DipendenteCreatoDto(dipendente.ToDto(), new PasswordTemporaneaDto(dipendente.Utente.Id, email, password)));
    }

    /// <summary>Modifica l'anagrafica. L'accesso (email, ruolo, password) si gestisce da api/utenti.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<ActionResult<DipendenteDto>> Modifica(int id, DipendenteInput input)
    {
        var dipendente = await db.Dipendenti.Include(d => d.Sedi).Include(d => d.Utente).FirstOrDefaultAsync(d => d.Id == id);
        if (dipendente is null) return NotFound();

        if (await Applica(dipendente, input) is { } errore) return errore;
        await db.SaveChangesAsync();
        return dipendente.ToDto();
    }

    /// <summary>Cessa il rapporto: il dipendente non compare più tra gli attivi, i turni restano e l'accesso viene bloccato.</summary>
    [HttpPost("{id:int}/cessa")]
    [Authorize(Roles = Ruoli.Amministrazione)]
    public async Task<IActionResult> Cessa(int id)
    {
        var dipendente = await db.Dipendenti.Include(d => d.Utente).FirstOrDefaultAsync(d => d.Id == id);
        if (dipendente is null) return NotFound();
        if (id == User.DipendenteId()) return Problem("Non puoi cessare il tuo rapporto.", statusCode: StatusCodes.Status403Forbidden);

        dipendente.Attivo = false;
        dipendente.DataCessazione ??= DateOnly.FromDateTime(DateTime.Today);
        if (dipendente.Utente is { } utente)
        {
            utente.Attivo = false;
            utente.InvalidaToken();
        }
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Elimina definitivamente il dipendente, i suoi turni e il suo accesso.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Ruoli.Amministrazione)]
    public async Task<IActionResult> Elimina(int id)
    {
        var dipendente = await db.Dipendenti.FindAsync(id);
        if (dipendente is null) return NotFound();
        if (id == User.DipendenteId()) return Problem("Non puoi eliminare te stesso.", statusCode: StatusCodes.Status403Forbidden);

        db.Dipendenti.Remove(dipendente);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ---- Turni / presenze ----

    [HttpGet("{id:int}/turni")]
    public async Task<ActionResult<IEnumerable<TurnoDto>>> Turni(int id, DateOnly? da, DateOnly? a)
    {
        if (!PuoVedere(id)) return Forbid();
        return Ok(await CaricaTurni(id, da, a));
    }

    [HttpPost("{id:int}/turni")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<ActionResult<TurnoDto>> AggiungiTurno(int id, TurnoInput input)
    {
        if (input.Fine <= input.Inizio) return ValidationProblem("La fine del turno deve essere successiva all'inizio.");
        if (!await db.Dipendenti.AnyAsync(d => d.Id == id)) return NotFound();

        var turno = new Turno { DipendenteId = id, Inizio = input.Inizio, Fine = input.Fine, Note = input.Note };
        db.Turni.Add(turno);
        await db.SaveChangesAsync();
        return turno.ToDto();
    }

    [HttpPut("{id:int}/turni/{turnoId:int}")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<ActionResult<TurnoDto>> ModificaTurno(int id, int turnoId, TurnoInput input)
    {
        if (input.Fine <= input.Inizio) return ValidationProblem("La fine del turno deve essere successiva all'inizio.");

        var turno = await db.Turni.FirstOrDefaultAsync(t => t.Id == turnoId && t.DipendenteId == id);
        if (turno is null) return NotFound();

        turno.Inizio = input.Inizio;
        turno.Fine = input.Fine;
        turno.Note = input.Note;
        await db.SaveChangesAsync();
        return turno.ToDto();
    }

    [HttpDelete("{id:int}/turni/{turnoId:int}")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<IActionResult> EliminaTurno(int id, int turnoId)
    {
        var turno = await db.Turni.FirstOrDefaultAsync(t => t.Id == turnoId && t.DipendenteId == id);
        if (turno is null) return NotFound();

        db.Turni.Remove(turno);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Ore lavorate e compenso maturato nel periodo (estremi inclusi).</summary>
    [HttpGet("{id:int}/compenso")]
    public async Task<ActionResult<CompensoDto>> Compenso(int id, DateOnly da, DateOnly a)
    {
        if (!PuoVedere(id)) return Forbid();

        var dipendente = await db.Dipendenti.FindAsync(id);
        if (dipendente is null) return NotFound();

        var ore = (await CaricaTurni(id, da, a)).Sum(t => t.Ore);
        return new CompensoDto(id, da, a, ore, dipendente.CompensoOrario, Math.Round(ore * dipendente.CompensoOrario, 2));
    }

    /// <summary>La gestione vede tutti; gli altri solo la propria scheda.</summary>
    private bool PuoVedere(int dipendenteId) => User.PuoGestire() || User.DipendenteId() == dipendenteId;

    private Task<Dipendente?> Carica(int id) =>
        db.Dipendenti.Include(d => d.Sedi).Include(d => d.Utente).AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    private async Task<List<TurnoDto>> CaricaTurni(int id, DateOnly? da, DateOnly? a)
    {
        var query = db.Turni.AsNoTracking().Where(t => t.DipendenteId == id);
        if (da is not null) query = query.Where(t => t.Inizio >= da.Value.ToDateTime(TimeOnly.MinValue));
        if (a is not null) query = query.Where(t => t.Inizio < a.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));

        return (await query.OrderBy(t => t.Inizio).ToListAsync()).Select(t => t.ToDto()).ToList();
    }

    /// <returns>Un errore di validazione, oppure null se i dati sono stati applicati.</returns>
    private async Task<ActionResult?> Applica(Dipendente d, DipendenteInput input)
    {
        var sediIds = input.SediIds.Distinct().ToList();
        var sedi = await db.Sedi.Where(s => sediIds.Contains(s.Id)).ToListAsync();
        if (sedi.Count != sediIds.Count) return ValidationProblem("Una delle sedi selezionate non esiste più.");
        d.Sedi = sedi;

        d.Nome = input.Nome;
        d.Cognome = input.Cognome;
        d.Email = input.Email;
        d.Telefono = input.Telefono;
        d.CodiceFiscale = input.CodiceFiscale?.ToUpperInvariant();
        d.Ruolo = input.Ruolo;
        d.Contratto = input.Contratto;
        d.DataAssunzione = input.DataAssunzione;
        d.DataCessazione = input.DataCessazione;
        d.CompensoOrario = input.CompensoOrario;
        d.Materie = input.Materie;
        d.Attivo = input.Attivo;
        return null;
    }
}
