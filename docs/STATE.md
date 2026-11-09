# Etat

## Fait
- Shell WPF + WPF-UI (Mica, dark)
- Manifeste distant + validation stricte
- Compte hors-ligne (UUID MD5 deterministe) + persistance
- Compte Microsoft (device flow WebView2)
- Sync sha1 + nettoyage des obsoletes + mode headless --sync
- Lancement du jeu + runtime Java Mojang automatique
- UI Play + journal in-app
- Support loader Fabric (FabricInstaller, namespace CmlLib.Core.ModLoaders.FabricMC)
- Teste de bout en bout : Fabric 0.16.5 / 1.21.1 installe, processus lance, jeu demarre

## En cours
- rien

## A faire
- Publication : GitHub Actions produisant installeur + manifeste
- Manifeste reel : mods et assets heberges dans les Releases GitHub
- Lunar (injecteur) : patch de selection de thread applique mais jamais commite ni teste