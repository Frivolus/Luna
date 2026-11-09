using System.Text.Json.Serialization;

namespace Luna.Models;

public enum LoaderKind
{
    Vanilla,
    Fabric,
    Forge,
    NeoForge
}

public sealed class ManifestFile
{
    [JsonPropertyName("path")] public string Path { get; init; } = "";
    [JsonPropertyName("url")] public string Url { get; init; } = "";
    [JsonPropertyName("sha1")] public string Sha1 { get; init; } = "";
    [JsonPropertyName("size")] public long Size { get; init; }
}

public sealed class Manifest
{
    [JsonPropertyName("schema")] public int Schema { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("mcVersion")] public string McVersion { get; init; } = "";
    [JsonPropertyName("loader")] public LoaderKind Loader { get; init; }
    [JsonPropertyName("loaderVersion")] public string? LoaderVersion { get; init; }
    [JsonPropertyName("ramMb")] public int RamMb { get; init; } = 4096;
    [JsonPropertyName("jvmArgs")] public List<string> JvmArgs { get; init; } = new();
    [JsonPropertyName("files")] public List<ManifestFile> Files { get; init; } = new();
}