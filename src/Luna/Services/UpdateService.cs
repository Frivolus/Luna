using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Luna.Services;

public sealed class UpdateInfo
{
    [JsonPropertyName("version")] public string Version { get; init; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
    [JsonPropertyName("sha256")] public string Sha256 { get; init; } = string.Empty;
}

public sealed class UpdateService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(10) };

    public async Task<UpdateInfo?> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var json = await Http.GetStringAsync(AppInfo.UpdateUrl, cancellationToken).ConfigureAwait(false);
            var info = JsonSerializer.Deserialize<UpdateInfo>(json);
            if (info is null || string.IsNullOrWhiteSpace(info.Url) || string.IsNullOrWhiteSpace(info.Sha256))
                return null;

            if (!System.Version.TryParse(info.Version, out var remote))
                return null;

            if (!System.Version.TryParse(AppInfo.Version, out var local))
                return null;

            return remote > local ? info : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> DownloadAsync(UpdateInfo info, CancellationToken cancellationToken)
    {
        var workDirectory = Path.Combine(Path.GetTempPath(), "luna-update-" + Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(workDirectory);

            var archivePath = Path.Combine(workDirectory, "luna.zip");
            var bytes = await Http.GetByteArrayAsync(info.Url, cancellationToken).ConfigureAwait(false);
            await File.WriteAllBytesAsync(archivePath, bytes, cancellationToken).ConfigureAwait(false);

            var actual = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            if (!string.Equals(actual, info.Sha256, StringComparison.OrdinalIgnoreCase))
                return null;

            var extracted = Path.Combine(workDirectory, "files");
            ZipFile.ExtractToDirectory(archivePath, extracted);
            return extracted;
        }
        catch
        {
            return null;
        }
    }

    public void Apply(string extractedDirectory)
    {
        var target = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var script = Path.Combine(Path.GetTempPath(), "luna-update-" + Guid.NewGuid().ToString("N") + ".cmd");
        var processId = Environment.ProcessId;
        var executable = Path.Combine(target, "Luna.exe");

        var lines = new[]
        {
            "@echo off",
            "setlocal",
            ":wait",
            "tasklist /FI \"PID eq " + processId + "\" | findstr \"" + processId + "\" >nul",
            "if not errorlevel 1 (",
            "  timeout /t 1 /nobreak >nul",
            "  goto wait",
            ")",
            "robocopy \"" + extractedDirectory + "\" \"" + target + "\" /E /IS /IT /NFL /NDL /NJH /NJS /R:5 /W:2 >nul",
            "start \"\" \"" + executable + "\"",
            "rmdir /s /q \"" + extractedDirectory + "\"",
            "del \"%~f0\"",
        };

        File.WriteAllLines(script, lines);

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c \"" + script + "\"",
            CreateNoWindow = true,
            UseShellExecute = false,
        });
    }
}