using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using BeloteFreeze.Core;
using BeloteFreeze.UI;

/// <summary>
/// Script Editor : génère toute la GameScene Belote Freeze en un clic.
/// Menu Unity : BeloteFreeze > Build GameScene  (Ctrl+Shift+B)
/// Utilise UnityEngine.UI.Text uniquement — pas de TMPro requis.
/// </summary>
public static class SceneBuilder
{
    static readonly Color C_BG        = new Color(0.08f, 0.36f, 0.16f);
    static readonly Color C_PANEL     = new Color(0f, 0f, 0f, 0.72f);
    static readonly Color C_PANEL_LT  = new Color(0f, 0f, 0f, 0.45f);
    static readonly Color C_BTN_GRN   = new Color(0.17f, 0.48f, 0.17f);
    static readonly Color C_BTN_RED   = new Color(0.55f, 0.15f, 0.15f);
    static readonly Color C_CARD      = Color.white;
    static readonly Color C_BACK      = new Color(0.12f, 0.25f, 0.60f);
    static readonly Color C_RED_SUIT  = new Color(0.80f, 0.10f, 0.10f);
    static readonly Color C_BLK_SUIT  = new Color(0.10f, 0.10f, 0.10f);
    static readonly Color C_GOLD      = new Color(1.00f, 0.84f, 0.00f);
    static readonly Color C_WHITE     = Color.white;
    static readonly Color C_GRN_LBL   = new Color(0.6f, 0.9f, 0.6f);
    static readonly Color C_RED_LBL   = new Color(0.9f, 0.6f, 0.6f);
    static readonly Color C_AI_LBL    = new Color(0.8f, 1f, 0.8f);

