using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Dipendenti;

namespace StudiorumLoci.Api.Controllers;

[ApiController]
[Route("api/dipendenti")]
public class DipendentiController(AppDbContext db) : ControllerBase
{
    /// <param name="sedeId">Filtra per sede di lavoro; 0 = dipendenti senza sede assegnata.</param>
    [HttpGet]
    public async Task<IEnumerable<DipendenteDto>> Elenco(RuoloDipendente? ruolo, int? sedeId, bool soloAttivi = true)
    {
        var query = db.Dipendenti.Include(d => d.Sedi).AsNoTracking();
        if (soloAttivi) query = query.Where(d => d.Attivo);
        if (ruolo is not null) query = query.Where(d => d.Ruolo == ruolo);
        if (sedeId == 0) query = query.Where(d => !d.Sedi.Any());
        else if (sedeId is not null) query = query.Where(d => d.Sedi.Any(s => s.Id == sedeId));

        return (await query.OrderBy(d => d.Cognome).ThenBy(d => d.Nome).ToListAsync()).Select(d => d.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DipendenteDto>> Dettaglio(int id)
    {
        var dipendente = await db.Dipendenti.Include(d => d.Sedi).AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        return dipendente is null ? NotFound() : dipendente.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<DipendenteDto>> Crea(DipendenteInput input)
    {
        var dipendente = new Dipendente { Nome = input.Nome, Cognome = input.Cognome };
        if (await Applica(dipendente, input) is { } errore) return errore;
        db.Dipendenti.Add(dipendente);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = dipendente.Id }, dipendente.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DipendenteDto>> Modifica(int id, DipendenteInput input)
    {
        var dipendente = await db.Dipendenti.Include(d => d.Sedi).FirstOrDefaultAsync(d => d.Id == id);
        if (dipendente is null) return NotFound();

        if (await Applica(dipendente, input) is { } errore) return errore;
        await db.SaveChangesAsync();
        return dipendente.ToDto();
    }

    /// <summary>Cessa il rapporto: il dipendente non compare più tra gli attivi ma i turni restano.</summary>
    [HttpPost("{id:int}/cessa")]
    public async Task<IActionResult> Cessa(int id)
    {
        var dipendente = await db.Dipendenti.FindAsync(id);
        if (dipendente is null) return NotFound();

        dipendente.Attivo = false;
        dipendente.DataCessazione ??= DateOnly.FromDateTime(DateTime.Today);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Elimina definitivamente il dipendente insieme a tutti i suoi turni.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id)
    {
        var dipendente = await db.Dipendenti.FindAsync(id);
        if (dipendente is null) return NotFound();

        db.Dipendenti.Remove(dipendente);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ---- Turni / presenze ----

    [HttpGet("{id:int}/turni")]
    public async Task<IEnumerable<TurnoDto>> Turni(int id, DateOnly? da, DateOnly? a)
    {
        var query = db.Turni.AsNoTracking().Where(t => t.DipendenteId == id);
        if (da is not null) query = query.Where(t => t.Inizio >= da.Value.ToDateTime(TimeOnly.MinValue));
        if (a is not null) query = query.Where(t => t.Inizio < a.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));

        return (await query.OrderBy(t => t.Inizio).ToListAsync()).Select(t => t.ToDto());
    }

    [HttpPost("{id:int}/turni")]
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
        var dipendente = await db.Dipendenti.FindAsync(id);
        if (dipendente is null) return NotFound();

        var turni = await Turni(id, da, a);
        var ore = turni.Sum(t => t.Ore);
        return new CompensoDto(id, da, a, ore, dipendente.CompensoOrario, Math.Round(ore * dipendente.CompensoOrario, 2));
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
