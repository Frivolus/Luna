# ETAT DU PROJET

> **Lu par l'agent AVANT toute modification. Mis a jour APRES chaque tache.**

## Position

- Phase : 0 — fondations
- Derniere action : lancement valide, jeu demarre (runtime java auto-installe)
- Prochaine action : cabler Play sur sync + lancement, journal de sortie dans l UI

## Fait

- [x] Depot public cree
- [x] Solution, projet WPF net10.0-windows, .gitignore
- [x] CmlLib.Core 4.1.0
- [x] Regles et memoire de travail

## A faire (ordre impose)

- [x] Coquille WPF : fenetre, selection de compte, Play desactive
- [x] Modele de manifeste + lecture distante et validation
- [x] Compte hors-ligne (pseudo, UUID deterministe, persistance)
- [x] Compte Microsoft (WebView2, restauration silencieuse)
- [x] Synchronisation des mods avec verification sha1
- [x] Lancement via CmlLib (installation, runtime java auto)
- [ ] Journal de sortie, erreurs, progression
- [ ] Publication : installeur + manifeste via GitHub Actions

## Conventions figees

- C# 13, .NET 10, WPF, x64. Aucune dependance externe non validee.
- `PascalCase` membres publics, `_camelCase` champs prives, `async` de bout en bout.
- MVVM leger, pas de logique dans le code-behind.
- Aucun secret commite.
- Un commit = une brique. Messages `type: portee - description`.

## Journal

- 09/11/2026 : depot cree, scaffold, build verte.