    [MenuItem("BeloteFreeze/Build GameScene %#b")]
    public static void BuildScene()
    {
        // Caméra fond vert
        var cam = Camera.main ?? new GameObject("Main Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = C_BG;
        cam.orthographic = true;
        cam.gameObject.tag = "MainCamera";

        // Canvas principal
        var cvGo = new GameObject("Canvas");
        var cv   = cvGo.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = cvGo.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080, 1920);
        sc.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();
        Transform cvT = cvGo.transform;

        // ── Score ──────────────────────────────────────────────────────────
        var scorePanel = Panel("ScorePanel", cvT, new Vector2(1080,80), AnchorTop, new Vector2(0,-40), C_PANEL);
        var tScoreUs   = Lbl("ScoreUs",   "0",    scorePanel, new Vector2(-200,0),  28, C_WHITE,   true);
        var tScoreThem = Lbl("ScoreThem", "0",    scorePanel, new Vector2( 200,0),  28, C_WHITE,   true);
        var tTrumpInd  = Lbl("TrumpInd",  "",     scorePanel, new Vector2(0, 14),   18, C_GOLD,    true);
                         Lbl("LblNous",   "Nous", scorePanel, new Vector2(-200,-24),13, C_GRN_LBL, false);
                         Lbl("LblEux",    "Eux",  scorePanel, new Vector2( 200,-24),13, C_RED_LBL, false);

        // ── IA Haut (Nord) ─────────────────────────────────────────────────
        var aiTopZone = Panel("AITopZone", cvT, new Vector2(720,120), AnchorTop, new Vector2(0,-130), Color.clear);
                        Lbl("LblNord","Nord", aiTopZone.transform, new Vector2(0,42), 13, C_AI_LBL, false);
        var aiTopHand = Container("AITopHand", aiTopZone.transform, new Vector2(0,0), new Vector2(680,80), true);

        // ── IA Gauche (Ouest) ──────────────────────────────────────────────
        var aiLeftZone = Panel("AILeftZone", cvT, new Vector2(110,400), AnchorLeft, new Vector2(55,0), Color.clear);
                         Lbl("LblOuest","Ouest", aiLeftZone.transform, new Vector2(0,160), 13, C_AI_LBL, false);
        var aiLeftHand = Container("AILeftHand", aiLeftZone.transform, new Vector2(0,0), new Vector2(80,360), false);

        // ── IA Droite (Est) ────────────────────────────────────────────────
        var aiRightZone = Panel("AIRightZone", cvT, new Vector2(110,400), AnchorRight, new Vector2(-55,0), Color.clear);
                          Lbl("LblEst","Est", aiRightZone.transform, new Vector2(0,160), 13, C_AI_LBL, false);
        var aiRightHand = Container("AIRightHand", aiRightZone.transform, new Vector2(0,0), new Vector2(80,360), false);

        // ── Table centrale (4 slots) ───────────────────────────────────────
        var tableZone = Panel("TableZone", cvT, new Vector2(420,360), AnchorMid, new Vector2(0,60), Color.clear);
        var slotSud   = Slot("SlotSud",   tableZone.transform, new Vector2(  0,-110));
        var slotOuest = Slot("SlotOuest", tableZone.transform, new Vector2(-140,  0));
        var slotNord  = Slot("SlotNord",  tableZone.transform, new Vector2(  0, 110));
        var slotEst   = Slot("SlotEst",   tableZone.transform, new Vector2( 140,  0));

        // ── Main joueur (Sud) ──────────────────────────────────────────────
        var handZone  = Panel("HandZone", cvT, new Vector2(1080,200), AnchorBot, new Vector2(0,110), Color.clear);
        var humanHand = Container("HumanHand", handZone.transform, Vector2.zero, new Vector2(1020,180), true);

        // ── Info bar ───────────────────────────────────────────────────────
        var infoBar  = Panel("InfoBar", cvT, new Vector2(780,44), AnchorBot, new Vector2(0,314), C_PANEL_LT);
        var tInfoBar = Lbl("InfoBarText","A vous de jouer", infoBar.transform, Vector2.zero, 16, C_WHITE, false);

        // ── Bouton Dernier pli ─────────────────────────────────────────────
        var lastBtn  = Btn("LastTrickBtn", cvT, "Dernier pli", new Vector2(160,44), AnchorBotL, new Vector2(90,314), C_PANEL);

        // ── Panneau Dernier pli ────────────────────────────────────────────
        var lpPanel  = Panel("LastTrickPanel", cvT, new Vector2(520,190), AnchorMid, Vector2.zero, C_PANEL);
        var tLPText  = Lbl("LastTrickText","", lpPanel.transform, Vector2.zero, 15, C_WHITE, false);
        lpPanel.SetActive(false);

        // ── Panneau Prise d'Atout ──────────────────────────────────────────
        var tPanel   = Panel("TrumpPanel", cvT, new Vector2(520,340), AnchorMid, Vector2.zero, C_PANEL);
        tPanel.SetActive(false);
        var tTitle   = Lbl("TrumpTitle","Prendre ?",   tPanel.transform, new Vector2(0,120), 20, C_WHITE, true);
        var tCardTxt = Lbl("TrumpCardText","A♠",       tPanel.transform, new Vector2(0, 50), 38, C_BLK_SUIT, true);
        var takeBt   = Btn("TakeBtn",  tPanel.transform, "Prendre",  new Vector2(180,54), AnchorMid, new Vector2(-100,-50), C_BTN_GRN);
        var passBt   = Btn("PassBtn",  tPanel.transform, "Passer",   new Vector2(180,54), AnchorMid, new Vector2( 100,-50), C_BTN_RED);

        var suitCont = Panel("SuitContainer", tPanel.transform, new Vector2(460,70), AnchorMid, new Vector2(0,-100), Color.clear);
        suitCont.SetActive(false);
        var suitBtns = new Button[4];
        string[] sym    = {"♠","♥","♦","♣"};
        Color[]  symCol = {C_BLK_SUIT, C_RED_SUIT, C_RED_SUIT, C_BLK_SUIT};
        for (int i = 0; i < 4; i++)
        {
            var sb  = Btn("Suit_"+sym[i], suitCont.transform, sym[i], new Vector2(80,60), AnchorMid, new Vector2(-150+i*100,0), C_CARD);
            var sbt = sb.GetComponentInChildren<Text>();
            if (sbt != null) sbt.color = symCol[i];
            suitBtns[i] = sb.GetComponent<Button>();
        }

        // ── Panneau Fin de manche ──────────────────────────────────────────
        var endPanel   = Panel("EndPanel", cvT, new Vector2(620,520), AnchorMid, Vector2.zero, C_PANEL);
        endPanel.SetActive(false);
        var tEndTitle  = Lbl("EndTitle",   "Fin de manche", endPanel.transform, new Vector2(0,180), 26, C_GOLD,  true);
        var tEndDet    = Lbl("EndDetails", "",              endPanel.transform, new Vector2(0, 30), 15, C_WHITE, false);
        var nextBt     = Btn("NextBtn",    endPanel.transform,"Nouvelle manche", new Vector2(290,58), AnchorMid, new Vector2(0,-190), C_BTN_GRN);

        // ── Panneau Message flash ──────────────────────────────────────────
        var msgPanel   = Panel("MsgPanel", cvT, new Vector2(620,110), AnchorMid, new Vector2(0,100), C_PANEL);
        msgPanel.SetActive(false);
        var tMsgText   = Lbl("MsgText","", msgPanel.transform, Vector2.zero, 18, C_WHITE, false);

        // ── Panneau Belote/Rebelote ────────────────────────────────────────
        var belPanel   = Panel("BelotePanel", cvT, new Vector2(420,84), AnchorMid, new Vector2(0,210), new Color(1f,0.84f,0f,0.96f));
        belPanel.SetActive(false);
        var tBelText   = Lbl("BeloteText","Belote !", belPanel.transform, Vector2.zero, 22, C_BLK_SUIT, true);

        // ── Prefabs ────────────────────────────────────────────────────────
        const string PF = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(PF)) AssetDatabase.CreateFolder("Assets","Prefabs");
        MakeCardPrefab(PF);
        MakeCardBackPrefab(PF);
        MakeTrickCardPrefab(PF);

