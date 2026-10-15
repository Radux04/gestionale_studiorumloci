using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Crm;
using StudiorumLoci.Api.Domain.Sedi;

namespace StudiorumLoci.Api.Controllers.Crm;

[ApiController]
[Route("api/crm/studenti")]
public class StudentiController(AppDbContext db) : ControllerBase
{
    /// <param name="sedeId">Filtra per sede; 0 = studenti senza sede assegnata.</param>
    [HttpGet]
    public async Task<IEnumerable<StudenteDto>> Elenco(string? cerca, int? sedeId, bool soloAttivi = true)
    {
        var query = db.Studenti.Include(s => s.Genitori).Include(s => s.Sede).AsNoTracking();
        if (soloAttivi) query = query.Where(s => s.Attivo);
        if (sedeId == 0) query = query.Where(s => s.SedeId == null);
        else if (sedeId is not null) query = query.Where(s => s.SedeId == sedeId);
        if (!string.IsNullOrWhiteSpace(cerca))
            query = query.Where(s => s.Nome.Contains(cerca) || s.Cognome.Contains(cerca));

        var studenti = await query.OrderBy(s => s.Cognome).ThenBy(s => s.Nome).ToListAsync();
        return studenti.Select(s => s.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudenteDto>> Dettaglio(int id)
    {
        var studente = await db.Studenti.Include(s => s.Genitori).Include(s => s.Sede).AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        return studente is null ? NotFound() : studente.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<StudenteDto>> Crea(StudenteInput input)
    {
        var sede = await db.Sedi.FindAsync(input.SedeId);
        if (sede is null) return ValidationProblem("Seleziona una sede esistente.");

        var studente = new Studente { Nome = input.Nome, Cognome = input.Cognome };
        await Applica(studente, input, sede);
        db.Studenti.Add(studente);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = studente.Id }, studente.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudenteDto>> Modifica(int id, StudenteInput input)
    {
        var sede = await db.Sedi.FindAsync(input.SedeId);
        if (sede is null) return ValidationProblem("Seleziona una sede esistente.");

        var studente = await db.Studenti.Include(s => s.Genitori).FirstOrDefaultAsync(s => s.Id == id);
        if (studente is null) return NotFound();

        await Applica(studente, input, sede);
        await db.SaveChangesAsync();
        return studente.ToDto();
    }

    /// <summary>Disattiva lo studente: lo storico (pagamenti, ecc.) resta consultabile.</summary>
    [HttpPost("{id:int}/disattiva")]
    public async Task<IActionResult> Disattiva(int id)
    {
        var studente = await db.Studenti.FindAsync(id);
        if (studente is null) return NotFound();

        studente.Attivo = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Elimina definitivamente lo studente. Non consentito se ha pagamenti registrati.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id)
    {
        var studente = await db.Studenti.FindAsync(id);
        if (studente is null) return NotFound();

        var pagamenti = await db.Pagamenti.CountAsync(p => p.StudenteId == id);
        if (pagamenti > 0)
            return Conflict($"{studente.Nome} {studente.Cognome} ha {pagamenti} pagamenti registrati: " +
                "eliminali prima dalla pagina Pagamenti, oppure disattiva lo studente per conservarne lo storico.");

        db.Studenti.Remove(studente);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task Applica(Studente s, StudenteInput input, Sede sede)
    {
        s.Nome = input.Nome;
        s.Cognome = input.Cognome;
        s.Sede = sede;
        s.DataNascita = input.DataNascita;
        s.Scuola = input.Scuola;
        s.Classe = input.Classe;
        s.Note = input.Note;
        s.Attivo = input.Attivo;

        if (input.GenitoriIds is not null)
            s.Genitori = await db.Genitori.Where(g => input.GenitoriIds.Contains(g.Id)).ToListAsync();
    }
}
