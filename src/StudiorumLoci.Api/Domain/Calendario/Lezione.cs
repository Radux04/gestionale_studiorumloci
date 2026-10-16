using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Crm;
using StudiorumLoci.Api.Domain.Dipendenti;
using StudiorumLoci.Api.Domain.Sedi;

namespace StudiorumLoci.Api.Domain.Calendario;

/// <summary>Lezione in calendario: quando, dove, con chi.</summary>
public class Lezione : Entity
{
    /// <summary>Materia o descrizione (es. "Matematica", "Compiti 2ª media").</summary>
    public required string Titolo { get; set; }

    /// <summary>Orari "da calendario" (ora locale), come i turni.</summary>
    public DateTime Inizio { get; set; }
    public DateTime Fine { get; set; }

    public int SedeId { get; set; }
    public Sede Sede { get; set; } = null!;

    /// <summary>Professore che tiene la lezione (facoltativo).</summary>
    public int? DipendenteId { get; set; }
    public Dipendente? Dipendente { get; set; }

    public string? Note { get; set; }

    /// <summary>Identifica le lezioni create insieme come serie settimanale.</summary>
    public Guid? SerieId { get; set; }

    public List<LezioneStudente> Studenti { get; set; } = [];
}

/// <summary>Studente iscritto a una lezione, con l'eventuale presenza.</summary>
public class LezioneStudente
{
    public int LezioneId { get; set; }
    public Lezione Lezione { get; set; } = null!;

    public int StudenteId { get; set; }
    public Studente Studente { get; set; } = null!;

    /// <summary>null = presenza non ancora segnata.</summary>
    public bool? Presente { get; set; }
}
