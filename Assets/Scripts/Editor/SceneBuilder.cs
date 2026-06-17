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
    static readonly Color C_BTNGRN  = new Color(0.22f,0.64f,0.29f);
    static readonly Color C_BTNRED  = new Color(0.78f,0.22f,0.22f);
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
        // Reference resolution divisee par 2 (meme ratio 9:16) : tous les elements
        // de l'UI (cartes, textes, avatars...) s'affichent ~2x plus grands a l'ecran
        // sans changer la disposition relative.
        sc.referenceResolution = new Vector2(540, 960);
        sc.matchWidthOrHeight  = 0.5f;
        // Mode Shrink : en paysage (ou tout ecran dont le ratio differe de 9:16),
        // toute la composition (pensee en portrait) reste visible sans etre
        // coupee ni deformee — elle est simplement reduite et centree, avec
        // des bandes laterales comblees par le fond.
        sc.screenMatchMode     = CanvasScaler.ScreenMatchMode.Shrink;
        cvGo.AddComponent<GraphicRaycaster>();
        var cvT = cvGo.transform;

        // ── DECOR : ARRIERE-PLAN ─────────────────────────────────────────────────
        var bgGo  = new GameObject("Background"); bgGo.transform.SetParent(cvT,false);
        FillRT(bgGo.AddComponent<RectTransform>());
        var bgImg = bgGo.AddComponent<Image>(); bgImg.raycastTarget = false; bgImg.preserveAspect = false;

        // ── SCORE ─────────────────────────────────────────────────────────────
        var scoreP  = Pnl("ScorePanel", cvT, new Vector2(230,130), ATopL, new Vector2(125,-70), C_PANEL);
        var tUs     = Lbl("ScoreUs",   "0",     scoreP.transform, new Vector2(-50, 8),  28, C_WHITE, true);
        var tThem   = Lbl("ScoreThem", "0",     scoreP.transform, new Vector2( 50, 8),  28, C_WHITE, true);
        var tTrump  = Lbl("TrumpInd",  "",      scoreP.transform, new Vector2(0,-44),   14, C_GOLD,  true);
                      Lbl("LblNous","Nous",     scoreP.transform, new Vector2(-50, 42), 18, new Color(.6f,.9f,.6f), false);
                      Lbl("LblEux", "Eux",      scoreP.transform, new Vector2( 50, 42), 18, new Color(.9f,.6f,.6f), false);

        // ── IA HAUT (Nord) ─────────────────────────────────────────────────────
        var aiTopZ  = Pnl("AITopZone",  cvT, new Vector2(900,220), ATop, new Vector2(0,-165), Color.clear);
        var avNordBg = Pnl("AvatarNordBg", aiTopZ.transform, new Vector2(108,108), AMid, new Vector2(-380,10), C_PANLT);
                      Lbl("LblNord","Nord", avNordBg.transform, new Vector2(0,76), 20, C_AILBL, false);
        var avNord  = Img("AvatarNord", avNordBg.transform, Vector2.zero, new Vector2(96,96));
        var aiTopH  = FanContainer("AITopHand", aiTopZ.transform, new Vector2(50,0), new Vector2(760,160), false, 46f, 14f, 24f);

        // ── IA GAUCHE (Ouest) ──────────────────────────────────────────────────
        var aiLftZ  = Pnl("AILeftZone",  cvT, new Vector2(160,520), ALeft, new Vector2(90,0),  Color.clear);
        var avOuestBg = Pnl("AvatarOuestBg", aiLftZ.transform, new Vector2(108,108), AMid, new Vector2(0,225), C_PANLT);
                      Lbl("LblOuest","Ouest", avOuestBg.transform, new Vector2(0,76), 20, C_AILBL, false);
        var avOuest = Img("AvatarOuest", avOuestBg.transform, Vector2.zero, new Vector2(96,96));
        var aiLftH  = FanContainer("AILeftHand", aiLftZ.transform, new Vector2(0,-60), new Vector2(140,400), true, 44f, 14f, 22f);

        // ── IA DROITE (Est) ────────────────────────────────────────────────────
        var aiRgtZ  = Pnl("AIRightZone", cvT, new Vector2(160,520), ARight, new Vector2(-90,0), Color.clear);
        var avEstBg = Pnl("AvatarEstBg", aiRgtZ.transform, new Vector2(108,108), AMid, new Vector2(0,225), C_PANLT);
                      Lbl("LblEst","Est", avEstBg.transform, new Vector2(0,76), 20, C_AILBL, false);
        var avEst   = Img("AvatarEst", avEstBg.transform, Vector2.zero, new Vector2(96,96));
        var aiRgtH  = FanContainer("AIRightHand", aiRgtZ.transform, new Vector2(0,-60), new Vector2(140,400), true, 44f, 14f, 22f);

        // ── TABLE (4 slots) ────────────────────────────────────────────────────
        // table.png est un ovale "paysage" (plus large que haut). En portrait, on le
        // pivote de 90° pour obtenir un ovale "portrait" aligné avec la disposition
        // Nord/Sud/Est/Ouest des joueurs.
        var tableZ  = Pnl("TableZone", cvT, new Vector2(560,680), AMid, new Vector2(0,30), Color.clear);
        var tableImgGo = new GameObject("TableImage"); tableImgGo.transform.SetParent(tableZ.transform,false);
        var tableImgRt = tableImgGo.AddComponent<RectTransform>();
        FillRT(tableImgRt);
        // Recentre l'ovale : table.png n'est pas centre dans son cadre (le
        // dessin est decale vers le bas), ce qui, apres rotation de 90°,
        // se traduit par un decalage visuel vers la gauche. On compense
        // par un offset horizontal.
        tableImgRt.anchoredPosition = new Vector2(24, 0);
        tableImgRt.localEulerAngles = new Vector3(0,0,90);
        var tableImg = tableImgGo.AddComponent<Image>(); tableImg.raycastTarget = false; tableImg.preserveAspect = true;
        var toc = tableZ.AddComponent<TableOrientationController>();
        toc.TableImage = tableImgRt;
        var slotS   = Slot("SlotSud",   tableZ.transform, new Vector2(   0,-220));
        var slotO   = Slot("SlotOuest", tableZ.transform, new Vector2(-185,   0));
        var slotN   = Slot("SlotNord",  tableZ.transform, new Vector2(   0, 220));
        var slotE   = Slot("SlotEst",   tableZ.transform, new Vector2( 185,   0));

        // ── CARTE RETOURNEE (centre table, phase de prise) ─────────────────────
        var turnedGo = new GameObject("TurnedCardSlot"); turnedGo.transform.SetParent(tableZ.transform,false);
        var turnedRt = turnedGo.AddComponent<RectTransform>();
        turnedRt.anchorMin = turnedRt.anchorMax = AMid;
        turnedRt.anchoredPosition = Vector2.zero;
        turnedRt.sizeDelta = new Vector2(128,192);

        // ── MAIN JOUEUR (Sud) ──────────────────────────────────────────────────
        var handZ   = Pnl("HandZone", cvT, new Vector2(1080,320), ABot, new Vector2(0,160), Color.clear);
        var humanH  = FanContainer("HumanHand", handZ.transform, new Vector2(0,-20), new Vector2(1040,300), false, 50f, 16f, 40f);

        // ── INFO BAR ───────────────────────────────────────────────────────────
        var infoP   = Pnl("InfoBar", cvT, new Vector2(790,50), ABot, new Vector2(0,356), C_PANLT);
        var tInfo   = Lbl("InfoBarText","A vous de jouer", infoP.transform, Vector2.zero, 20, C_WHITE, false);

        // ── BOUTON DERNIER PLI ─────────────────────────────────────────────────
        var lastBt  = Btn("LastTrickBtn", cvT, "Dernier pli",
                          new Vector2(180,50), ABotL, new Vector2(98,356), C_PANLT);

        // ── PANNEAU DERNIER PLI ────────────────────────────────────────────────
        var lpPnl   = Pnl("LastTrickPanel", cvT, new Vector2(320,340), ABotL, new Vector2(170,210), C_PANEL);
        var tLPTit  = Lbl("LastTrickTitle","Dernier pli", lpPnl.transform, new Vector2(0,142), 18, C_GOLD, true);
        var lpSlotS = Slot("LP_Sud",   lpPnl.transform, new Vector2(   0,-80));
        var lpSlotO = Slot("LP_Ouest", lpPnl.transform, new Vector2( -78,  0));
        var lpSlotN = Slot("LP_Nord",  lpPnl.transform, new Vector2(   0, 80));
        var lpSlotE = Slot("LP_Est",   lpPnl.transform, new Vector2(  78,  0));
        lpPnl.SetActive(false);

        // ── PANNEAU PRISE D'ATOUT ──────────────────────────────────────────────
        var tPnl    = Pnl("TrumpPanel", cvT, new Vector2(560,460), AMid, Vector2.zero, C_PANEL);
        tPnl.SetActive(false);
        var tTitle  = Lbl("TrumpTitle",   "Prendre ?", tPnl.transform, new Vector2(0, 135), 24, C_WHITE, true);
        var tCrdTxt = Lbl("TrumpCardTxt", "A♠",        tPnl.transform, new Vector2(0,  50), 48, C_BLKSUIT, true);
        var takeBt  = Btn("TakeBtn", tPnl.transform, "Prendre",
                          new Vector2(210,70), AMid, new Vector2(-112,-58), C_BTNGRN);
        var passBt  = Btn("PassBtn", tPnl.transform, "Passer",
                          new Vector2(210,70), AMid, new Vector2( 112,-58), C_BTNRED);
        foreach (var b in new[]{ takeBt, passBt })
        {
            var lblT = b.GetComponentInChildren<Text>();
            if (lblT) lblT.fontSize = 26;
        }

        var suitCt  = Pnl("SuitContainer", tPnl.transform, new Vector2(490,76), AMid, new Vector2(0,-180), Color.clear);
        suitCt.SetActive(false);
        var suitBtns = new Button[4];
        string[] sym = {"♠","♥","♦","♣"};
        Color[]  sco = {C_BLKSUIT, C_REDSUIT, C_REDSUIT, C_BLKSUIT};
        for (int i = 0; i < 4; i++)
        {
            var sb  = Btn("SB_"+sym[i], suitCt.transform, sym[i],
                          new Vector2(86,68), AMid, new Vector2(-155+i*104,0), C_WHITE);
            var sbt = sb.GetComponentInChildren<Text>(); if (sbt) sbt.color = sco[i];
            suitBtns[i] = sb.GetComponent<Button>();
        }

        // ── PANNEAU FIN DE MANCHE ──────────────────────────────────────────────
        var endPnl  = Pnl("EndPanel", cvT, new Vector2(660,550), AMid, Vector2.zero, C_PANEL);
        endPnl.SetActive(false);
        var tEndTi  = Lbl("EndTitle",   "Fin de manche", endPnl.transform, new Vector2(0,190), 30, C_GOLD,  true);
        var tEndDt  = Lbl("EndDetails", "",              endPnl.transform, new Vector2(0, 30), 18, C_WHITE, false);
        var nextBt  = Btn("NextBtn", endPnl.transform, "Continuer",
                          new Vector2(330,64), AMid, new Vector2(0,-202), C_BTNGRN);

        // ── PANNEAU MESSAGE FLASH ──────────────────────────────────────────────
        var msgPnl  = Pnl("MsgPanel", cvT, new Vector2(660,120), AMid, new Vector2(0,100), C_PANEL);
        msgPnl.SetActive(false);
        var tMsg    = Lbl("MsgText","", msgPnl.transform, Vector2.zero, 22, C_WHITE, false);

        // ── PANNEAU BELOTE/REBELOTE ────────────────────────────────────────────
        var belPnl  = Pnl("BelotePanel", cvT, new Vector2(450,92), AMid, new Vector2(0,215), new Color(1f,0.84f,0f,0.96f));
        belPnl.SetActive(false);
        var tBel    = Lbl("BeloteText","Belote !", belPnl.transform, Vector2.zero, 26, C_BLKSUIT, true);

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
        uim.LastTrickSlots       = new Transform[]{ lpSlotS.transform, lpSlotO.transform, lpSlotN.transform, lpSlotE.transform };
        uim.TurnedCardSlot       = turnedRt;

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
        var rt   = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(85,128);
        var bg   = root.AddComponent<Image>(); bg.color = Color.white;
        root.AddComponent<CanvasGroup>();

        var faceGo = new GameObject("CardFaceImage"); faceGo.transform.SetParent(root.transform,false);
        var faceImg = faceGo.AddComponent<Image>(); faceImg.raycastTarget = false;
        FillRT(faceGo.GetComponent<RectTransform>());

        var lblGo = new GameObject("CardLabel"); lblGo.transform.SetParent(root.transform,false);
        var t = lblGo.AddComponent<Text>();
        t.text = "A♠"; t.fontSize = 17; t.color = new Color(0.1f,0.1f,0.1f);
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
        var rt   = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(75,113);
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
        var rt   = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(88,132);
        var bg   = root.AddComponent<Image>(); bg.color = Color.white;
        root.AddComponent<CanvasGroup>();

        var faceGo = new GameObject("CardFaceImage"); faceGo.transform.SetParent(root.transform,false);
        var faceImg = faceGo.AddComponent<Image>(); faceImg.raycastTarget = false;
        FillRT(faceGo.GetComponent<RectTransform>());

        var lblGo = new GameObject("CardLabel"); lblGo.transform.SetParent(root.transform,false);
        var t = lblGo.AddComponent<Text>();
        t.text = "A♠"; t.fontSize = 19; t.color = new Color(0.1f,0.1f,0.1f);
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
    static readonly Vector2 ATopL  = new Vector2(0f,1f);
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
        var img = go.AddComponent<Image>();
        img.color  = col;
        img.sprite = EnsureRoundedSprite();
        img.type   = Image.Type.Sliced;
        go.AddComponent<Button>();
        var tGo = new GameObject("Label"); tGo.transform.SetParent(go.transform,false);
        FillRT(tGo.AddComponent<RectTransform>());
        var t = tGo.AddComponent<Text>();
        t.text = label; t.fontSize = 20; t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    /// <summary>
    /// Genere (une seule fois) un sprite de rectangle a coins arrondis,
    /// utilise comme fond 9-slice pour tous les boutons.
    /// </summary>
    static Sprite EnsureRoundedSprite()
    {
        const string path = "Assets/Art/UI/ui_button_round.png";
        const int    size   = 64;
        const int    radius = 20;

        if (!AssetDatabase.IsValidFolder("Assets/Art/UI"))
            AssetDatabase.CreateFolder("Assets/Art","UI");

        if (!System.IO.File.Exists(path))
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int cx = Mathf.Clamp(x, radius, size-1-radius);
                int cy = Mathf.Clamp(y, radius, size-1-radius);
                float dist = Vector2.Distance(new Vector2(x,y), new Vector2(cx,cy));
                tex.SetPixel(x, y, new Color(1f,1f,1f, dist <= radius ? 1f : 0f));
            }
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType  = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(radius,radius,radius,radius);
            importer.filterMode   = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static GameObject FanContainer(string name, Transform parent, Vector2 pos, Vector2 size,
                                    bool vertical = false, float spacing = 76f, float maxAngle = 16f, float arcHeight = 40f)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AMid; rt.anchoredPosition = pos; rt.sizeDelta = size;
        var fan = go.AddComponent<CardFanLayout>();
        fan.Vertical   = vertical;
        fan.CardSpacing = spacing;
        fan.MaxAngle    = maxAngle;
        fan.ArcHeight   = arcHeight;
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
