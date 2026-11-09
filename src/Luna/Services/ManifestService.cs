using System.Net.Http;
using System.Text.Json;
using Luna.Models;

namespace Luna.Services;

public sealed record ManifestResult(Manifest? Value, string? Error)
{
    public bool Ok => Value is not null;

    public static ManifestResult Success(Manifest value) => new(value, null);
    public static ManifestResult Failure(string error) => new(null, error);
}

public sealed class ManifestService
{
    public const int SupportedSchema = 1;

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(20) };

    public async Task<ManifestResult> LoadAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return ManifestResult.Failure($"manifeste inaccessible ({(int)response.StatusCode})");

            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            var manifest = await JsonSerializer
                .DeserializeAsync<Manifest>(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (manifest is null) return ManifestResult.Failure("manifeste illisible");

            var error = Validate(manifest);
            return error is null ? ManifestResult.Success(manifest) : ManifestResult.Failure(error);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ManifestResult.Failure("manifeste injoignable : " + ex.Message);
        }
    }

    private static string? Validate(Manifest manifest)
    {
        if (manifest.Schema != SupportedSchema)
            return $"schema {manifest.Schema} non supporte (attendu {SupportedSchema})";

        if (string.IsNullOrWhiteSpace(manifest.McVersion))
            return "version Minecraft absente";

        if (manifest.RamMb is < 1024 or > 32768)
            return "RAM hors limites";

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in manifest.Files)
        {
            if (string.IsNullOrWhiteSpace(file.Path))
                return "chemin de fichier vide";

            if (file.Path.Contains("..", StringComparison.Ordinal))
                return "chemin relatif suspect : " + file.Path;

            if (!seen.Add(file.Path))
                return "chemin duplique : " + file.Path;

            if (file.Sha1.Length != 40 || !file.Sha1.All(Uri.IsHexDigit))
                return "sha1 invalide : " + file.Path;

            if (!Uri.TryCreate(file.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                return "url invalide : " + file.Path;
        }

        return null;
    }
}