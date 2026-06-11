# Belote Freeze 🃏

Belote classique mode solo contre 3 IA — Unity 6 (Mobile Android/iOS).

---

## Architecture (Bible Demo V0.1)

```
Assets/Scripts/
├── Core/
│   ├── Card.cs          — Modèle carte : valeurs, ordres, symboles
│   ├── Deck.cs          — Distribution 3+2 officielle
│   ├── Player.cs        — Modèle joueur (humain ou IA)
│   └── Team.cs          — Modèle équipe
├── Rules/
│   ├── RuleEngine.cs    — Les 15 cas officiels (fourniture, coupe, surcoupe, pisse…)
│   ├── TrickManager.cs  — Gestion des plis
│   ├── TrumpManager.cs  — Prise d'atout tours 1 et 2
│   ├── ScoreManager.cs  — Calcul scores, litige, capot, dedans
│   └── BeloteTracker.cs — Belote / Rebelote (cas 6 et 7)
├── AI/
│   └── AIPlayer.cs      — IA Officielle V1 avec mémoire et priorités
└── UI/
    ├── GameManager.cs   — Chef d'orchestre (MonoBehaviour)
    ├── UIManager.cs     — Affichage Unity UI
    ├── CardView.cs      — Composant visuel carte
    └── GameState.cs     — État centralisé
```

---

## Règles implémentées (Bible Rule Engine V1)

| Cas | Règle |
|-----|-------|
| 1 | Fourniture obligatoire |
| 2 | Coupe obligatoire |
| 3 | Surcoupe obligatoire sur adversaire maître |
| 4 | Impossible de surcouper : jouer atout quand même |
| 5 | Partenaire maître : pisse autorisée |
| 6 | Belote : Dame d'atout jouée (annonce auto) |
| 7 | Rebelote : Roi d'atout joué (validation + bonus) |
| 8 | Dernier pli : +10 points |
| 9 | Capot : 8 plis sur 8 (252 pts) |
| 10 | Litige : 81-81, points suspendus |
| 11 | Litiges successifs accumulés |
| 12 | Prise d'atout tour 1 : prendre ou passer |
| 13 | Prise d'atout tour 2 : autre couleur |
| 14 | Nouvelle donne si tout le monde passe |
| 15 | Attribution du pli selon règles d'atout |

---

## IA (Bible IA Officielle V1)

**Priorités :**
1. Respect absolu des règles (cartes autorisées par RuleEngine)
2. Gagner un pli au coût minimal
3. Conserver les gros atouts : Valet, 9, As, 10
4. Préserver les As
5. Préserver les Dix

**Mémoire :** atouts joués, couleurs épuisées par joueur, cartes maîtresses restantes.

**Anti-comportements :** ne jamais couper son partenaire, ne pas jeter un As inutilement, ne pas gaspiller un gros atout.

---

## Setup Unity 6

### 1. Cloner le dépôt
```bash
git clone https://github.com/UrsocSRL/belote.git
```

### 2. Ouvrir dans Unity Hub
- Unity Hub → **Add project from disk**
- Sélectionner le dossier `BeloteFreeze/`
- Ouvrir avec **Unity 6 (6000.x)**

### 3. Créer la scène principale
1. `File > New Scene` → nommer `GameScene`
2. Créer un GameObject vide → `GameManager` → attacher `GameManager.cs`
3. Créer un GameObject vide → `UIManager` → attacher `UIManager.cs`
4. Relier les références dans l'Inspector

### 4. Hiérarchie UI recommandée (Canvas)
```
Canvas (Screen Space - Overlay)
├── ScorePanel
│   ├── ScoreUsText (TMP)
│   └── ScoreThemText (TMP)
├── TrumpIndicator (TMP)
├── PlayerTop (Nord — dos de cartes)
├── PlayerLeft (Ouest — dos de cartes)
├── PlayerRight (Est — dos de cartes)
├── Table (4 slots pour le pli en cours)
├── HumanHand (main du joueur Sud)
├── TrumpPanel (panneau prise d'atout)
├── EndPanel (fin de manche)
├── MessagePanel
└── BeloteAnnouncePanel
```

### 5. Prefabs à créer
- **CardPrefab** : Image (blanc) + TextMeshProUGUI + Button + CardView.cs + CanvasGroup
- **CardBackPrefab** : Image bleue (dos de carte), pas de bouton

### 6. Build Mobile
- `File > Build Settings`
- Plateforme : Android ou iOS
- `Player Settings > Resolution > Default Orientation` → **Auto Rotation**
  (portrait + paysage selon Bible Demo V0.1)

---

## Valeurs des cartes

| Carte | Hors atout | À l'atout |
|-------|-----------|-----------|
| 7     | 0 | 0 |
| 8     | 0 | 0 |
| 9     | 0 | **14** |
| Valet | 2 | **20** |
| Dame  | 3 | 3 |
| Roi   | 4 | 4 |
| 10    | 10 | 10 |
| As    | 11 | 11 |
| **Total** | **30/couleur** | **62** |

Total en jeu : 152 + 10 (dernier pli) = **162 pts**

---

## Tests obligatoires (Bible Demo V0.1)
- [ ] Prise d'atout tour 1
- [ ] Prise d'atout tour 2
- [ ] Nouvelle donne (tout le monde passe)
- [ ] Fourniture obligatoire
- [ ] Coupe obligatoire
- [ ] Surcoupe obligatoire
- [ ] Pisse (partenaire maître)
- [ ] Belote / Rebelote (+20 pts)
- [ ] Capot (252 pts)
- [ ] Litige (81-81)
- [ ] Litiges successifs
- [ ] Calcul du score
- [ ] Attribution du pli
