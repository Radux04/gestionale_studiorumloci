using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Auth;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Calendario;

namespace StudiorumLoci.Api.Controllers;

/// <summary>
/// Calendario delle lezioni. La gestione (superadmin, admin, manager) crea e modifica tutto;
/// il professore vede solo le lezioni che tiene e ne segna le presenze.
/// </summary>
[ApiController]
[Route("api/lezioni")]
[Authorize]
public class LezioniController(AppDbContext db) : ControllerBase
{
    /// <summary>Massima estensione di una serie settimanale.</summary>
    private const int SettimaneMassime = 53;

    /// <summary>Lezioni che iniziano tra <paramref name="da"/> e <paramref name="a"/> (estremi inclusi).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LezioneDto>>> Elenco(DateOnly da, DateOnly a, int? sedeId, int? dipendenteId)
    {
        if (a < da || a.DayNumber - da.DayNumber > 62) return ValidationProblem("Intervallo di date non valido (massimo 2 mesi).");

        var query = Visibili(Carica())
            .Where(l => l.Inizio >= da.ToDateTime(TimeOnly.MinValue) && l.Inizio < a.AddDays(1).ToDateTime(TimeOnly.MinValue));
        if (sedeId is not null) query = query.Where(l => l.SedeId == sedeId);
        if (dipendenteId is not null) query = query.Where(l => l.DipendenteId == dipendenteId);

        return Ok((await query.OrderBy(l => l.Inizio).ToListAsync()).Select(l => l.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LezioneDto>> Dettaglio(int id)
    {
        var lezione = await Visibili(Carica()).FirstOrDefaultAsync(l => l.Id == id);
        return lezione is null ? NotFound() : lezione.ToDto();
    }

    /// <summary>Crea la lezione; con RipetiFinoAl crea una serie settimanale. Restituisce tutte le lezioni create.</summary>
    [HttpPost]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<ActionResult<IEnumerable<LezioneDto>>> Crea(LezioneInput input)
    {
        if (await Verifica(input) is { } errore) return errore;

        var date = new List<DateTime> { input.Inizio };
        if (input.RipetiFinoAl is { } fino)
        {
            var limite = fino.ToDateTime(TimeOnly.MaxValue);
            if (fino < DateOnly.FromDateTime(input.Inizio)) return ValidationProblem("La data di fine ripetizione è precedente alla lezione.");
            for (var d = input.Inizio.AddDays(7); d <= limite; d = d.AddDays(7)) date.Add(d);
            if (date.Count > SettimaneMassime) return ValidationProblem("Una serie può durare al massimo un anno.");
        }

        var durata = input.Fine - input.Inizio;
        var serie = date.Count > 1 ? Guid.NewGuid() : (Guid?)null;
        var create = date.Select(inizio => Nuova(input, inizio, inizio + durata, serie)).ToList();
        db.Lezioni.AddRange(create);
        await db.SaveChangesAsync();

        var ids = create.Select(l => l.Id).ToList();
        return Ok((await Carica().Where(l => ids.Contains(l.Id)).OrderBy(l => l.Inizio).ToListAsync()).Select(l => l.ToDto()));
    }

    /// <summary>Modifica una lezione; con <paramref name="successive"/> applica le stesse modifiche anche alle lezioni successive della serie.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<ActionResult<LezioneDto>> Modifica(int id, LezioneInput input, bool successive = false)
    {
        if (await Verifica(input) is { } errore) return errore;

        var lezione = await db.Lezioni.Include(l => l.Studenti).FirstOrDefaultAsync(l => l.Id == id);
        if (lezione is null) return NotFound();

        var daModificare = successive && lezione.SerieId is { } serie
            ? await db.Lezioni.Include(l => l.Studenti).Where(l => l.SerieId == serie && l.Inizio >= lezione.Inizio).ToListAsync()
            : [lezione];

        // Spostamento di orario applicato a tutte: stessa variazione di giorno/ora rispetto alla lezione modificata.
        var spostamento = input.Inizio - lezione.Inizio;
        var durata = input.Fine - input.Inizio;
        foreach (var l in daModificare)
        {
            l.Inizio = l.Id == id ? input.Inizio : l.Inizio + spostamento;
            l.Fine = l.Inizio + durata;
            l.Titolo = input.Titolo.Trim();
            l.SedeId = input.SedeId;
            l.DipendenteId = input.DipendenteId;
            l.Note = input.Note;
            AggiornaStudenti(l, input.StudentiIds);
        }
        await db.SaveChangesAsync();

        return (await Carica().FirstAsync(l => l.Id == id)).ToDto();
    }

    /// <summary>Elimina la lezione; con <paramref name="successive"/> anche le lezioni successive della stessa serie.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<IActionResult> Elimina(int id, bool successive = false)
    {
        var lezione = await db.Lezioni.FindAsync(id);
        if (lezione is null) return NotFound();

        if (successive && lezione.SerieId is { } serie)
            await db.Lezioni.Where(l => l.SerieId == serie && l.Inizio >= lezione.Inizio).ExecuteDeleteAsync();
        else
        {
            db.Lezioni.Remove(lezione);
            await db.SaveChangesAsync();
        }
        return NoContent();
    }

    /// <summary>Segna le presenze. Consentito alla gestione e al professore che tiene la lezione.</summary>
    [HttpPut("{id:int}/presenze")]
    public async Task<ActionResult<LezioneDto>> Presenze(int id, List<PresenzaInput> presenze)
    {
        var lezione = await Visibili(db.Lezioni.Include(l => l.Studenti)).FirstOrDefaultAsync(l => l.Id == id);
        if (lezione is null) return NotFound();

        foreach (var p in presenze)
        {
            var iscritto = lezione.Studenti.FirstOrDefault(s => s.StudenteId == p.StudenteId);
            if (iscritto is not null) iscritto.Presente = p.Presente;
        }
        await db.SaveChangesAsync();
        return (await Carica().FirstAsync(l => l.Id == id)).ToDto();
    }

    private IQueryable<Lezione> Carica() => db.Lezioni.AsNoTracking()
        .Include(l => l.Sede)
        .Include(l => l.Dipendente)
        .Include(l => l.Studenti).ThenInclude(s => s.Studente);

    /// <summary>Il professore vede solo le lezioni che tiene.</summary>
    private IQueryable<Lezione> Visibili(IQueryable<Lezione> query)
    {
        if (User.PuoGestire()) return query;
        var dipendenteId = User.DipendenteId();
        return query.Where(l => l.DipendenteId != null && l.DipendenteId == dipendenteId);
    }

    private async Task<ActionResult?> Verifica(LezioneInput input)
    {
        if (input.Fine <= input.Inizio) return ValidationProblem("L'orario di fine deve essere successivo all'inizio.");
        if (input.Fine.Date != input.Inizio.Date) return ValidationProblem("Una lezione deve iniziare e finire nello stesso giorno.");
        if (!await db.Sedi.AnyAsync(s => s.Id == input.SedeId)) return ValidationProblem("Seleziona una sede esistente.");
        if (input.DipendenteId is { } dip && !await db.Dipendenti.AnyAsync(d => d.Id == dip))
            return ValidationProblem("Il professore selezionato non esiste più.");

        var studenti = input.StudentiIds.Distinct().ToList();
        if (await db.Studenti.CountAsync(s => studenti.Contains(s.Id)) != studenti.Count)
            return ValidationProblem("Uno degli studenti selezionati non esiste più.");
        return null;
    }

    private static Lezione Nuova(LezioneInput input, DateTime inizio, DateTime fine, Guid? serie) => new()
    {
        Titolo = input.Titolo.Trim(),
        Inizio = inizio,
        Fine = fine,
        SedeId = input.SedeId,
        DipendenteId = input.DipendenteId,
        Note = input.Note,
        SerieId = serie,
        Studenti = input.StudentiIds.Distinct().Select(id => new LezioneStudente { StudenteId = id }).ToList(),
    };

    /// <summary>Aggiunge/toglie studenti mantenendo le presenze già segnate per chi resta.</summary>
    private static void AggiornaStudenti(Lezione l, List<int> studentiIds)
    {
        var richiesti = studentiIds.ToHashSet();
        l.Studenti.RemoveAll(s => !richiesti.Contains(s.StudenteId));
        foreach (var id in richiesti.Where(id => l.Studenti.All(s => s.StudenteId != id)))
            l.Studenti.Add(new LezioneStudente { StudenteId = id });
    }
}
