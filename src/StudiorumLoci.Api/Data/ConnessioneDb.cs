using Npgsql;

namespace StudiorumLoci.Api.Data;

public static class ConnessioneDb
{
    /// <summary>
    /// Costruisce la stringa di connessione Npgsql. Accetta sia il formato "Host=...;Username=..."
    /// sia l'URI "postgresql://utente:password@host:porta/db" fornito da Supabase.
    /// Se <paramref name="password"/> è valorizzata, sovrascrive quella presente nella stringa.
    /// </summary>
    public static string Costruisci(string? connessione, string? password)
    {
        connessione = connessione?.Trim().Trim('"', '\'');
        if (string.IsNullOrWhiteSpace(connessione))
            throw new InvalidOperationException("ConnectionStrings:Gestionale non configurata.");

        var builder = connessione.StartsWith("postgres://") || connessione.StartsWith("postgresql://")
            ? DaUri(new Uri(connessione))
            : new NpgsqlConnectionStringBuilder(connessione);

        if (!string.IsNullOrEmpty(password)) builder.Password = password;
        return builder.ConnectionString;
    }

    private static NpgsqlConnectionStringBuilder DaUri(Uri uri)
    {
        var credenziali = uri.UserInfo.Split(':', 2);
        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/') is { Length: > 0 } db ? db : "postgres",
            Username = Uri.UnescapeDataString(credenziali[0]),
            Password = credenziali.Length > 1 ? Uri.UnescapeDataString(credenziali[1]) : null,
            SslMode = SslMode.Require,
        };
    }
}
