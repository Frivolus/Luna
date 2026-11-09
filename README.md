# Luna

Launcher Minecraft pour un groupe prive. Un seul bouton Play, modpack impose, mises a jour silencieuses.

## Installation

1. Telecharger Luna-win-x64.zip depuis la derniere release.
2. Extraire dans un dossier fixe, par exemple C:\Luna.
3. Lancer Luna.exe.

Aucun runtime .NET ni Java n'est necessaire : tout est embarque et le runtime Java du jeu est installe automatiquement.

## Comptes

- Hors-ligne : un pseudo suffit, l'UUID est derive de facon deterministe.
- Microsoft : connexion par device flow dans une fenetre WebView2.

## Modpack

Le contenu du pack vit dans le dossier modpack/. Pour modifier le pack, il suffit d'ajouter, changer ou supprimer des fichiers dedans puis de pousser sur main.

- modpack/pack.json declare la version de Minecraft, le loader, la RAM et les arguments JVM.
- Le workflow modpack heberge les fichiers en assets de la release modpack, supprime les assets devenus inutiles et regenere manifest.json.
- Le launcher applique manifest.json : telechargement par sha1, suppression des fichiers obsoletes.

Pour regenerer le manifeste en local :

    .\tools\make-manifest.ps1 -PackDirectory modpack -BaseUrl "https://github.com/Frivolus/Luna/releases/download/modpack" -OutputPath manifest.json

## Mises a jour du launcher

version.txt porte la version. Un push sur main declenche le workflow release qui publie un exe self-contained et met a jour update.json. Au demarrage, le launcher compare sa version, propose la mise a jour, verifie le sha256, remplace les fichiers et redemarre.

## Modes headless

- Luna.exe --sync : synchronise le pack sans lancer le jeu.
- Luna.exe --launch : synchronise et lance.
- Luna.exe --no-update : demarre sans verifier les mises a jour.

## Build

    dotnet build
    dotnet publish src\Luna\Luna.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish