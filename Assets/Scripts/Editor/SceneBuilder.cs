using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using BeloteFreeze.Core;
using BeloteFreeze.UI;

/// <summary>
/// Menu BeloteFreeze > Build GameScene (Ctrl+Shift+B)
/// Génère toute la scène et les prefabs automatiquement.
/// Nécessite : module UI Built-in activé (Built-in > UI > Enable).
/// </summary>
public static class SceneBuilder
{
    static readonly Color C_BG      = new Color(0.08f,0.36f,0.16f);
    static readonly Color C_PANEL   = new Color(0f,0f,0f,0.75f);
    static readonly Color C_PANLT   = new Color(0f,0f,0f,0.48f);
    static readonly Color C_BTNGRN  = new Color(0.17f,0.50f,0.17f);
    static readonly Color C_BTNRED  = new Color(0.55f,0.15f,0.15f);
    static readonly Color C_WHITE   = Color.white;
    static readonly Color C_GOLD    = new Color(1f,0.84f,0f);
    static readonly Color C_BACK    = new Color(0.12f,0.25f,0.60f);
    static readonly Color C_REDSUIT = new Color(0.80f,0.10f,0.10f);
    static readonly Color C_BLKSUIT = new Color(0.10f,0.10f,0.10f);
    static readonly Color C_AILBL   = new Color(0.75f,1f,0.75f);
    static readonly Color C_TRUMP   = new Color(1f,0.98f,0.85f);

    [MenuItem("BeloteFreeze/Build GameScene %#b")]
    public static void BuildScene()
    {
        // Nettoyage : supprime les objets générés par une exécution précédente
        // pour éviter les doublons (Canvas, UIManager, GameManager, EventSystem)
        foreach (var name in new[] { "Canvas", "UIManager", "GameManager", "EventSystem" })
        {
            var existing = GameObject.Find(name);
            while (existing != null)
            {
                Object.DestroyImmediate(existing);
                existing = GameObject.Find(name);
            }
        }

        // Caméra
        var camGo = GameObject.FindWithTag("MainCamera")
                    ?? new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>() ?? camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = C_BG;
        cam.orthographic = true;

        // Canvas
        var cvGo = new GameObject("Canvas");
        var cv   = cvGo.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 0;
        var sc = cvGo.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080, 1920);
        sc.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();
        var cvT = cvGo.transform;

        // ── DECOR : ARRIERE-PLAN ─────────────────────────────────────────────────
        var bgGo  = new GameObject("Background"); bgGo.transform.SetParent(cvT,false);
        FillRT(bgGo.AddComponent<RectTransform>());
        var bgImg = bgGo.AddComponent<Image>(); bgImg.raycastTarget = false; bgImg.preserveAspect = false;

        // ── SCORE ─────────────────────────────────────────────────────────────
        var scoreP  = Pnl("ScorePanel", cvT, new Vector2(1080,80), ATop, new Vector2(0,-40), C_PANEL);
        var tUs     = Lbl("ScoreUs",   "0",     scoreP.transform, new Vector2(-200,0),  28, C_WHITE, true);
        var tThem   = Lbl("ScoreThem", "0",     scoreP.transform, new Vector2( 200,0),  28, C_WHITE, true);
        var tTrump  = Lbl("TrumpInd",  "",      scoreP.transform, new Vector2(0, 14),   18, C_GOLD,  true);
                      Lbl("LblNous","Nous",     scoreP.transform, new Vector2(-200,-26),13, new Color(.6f,.9f,.6f), false);
                      Lbl("LblEux", "Eux",      scoreP.transform, new Vector2( 200,-26),13, new Color(.9f,.6f,.6f), false);

        // ── IA HAUT (Nord) ─────────────────────────────────────────────────────
        var aiTopZ  = Pnl("AITopZone",  cvT, new Vector2(720,120), ATop, new Vector2(0,-135), Color.clear);
                      Lbl("LblNord","Nord", aiTopZ.transform, new Vector2(0,44), 13, C_AILBL, false);
        var avNord  = Img("AvatarNord", aiTopZ.transform, new Vector2(-330,10), new Vector2(56,56));
        var aiTopH  = Ctr("AITopHand",  aiTopZ.transform, Vector2.zero, new Vector2(700,80),  true);

