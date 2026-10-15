using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Sedi;

namespace StudiorumLoci.Api.Controllers;

[ApiController]
[Route("api/sedi")]
public class SediController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<SedeDto>> Elenco(string? regione, bool soloAttive = false)
    {
        var query = db.Sedi.AsNoTracking();
        if (soloAttive) query = query.Where(s => s.Attiva);
        if (!string.IsNullOrWhiteSpace(regione)) query = query.Where(s => s.Regione == regione);

        var sedi = await query
            .OrderBy(s => s.Regione).ThenBy(s => s.Comune).ThenBy(s => s.Nome)
            .Select(s => new { Sede = s, Studenti = s.Studenti.Count(x => x.Attivo), Dipendenti = s.Dipendenti.Count(x => x.Attivo) })
            .ToListAsync();
        return sedi.Select(x => x.Sede.ToDto(x.Studenti, x.Dipendenti));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SedeDto>> Dettaglio(int id)
    {
        var sede = await db.Sedi.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new { Sede = s, Studenti = s.Studenti.Count(x => x.Attivo), Dipendenti = s.Dipendenti.Count(x => x.Attivo) })
            .FirstOrDefaultAsync();
        return sede is null ? NotFound() : sede.Sede.ToDto(sede.Studenti, sede.Dipendenti);
    }

    [HttpPost]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<ActionResult<SedeDto>> Crea(SedeInput input)
    {
        if (Verifica(input) is { } errore) return errore;

        var sede = new Sede { Nome = input.Nome, Comune = input.Comune, Provincia = input.Provincia, Regione = input.Regione };
        Applica(sede, input);
        db.Sedi.Add(sede);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = sede.Id }, sede.ToDto(0, 0));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<ActionResult<SedeDto>> Modifica(int id, SedeInput input)
    {
        if (Verifica(input) is { } errore) return errore;

        var sede = await db.Sedi.FindAsync(id);
        if (sede is null) return NotFound();

        Applica(sede, input);
        await db.SaveChangesAsync();
        return sede.ToDto(
            await db.Studenti.CountAsync(s => s.SedeId == id && s.Attivo),
            await db.Dipendenti.CountAsync(d => d.Attivo && d.Sedi.Any(s => s.Id == id)));
    }

    /// <summary>Elimina la sede. Non consentito se ha studenti assegnati (anche non attivi).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Ruoli.Gestione)]
    public async Task<IActionResult> Elimina(int id)
    {
        var sede = await db.Sedi.FindAsync(id);
        if (sede is null) return NotFound();

        var studenti = await db.Studenti.CountAsync(s => s.SedeId == id);
        if (studenti > 0)
            return Conflict($"La sede {sede.Nome} ha {studenti} studenti assegnati: spostali in un'altra sede " +
                "oppure disattiva la sede.");

        db.Sedi.Remove(sede);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private ActionResult? Verifica(SedeInput input) =>
        Regioni.Elenco.Contains(input.Regione) ? null : ValidationProblem($"Regione non valida: {input.Regione}.");

    private static void Applica(Sede s, SedeInput input)
    {
        s.Nome = input.Nome.Trim();
        s.Indirizzo = input.Indirizzo;
        s.Cap = input.Cap;
        s.Comune = input.Comune.Trim();
        s.Provincia = input.Provincia.ToUpperInvariant();
        s.Regione = input.Regione;
        s.Telefono = input.Telefono;
        s.Email = input.Email;
        s.Note = input.Note;
        s.Attiva = input.Attiva;
    }
}
