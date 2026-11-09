using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using Luna.Models;

namespace Luna.Services;

public sealed record SyncProgress(int Done, int Total, string Current);

public sealed record SyncResult(bool Ok, string? Error)
{
    public static SyncResult Success() => new(true, null);
    public static SyncResult Failure(string error) => new(false, error);
}

public sealed class FileSyncService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(15) };

    private static readonly string[] ManagedRoots = { "mods", "config", "resourcepacks", "shaderpacks" };

    public async Task<SyncResult> ApplyAsync(Manifest manifest, string gameDirectory,
                                             IProgress<SyncProgress>? progress, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(gameDirectory);
        Directory.CreateDirectory(root);

        var files = manifest.Files;
        var total = files.Count;
        var done = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new SyncProgress(done, total, file.Path));

            var target = Path.GetFullPath(Path.Combine(root, file.Path));
            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return SyncResult.Failure("chemin hors du dossier de jeu : " + file.Path);

            if (File.Exists(target) && await HashMatchesAsync(target, file.Sha1, cancellationToken))
            {
                done++;
                continue;
            }

            var parent = Path.GetDirectoryName(target);
            if (parent is not null) Directory.CreateDirectory(parent);

            var temp = target + ".part";

            try
            {
                using var response = await Http
                    .GetAsync(file.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return SyncResult.Failure($"telechargement refuse ({(int)response.StatusCode}) : {file.Path}");

                await using (var stream = File.Create(temp))
                    await response.Content.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);

                if (!await HashMatchesAsync(temp, file.Sha1, cancellationToken))
                {
                    File.Delete(temp);
                    return SyncResult.Failure("sha1 incorrect : " + file.Path);
                }

                File.Move(temp, target, true);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return SyncResult.Failure($"{file.Path} : {ex.Message}");
            }

            done++;
        }

        RemoveObsolete(root, files);
        progress?.Report(new SyncProgress(total, total, "termine"));
        return SyncResult.Success();
    }

    private static async Task<bool> HashMatchesAsync(string path, string expected, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA1.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant() == expected;
    }

    private static void RemoveObsolete(string root, List<ManifestFile> files)
    {
        var keep = new HashSet<string>(
            files.Select(f => f.Path.Replace('/', '\\')),
            StringComparer.OrdinalIgnoreCase);

        foreach (var managed in ManagedRoots)
        {
            var directory = Path.Combine(root, managed);
            if (!Directory.Exists(directory)) continue;

            foreach (var existing in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(root, existing);
                if (keep.Contains(relative)) continue;

                try
                {
                    File.Delete(existing);
                }
                catch
                {
                }
            }
        }
    }
}