        // ── IA GAUCHE (Ouest) ──────────────────────────────────────────────────
        var aiLftZ  = Pnl("AILeftZone",  cvT, new Vector2(110,400), ALeft, new Vector2(58,0),  Color.clear);
                      Lbl("LblOuest","Ouest", aiLftZ.transform, new Vector2(0,165), 13, C_AILBL, false);
        var avOuest = Img("AvatarOuest", aiLftZ.transform, new Vector2(0,195), new Vector2(56,56));
        var aiLftH  = Ctr("AILeftHand",  aiLftZ.transform, Vector2.zero, new Vector2(80,360),  false);

        // ── IA DROITE (Est) ────────────────────────────────────────────────────
        var aiRgtZ  = Pnl("AIRightZone", cvT, new Vector2(110,400), ARight, new Vector2(-58,0), Color.clear);
                      Lbl("LblEst","Est", aiRgtZ.transform, new Vector2(0,165), 13, C_AILBL, false);
        var avEst   = Img("AvatarEst", aiRgtZ.transform, new Vector2(0,195), new Vector2(56,56));
        var aiRgtH  = Ctr("AIRightHand", aiRgtZ.transform, Vector2.zero, new Vector2(80,360),  false);

        // ── TABLE (4 slots) ────────────────────────────────────────────────────
        var tableZ  = Pnl("TableZone", cvT, new Vector2(420,370), AMid, new Vector2(0,55), Color.clear);
        var tableImgGo = new GameObject("TableImage"); tableImgGo.transform.SetParent(tableZ.transform,false);
        FillRT(tableImgGo.AddComponent<RectTransform>());
        var tableImg = tableImgGo.AddComponent<Image>(); tableImg.raycastTarget = false; tableImg.preserveAspect = false;
        tableZ.AddComponent<TableOrientationController>();
        var slotS   = Slot("SlotSud",   tableZ.transform, new Vector2(  0,-115));
        var slotO   = Slot("SlotOuest", tableZ.transform, new Vector2(-145,  0));
        var slotN   = Slot("SlotNord",  tableZ.transform, new Vector2(  0, 115));
        var slotE   = Slot("SlotEst",   tableZ.transform, new Vector2( 145,  0));

        // ── MAIN JOUEUR (Sud) ──────────────────────────────────────────────────
        var handZ   = Pnl("HandZone", cvT, new Vector2(1080,200), ABot, new Vector2(0,108), Color.clear);
        var humanH  = Ctr("HumanHand", handZ.transform, Vector2.zero, new Vector2(1040,188), true);

        // ── INFO BAR ───────────────────────────────────────────────────────────
        var infoP   = Pnl("InfoBar", cvT, new Vector2(790,44), ABot, new Vector2(0,316), C_PANLT);
        var tInfo   = Lbl("InfoBarText","A vous de jouer", infoP.transform, Vector2.zero, 16, C_WHITE, false);

        // ── BOUTON DERNIER PLI ─────────────────────────────────────────────────
        var lastBt  = Btn("LastTrickBtn", cvT, "Dernier pli",
                          new Vector2(168,44), ABotL, new Vector2(92,316), C_PANLT);

        // ── PANNEAU DERNIER PLI ────────────────────────────────────────────────
        var lpPnl   = Pnl("LastTrickPanel", cvT, new Vector2(540,196), AMid, Vector2.zero, C_PANEL);
        var tLPTxt  = Lbl("LastTrickText","", lpPnl.transform, Vector2.zero, 15, C_WHITE, false);
        lpPnl.SetActive(false);

