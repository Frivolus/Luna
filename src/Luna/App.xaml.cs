using System.IO;
using System.Windows;
using Luna.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Luna;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica, true);
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "luna-theme.log"), ex.ToString());
        }

        if (e.Args.Contains("--sync"))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _ = RunHeadlessSyncAsync();
            return;
        }

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    private static string GameDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Luna", "game");

    private static string LogPath => Path.Combine(Path.GetTempPath(), "luna-sync.log");

    private async Task RunHeadlessSyncAsync()
    {
        var log = new List<string>();
        var exit = 1;

        try
        {
            var manifests = new ManifestService();
            var result = await manifests.LoadAsync(AppInfo.ManifestUrl, CancellationToken.None);

            if (!result.Ok || result.Value is null)
            {
                log.Add("manifeste: " + (result.Error ?? "inconnu"));
            }
            else
            {
                log.Add($"manifeste ok, {result.Value.Files.Count} fichier(s)");

                var progress = new Progress<SyncProgress>(p => log.Add($"{p.Done}/{p.Total} {p.Current}"));
                var sync = new FileSyncService();
                var syncResult = await sync.ApplyAsync(result.Value, GameDirectory, progress, CancellationToken.None);

                log.Add(syncResult.Ok ? "synchronisation ok" : "echec: " + syncResult.Error);
                exit = syncResult.Ok ? 0 : 1;
            }
        }
        catch (Exception ex)
        {
            log.Add("exception: " + ex);
        }
        finally
        {
            File.WriteAllLines(LogPath, log);
            Shutdown(exit);
        }
    }
}