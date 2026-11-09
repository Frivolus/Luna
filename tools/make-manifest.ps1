param(
    [Parameter(Mandatory)][string]$PackDirectory,
    [Parameter(Mandatory)][string]$BaseUrl,
    [Parameter(Mandatory)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$packFile = Join-Path $PackDirectory 'pack.json'
if (-not (Test-Path $packFile)) { throw "pack.json introuvable" }

$pack = Get-Content $packFile -Raw | ConvertFrom-Json
foreach ($field in 'name', 'mcVersion', 'loader') {
    if ([string]::IsNullOrWhiteSpace($pack.$field)) { throw "champ manquant dans pack.json: $field" }
}

$baseFull = (Resolve-Path $PackDirectory).Path.TrimEnd('\') + '\'
$files = @()

Get-ChildItem -Path $PackDirectory -Recurse -File |
    Where-Object { $_.Name -ne 'pack.json' } |
    Sort-Object FullName |
    ForEach-Object {
        $relative = $_.FullName.Substring($baseFull.Length) -replace '\\', '/'
        $sha1 = (Get-FileHash $_.FullName -Algorithm SHA1).Hash.ToLower()

        $files += [ordered]@{
            path = $relative
            url  = "$BaseUrl/$sha1-$($_.Name)"
            sha1 = $sha1
            size = $_.Length
        }
    }

$manifest = [ordered]@{
    schema        = 1
    name          = $pack.name
    mcVersion     = $pack.mcVersion
    loader        = $pack.loader
    loaderVersion = $pack.loaderVersion
    ramMb         = if ($pack.ramMb) { $pack.ramMb } else { 4096 }
    jvmArgs       = @($pack.jvmArgs)
    files         = $files
}

$json = $manifest | ConvertTo-Json -Depth 6
[IO.File]::WriteAllText((Join-Path (Get-Location) $OutputPath), $json, (New-Object System.Text.UTF8Encoding($false)))

"manifeste genere : $($files.Count) fichier(s)"