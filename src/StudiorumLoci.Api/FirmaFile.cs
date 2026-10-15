using System.Text;

namespace StudiorumLoci.Api;

/// <summary>
/// Riconosce il tipo di un file dai primi byte ("magic number"), senza fidarsi
/// dell'estensione o del Content-Type dichiarati dal browser.
/// </summary>
public static class FirmaFile
{
    private static ReadOnlySpan<byte> Jpeg => [0xFF, 0xD8, 0xFF];
    private static ReadOnlySpan<byte> Png => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static string? Riconosci(ReadOnlySpan<byte> dati)
    {
        if (dati.StartsWith("%PDF-"u8)) return "application/pdf";
        if (dati.StartsWith(Jpeg)) return "image/jpeg";
        if (dati.StartsWith(Png)) return "image/png";
        if (dati.Length >= 12 && dati.StartsWith("RIFF"u8) && dati[8..12].SequenceEqual("WEBP"u8)) return "image/webp";

        // HEIC/HEIF (foto iPhone): box "ftyp" all'offset 4 seguito dal brand.
        if (dati.Length >= 12 && dati[4..8].SequenceEqual("ftyp"u8))
        {
            var brand = Encoding.ASCII.GetString(dati[8..12]);
            if (brand is "heic" or "heix" or "hevc" or "hevx") return "image/heic";
            if (brand is "mif1" or "msf1") return "image/heif";
        }
        return null;
    }
}
