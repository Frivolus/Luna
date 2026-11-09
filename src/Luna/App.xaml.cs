using System.IO;
using System.Windows;
using Luna.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;

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

        if (e.Args.Contains("--launch"))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _ = RunHeadlessLaunchAsync(e.Args);
            return;
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

        if (!e.Args.Contains("--no-update"))
            _ = CheckUpdatesAsync(window);
    }

    private static string GameDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Luna", "game");

    private static string LogPath => Path.Combine(Path.GetTempPath(), "luna-sync.log");

    private async Task CheckUpdatesAsync(Window owner)
    {
        var service = new UpdateService();
        var info = await service.CheckAsync(CancellationToken.None);
        if (info is null)
            return;

        var answer = owner.Dispatcher.Invoke(() => MessageBox.Show(
            owner,
            $"La version {info.Version} est disponible. L'installer maintenant ?",
            "Mise a jour",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information));

        if (answer != MessageBoxResult.Yes)
            return;

        var directory = await service.DownloadAsync(info, CancellationToken.None);
        if (directory is null)
        {
            owner.Dispatcher.Invoke(() => MessageBox.Show(
                owner, "Le telechargement de la mise a jour a echoue.", "Mise a jour",
                MessageBoxButton.OK, MessageBoxImage.Warning));
            return;
        }

        service.Apply(directory);
        Shutdown(0);
    }

    private async Task RunHeadlessLaunchAsync(string[] args)
    {
        var log = new List<string>();
        var exit = 1;

        try
        {
            var manifests = new ManifestService();
            var loaded = await manifests.LoadAsync(AppInfo.ManifestUrl, CancellationToken.None);
            if (!loaded.Ok || loaded.Value is null)
            {
                log.Add("manifeste: " + (loaded.Error ?? "inconnu"));
            }
            else
            {
                var sync = new FileSyncService();
                var syncResult = await sync.ApplyAsync(loaded.Value, GameDirectory, null, CancellationToken.None);
                log.Add(syncResult.Ok ? "sync ok" : "sync echec: " + syncResult.Error);

                if (syncResult.Ok)
                {
                    var launch = new GameLaunchService();
                    var session = Luna.Models.SessionFactory.Offline("LunaTest");

                    var prepared = await launch.PrepareAsync(loaded.Value, GameDirectory, session,
                        f => log.Add("fichier " + f),
                        (d, t) => { },
                        CancellationToken.None);

                    log.Add(prepared.Ok ? "processus construit" : "echec: " + prepared.Error);

                    if (prepared.Ok && prepared.Process is not null)
                    {
                        var wrapper = new CmlLib.Core.ProcessBuilder.ProcessWrapper(prepared.Process);
                        wrapper.OutputReceived += (_, line) => log.Add("jeu: " + line);
                        wrapper.StartWithEvents();

                        log.Add("jeu demarre, pid " + prepared.Process.Id);
                        exit = 0;

                        if (args.Contains("--wait"))
                        {
                            var code = await wrapper.WaitForExitTaskAsync();
                            log.Add("jeu termine, code " + code);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.Add("exception: " + ex);
        }
        finally
        {
            File.WriteAllLines(Path.Combine(Path.GetTempPath(), "luna-launch.log"), log);
            Shutdown(exit);
        }
    }

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
