# Handoff

## Nature
Launcher Minecraft WPF (.NET 10) nomme Luna, depot public Frivolus/Luna, chemin local C:\dev\Luna.

Objectif : un groupe d'amis installe une fois, clique Play, et recoit le modpack et les mises a jour sans rien faire.

## Contraintes
- Pas d'Electron. UI en WPF avec WPF-UI.
- L'utilisateur ne code pas : un seul bloc PowerShell par reponse, il colle la sortie.
- Garder des blocs courts, sinon le collage se perd.
- Maintenir docs/STATE.md et ce fichier pour survivre a la compaction.

## Architecture
- src/Luna/App.xaml.cs : demarrage, modes headless, verification de mise a jour.
- src/Luna/Services/ManifestService.cs : recuperation et validation du manifeste.
- src/Luna/Services/FileSyncService.cs : telechargement sha1, suppression des obsoletes.
- src/Luna/Services/GameLaunchService.cs : loader, runtime Java, construction du processus.
- src/Luna/Services/UpdateService.cs : controle de version, telechargement verifie, script de remplacement.
- src/Luna/ViewModels/MainViewModel.cs : etat de l'UI, Play.
- modpack/ : source du pack. tools/make-manifest.ps1 le convertit en manifeste.
- .github/workflows/release.yml : publie l'exe et update.json.
- .github/workflows/modpack.yml : heberge les assets et regenere manifest.json.

## Donnees locales
- Jeu : %APPDATA%\Luna\game
- Journaux headless : %TEMP%\luna-sync.log, %TEMP%\luna-launch.log

## Projet parke
C:\dev\Lunar (injecteur C++ Roblox). Un patch de selection de thread y a ete applique et compile mais jamais commite.