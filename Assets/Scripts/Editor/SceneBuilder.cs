using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using BeloteFreeze.Core;
using BeloteFreeze.UI;

/// <summary>
/// Script Editor : génère toute la GameScene Belote Freeze en un clic.
/// Menu Unity : BeloteFreeze > Build GameScene
///
/// Tâche 2 — Création de la GameScene :
///   Canvas principal, Zone joueur, Zone IA gauche/haut/droite,
///   Zone pli central, Zone score, Zone annonces, Boutons d'action.
///
/// Tâche 3 — Création des Prefabs :
///   CardPrefab, CardBackPrefab, TrickCardPrefab.
/// </summary>
public static class SceneBuilder
{
    // ── Palette de couleurs ───────────────────────────────────────────────────
    static readonly Color COL_BG          = new Color(0.08f, 0.36f, 0.16f);   // vert table
    static readonly Color COL_CARD_BG     = Color.white;
    static readonly Color COL_CARD_BACK   = new Color(0.12f, 0.25f, 0.60f);   // bleu dos
    static readonly Color COL_PANEL       = new Color(0f, 0f, 0f, 0.70f);
    static readonly Color COL_PANEL_LIGHT = new Color(0f, 0f, 0f, 0.50f);
    static readonly Color COL_BTN_GREEN   = new Color(0.17f, 0.48f, 0.17f);
    static readonly Color COL_BTN_RED     = new Color(0.55f, 0.15f, 0.15f);
    static readonly Color COL_SUIT_RED    = new Color(0.80f, 0.10f, 0.10f);
    static readonly Color COL_SUIT_BLACK  = new Color(0.10f, 0.10f, 0.10f);
    static readonly Color COL_GOLD        = new Color(1.00f, 0.84f, 0.00f);
    static readonly Color COL_WHITE_TEXT  = Color.white;

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("BeloteFreeze/Build GameScene %#b")]
    public static void BuildScene()
    {
        // --- Fond de scène ---
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = COL_BG;
        cam.orthographic    = true;

        // --- Canvas principal ---
        var canvasGo = CreateGO("Canvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Zone Score ───────────────────────────────────────────────────────
        var scorePanel = CreatePanel("ScorePanel", canvasGo.transform,
            new Vector2(1080, 80),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -40), COL_PANEL);

        var scoreUsGo  = CreateLabel("ScoreUs",   "0", scorePanel.transform, new Vector2(-200, 0), 28, COL_WHITE_TEXT, FontStyles.Bold);
        var scoreSep   = CreateLabel("ScoreSep",  "—", scorePanel.transform, new Vector2(0, 0),    20, COL_WHITE_TEXT, FontStyles.Normal);
        var scoreThGo  = CreateLabel("ScoreThem", "0", scorePanel.transform, new Vector2(200, 0),  28, COL_WHITE_TEXT, FontStyles.Bold);
        CreateLabel("LabelUs",   "Nous", scorePanel.transform, new Vector2(-200, -24), 14, new Color(0.6f, 0.9f, 0.6f), FontStyles.Normal);
        CreateLabel("LabelThem", "Eux",  scorePanel.transform, new Vector2(200,  -24), 14, new Color(0.9f, 0.6f, 0.6f), FontStyles.Normal);
        var trumpIndGo = CreateLabel("TrumpIndicator", "", scorePanel.transform, new Vector2(0, 16), 18, COL_GOLD, FontStyles.Bold);

        // ── Zone IA Haut (Nord) ───────────────────────────────────────────────
        var aiTopZone = CreatePanel("AITopZone", canvasGo.transform,
            new Vector2(700, 120),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -130), Color.clear);
        CreateLabel("LabelNord", "Nord", aiTopZone.transform, new Vector2(0, 40), 14, new Color(0.8f, 1f, 0.8f), FontStyles.Normal);
        var aiTopHand = CreateContainer("AITopHand", aiTopZone.transform, new Vector2(0, 0), new Vector2(660, 80), horizontal: true, spacing: 4);

        // ── Zone IA Gauche (Ouest) ────────────────────────────────────────────
        var aiLeftZone = CreatePanel("AILeftZone", canvasGo.transform,
            new Vector2(120, 400),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(60, 0), Color.clear);
        CreateLabel("LabelOuest", "Ouest", aiLeftZone.transform, new Vector2(0, 160), 14, new Color(0.8f, 1f, 0.8f), FontStyles.Normal);
        var aiLeftHand = CreateContainer("AILeftHand", aiLeftZone.transform, new Vector2(0, 0), new Vector2(80, 360), horizontal: false, spacing: 4);

        // ── Zone IA Droite (Est) ──────────────────────────────────────────────
        var aiRightZone = CreatePanel("AIRightZone", canvasGo.transform,
            new Vector2(120, 400),
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-60, 0), Color.clear);
        CreateLabel("LabelEst", "Est", aiRightZone.transform, new Vector2(0, 160), 14, new Color(0.8f, 1f, 0.8f), FontStyles.Normal);
        var aiRightHand = CreateContainer("AIRightHand", aiRightZone.transform, new Vector2(0, 0), new Vector2(80, 360), horizontal: false, spacing: 4);

        // ── Zone Pli Central (4 slots) ────────────────────────────────────────
        var tableZone = CreatePanel("TableZone", canvasGo.transform,
            new Vector2(400, 340),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), Color.clear);

        // Slots : [0]=Sud [1]=Ouest [2]=Nord [3]=Est
        // Positions dans le carré central
        var slotSud   = CreateSlot("SlotSud",   tableZone.transform, new Vector2(  0, -100));
        var slotOuest = CreateSlot("SlotOuest", tableZone.transform, new Vector2(-130,  0));
        var slotNord  = CreateSlot("SlotNord",  tableZone.transform, new Vector2(  0,  100));
        var slotEst   = CreateSlot("SlotEst",   tableZone.transform, new Vector2( 130,  0));

        // ── Zone Main Joueur (Sud) ────────────────────────────────────────────
        var handZone = CreatePanel("HumanHandZone", canvasGo.transform,
            new Vector2(1080, 200),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 110), Color.clear);
        var humanHand = CreateContainer("HumanHand", handZone.transform, new Vector2(0, 0), new Vector2(1020, 180), horizontal: true, spacing: 6);

        // ── Zone Info Bar ─────────────────────────────────────────────────────
        var infoBar = CreatePanel("InfoBar", canvasGo.transform,
            new Vector2(800, 40),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 310), COL_PANEL_LIGHT);
        var infoText = CreateLabel("InfoBarText", "À vous de jouer", infoBar.transform, Vector2.zero, 16, COL_WHITE_TEXT, FontStyles.Normal);

        // ── Bouton Dernier Pli ────────────────────────────────────────────────
        var lastTrickBtnGo = CreateButton("LastTrickBtn", canvasGo.transform,
            "Dernier pli", new Vector2(160, 44),
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(90, 310), COL_PANEL);

        // ── Panneau Dernier Pli ───────────────────────────────────────────────
        var lastTrickPanel = CreatePanel("LastTrickPanel", canvasGo.transform,
            new Vector2(500, 180),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, COL_PANEL);
        lastTrickPanel.SetActive(false);
        var lastTrickText = CreateLabel("LastTrickText", "", lastTrickPanel.transform, Vector2.zero, 16, COL_WHITE_TEXT, FontStyles.Normal);

        // ── Panneau Prise d'Atout ─────────────────────────────────────────────
        var trumpPanel = CreatePanel("TrumpPanel", canvasGo.transform,
            new Vector2(500, 320),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, COL_PANEL);
        trumpPanel.SetActive(false);

        var trumpTitle   = CreateLabel("TrumpPanelTitle", "Prendre ?", trumpPanel.transform, new Vector2(0, 110), 20, COL_WHITE_TEXT, FontStyles.Bold);
        var trumpCardTxt = CreateLabel("TrumpCardText",   "A♠",        trumpPanel.transform, new Vector2(0,  50), 36, COL_SUIT_BLACK,  FontStyles.Bold);
        var trumpCardImg = trumpPanel.transform.Find("TrumpCardText").gameObject.AddComponent<Image>();

        var takeBtnGo  = CreateButton("TakeButton",  trumpPanel.transform, "Prendre", new Vector2(180, 56), pivot: new Vector2(0.5f,0.5f), anchor: new Vector2(0.5f,0.5f), pos: new Vector2(-100, -40), color: COL_BTN_GREEN);
        var passBtnGo  = CreateButton("PassButton",  trumpPanel.transform, "Passer",  new Vector2(180, 56), pivot: new Vector2(0.5f,0.5f), anchor: new Vector2(0.5f,0.5f), pos: new Vector2( 100, -40), color: COL_BTN_RED);

        // Container boutons couleurs (tour 2)
        var suitContainer = CreatePanel("SuitButtonsContainer", trumpPanel.transform,
            new Vector2(440, 70),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -90), Color.clear);
        suitContainer.SetActive(false);
        var suitBtns = new Button[4];
        string[] suitSymbols = { "♠", "♥", "♦", "♣" };
        Color[]  suitColors  = { COL_SUIT_BLACK, COL_SUIT_RED, COL_SUIT_RED, COL_SUIT_BLACK };
        for (int i = 0; i < 4; i++)
        {
            var sb = CreateButton($"SuitBtn_{suitSymbols[i]}", suitContainer.transform,
                suitSymbols[i], new Vector2(80, 64),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-150 + i * 100, 0), COL_CARD_BG);
            var sbtmp = sb.GetComponentInChildren<TextMeshProUGUI>();
            if (sbtmp != null) sbtmp.color = suitColors[i];
            suitBtns[i] = sb.GetComponent<Button>();
        }

        // ── Panneau Fin de Manche ─────────────────────────────────────────────
        var endPanel = CreatePanel("EndPanel", canvasGo.transform,
            new Vector2(600, 500),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, COL_PANEL);
        endPanel.SetActive(false);
        var endTitle   = CreateLabel("EndTitleText",   "Fin de manche", endPanel.transform, new Vector2(0, 170), 26, COL_GOLD,       FontStyles.Bold);
        var endDetails = CreateLabel("EndDetailsText", "",              endPanel.transform, new Vector2(0,  30), 16, COL_WHITE_TEXT, FontStyles.Normal);
        var nextBtnGo  = CreateButton("NextHandButton", endPanel.transform, "Nouvelle manche",
            new Vector2(280, 60),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -180), COL_BTN_GREEN);

        // ── Panneau Message Flash ─────────────────────────────────────────────
        var msgPanel = CreatePanel("MessagePanel", canvasGo.transform,
            new Vector2(600, 100),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 100), COL_PANEL);
        msgPanel.SetActive(false);
        var msgText = CreateLabel("MessageText", "", msgPanel.transform, Vector2.zero, 18, COL_WHITE_TEXT, FontStyles.Normal);

        // ── Panneau Belote/Rebelote ───────────────────────────────────────────
        var belotePanel = CreatePanel("BeloteAnnouncePanel", canvasGo.transform,
            new Vector2(400, 80),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 200), new Color(1f, 0.84f, 0f, 0.95f));
        belotePanel.SetActive(false);
        var beloteText = CreateLabel("BeloteAnnounceText", "Belote !", belotePanel.transform, Vector2.zero, 22, COL_SUIT_BLACK, FontStyles.Bold);

        // ─────────────────────────────────────────────────────────────────────
        // PREFABS — Tâche 3
        // ─────────────────────────────────────────────────────────────────────
        string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        BuildCardPrefab(prefabFolder);
        BuildCardBackPrefab(prefabFolder);
        BuildTrickCardPrefab(prefabFolder);

        // ─────────────────────────────────────────────────────────────────────
        // GAMEOBJECTS LOGIQUE
        // ─────────────────────────────────────────────────────────────────────
        var gmGo = CreateGO("GameManager");
        var gm   = gmGo.AddComponent<GameManager>();

        var uimGo = CreateGO("UIManager");
        var uim   = uimGo.AddComponent<UIManager>();

        // Brancher les références GameManager → UIManager
        gm.UIManager = uim;

        // Brancher les références UIManager
        uim.ScoreUsText          = scoreUsGo.GetComponent<TextMeshProUGUI>();
        uim.ScoreThemText        = scoreThGo.GetComponent<TextMeshProUGUI>();
        uim.TrumpIndicatorText   = trumpIndGo.GetComponent<TextMeshProUGUI>();

        uim.TrumpPanel           = trumpPanel;
        uim.TrumpPanelTitle      = trumpTitle.GetComponent<TextMeshProUGUI>();
        uim.TrumpCardText        = trumpCardTxt.GetComponent<TextMeshProUGUI>();
        uim.TakeButton           = takeBtnGo.GetComponent<Button>();
        uim.PassButton           = passBtnGo.GetComponent<Button>();
        uim.SuitButtonsContainer = suitContainer;
        uim.SuitButtons          = suitBtns;

        uim.HumanHandContainer   = humanHand.transform;
        uim.AIHandContainers     = new Transform[]
        {
            aiLeftHand.transform,
            aiTopHand.transform,
            aiRightHand.transform
        };

        // Charger les prefabs depuis Assets/Prefabs
        uim.CardPrefab      = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabFolder}/CardPrefab.prefab");
        uim.CardBackPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabFolder}/CardBackPrefab.prefab");
        uim.TrickCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabFolder}/TrickCardPrefab.prefab");

        uim.TrickSlots = new Transform[]
        {
            slotSud.transform,
            slotOuest.transform,
            slotNord.transform,
            slotEst.transform
        };

        uim.MessagePanel         = msgPanel;
        uim.MessageText          = msgText.GetComponent<TextMeshProUGUI>();
        uim.BeloteAnnouncePanel  = belotePanel;
        uim.BeloteAnnounceText   = beloteText.GetComponent<TextMeshProUGUI>();

        uim.EndPanel             = endPanel;
        uim.EndTitleText         = endTitle.GetComponent<TextMeshProUGUI>();
        uim.EndDetailsText       = endDetails.GetComponent<TextMeshProUGUI>();
        uim.NextHandButton       = nextBtnGo.GetComponent<Button>();

        uim.InfoBarText          = infoText.GetComponent<TextMeshProUGUI>();
        uim.LastTrickButton      = lastTrickBtnGo.GetComponent<Button>();
        uim.LastTrickPanel       = lastTrickPanel;
        uim.LastTrickText        = lastTrickText.GetComponent<TextMeshProUGUI>();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = CreateGO("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Sauvegarder la scène
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(),
            "Assets/Scenes/GameScene.unity");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[BeloteFreeze] GameScene construite avec succès !");
    }

    // ── Constructeurs de Prefabs (Tâche 3) ───────────────────────────────────

    static void BuildCardPrefab(string folder)
    {
        // CardPrefab : Image + TMP + Button + CanvasGroup + CardView
        var root = CreateGO("CardPrefab");
        root.AddComponent<CanvasGroup>();

        var bg = root.AddComponent<Image>();
        bg.color = Color.white;

        // RectTransform
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(72, 100);

        // TextMeshPro (rang + couleur)
        var textGo = CreateGO("CardText");
        textGo.transform.SetParent(root.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = "A♠";
        tmp.fontSize  = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = COL_SUIT_BLACK;
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        // TrumpHighlight (bordure dorée, désactivée par défaut)
        var highlightGo = CreateGO("TrumpHighlight");
        highlightGo.transform.SetParent(root.transform, false);
        var hlImg = highlightGo.AddComponent<Image>();
        hlImg.color = new Color(1f, 0.84f, 0f, 0.5f);
        var hlRt = highlightGo.GetComponent<RectTransform>();
        hlRt.anchorMin = Vector2.zero;
        hlRt.anchorMax = Vector2.one;
        hlRt.offsetMin = new Vector2(-3, -3);
        hlRt.offsetMax = new Vector2(3, 3);
        highlightGo.SetActive(false);

        // CardView
        var cv = root.AddComponent<CardView>();
        cv.RankText      = tmp;
        cv.SuitText      = tmp;
        cv.CardBackground = bg;
        cv.TrumpHighlight = hlImg;

        // Button
        root.AddComponent<Button>();

        // Sauvegarder
        PrefabUtility.SaveAsPrefabAsset(root, $"{folder}/CardPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    static void BuildCardBackPrefab(string folder)
    {
        // CardBackPrefab : Image bleue, pas de bouton
        var root = CreateGO("CardBackPrefab");
        var rt   = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(72, 100);

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.25f, 0.60f);

        // Motif croix (décoratif)
        var lineGo = CreateGO("Pattern");
        lineGo.transform.SetParent(root.transform, false);
        var lineImg = lineGo.AddComponent<Image>();
        lineImg.color = new Color(1f, 1f, 1f, 0.12f);
        var lineRt = lineGo.GetComponent<RectTransform>();
        lineRt.anchorMin = new Vector2(0.1f, 0.1f);
        lineRt.anchorMax = new Vector2(0.9f, 0.9f);
        lineRt.offsetMin = Vector2.zero;
        lineRt.offsetMax = Vector2.zero;

        PrefabUtility.SaveAsPrefabAsset(root, $"{folder}/CardBackPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    static void BuildTrickCardPrefab(string folder)
    {
        // TrickCardPrefab : même que CardPrefab mais légèrement plus grand, sans Button interactif
        var root = CreateGO("TrickCardPrefab");
        root.AddComponent<CanvasGroup>();
        var bg = root.AddComponent<Image>();
        bg.color = Color.white;

        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 112);

        var textGo = CreateGO("CardText");
        textGo.transform.SetParent(root.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = "A♠";
        tmp.fontSize  = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = COL_SUIT_BLACK;
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        // TrumpHighlight
        var hlGo  = CreateGO("TrumpHighlight");
        hlGo.transform.SetParent(root.transform, false);
        var hlImg = hlGo.AddComponent<Image>();
        hlImg.color = new Color(1f, 0.84f, 0f, 0.6f);
        var hlRt  = hlGo.GetComponent<RectTransform>();
        hlRt.anchorMin = Vector2.zero;
        hlRt.anchorMax = Vector2.one;
        hlRt.offsetMin = new Vector2(-4, -4);
        hlRt.offsetMax = new Vector2(4, 4);
        hlGo.SetActive(false);

        var cv = root.AddComponent<CardView>();
        cv.RankText       = tmp;
        cv.SuitText       = tmp;
        cv.CardBackground = bg;
        cv.TrumpHighlight = hlImg;

        // Pas de Button (cartes sur la table ne sont pas cliquables)

        PrefabUtility.SaveAsPrefabAsset(root, $"{folder}/TrickCardPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    // ── Helpers de création UI ────────────────────────────────────────────────

    static GameObject CreateGO(string name)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        return go;
    }

    static GameObject CreatePanel(string name, Transform parent, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;

        if (color.a > 0.001f)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            // Arrondir les coins (Unity 2020+)
            img.raycastTarget = false;
        }
        return go;
    }

    static GameObject CreateLabel(string name, string text, Transform parent,
        Vector2 anchoredPos, float fontSize, Color color, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = new Vector2(400, 50);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = fontSize;
        tmp.color      = color;
        tmp.fontStyle  = style;
        tmp.alignment  = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return go;
    }

    static GameObject CreateButton(string name, Transform parent, string label,
        Vector2 size, Vector2 pivot, Vector2 anchor, Vector2 pos, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.pivot            = pivot;
        rt.anchorMin        = anchor;
        rt.anchorMax        = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var img = go.AddComponent<Image>();
        img.color = color;

        var btn = go.AddComponent<Button>();
        var cs  = btn.colors;
        cs.highlightedColor = color * 1.2f;
        cs.pressedColor     = color * 0.8f;
        btn.colors = cs;

        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 18;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }

    static GameObject CreateContainer(string name, Transform parent, Vector2 pos,
        Vector2 size, bool horizontal, float spacing)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var layout = go.AddComponent<HorizontalOrVerticalLayoutGroup>();
        // layout sera HorizontalLayoutGroup ou VerticalLayoutGroup selon bool
        Object.DestroyImmediate(layout);

        if (horizontal)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing              = spacing;
            hlg.childAlignment       = TextAnchor.MiddleCenter;
            hlg.childControlWidth    = false;
            hlg.childControlHeight   = false;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
        }
        else
        {
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing              = spacing;
            vlg.childAlignment       = TextAnchor.MiddleCenter;
            vlg.childControlWidth    = false;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = false;
            vlg.childForceExpandHeight = false;
        }
        return go;
    }

    static GameObject CreateSlot(string name, Transform parent, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(88, 120);
        return go;
    }
}
