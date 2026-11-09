# Etat

## Fait
- Shell WPF + WPF-UI (Mica, dark)
- Manifeste distant + validation stricte
- Compte hors-ligne (UUID MD5 deterministe) + persistance
- Compte Microsoft (device flow WebView2)
- Sync sha1 + nettoyage des obsoletes + mode headless --sync
- Lancement du jeu + runtime Java Mojang automatique
- UI Play + journal in-app
- Support loader Fabric
- Publication automatique + auto-update verifiee de bout en bout
- Modpack versionne + hebergement des assets + generation du manifeste
- Sync verifiee sur les assets de release

## En cours
- rien

## A faire
- Ajouter les vrais mods dans modpack/mods
- Lunar (injecteur) : patch de selection de thread jamais commite ni teste

## Procedures
- Release launcher : incrementer version.txt, commit, push
- Modpack : modifier modpack/, commit, push