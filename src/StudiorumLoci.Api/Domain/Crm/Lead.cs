using StudiorumLoci.Api.Domain.Common;

namespace StudiorumLoci.Api.Domain.Crm;

/// <summary>Richiesta di informazioni (es. dal form del sito) non ancora diventata iscrizione.</summary>
public class Lead : Entity
{
    public required string Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Messaggio { get; set; }
    public FonteLead Fonte { get; set; } = FonteLead.Sito;
    public StatoLead Stato { get; set; } = StatoLead.Nuovo;

    /// <summary>Valorizzato quando il lead viene convertito in genitore.</summary>
    public int? GenitoreId { get; set; }
    public Genitore? Genitore { get; set; }

    public List<Interazione> Interazioni { get; set; } = [];
}

public enum FonteLead { Sito, Telefono, Passaparola, Social, Altro }

public enum StatoLead { Nuovo, Contattato, Convertito, Perso }
