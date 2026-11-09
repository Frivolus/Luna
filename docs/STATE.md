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
- Publication automatique : workflow release.yml, exe self-contained single-file, release GitHub, update.json
- Auto-update integree (verif au demarrage, hash sha256, swap par script cmd, relance)
- Teste de bout en bout : 0.1.0 installee, 0.2.0 publiee, mise a jour acceptee, exe remplace et relance

## En cours
- rien

## A faire
- Manifeste reel : mods et assets heberges dans les Releases GitHub
- Generation automatique du manifeste depuis un dossier modpack
- Lunar (injecteur) : patch de selection de thread applique mais jamais commite ni teste

## Procedure de release
1. Incrementer version.txt
2. commit + push sur main
3. Le workflow publie le tag vX.Y.Z et met a jour update.json