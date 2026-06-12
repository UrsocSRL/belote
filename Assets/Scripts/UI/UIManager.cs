using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// UIManager — IMGUI (OnGUI) uniquement.
    /// Zéro dépendance externe : fonctionne dans Unity 6 sans aucun package.
    /// Bible Demo V0.1 : carte touchée = jouée immédiatement.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Styles ────────────────────────────────────────────────────────────
        GUIStyle _styleCard;
        GUIStyle _styleCardForbidden;
        GUIStyle _styleCardBack;
        GUIStyle _styleTrumpCard;
        GUIStyle _stylePanel;
        GUIStyle _styleBtnGreen;
        GUIStyle _styleBtnRed;
        GUIStyle _styleBtnSuit;
        GUIStyle _styleLabel;
        GUIStyle _styleLabelBig;
        GUIStyle _styleLabelGold;
        GUIStyle _styleLabelSmall;
        GUIStyle _styleInfoBar;
        GUIStyle _styleBelote;
        bool _stylesInit;

        // ── Textures ──────────────────────────────────────────────────────────
        Texture2D _texGreen;
        Texture2D _texBlue;
        Texture2D _texWhite;
        Texture2D _texBlack;
        Texture2D _texYellow;
        Texture2D _texDarkPanel;
        Texture2D _texBtnGreen;
        Texture2D _texBtnRed;

        // ── État UI ───────────────────────────────────────────────────────────
        List<Card> _humanHand      = new();
        List<Card> _allowedCards   = new();
        List<TrickPlay> _currentTrick  = new();
        List<TrickPlay> _lastTrick     = new();
        int[]      _aiCardCounts   = new int[3]; // Ouest Nord Est

        string  _infoText    = "Chargement...";
        string  _messageText = "";
        float   _messageTimer;
        string  _beloteText  = "";
        float   _beloteTimer;

        bool    _showTrumpPanel;
        int     _trumpAsker;
        TrumpPhase _trumpPhase;
        Card    _trumpCard;
        Suit    _trump;
        int     _takerSeat;

        bool    _showEndPanel;
        string  _endTitle   = "";
        string  _endDetails = "";
        int     _totalUs, _totalThem;

        bool    _showLastTrick;
        float   _lastTrickTimer;

        // Scroll pour la main
        Vector2 _handScroll;

        // ─────────────────────────────────────────────────────────────────────
        void InitStyles()
        {
            if (_stylesInit) return;
            _stylesInit = true;

            _texGreen     = MakeTex(new Color(0.08f,0.36f,0.16f));
            _texBlue      = MakeTex(new Color(0.12f,0.25f,0.60f));
            _texWhite     = MakeTex(Color.white);
            _texBlack     = MakeTex(new Color(0.1f,0.1f,0.1f));
            _texYellow    = MakeTex(new Color(1f,0.84f,0f));
            _texDarkPanel = MakeTex(new Color(0,0,0,0.78f));
            _texBtnGreen  = MakeTex(new Color(0.17f,0.50f,0.17f));
            _texBtnRed    = MakeTex(new Color(0.55f,0.15f,0.15f));

            _styleCard = new GUIStyle(GUI.skin.button)
            {
                fontSize  = Sz(18), fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { background = _texWhite, textColor = Color.black },
                hover     = { background = _texYellow, textColor = Color.black },
                active    = { background = _texYellow, textColor = Color.black },
                padding   = new RectOffset(4,4,4,4)
            };
            _styleCardForbidden = new GUIStyle(_styleCard)
            { normal = { background = _texWhite, textColor = new Color(0.5f,0.5f,0.5f,0.4f) } };
            _styleCardBack = new GUIStyle(GUI.skin.box)
            { normal = { background = _texBlue, textColor = Color.white } };
            _styleTrumpCard = new GUIStyle(_styleCard)
            { normal = { background = MakeTex(new Color(1f,0.98f,0.85f)), textColor = Color.black } };
            _stylePanel = new GUIStyle(GUI.skin.box)
            { normal = { background = _texDarkPanel, textColor = Color.white } };
            _styleBtnGreen = new GUIStyle(GUI.skin.button)
            {
                fontSize = Sz(16), fontStyle = FontStyle.Bold,
                normal  = { background = _texBtnGreen, textColor = Color.white },
                hover   = { background = MakeTex(new Color(0.22f,0.62f,0.22f)), textColor = Color.white },
                active  = { background = _texBtnGreen, textColor = Color.white }
            };
            _styleBtnRed = new GUIStyle(_styleBtnGreen)
            {
                normal = { background = _texBtnRed, textColor = Color.white },
                hover  = { background = MakeTex(new Color(0.70f,0.20f,0.20f)), textColor = Color.white },
                active = { background = _texBtnRed, textColor = Color.white }
            };
            _styleBtnSuit = new GUIStyle(GUI.skin.button)
            {
                fontSize  = Sz(26), fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { background = _texWhite, textColor = Color.black }
            };
            _styleLabel = new GUIStyle(GUI.skin.label)
            { fontSize = Sz(16), fontStyle = FontStyle.Normal, alignment = TextAnchor.MiddleCenter,
              normal   = { textColor = Color.white } };
            _styleLabelBig = new GUIStyle(_styleLabel)
            { fontSize = Sz(26), fontStyle = FontStyle.Bold };
            _styleLabelGold = new GUIStyle(_styleLabelBig)
            { normal = { textColor = new Color(1f,0.84f,0f) } };
            _styleLabelSmall = new GUIStyle(_styleLabel) { fontSize = Sz(13) };
            _styleInfoBar = new GUIStyle(_styleLabel) { fontSize = Sz(15) };
            _styleBelote = new GUIStyle(_styleLabelBig)
            { normal = { background = _texYellow, textColor = Color.black } };
        }

        // ─────────────────────────────────────────────────────────────────────
        void Update()
        {
            if (_messageTimer > 0) _messageTimer -= Time.deltaTime;
            if (_beloteTimer  > 0) _beloteTimer  -= Time.deltaTime;
            if (_lastTrickTimer > 0) { _lastTrickTimer -= Time.deltaTime; if (_lastTrickTimer <= 0) _showLastTrick = false; }
        }

        // ── OnGUI principal ───────────────────────────────────────────────────
        void OnGUI()
        {
            InitStyles();
            float W = Screen.width, H = Screen.height;

            // Fond
            GUI.DrawTexture(new Rect(0,0,W,H), _texGreen, ScaleMode.StretchToFill);

            // Score
            DrawScoreBar(W, H);
            // IA Haut
            DrawAITop(W, H);
            // IA Gauche
            DrawAILeft(W, H);
            // IA Droite
            DrawAIRight(W, H);
            // Table pli
            DrawTable(W, H);
            // Main joueur
            DrawHumanHand(W, H);
            // Info bar
            DrawInfoBar(W, H);
            // Bouton dernier pli
            DrawLastTrickBtn(W, H);

            // Panneaux modaux (par-dessus tout)
            if (_showTrumpPanel) DrawTrumpPanel(W, H);
            if (_showEndPanel)   DrawEndPanel(W, H);
            if (_showLastTrick)  DrawLastTrickPanel(W, H);
            if (_messageTimer > 0) DrawMessage(W, H);
            if (_beloteTimer  > 0) DrawBelote(W, H);
        }

        // ── Score ─────────────────────────────────────────────────────────────
        void DrawScoreBar(float W, float H)
        {
            float bh = H * 0.06f;
            GUI.Box(new Rect(0,0,W,bh), "", _stylePanel);
            var gm = GameManager.Instance;
            GUI.Label(new Rect(W*0.15f,0,W*0.2f,bh),
                "Nous\n" + (gm != null ? gm._scoreMgrTotal[0].ToString() : "0"), _styleLabel);
            GUI.Label(new Rect(W*0.65f,0,W*0.2f,bh),
                "Eux\n"  + (gm != null ? gm._scoreMgrTotal[1].ToString() : "0"), _styleLabel);
            if (_trump != default)
                GUI.Label(new Rect(W*0.38f,0,W*0.24f,bh), "Atout: " + SuitStr(_trump), _styleLabelGold);
        }

        // ── IA Haut (Nord) ────────────────────────────────────────────────────
        void DrawAITop(float W, float H)
        {
            float cardW = W*0.06f, cardH = H*0.055f, gap = W*0.008f;
            int n = _aiCardCounts[1]; // Nord = index 1
            float totalW = n*(cardW+gap);
            float startX = W/2 - totalW/2;
            float y = H*0.07f;
            GUI.Label(new Rect(W/2-40, y, 80, 20), "Nord", _styleLabelSmall);
            for (int i = 0; i < n; i++)
                GUI.Box(new Rect(startX + i*(cardW+gap), y+22, cardW, cardH), "", _styleCardBack);
        }

        // ── IA Gauche (Ouest) ─────────────────────────────────────────────────
        void DrawAILeft(float W, float H)
        {
            float cardW = W*0.055f, cardH = H*0.05f, gap = H*0.007f;
            int n = _aiCardCounts[0]; // Ouest = index 0
            float totalH = n*(cardH+gap);
            float startY = H/2 - totalH/2;
            GUI.Label(new Rect(4, H/2-10, 60, 20), "Ouest", _styleLabelSmall);
            for (int i = 0; i < n; i++)
                GUI.Box(new Rect(6, startY + i*(cardH+gap), cardW, cardH), "", _styleCardBack);
        }

        // ── IA Droite (Est) ───────────────────────────────────────────────────
        void DrawAIRight(float W, float H)
        {
            float cardW = W*0.055f, cardH = H*0.05f, gap = H*0.007f;
            int n = _aiCardCounts[2]; // Est = index 2
            float totalH = n*(cardH+gap);
            float startY = H/2 - totalH/2;
            GUI.Label(new Rect(W-64, H/2-10, 60, 20), "Est", _styleLabelSmall);
            for (int i = 0; i < n; i++)
                GUI.Box(new Rect(W-6-cardW, startY + i*(cardH+gap), cardW, cardH), "", _styleCardBack);
        }

        // ── Table pli central ─────────────────────────────────────────────────
        void DrawTable(float W, float H)
        {
            float cw = W*0.09f, ch = H*0.08f;
            float cx = W/2, cy = H/2 - H*0.04f;
            // Positions : Sud bas, Ouest gauche, Nord haut, Est droite
            Vector2[] positions =
            {
                new Vector2(cx - cw/2,        cy + ch*0.8f),   // 0=Sud
                new Vector2(cx - cw*1.8f,      cy),             // 1=Ouest
                new Vector2(cx - cw/2,        cy - ch*1.1f),   // 2=Nord
                new Vector2(cx + cw*0.8f,      cy),             // 3=Est
            };
            foreach (var play in _currentTrick)
            {
                var pos = positions[play.PlayerSeat];
                bool isTrump = play.Card.Suit == _trump;
                bool isWinner = play.PlayerSeat == GetCurrentTrickWinner();
                var style = isTrump ? _styleTrumpCard : _styleCard;
                string label = CardStr(play.Card);
                var rect = new Rect(pos.x, pos.y, cw, ch);
                if (isWinner && _currentTrick.Count < 4)
                    GUI.DrawTexture(new Rect(pos.x-3,pos.y-3,cw+6,ch+6), _texYellow);
                GUI.Box(rect, label, style);
            }
        }

        // ── Main joueur ───────────────────────────────────────────────────────
        void DrawHumanHand(float W, float H)
        {
            if (_humanHand.Count == 0) return;
            float cardW = Mathf.Min(W * 0.09f, (W - 20f) / _humanHand.Count);
            float cardH = H * 0.10f;
            float gap   = cardW * 0.15f;
            float totalW = _humanHand.Count * (cardW + gap);
            float startX = W/2 - totalW/2;
            float y = H - cardH - H*0.015f;

            for (int i = 0; i < _humanHand.Count; i++)
            {
                var card = _humanHand[i];
                bool allowed = _allowedCards.Contains(card);
                bool isTrump = card.Suit == _trump;
                var style = !allowed ? _styleCardForbidden : (isTrump ? _styleTrumpCard : _styleCard);
                string label = CardStr(card);
                var rect = new Rect(startX + i*(cardW+gap), y, cardW, cardH);
                bool isRed = card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds;
                var oldColor = GUI.color;
                if (!allowed) GUI.color = new Color(1,1,1,0.4f);
                else if (isRed) GUI.contentColor = new Color(0.8f,0.1f,0.1f);

                if (GUI.Button(rect, label, style) && allowed)
                    GameManager.Instance.HumanCardTouched(card);

                GUI.color = oldColor;
                GUI.contentColor = Color.white;
            }
        }

        // ── Info bar ──────────────────────────────────────────────────────────
        void DrawInfoBar(float W, float H)
        {
            float bh = H*0.04f;
            float y  = H - H*0.13f - bh;
            GUI.Box(new Rect(W*0.1f, y, W*0.8f, bh), "", _stylePanel);
            GUI.Label(new Rect(W*0.1f, y, W*0.8f, bh), _infoText, _styleInfoBar);
        }

        // ── Bouton Dernier Pli ────────────────────────────────────────────────
        void DrawLastTrickBtn(float W, float H)
        {
            if (_lastTrick.Count == 0) return;
            float bw = W*0.22f, bh = H*0.04f;
            float y  = H - H*0.13f - bh;
            if (GUI.Button(new Rect(W*0.78f+4, y, bw*0.8f, bh), "Dernier pli", _styleBtnGreen))
            {
                _showLastTrick  = true;
                _lastTrickTimer = 2.8f;
            }
        }

        // ── Panneau Prise d'Atout ─────────────────────────────────────────────
        void DrawTrumpPanel(float W, float H)
        {
            float pw = W*0.70f, ph = H*0.38f;
            float px = W/2-pw/2, py = H/2-ph/2;
            GUI.Box(new Rect(px,py,pw,ph), "", _stylePanel);

            string titleTxt = _trumpAsker == 0
                ? (_trumpPhase == TrumpPhase.Round1
                    ? "Prendre a " + SuitStr(_trumpCard.Suit) + " ?"
                    : "Choisir l'atout (sauf " + SuitStr(_trumpCard.Suit) + ")")
                : PlayerName(_trumpAsker) + " reflechit...";

            GUI.Label(new Rect(px, py+8, pw, H*0.05f), titleTxt, _styleLabel);

            // Carte retournée
            float cw = W*0.13f, ch = H*0.11f;
            bool isTrump = _trumpPhase == TrumpPhase.Done;
            GUI.Box(new Rect(W/2-cw/2, py+ph*0.22f, cw, ch), CardStr(_trumpCard),
                    _trumpCard.Suit == Suit.Hearts || _trumpCard.Suit == Suit.Diamonds ? _styleTrumpCard : _styleCard);

            if (_trumpAsker != 0) return;

            float bw = pw*0.35f, bh = H*0.065f;
            float by = py + ph - bh - ph*0.08f;

            if (_trumpPhase == TrumpPhase.Round1)
            {
                if (GUI.Button(new Rect(W/2-bw-8, by, bw, bh), "Prendre", _styleBtnGreen))
                    GameManager.Instance.HumanTake();
                if (GUI.Button(new Rect(W/2+8, by, bw, bh), "Passer", _styleBtnRed))
                    GameManager.Instance.HumanPass();
            }
            else
            {
                Suit[] suits  = {Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs};
                string[] sym  = {"♠","♥","♦","♣"};
                Color[] cols  = {Color.black, new Color(0.8f,0.1f,0.1f), new Color(0.8f,0.1f,0.1f), Color.black};
                float sbw = pw*0.18f, sbh = H*0.075f;
                float totalSW = 4*sbw + 3*8f;
                float sx = W/2-totalSW/2;
                for (int i = 0; i < 4; i++)
                {
                    if (suits[i] == _trumpCard.Suit) { sx += sbw+8; continue; }
                    GUI.contentColor = cols[i];
                    if (GUI.Button(new Rect(sx, by, sbw, sbh), sym[i], _styleBtnSuit))
                        GameManager.Instance.HumanTakeSuit(suits[i]);
                    GUI.contentColor = Color.white;
                    sx += sbw + 8;
                }
                if (GUI.Button(new Rect(W/2-bw/2, by+sbh+6, bw, bh*0.8f), "Passer", _styleBtnRed))
                    GameManager.Instance.HumanPass();
            }
        }

        // ── Panneau Fin de manche ─────────────────────────────────────────────
        void DrawEndPanel(float W, float H)
        {
            float pw = W*0.80f, ph = H*0.55f;
            float px = W/2-pw/2, py = H/2-ph/2;
            GUI.Box(new Rect(px,py,pw,ph), "", _stylePanel);
            GUI.Label(new Rect(px, py+12, pw, H*0.06f), _endTitle,   _styleLabelGold);
            GUI.Label(new Rect(px+20, py+ph*0.18f, pw-40, ph*0.55f), _endDetails, _styleLabel);
            float bw = pw*0.5f, bh = H*0.065f;
            if (GUI.Button(new Rect(W/2-bw/2, py+ph-bh-16, bw, bh), "Nouvelle manche", _styleBtnGreen))
            {
                _showEndPanel = false;
                GameManager.Instance.OnNextHandRequested();
            }
        }

        // ── Panneau Dernier Pli ───────────────────────────────────────────────
        void DrawLastTrickPanel(float W, float H)
        {
            float pw = W*0.75f, ph = H*0.22f;
            float px = W/2-pw/2, py = H/2-ph/2;
            GUI.Box(new Rect(px,py,pw,ph), "", _stylePanel);
            var sb = new StringBuilder("Dernier pli :\n");
            foreach (var p in _lastTrick)
                sb.Append(PlayerName(p.PlayerSeat) + ": " + CardStr(p.Card) + "   ");
            GUI.Label(new Rect(px+8, py+8, pw-16, ph-16), sb.ToString(), _styleLabel);
        }

        // ── Message flash ─────────────────────────────────────────────────────
        void DrawMessage(float W, float H)
        {
            float pw = W*0.70f, ph = H*0.09f;
            GUI.Box(new Rect(W/2-pw/2, H/2-ph/2+H*0.1f, pw, ph), "", _stylePanel);
            GUI.Label(new Rect(W/2-pw/2, H/2-ph/2+H*0.1f, pw, ph), _messageText, _styleLabel);
        }

        // ── Annonce Belote ────────────────────────────────────────────────────
        void DrawBelote(float W, float H)
        {
            float pw = W*0.55f, ph = H*0.07f;
            GUI.Box(new Rect(W/2-pw/2, H*0.42f, pw, ph), _beloteText, _styleBelote);
        }

        // ── API publique appelée par GameManager ──────────────────────────────
        public void OnDeal(Player[] players, Card trumpCard)
        {
            _humanHand    = new List<Card>(players[0].Hand);
            _allowedCards.Clear();
            _currentTrick.Clear();
            _lastTrick.Clear();
            for (int i = 0; i < 3; i++) _aiCardCounts[i] = players[i+1].Hand.Count;
            _trump = default;
            _showEndPanel = false;
        }

        public void OnTrumpAsk(int askerSeat, TrumpPhase phase, Card trumpCard)
        {
            _showTrumpPanel = true;
            _trumpAsker = askerSeat;
            _trumpPhase = phase;
            _trumpCard  = trumpCard;
        }

        public void OnTrumpChosen(Suit trump, int takerSeat)
        {
            _showTrumpPanel = false;
            _trump      = trump;
            _takerSeat  = takerSeat;
            _infoText   = PlayerName(takerSeat) + " prend a " + SuitStr(trump);
        }

        public void OnPlayerTurn(int seat)
        {
            _infoText = seat == 0 ? "A vous de jouer" : PlayerName(seat) + " joue...";
        }

        public void HighlightAllowedCards(List<Card> allowed)
        {
            _allowedCards = new List<Card>(allowed);
        }

        public void OnCardPlayed(int seat, Card card, List<TrickPlay> trick)
        {
            _currentTrick = new List<TrickPlay>(trick);
            if (seat == 0) _humanHand.Remove(card);
            else _aiCardCounts[seat-1] = GameManager.Instance.GetPlayerHandCount(seat);
        }

        public void OnTrickEnd(int winnerSeat, int pts)
        {
            _lastTrick    = new List<TrickPlay>(GameManager.Instance.GetLastTrick());
            _infoText     = PlayerName(winnerSeat) + " remporte le pli (" + pts + " pts)";
            _allowedCards.Clear();
            StartCoroutine(ClearTrickDelayed(0.7f));
        }

        public void OnRoundEnd(RoundResult result, int totalUs, int totalThem)
        {
            _showEndPanel = true;
            _totalUs      = totalUs;
            _totalThem    = totalThem;
            _endTitle     = result.Outcome switch
            {
                RoundOutcome.Capot        => "Capot !",
                RoundOutcome.CapotAdverse => "Capot adverse !",
                RoundOutcome.Dedans       => "Dedans !",
                RoundOutcome.Litige       => "Litige !",
                _                         => "Fin de manche"
            };
            _endDetails = result.Description + "\n\nTotal — Nous : " + totalUs + " | Eux : " + totalThem;
        }

        public void ShowBeloteAnnounce(int seat, string announcement)
        {
            _beloteText  = PlayerName(seat) + " : " + announcement;
            _beloteTimer = 1.5f;
        }

        public void ShowMessage(string text, float duration)
        {
            _messageText  = text;
            _messageTimer = duration;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        IEnumerator ClearTrickDelayed(float d)
        { yield return new WaitForSeconds(d); _currentTrick.Clear(); }

        int GetCurrentTrickWinner()
        {
            if (_currentTrick.Count == 0) return -1;
            return RuleEngine.GetTrickWinner(_currentTrick, _trump);
        }

        string CardStr(Card c)
        {
            bool red = c.Suit == Suit.Hearts || c.Suit == Suit.Diamonds;
            return c.RankSymbol() + c.SuitSymbol();
        }

        string SuitStr(Suit s) => s switch
        { Suit.Spades=>"♠", Suit.Hearts=>"♥", Suit.Diamonds=>"♦", Suit.Clubs=>"♣", _=>"?" };

        public static string PlayerName(int seat) => seat switch
        { 0=>"Vous", 1=>"Ouest", 2=>"Nord", 3=>"Est", _=>"?" };

        int Sz(int px) => Mathf.Max(10, Mathf.RoundToInt(px * Screen.height / 900f));

        Texture2D MakeTex(Color col)
        {
            var t = new Texture2D(1,1);
            t.SetPixel(0,0,col); t.Apply(); return t;
        }
    }
}
