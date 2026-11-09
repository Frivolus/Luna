using System.IO;
using System.Text.Json;
using Luna.Models;

namespace Luna.Services;

public sealed class AccountStore
{
    private static readonly string Folder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Luna");

    private static readonly string FilePath = Path.Combine(Folder, "account.json");

    public Account? Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return null;
            return JsonSerializer.Deserialize<Account>(File.ReadAllText(FilePath));
        }
        catch
        {
            return null;
        }
    }

    public void Save(Account account)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(account));
        }
        catch
        {
        }
    }
}