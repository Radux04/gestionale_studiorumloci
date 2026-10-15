using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Domain.Common;
using StudiorumLoci.Api.Domain.Crm;
using StudiorumLoci.Api.Domain.Dipendenti;
using StudiorumLoci.Api.Domain.Pagamenti;

namespace StudiorumLoci.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // CRM
    public DbSet<Studente> Studenti => Set<Studente>();
    public DbSet<Genitore> Genitori => Set<Genitore>();
    public DbSet<Lead> Lead => Set<Lead>();
    public DbSet<Interazione> Interazioni => Set<Interazione>();

    // Dipendenti
    public DbSet<Dipendente> Dipendenti => Set<Dipendente>();
    public DbSet<Turno> Turni => Set<Turno>();

    // Pagamenti
    public DbSet<Pagamento> Pagamenti => Set<Pagamento>();
    public DbSet<RicevutaFile> RicevuteFile => Set<RicevutaFile>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Schema dedicato: Supabase espone via Data API solo "public", così i dati
        // del gestionale restano raggiungibili esclusivamente tramite questa API.
        b.HasDefaultSchema("gestionale");

        b.Entity<Studente>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(100);
            e.Property(x => x.Cognome).HasMaxLength(100);
            e.HasMany(x => x.Genitori).WithMany(x => x.Studenti).UsingEntity("StudenteGenitore");
        });

        b.Entity<Genitore>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(100);
            e.Property(x => x.Cognome).HasMaxLength(100);
            e.Property(x => x.CodiceFiscale).HasMaxLength(16);
        });

        b.Entity<Lead>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(200);
            e.Property(x => x.Stato).HasConversion<string>();
            e.Property(x => x.Fonte).HasConversion<string>();
            e.HasOne(x => x.Genitore).WithMany().OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Interazione>(e =>
        {
            e.Property(x => x.Tipo).HasConversion<string>();
            // Lo storico contatti appartiene al lead/genitore: si elimina insieme a loro.
            e.HasOne(x => x.Lead).WithMany(x => x.Interazioni).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Genitore).WithMany(x => x.Interazioni).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Dipendente>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(100);
            e.Property(x => x.Cognome).HasMaxLength(100);
            e.Property(x => x.CodiceFiscale).HasMaxLength(16);
            e.Property(x => x.Ruolo).HasConversion<string>();
            e.Property(x => x.Contratto).HasConversion<string>();
            e.Property(x => x.CompensoOrario).HasPrecision(10, 2);
        });

        b.Entity<Turno>(e =>
        {
            e.Ignore(x => x.Ore);
            // Orari "da calendario" (ora locale), non istanti UTC.
            e.Property(x => x.Inizio).HasColumnType("timestamp without time zone");
            e.Property(x => x.Fine).HasColumnType("timestamp without time zone");
        });

        b.Entity<Pagamento>(e =>
        {
            e.Property(x => x.Importo).HasPrecision(10, 2);
            e.Property(x => x.Stato).HasConversion<string>();
            e.Property(x => x.Metodo).HasConversion<string>();
            e.HasOne(x => x.Studente).WithMany(x => x.Pagamenti).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.RicevutaNome).HasMaxLength(255);
            e.Property(x => x.RicevutaTipo).HasMaxLength(100);
        });

        b.Entity<RicevutaFile>(e =>
        {
            e.ToTable("RicevuteFile");
            e.HasKey(x => x.PagamentoId);
            e.HasOne<Pagamento>().WithOne().HasForeignKey<RicevutaFile>(x => x.PagamentoId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var adesso = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatoIl = adesso;
            if (entry.State == EntityState.Modified) entry.Entity.ModificatoIl = adesso;
        }
        return base.SaveChangesAsync(ct);
    }
}