        // ── PANNEAU PRISE D'ATOUT ──────────────────────────────────────────────
        var tPnl    = Pnl("TrumpPanel", cvT, new Vector2(540,360), AMid, Vector2.zero, C_PANEL);
        tPnl.SetActive(false);
        var tTitle  = Lbl("TrumpTitle",   "Prendre ?", tPnl.transform, new Vector2(0, 130), 20, C_WHITE, true);
        var tCrdTxt = Lbl("TrumpCardTxt", "A♠",        tPnl.transform, new Vector2(0,  50), 40, C_BLKSUIT, true);
        var takeBt  = Btn("TakeBtn", tPnl.transform, "Prendre",
                          new Vector2(190,56), AMid, new Vector2(-104,-55), C_BTNGRN);
        var passBt  = Btn("PassBtn", tPnl.transform, "Passer",
                          new Vector2(190,56), AMid, new Vector2( 104,-55), C_BTNRED);

        var suitCt  = Pnl("SuitContainer", tPnl.transform, new Vector2(470,72), AMid, new Vector2(0,-105), Color.clear);
        suitCt.SetActive(false);
        var suitBtns = new Button[4];
        string[] sym = {"♠","♥","♦","♣"};
        Color[]  sco = {C_BLKSUIT, C_REDSUIT, C_REDSUIT, C_BLKSUIT};
        for (int i = 0; i < 4; i++)
        {
            var sb  = Btn("SB_"+sym[i], suitCt.transform, sym[i],
                          new Vector2(82,64), AMid, new Vector2(-150+i*102,0), C_WHITE);
            var sbt = sb.GetComponentInChildren<Text>(); if (sbt) sbt.color = sco[i];
            suitBtns[i] = sb.GetComponent<Button>();
        }

        // ── PANNEAU FIN DE MANCHE ──────────────────────────────────────────────
        var endPnl  = Pnl("EndPanel", cvT, new Vector2(640,530), AMid, Vector2.zero, C_PANEL);
        endPnl.SetActive(false);
        var tEndTi  = Lbl("EndTitle",   "Fin de manche", endPnl.transform, new Vector2(0,185), 26, C_GOLD,  true);
        var tEndDt  = Lbl("EndDetails", "",              endPnl.transform, new Vector2(0, 30), 15, C_WHITE, false);
        var nextBt  = Btn("NextBtn", endPnl.transform, "Nouvelle manche",
                          new Vector2(310,60), AMid, new Vector2(0,-198), C_BTNGRN);

        // ── PANNEAU MESSAGE FLASH ──────────────────────────────────────────────
        var msgPnl  = Pnl("MsgPanel", cvT, new Vector2(640,110), AMid, new Vector2(0,100), C_PANEL);
        msgPnl.SetActive(false);
        var tMsg    = Lbl("MsgText","", msgPnl.transform, Vector2.zero, 18, C_WHITE, false);

        // ── PANNEAU BELOTE/REBELOTE ────────────────────────────────────────────
        var belPnl  = Pnl("BelotePanel", cvT, new Vector2(430,86), AMid, new Vector2(0,215), new Color(1f,0.84f,0f,0.96f));
        belPnl.SetActive(false);
        var tBel    = Lbl("BeloteText","Belote !", belPnl.transform, Vector2.zero, 22, C_BLKSUIT, true);

