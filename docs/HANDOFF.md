# HANDOFF — memoire de travail agent

> Lu en entier AVANT toute action et apres chaque compression de conversation.
> Ne jamais reecrire ce fichier depuis zero : completer Journal et Prochaine action.

## Protocole de travail

- L'utilisateur ne fait qu'une chose : executer les commandes PowerShell fournies et coller les sorties, erreurs comprises.
- L'agent fournit les commandes, ecrit le code, interprete les erreurs, corrige. L'utilisateur ne code pas et ne choisit pas l'ordre des briques.
- UN SEUL bloc de commande par reponse. Jamais deux scripts dans le meme message.
- Toujours se placer a la racine du repo en debut de bloc.
- Jamais de fichier ecrit a la main par l'utilisateur : tout passe par here-string PowerShell, UTF-8 avec BOM.
- En cas d'erreur : lire la sortie complete, corriger, redonner un bloc. Ne jamais dire "reessaie".
- Ne jamais demander de confirmation pour une brique deja planifiee ici.

## Etat technique

- Depot : Frivolus/Luna (public). Branche main. Chemin local : C:\dev\Luna.
- Launcher Minecraft pour un groupe prive. Windows uniquement.
- Comptes : hors-ligne (pseudo libre) ou Microsoft (device code).
- Bouton Play qui lance une version verrouillee par l'auteur, avec les mods verrouilles.
- Distribution aux amis : mise a jour sans action de leur part.

## Architecture retenue

- .NET 10 / WPF, C#. Solution `Luna.slnx` ou `Luna.sln`, projet `src/Luna`.
- `CmlLib.Core` 4.1.0 pour l'installation, les versions, le lancement et l'auth.
- `CmlLib.Core.Auth.Microsoft` pour le compte Microsoft.
- Manifeste `manifest.json` versionne dans le depot : version MC, loader, liste de mods (url + sha1), RAM, args JVM.
- Assets (jars de mods, configs) dans les GitHub Releases, jamais dans le depot : pas de binaire Mojang commite.
- Etat local dans `%APPDATA%\Luna` : compte, progression de telechargement, cache.
- Flux de mise a jour : lecture du manifeste, comparaison sha1, telechargement des manquants, suppression des obsoletes, puis Play.

## Plan des briques

1. Coquille WPF : fenetre, selection de compte, Play desactive.
2. Modele de manifeste + lecture distante et validation.
3. Compte hors-ligne : pseudo, UUID deterministe, persistance.
4. Compte Microsoft via device code.
5. Synchronisation des fichiers de mods avec verification sha1.
6. Lancement via CmlLib avec le manifeste.
7. Journal de sortie du jeu, gestion des erreurs, barre de progression.
8. Publication : GitHub Actions qui produit un installeur et le manifeste.

## Conventions figees

- C# 13, .NET 10, WPF, x64. Aucune dependance externe non validee.
- `PascalCase` membres publics, `_camelCase` champs prives, `async` de bout en bout.
- MVVM leger, pas de logique dans le code-behind.
- Aucun secret commite.
- Un commit = une brique. Messages `type: portee - description`.
- Patcher, jamais regenerer un fichier existant.

## Decisions

| Date | Decision | Raison |
|---|---|---|
| 09/11/2026 | Depuis zero sur un depot neuf `Luna` | L'executeur Lunar est parque |
| 09/11/2026 | WPF / .NET 10 | Windows uniquement, pas de multiplateforme a porter |
| 09/11/2026 | CmlLib.Core 4.1.0 | Auth, versions et lancement deja couverts, MIT |
| 09/11/2026 | Manifeste dans le depot, assets en Releases | Depot leger, pas de binaire Mojang |
| 09/11/2026 | UUID hors-ligne deterministe | Inventaire et skin stables entre sessions |

## Journal

- 09/11/2026 : depot cree, scaffold WPF, CmlLib.Core ajoute, build verte.
## Mise a jour 09/11/2026

- UI passee sur WPF-UI 4.3.0 : theme sombre, fond Mica, FluentWindow, Card, Button. Pas d'Electron, jamais.
- Manifeste : modele, lecture HTTP, validation stricte (schema, version, RAM, chemins, sha1, https), manifest.json a la racine.
- Compte hors-ligne : UUID v3 MD5 de "OfflinePlayer:<pseudo>", persistance dans %APPDATA%\Luna\account.json, restauration au demarrage.
- Prochain chantier : compte Microsoft (CmlLib.Core.Auth.Microsoft, device code).

## Mise a jour 09/11/2026 (2)

- Sync validee en headless : telechargement, verification sha1, idempotence, suppression des obsoletes dans les dossiers geres (mods, config, resourcepacks, shaderpacks), preservation du reste.
- Mode `--sync` ajoute pour tester sans UI. `StartupUri` retire du XAML : WPF refuse `StartupUri = null`, la fenetre est creee a la main.
- `JsonStringEnumConverter` requis sur `LoaderKind`.
- Prochain chantier : lancement via CmlLib (CreateProcessAsync + MLaunchOption).
