using System.Reflection;

namespace Luna;

public static class AppInfo
{
    public static string Version { get; } = ReadVersion();

    public const string ManifestUrl = "https://raw.githubusercontent.com/Frivolus/Luna/main/manifest.json";
    public const string UpdateUrl = "https://raw.githubusercontent.com/Frivolus/Luna/main/update.json";

    private static string ReadVersion()
    {
        var assembly = typeof(AppInfo).Assembly;

        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+');
            return plus > 0 ? informational[..plus] : informational;
        }

        return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    }
}