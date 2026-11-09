using System.Diagnostics;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using Luna.Models;

namespace Luna.Services;

public sealed record LaunchResult(bool Ok, string? Error, Process? Process)
{
    public static LaunchResult Success(Process process) => new(true, null, process);
    public static LaunchResult Failure(string error) => new(false, error, null);
}

public sealed class GameLaunchService
{
    public async Task<LaunchResult> PrepareAsync(Manifest manifest, string gameDirectory, MSession session,
                                                 Action<string> onFile, Action<int, int> onFiles,
                                                 CancellationToken cancellationToken)
    {
        var path = new MinecraftPath(gameDirectory);
        var launcher = new MinecraftLauncher(path);

        launcher.FileProgressChanged += (_, args) =>
        {
            onFiles(args.ProgressedTasks, args.TotalTasks);
            onFile(args.Name ?? string.Empty);
        };

        var version = await launcher.GetVersionAsync(manifest.McVersion, cancellationToken)
            .ConfigureAwait(false);

        // InstallAsync extrait aussi le runtime Java fourni par Mojang.
        await launcher.InstallAsync(version, cancellationToken).ConfigureAwait(false);

        var java = launcher.GetJavaPath(version);
        if (string.IsNullOrEmpty(java))
            java = launcher.GetDefaultJavaPath();

        if (string.IsNullOrEmpty(java))
            return LaunchResult.Failure("Java introuvable, meme apres installation du runtime.");

        var option = new MLaunchOption
        {
            Session = session,
            JavaPath = java,
            MaximumRamMb = manifest.RamMb,
            MinimumRamMb = Math.Min(1024, manifest.RamMb),
            GameLauncherName = "Luna",
            GameLauncherVersion = AppInfo.Version,
            ExtraJvmArguments = manifest.JvmArgs.Select(a => new MArgument(a)).ToArray(),
        };

        var process = launcher.BuildProcess(version, option);

        return LaunchResult.Success(process);
    }
}