        // ── PREFABS ────────────────────────────────────────────────────────────
        const string PF = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(PF)) AssetDatabase.CreateFolder("Assets","Prefabs");
        MakeCardPrefab(PF);
        MakeCardBackPrefab(PF);
        MakeTrickCardPrefab(PF);

        // ── GAMEOBJECTS LOGIQUE ────────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gm   = gmGo.AddComponent<GameManager>();
        var uiGo = new GameObject("UIManager");
        var uim  = uiGo.AddComponent<UIManager>();
        gm.UIManager = uim;

        uim.ScoreUsText          = tUs;
        uim.ScoreThemText        = tThem;
        uim.TrumpIndicatorText   = tTrump;
        uim.TrumpPanel           = tPnl;
        uim.TrumpPanelTitle      = tTitle;
        uim.TrumpCardText        = tCrdTxt;
        uim.TakeButton           = takeBt.GetComponent<Button>();
        uim.PassButton           = passBt.GetComponent<Button>();
        uim.SuitButtonsContainer = suitCt;
        uim.SuitButtons          = suitBtns;
        uim.HumanHandContainer   = humanH.transform;
        uim.AIHandContainers     = new Transform[]{ aiLftH.transform, aiTopH.transform, aiRgtH.transform };
        uim.CardPrefab           = AssetDatabase.LoadAssetAtPath<GameObject>(PF+"/CardPrefab.prefab");
        uim.CardBackPrefab       = AssetDatabase.LoadAssetAtPath<GameObject>(PF+"/CardBackPrefab.prefab");
        uim.TrickCardPrefab      = AssetDatabase.LoadAssetAtPath<GameObject>(PF+"/TrickCardPrefab.prefab");
        uim.TrickSlots           = new Transform[]{ slotS.transform, slotO.transform, slotN.transform, slotE.transform };
        uim.MessagePanel         = msgPnl;
        uim.MessageText          = tMsg;
        uim.BeloteAnnouncePanel  = belPnl;
        uim.BeloteAnnounceText   = tBel;
        uim.EndPanel             = endPnl;
        uim.EndTitleText         = tEndTi;
        uim.EndDetailsText       = tEndDt;
        uim.NextHandButton       = nextBt.GetComponent<Button>();
        uim.InfoBarText          = tInfo;
        uim.LastTrickButton      = lastBt.GetComponent<Button>();
        uim.LastTrickPanel       = lpPnl;
        uim.LastTrickText        = tLPTxt;

        // ── DECOR / ART ────────────────────────────────────────────────────────
        var cardArt = BuildCardArtSet();
        uim.CardArt        = cardArt;
        uim.BackgroundImage = bgImg;
        uim.TableImage      = tableImg;
        uim.AIAvatars       = new Image[]{ avOuest.GetComponent<Image>(), avNord.GetComponent<Image>(), avEst.GetComponent<Image>() };
        if (cardArt != null)
        {
            bgImg.sprite    = cardArt.Background;
            tableImg.sprite = cardArt.Table;
        }

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets","Scenes");
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(),
            "Assets/Scenes/GameScene.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BeloteFreeze] Scene construite ! Clique Play pour jouer.");
    }

    // ── BIBLIOTHEQUE DE SPRITES ───────────────────────────────────────────────
    static CardArtSet BuildCardArtSet()
    {
        const string assetPath = "Assets/Art/CardArtSet.asset";
        const string C = "Assets/Art/Cartes/";

        var set = AssetDatabase.LoadAssetAtPath<CardArtSet>(assetPath);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<CardArtSet>();
            AssetDatabase.CreateAsset(set, assetPath);
        }

        var entries = new (Suit suit, Rank rank, string file)[]
        {
            (Suit.Spades, Rank.Ace,"as_pique"),   (Suit.Spades, Rank.King,"roi_pique"),  (Suit.Spades, Rank.Queen,"dame_pique"), (Suit.Spades, Rank.Jack,"valet_pique"),
            (Suit.Spades, Rank.Ten,"dix_pique"),  (Suit.Spades, Rank.Nine,"neuf_pique"), (Suit.Spades, Rank.Eight,"huit_pique"), (Suit.Spades, Rank.Seven,"sept_pique"),

            (Suit.Hearts, Rank.Ace,"as_coeur"),   (Suit.Hearts, Rank.King,"roi_coeur"),  (Suit.Hearts, Rank.Queen,"dame_coeur"), (Suit.Hearts, Rank.Jack,"valet_coeur"),
            (Suit.Hearts, Rank.Ten,"dix_coeur"),  (Suit.Hearts, Rank.Nine,"neuf_coeur"), (Suit.Hearts, Rank.Eight,"huit_coeur"), (Suit.Hearts, Rank.Seven,"sept_coeur"),

            (Suit.Diamonds, Rank.Ace,"as_carreau"),  (Suit.Diamonds, Rank.King,"roi_carreau"),  (Suit.Diamonds, Rank.Queen,"dame_carreau"), (Suit.Diamonds, Rank.Jack,"valet_carreau"),
            (Suit.Diamonds, Rank.Ten,"dix_carreau"), (Suit.Diamonds, Rank.Nine,"neuf_carreau"), (Suit.Diamonds, Rank.Eight,"huit_carreau"), (Suit.Diamonds, Rank.Seven,"sept_carreau"),

            (Suit.Clubs, Rank.Ace,"as_trefle"),   (Suit.Clubs, Rank.King,"roi_trefle"),  (Suit.Clubs, Rank.Queen,"dame_trefle"), (Suit.Clubs, Rank.Jack,"valet_trefle"),
            (Suit.Clubs, Rank.Ten,"dix_trefle"),  (Suit.Clubs, Rank.Nine,"neuf_trefle"), (Suit.Clubs, Rank.Eight,"huit_trefle"), (Suit.Clubs, Rank.Seven,"sept_trefle"),
        };

        set.Cards = new CardArtSet.CardSprite[entries.Length];
        for (int i = 0; i < entries.Length; i++)
        {
            var (suit, rank, file) = entries[i];
            set.Cards[i] = new CardArtSet.CardSprite
            {
                Suit = suit, Rank = rank,
                Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(C + file + ".png")
            };
        }

        set.CardBack     = AssetDatabase.LoadAssetAtPath<Sprite>(C + "carte_dos.png");
        set.Background   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/bg_game.png");
        set.Table        = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Background/table.png");
        set.AvatarMale   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Avatars/IA_homme.png");
        set.AvatarFemale = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Avatars/IA_femme.png");
        set.IconSpades   = AssetDatabase.LoadAssetAtPath<Sprite>(C + "icone_pique.png");
        set.IconHearts   = AssetDatabase.LoadAssetAtPath<Sprite>(C + "icone_coeur.png");
        set.IconDiamonds = AssetDatabase.LoadAssetAtPath<Sprite>(C + "icone_carreau.png");
        set.IconClubs    = AssetDatabase.LoadAssetAtPath<Sprite>(C + "icone_trefle.png");

        EditorUtility.SetDirty(set);
        AssetDatabase.SaveAssets();
        return set;
    }

    // ── PREFABS ───────────────────────────────────────────────────────────────
    static void MakeCardPrefab(string dir)
    {
        var root = new GameObject("CardPrefab");
        var rt   = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(72,100);
        var bg   = root.AddComponent<Image>(); bg.color = Color.white;
        root.AddComponent<CanvasGroup>();

        var faceGo = new GameObject("CardFaceImage"); faceGo.transform.SetParent(root.transform,false);
        var faceImg = faceGo.AddComponent<Image>(); faceImg.raycastTarget = false;
        FillRT(faceGo.GetComponent<RectTransform>());

        var lblGo = new GameObject("CardLabel"); lblGo.transform.SetParent(root.transform,false);
        var t = lblGo.AddComponent<Text>();
        t.text = "A♠"; t.fontSize = 20; t.color = new Color(0.1f,0.1f,0.1f);
        t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        FillRT(lblGo.GetComponent<RectTransform>());

        var hlGo  = new GameObject("TrumpHighlight"); hlGo.transform.SetParent(root.transform,false);
        var hlImg = hlGo.AddComponent<Image>(); hlImg.color = new Color(1f,0.84f,0f,0.45f);
        FillRTExp(hlGo.GetComponent<RectTransform>(),3); hlGo.SetActive(false);

        var cv = root.AddComponent<CardView>();
        cv.CardLabel = t; cv.CardBackground = bg; cv.CardFaceImage = faceImg; cv.TrumpHighlight = hlImg;
        root.AddComponent<Button>();

        PrefabUtility.SaveAsPrefabAsset(root, dir+"/CardPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    static void MakeCardBackPrefab(string dir)
    {
        var root = new GameObject("CardBackPrefab");
        var rt   = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(72,100);
        var bg   = root.AddComponent<Image>(); bg.color = new Color(0.12f,0.25f,0.60f);

        var faceGo = new GameObject("CardFaceImage"); faceGo.transform.SetParent(root.transform,false);
        var faceImg = faceGo.AddComponent<Image>(); faceImg.raycastTarget = false;
        FillRT(faceGo.GetComponent<RectTransform>());

        var cv = root.AddComponent<CardView>();
        cv.CardBackground = bg; cv.CardFaceImage = faceImg;

        PrefabUtility.SaveAsPrefabAsset(root, dir+"/CardBackPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    static void MakeTrickCardPrefab(string dir)
    {
        var root = new GameObject("TrickCardPrefab");
        var rt   = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(80,112);
        var bg   = root.AddComponent<Image>(); bg.color = Color.white;
        root.AddComponent<CanvasGroup>();

        var faceGo = new GameObject("CardFaceImage"); faceGo.transform.SetParent(root.transform,false);
        var faceImg = faceGo.AddComponent<Image>(); faceImg.raycastTarget = false;
        FillRT(faceGo.GetComponent<RectTransform>());

        var lblGo = new GameObject("CardLabel"); lblGo.transform.SetParent(root.transform,false);
        var t = lblGo.AddComponent<Text>();
        t.text = "A♠"; t.fontSize = 22; t.color = new Color(0.1f,0.1f,0.1f);
        t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        FillRT(lblGo.GetComponent<RectTransform>());

        var hlGo  = new GameObject("TrumpHighlight"); hlGo.transform.SetParent(root.transform,false);
        var hlImg = hlGo.AddComponent<Image>(); hlImg.color = new Color(1f,0.84f,0f,0.55f);
        FillRTExp(hlGo.GetComponent<RectTransform>(),4); hlGo.SetActive(false);

        var cv = root.AddComponent<CardView>();
        cv.CardLabel = t; cv.CardBackground = bg; cv.CardFaceImage = faceImg; cv.TrumpHighlight = hlImg;

        PrefabUtility.SaveAsPrefabAsset(root, dir+"/TrickCardPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    // ── Helpers UI ────────────────────────────────────────────────────────────
    static readonly Vector2 ATop   = new Vector2(0.5f,1f);
    static readonly Vector2 ABot   = new Vector2(0.5f,0f);
    static readonly Vector2 ABotL  = new Vector2(0f,0f);
    static readonly Vector2 ALeft  = new Vector2(0f,0.5f);
    static readonly Vector2 ARight = new Vector2(1f,0.5f);
    static readonly Vector2 AMid   = new Vector2(0.5f,0.5f);

    static GameObject Pnl(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 pos, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
        if (col.a > 0.01f) { var img = go.AddComponent<Image>(); img.color = col; img.raycastTarget = false; }
        return go;
    }

    static Text Lbl(string name, string text, Transform parent,
                    Vector2 pos, float size, Color col, bool bold)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AMid; rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(420,54);
        var t = go.AddComponent<Text>();
        t.text = text; t.fontSize = (int)size; t.color = col;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.raycastTarget = false;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return t;
    }

    static GameObject Btn(string name, Transform parent, string label,
                          Vector2 size, Vector2 anchor, Vector2 pos, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = col;
        go.AddComponent<Button>();
        var tGo = new GameObject("Label"); tGo.transform.SetParent(go.transform,false);
        FillRT(tGo.AddComponent<RectTransform>());
        var t = tGo.AddComponent<Text>();
        t.text = label; t.fontSize = 17; t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static GameObject Ctr(string name, Transform parent, Vector2 pos, Vector2 size, bool horiz)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AMid; rt.anchoredPosition = pos; rt.sizeDelta = size;
        if (horiz)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing=4; h.childAlignment=TextAnchor.MiddleCenter;
            h.childControlWidth=false; h.childControlHeight=false;
            h.childForceExpandWidth=false; h.childForceExpandHeight=false;
        }
        else
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing=4; v.childAlignment=TextAnchor.MiddleCenter;
            v.childControlWidth=false; v.childControlHeight=false;
            v.childForceExpandWidth=false; v.childForceExpandHeight=false;
        }
        return go;
    }

    static GameObject Img(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AMid; rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.raycastTarget = false; img.preserveAspect = true;
        return go;
    }

    static GameObject Slot(string name, Transform parent, Vector2 pos)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AMid; rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(90,120);
        return go;
    }

    static void FillRT(RectTransform rt)
    { rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero; }

    static void FillRTExp(RectTransform rt, float e)
    { rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
      rt.offsetMin=new Vector2(-e,-e); rt.offsetMax=new Vector2(e,e); }
}
