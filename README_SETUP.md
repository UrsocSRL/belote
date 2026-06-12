# Belote Freeze — Guide de démarrage Unity 6

## Prérequis
- Unity 6 (6000.x) avec les modules **Android Build Support** et/ou **iOS Build Support**
- Package **TextMeshPro** (installé via Package Manager si absent)

---

## 1. Cloner et ouvrir

```bash
git clone https://github.com/UrsocSRL/belote.git
```

Dans **Unity Hub** → *Add project from disk* → sélectionner le dossier `BeloteFreeze/`

---

## 2. Installer TextMeshPro

Si Unity affiche des erreurs sur `TextMeshProUGUI` :

1. `Window > Package Manager`
2. Chercher **TextMeshPro** → *Install*
3. `Window > TextMeshPro > Import TMP Essential Resources`

---

## 3. Construire la GameScene automatiquement

> **C'est l'étape clé. Elle crée la scène, tous les prefabs et connecte toutes les références.**

1. Dans Unity, menu **BeloteFreeze > Build GameScene** (`Ctrl+Shift+B`)
2. Attendre quelques secondes
3. La console affiche : `[BeloteFreeze] GameScene construite avec succès !`

La scène `Assets/Scenes/GameScene.unity` est créée et sauvegardée automatiquement.
Les prefabs sont dans `Assets/Prefabs/`.

---

## 4. Lancer le jeu

1. Ouvrir `Assets/Scenes/GameScene.unity`
2. Cliquer sur **Play** ▶
3. La partie démarre immédiatement : distribution, prise d'atout, jeu

---

## 5. Build Android

1. `File > Build Settings`
2. Plateforme : **Android** → *Switch Platform*
3. `Player Settings` est pré-configuré :
   - Bundle ID : `com.belotefreeze.belote`
   - Min SDK : Android 23 (Android 6.0)
   - Target SDK : Android 34
   - Orientation : **Auto Rotation** (portrait + paysage)
   - Résolution de référence : 1080×1920
4. Brancher un device ou configurer un émulateur
5. **Build and Run**

---

## 6. Build iOS

1. `File > Build Settings`
2. Plateforme : **iOS** → *Switch Platform*
3. `Player Settings` :
   - Bundle ID : `com.belotefreeze.belote`
   - Target iOS : 14.0+
   - Orientation : Auto Rotation
4. **Build** → ouvrir le projet Xcode généré → Archive → TestFlight

---

## Structure de la scène (générée par SceneBuilder)

```
Canvas
├── ScorePanel          ← Scores + indicateur atout
├── AITopZone           ← Nord (dos de cartes)
├── AILeftZone          ← Ouest (dos de cartes)
├── AIRightZone         ← Est (dos de cartes)
├── TableZone           ← 4 slots pli central [Sud/Ouest/Nord/Est]
├── HumanHandZone       ← Main du joueur (Sud)
├── InfoBar             ← Message contextuel
├── LastTrickBtn        ← Bouton dernier pli
├── LastTrickPanel      ← Panneau affichage dernier pli
├── TrumpPanel          ← Prise d'atout
├── EndPanel            ← Fin de manche
├── MessagePanel        ← Messages flash
└── BeloteAnnouncePanel ← Annonce Belote/Rebelote
```

---

## Prefabs (générés par SceneBuilder)

| Prefab | Composants | Usage |
|--------|------------|-------|
| `CardPrefab` | Image + TMP + Button + CanvasGroup + CardView | Main du joueur humain |
| `CardBackPrefab` | Image bleue | Dos de cartes des IA |
| `TrickCardPrefab` | Image + TMP + CardView (sans Button) | Cartes posées sur la table |

---

## Règles implémentées (15/15)

Voir [README.md](README.md) pour le détail complet.

---

## Résolution cible mobile

| Format | Résolution |
|--------|-----------|
| Portrait | 1080 × 1920 |
| Portrait+ | 1080 × 2400 |
| Paysage | 1920 × 1080 |

Le `CanvasScaler` est configuré en **Scale With Screen Size** (référence 1080×1920, match 0.5) pour s'adapter automatiquement aux deux orientations.