        // ── GameObjects logique ────────────────────────────────────────────
        var gmGo = new GameObject("GameManager"); gmGo.AddComponent<RectTransform>();
        var gm   = gmGo.AddComponent<GameManager>();
        var uimGo = new GameObject("UIManager");  uimGo.AddComponent<RectTransform>();
        var uim   = uimGo.AddComponent<UIManager>();
        gm.UIManager = uim;

        // Brancher UIManager
        uim.ScoreUsText         = tScoreUs;
        uim.ScoreThemText       = tScoreThem;
        uim.TrumpIndicatorText  = tTrumpInd;
        uim.TrumpPanel          = tPanel;
        uim.TrumpPanelTitle     = tTitle;
        uim.TrumpCardText       = tCardTxt;
        uim.TakeButton          = takeBt.GetComponent<Button>();
        uim.PassButton          = passBt.GetComponent<Button>();
        uim.SuitButtonsContainer= suitCont;
        uim.SuitButtons         = suitBtns;
        uim.HumanHandContainer  = humanHand.transform;
        uim.AIHandContainers    = new Transform[]{ aiLeftHand.transform, aiTopHand.transform, aiRightHand.transform };
        uim.CardPrefab          = AssetDatabase.LoadAssetAtPath<GameObject>(PF+"/CardPrefab.prefab");
        uim.CardBackPrefab      = AssetDatabase.LoadAssetAtPath<GameObject>(PF+"/CardBackPrefab.prefab");
        uim.TrickCardPrefab     = AssetDatabase.LoadAssetAtPath<GameObject>(PF+"/TrickCardPrefab.prefab");
        uim.TrickSlots          = new Transform[]{ slotSud.transform, slotOuest.transform, slotNord.transform, slotEst.transform };
        uim.MessagePanel        = msgPanel;
        uim.MessageText         = tMsgText;
        uim.BeloteAnnouncePanel = belPanel;
        uim.BeloteAnnounceText  = tBelText;
        uim.EndPanel            = endPanel;
        uim.EndTitleText        = tEndTitle;
        uim.EndDetailsText      = tEndDet;
        uim.NextHandButton      = nextBt.GetComponent<Button>();
        uim.InfoBarText         = tInfoBar;
        uim.LastTrickButton     = lastBtn.GetComponent<Button>();
        uim.LastTrickPanel      = lpPanel;
        uim.LastTrickText       = tLPText;

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
        Debug.Log("[BeloteFreeze] GameScene construite avec succes !");
    }

    // ── Prefabs ───────────────────────────────────────────────────────────────
    static void MakeCardPrefab(string dir)
    {
        var root = new GameObject("CardPrefab");
        root.AddComponent<CanvasGroup>();
        var rt = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(72,100);
        // Supprimer doublon RectTransform ajouté par AddComponent si déjà présent
        var bg = root.AddComponent<Image>(); bg.color = Color.white;

        var txtGo = new GameObject("CardLabel"); txtGo.transform.SetParent(root.transform,false);
        var t = txtGo.AddComponent<Text>();
        t.text      = "A♠";
        t.fontSize  = 20;
        t.color     = new Color(0.1f,0.1f,0.1f);
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        FillParent(txtGo.GetComponent<RectTransform>());

        var hlGo = new GameObject("TrumpHighlight"); hlGo.transform.SetParent(root.transform,false);
        var hlImg = hlGo.AddComponent<Image>(); hlImg.color = new Color(1f,0.84f,0f,0.45f);
        FillParentExpanded(hlGo.GetComponent<RectTransform>(), 3);
        hlGo.SetActive(false);

        var cv = root.AddComponent<CardView>();
        cv.CardLabel       = t;
        cv.CardBackground  = bg;
        cv.TrumpHighlight  = hlImg;
        root.AddComponent<Button>();

        PrefabUtility.SaveAsPrefabAsset(root, dir+"/CardPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    static void MakeCardBackPrefab(string dir)
    {
        var root = new GameObject("CardBackPrefab");
        var rt = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(72,100);
        var bg = root.AddComponent<Image>(); bg.color = new Color(0.12f,0.25f,0.60f);
        PrefabUtility.SaveAsPrefabAsset(root, dir+"/CardBackPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    static void MakeTrickCardPrefab(string dir)
    {
        var root = new GameObject("TrickCardPrefab");
        root.AddComponent<CanvasGroup>();
        var rt = root.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(80,112);
        var bg = root.AddComponent<Image>(); bg.color = Color.white;

        var txtGo = new GameObject("CardLabel"); txtGo.transform.SetParent(root.transform,false);
        var t = txtGo.AddComponent<Text>();
        t.text = "A♠"; t.fontSize = 22; t.color = new Color(0.1f,0.1f,0.1f);
        t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
        FillParent(txtGo.GetComponent<RectTransform>());

        var hlGo  = new GameObject("TrumpHighlight"); hlGo.transform.SetParent(root.transform,false);
        var hlImg = hlGo.AddComponent<Image>(); hlImg.color = new Color(1f,0.84f,0f,0.55f);
        FillParentExpanded(hlGo.GetComponent<RectTransform>(), 4);
        hlGo.SetActive(false);

        var cv = root.AddComponent<CardView>();
        cv.CardLabel = t; cv.CardBackground = bg; cv.TrumpHighlight = hlImg;

        PrefabUtility.SaveAsPrefabAsset(root, dir+"/TrickCardPrefab.prefab");
        Object.DestroyImmediate(root);
    }

    // ── Helpers UI ────────────────────────────────────────────────────────────
    static Vector2 AnchorTop  = new Vector2(0.5f,1f);
    static Vector2 AnchorBot  = new Vector2(0.5f,0f);
    static Vector2 AnchorBotL = new Vector2(0f,0f);
    static Vector2 AnchorLeft = new Vector2(0f,0.5f);
    static Vector2 AnchorRight= new Vector2(1f,0.5f);
    static Vector2 AnchorMid  = new Vector2(0.5f,0.5f);

    static GameObject Panel(string name, Transform parent, Vector2 size,
        Vector2 anchor, Vector2 pos, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
        if (col.a > 0.01f) { var img = go.AddComponent<Image>(); img.color = col; img.raycastTarget = false; }
        return go;
    }

    static Text Lbl(string name, string txt, Transform parent, Vector2 pos,
        float size, Color col, bool bold)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AnchorMid; rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(400,50);
        var t = go.AddComponent<Text>();
        t.text = txt; t.fontSize = (int)size; t.color = col;
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
        var img = go.AddComponent<Image>(); img.color = col;
        go.AddComponent<Button>();

        var tGo = new GameObject("Label"); tGo.transform.SetParent(go.transform,false);
        FillParent(tGo.AddComponent<RectTransform>());
        var t = tGo.AddComponent<Text>();
        t.text = label; t.fontSize = 17; t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static GameObject Container(string name, Transform parent, Vector2 pos, Vector2 size, bool horiz)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AnchorMid; rt.anchoredPosition = pos; rt.sizeDelta = size;
        if (horiz) {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 4; h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = false; h.childControlHeight = false;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;
        } else {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = 4; v.childAlignment = TextAnchor.MiddleCenter;
            v.childControlWidth = false; v.childControlHeight = false;
            v.childForceExpandWidth = false; v.childForceExpandHeight = false;
        }
        return go;
    }

    static GameObject Slot(string name, Transform parent, Vector2 pos)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = AnchorMid; rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(88,118);
        return go;
    }

    static void FillParent(RectTransform rt)
    { rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero; }

    static void FillParentExpanded(RectTransform rt, float expand)
    { rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
      rt.offsetMin=new Vector2(-expand,-expand); rt.offsetMax=new Vector2(expand,expand); }
}
