using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Crm;

namespace StudiorumLoci.Api.Controllers.Crm;

[ApiController]
[Route("api/crm/studenti")]
public class StudentiController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<StudenteDto>> Elenco(string? cerca, bool soloAttivi = true)
    {
        var query = db.Studenti.Include(s => s.Genitori).AsNoTracking();
        if (soloAttivi) query = query.Where(s => s.Attivo);
        if (!string.IsNullOrWhiteSpace(cerca))
            query = query.Where(s => s.Nome.Contains(cerca) || s.Cognome.Contains(cerca));

        var studenti = await query.OrderBy(s => s.Cognome).ThenBy(s => s.Nome).ToListAsync();
        return studenti.Select(s => s.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudenteDto>> Dettaglio(int id)
    {
        var studente = await db.Studenti.Include(s => s.Genitori).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return studente is null ? NotFound() : studente.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<StudenteDto>> Crea(StudenteInput input)
    {
        var studente = new Studente { Nome = input.Nome, Cognome = input.Cognome };
        await Applica(studente, input);
        db.Studenti.Add(studente);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = studente.Id }, studente.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudenteDto>> Modifica(int id, StudenteInput input)
    {
        var studente = await db.Studenti.Include(s => s.Genitori).FirstOrDefaultAsync(s => s.Id == id);
        if (studente is null) return NotFound();

        await Applica(studente, input);
        await db.SaveChangesAsync();
        return studente.ToDto();
    }

    /// <summary>Disattiva lo studente: lo storico (pagamenti, ecc.) resta consultabile.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Disattiva(int id)
    {
        var studente = await db.Studenti.FindAsync(id);
        if (studente is null) return NotFound();

        studente.Attivo = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task Applica(Studente s, StudenteInput input)
    {
        s.Nome = input.Nome;
        s.Cognome = input.Cognome;
        s.DataNascita = input.DataNascita;
        s.Scuola = input.Scuola;
        s.Classe = input.Classe;
        s.Note = input.Note;
        s.Attivo = input.Attivo;

        if (input.GenitoriIds is not null)
            s.Genitori = await db.Genitori.Where(g => input.GenitoriIds.Contains(g.Id)).ToListAsync();
    }
}
