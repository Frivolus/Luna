using System.Security.Cryptography;
using System.Text;

namespace Luna.Services;

public static class OfflineIdentity
{
    // UUID hors-ligne de Minecraft : MD5 de "OfflinePlayer:<pseudo>", force en v3.
    public static string UuidFor(string username)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes("OfflinePlayer:" + username));
        hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"{hex[..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..]}";
    }
}