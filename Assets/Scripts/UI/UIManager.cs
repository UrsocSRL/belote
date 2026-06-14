using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// UIManager — UnityEngine.UI.Text (module UI Built-in Unity 6).
    /// Bible Demo V0.1 : carte touchée = jouée immédiatement, pas de bouton Jouer.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Scores")]
        public Text ScoreUsText;
        public Text ScoreThemText;
        public Text TrumpIndicatorText;

        [Header("Trump Panel")]
        public GameObject TrumpPanel;
        public Text       TrumpPanelTitle;
        public Text       TrumpCardText;
        public Button     TakeButton;
        public Button     PassButton;
        public GameObject SuitButtonsContainer;
        public Button[]   SuitButtons;

        [Header("Hands")]
        public Transform   HumanHandContainer;
        public Transform[] AIHandContainers;
        public GameObject  CardPrefab;
        public GameObject  CardBackPrefab;
        public GameObject  TrickCardPrefab;

        [Header("Table — [0]=Sud [1]=Ouest [2]=Nord [3]=Est")]
        public Transform[] TrickSlots;

        [Header("Messages")]
        public GameObject MessagePanel;
        public Text       MessageText;
        public GameObject BeloteAnnouncePanel;
        public Text       BeloteAnnounceText;

        [Header("End Panel")]
        public GameObject EndPanel;
        public Text       EndTitleText;
        public Text       EndDetailsText;
        public Button     NextHandButton;

        [Header("Info")]
        public Text       InfoBarText;
        public Button     LastTrickButton;
        public GameObject LastTrickPanel;
        public Text       LastTrickText;

        [Header("Table — Carte retournée")]
        public Transform  TurnedCardSlot;

        [Header("Decor")]
        public CardArtSet CardArt;
        public Image      BackgroundImage;
        public Image      TableImage;
        public Image[]    AIAvatars; // [0]=Ouest [1]=Nord [2]=Est

        readonly List<GameObject> _humanCardObjects = new();
        List<Card>      _currentHumanHand = new();
        List<TrickPlay> _lastTrickData    = new();

        GameObject _turnedCardObj;
        Coroutine  _turnedCardPulse;

        void Awake()
        {
            CardView.ArtSet = CardArt;
            if (CardArt != null)
            {
                if (BackgroundImage != null && CardArt.Background != null) BackgroundImage.sprite = CardArt.Background;
                if (TableImage != null && CardArt.Table != null) TableImage.sprite = CardArt.Table;
                if (AIAvatars != null)
                {
                    Sprite[] avatarSeq = { CardArt.AvatarMale, CardArt.AvatarFemale, CardArt.AvatarMale };
                    for (int i = 0; i < AIAvatars.Length && i < avatarSeq.Length; i++)
                        if (AIAvatars[i] != null && avatarSeq[i] != null) AIAvatars[i].sprite = avatarSeq[i];
                }
            }
        }

        void Start()
        {
            TrumpPanel?.SetActive(false);
            EndPanel?.SetActive(false);
            MessagePanel?.SetActive(false);
            BeloteAnnouncePanel?.SetActive(false);
            LastTrickPanel?.SetActive(false);

            NextHandButton?.onClick.AddListener(() => GameManager.Instance.OnNextHandRequested());
            TakeButton?.onClick.AddListener(()    => GameManager.Instance.HumanTake());
            PassButton?.onClick.AddListener(()    => GameManager.Instance.HumanPass());

            Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            if (SuitButtons != null)
                for (int i = 0; i < SuitButtons.Length && i < 4; i++)
                { Suit s = suits[i]; SuitButtons[i].onClick.AddListener(() => GameManager.Instance.HumanTakeSuit(s)); }

            LastTrickButton?.onClick.AddListener(ShowLastTrick);
            LastTrickButton?.gameObject.SetActive(false);
        }

        public void OnDeal(Player[] players, Card trumpCard)
        {
            EndPanel?.SetActive(false);
            for (int ai = 0; ai < 3; ai++)
                if (AIHandContainers != null && ai < AIHandContainers.Length)
                    RenderAIBack(AIHandContainers[ai], players[ai + 1].Hand.Count);
            _currentHumanHand = new List<Card>(players[0].Hand);
            RenderHumanHand(_currentHumanHand);
            ClearTrick();
            _lastTrickData.Clear();
            LastTrickButton?.gameObject.SetActive(false);
            if (TrumpIndicatorText != null) TrumpIndicatorText.text = "";
            HideTurnedCard();
        }

        /// <summary>
        /// Met à jour l'affichage des mains après la distribution complémentaire.
        /// </summary>
        public void RefreshHands(Player[] players)
        {
            for (int ai = 0; ai < 3; ai++)
                if (AIHandContainers != null && ai < AIHandContainers.Length)
                    RenderAIBack(AIHandContainers[ai], players[ai + 1].Hand.Count);
            _currentHumanHand = new List<Card>(players[0].Hand);
            RenderHumanHand(_currentHumanHand);
        }

        public void OnTrumpAsk(int askerSeat, TrumpPhase phase, Card trumpCard)
        {
            TrumpPanel?.SetActive(true);
            if (TrumpCardText != null) TrumpCardText.gameObject.SetActive(false);
            ShowTurnedCard(trumpCard);
            SetInfoBar("");
            if (askerSeat == 0)
            {
                if (phase == TrumpPhase.Round1)
                {
                    if (TrumpPanelTitle != null) TrumpPanelTitle.text = "Prendre a " + trumpCard.SuitSymbol() + " ?";
                    TakeButton?.gameObject.SetActive(true);
                    SuitButtonsContainer?.SetActive(false);
                }
                else
                {
                    if (TrumpPanelTitle != null) TrumpPanelTitle.text = "Choisir l'atout (sauf " + trumpCard.SuitSymbol() + ")";
                    TakeButton?.gameObject.SetActive(false);
                    SuitButtonsContainer?.SetActive(true);
                    Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
                    if (SuitButtons != null)
                        for (int i = 0; i < SuitButtons.Length && i < 4; i++)
                            SuitButtons[i].interactable = suits[i] != trumpCard.Suit;
                }
                PassButton?.gameObject.SetActive(true);
            }
            else
            {
                if (TrumpPanelTitle != null) TrumpPanelTitle.text = PlayerName(askerSeat) + " reflechit...";
                TakeButton?.gameObject.SetActive(false);
                PassButton?.gameObject.SetActive(false);
                SuitButtonsContainer?.SetActive(false);
            }
        }

        public void OnTrumpChosen(Suit trump, int takerSeat)
        {
            TrumpPanel?.SetActive(false);
            HideTurnedCard();
            if (TrumpIndicatorText != null) TrumpIndicatorText.text = PlayerName(takerSeat) + " a pris : " + SuitSymbol(trump) + " " + SuitName(trump);
            SetInfoBar(PlayerName(takerSeat) + " prend a " + SuitSymbol(trump));
        }

        // ── CARTE RETOURNEE (centre table) ────────────────────────────────────
        void ShowTurnedCard(Card trumpCard)
        {
            if (_turnedCardObj == null)
            {
                var prefab = TrickCardPrefab ? TrickCardPrefab : CardPrefab;
                if (!prefab || !TurnedCardSlot) return;
                _turnedCardObj = Instantiate(prefab, TurnedCardSlot);
                var cv = _turnedCardObj.GetComponent<CardView>();
                if (cv != null) { cv.SetCard(trumpCard, default); cv.SetPlayable(false); }
                _turnedCardPulse = StartCoroutine(PulseTurnedCard());
            }
        }

        void HideTurnedCard()
        {
            if (_turnedCardPulse != null) { StopCoroutine(_turnedCardPulse); _turnedCardPulse = null; }
            if (_turnedCardObj != null) { Destroy(_turnedCardObj); _turnedCardObj = null; }
        }

        IEnumerator PulseTurnedCard()
        {
            const float speed = 3f;
            const float amplitude = 0.06f;
            while (_turnedCardObj != null)
            {
                float scale = 1f + Mathf.Sin(Time.time * speed) * amplitude;
                _turnedCardObj.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }

        public void OnPlayerTurn(int seat)
        {
            SetInfoBar(seat == 0 ? "A vous de jouer" : PlayerName(seat) + " joue...");
        }

        public void HighlightAllowedCards(List<Card> allowedCards)
        {
            for (int i = 0; i < _humanCardObjects.Count && i < _currentHumanHand.Count; i++)
            {
                var go = _humanCardObjects[i];
                if (!go) continue;
                bool ok = allowedCards.Contains(_currentHumanHand[i]);
                var cv  = go.GetComponent<CardView>();
                if (cv != null) { cv.SetPlayable(ok); continue; }
                var btn = go.GetComponent<Button>();
                if (btn != null) btn.interactable = ok;
            }
        }

        public void OnCardPlayed(int seat, Card card, List<TrickPlay> trick)
        {
            PlaceCardOnTable(seat, card);
            if (seat == 0) { _currentHumanHand.Remove(card); RenderHumanHand(_currentHumanHand); }
            else
            {
                int ai = seat - 1;
                if (AIHandContainers != null && ai < AIHandContainers.Length)
                    RenderAIBack(AIHandContainers[ai], GameManager.Instance.GetPlayerHandCount(seat));
            }
        }

        public void OnTrickEnd(int winnerSeat, int pts)
        {
            _lastTrickData = new List<TrickPlay>(GameManager.Instance.GetLastTrick());
            LastTrickButton?.gameObject.SetActive(_lastTrickData.Count > 0);
            SetInfoBar(PlayerName(winnerSeat) + " remporte le pli (" + pts + " pts)");
            StartCoroutine(ClearTrickDelayed(0.6f));
        }

        public void OnRoundEnd(RoundResult result, int totalUs, int totalThem)
        {
            EndPanel?.SetActive(true);
            if (EndTitleText != null)
                EndTitleText.text = result.Outcome switch
                {
                    RoundOutcome.Capot        => "Capot !",
                    RoundOutcome.CapotAdverse => "Capot adverse !",
                    RoundOutcome.Dedans       => "Dedans !",
                    RoundOutcome.Litige       => "Litige !",
                    _                         => "Fin de manche"
                };
            if (EndDetailsText != null)
                EndDetailsText.text = result.Description + "\n\nTotal - Nous : " + totalUs + " | Eux : " + totalThem;
            if (ScoreUsText   != null) ScoreUsText.text   = totalUs.ToString();
            if (ScoreThemText != null) ScoreThemText.text = totalThem.ToString();
        }

        public void ShowBeloteAnnounce(int seat, string msg)
        {
            if (!BeloteAnnouncePanel) return;
            BeloteAnnouncePanel.SetActive(true);
            if (BeloteAnnounceText != null) BeloteAnnounceText.text = PlayerName(seat) + " : " + msg;
            StartCoroutine(HideAfter(BeloteAnnouncePanel, 1.4f));
        }

        public void ShowMessage(string text, float duration)
        {
            if (!MessagePanel) return;
            MessagePanel.SetActive(true);
            if (MessageText != null) MessageText.text = text;
            StartCoroutine(HideAfter(MessagePanel, duration));
        }

        void ShowLastTrick()
        {
            if (_lastTrickData == null || _lastTrickData.Count == 0) return;
            var sb = new StringBuilder();
            foreach (var p in _lastTrickData) sb.AppendLine(PlayerName(p.PlayerSeat) + " : " + p.Card);
            if (LastTrickPanel)
            {
                LastTrickPanel.SetActive(true);
                if (LastTrickText != null) LastTrickText.text = sb.ToString().TrimEnd();
                StartCoroutine(HideAfter(LastTrickPanel, 2.5f));
            }
            else ShowMessage("Dernier pli :\n" + sb, 2.5f);
        }

        void RenderHumanHand(List<Card> hand)
        {
            if (!HumanHandContainer) return;
            foreach (var go in _humanCardObjects) if (go) Destroy(go);
            _humanCardObjects.Clear();

            Suit trump = GameManager.Instance != null ? GameManager.Instance.State.Trump : default;
            var sorted = new List<Card>(hand);
            sorted.Sort((a, b) => {
                if (a.Suit == trump && b.Suit != trump) return 1;
                if (a.Suit != trump && b.Suit == trump) return -1;
                int si = (int)a.Suit - (int)b.Suit;
                return si != 0 ? si : a.NormalOrder() - b.NormalOrder();
            });
            _currentHumanHand = sorted;

            foreach (var card in sorted)
            {
                if (!CardPrefab) continue;
                var go = Instantiate(CardPrefab, HumanHandContainer);
                _humanCardObjects.Add(go);
                go.name = "Card_" + card;

                var cv = go.GetComponent<CardView>();
                if (cv != null) cv.SetCard(card, trump);
                else { var t = go.GetComponentInChildren<Text>(); if (t) t.text = card.ToString(); }

                Card captured = card;
                go.GetComponent<Button>()?.onClick.AddListener(() => GameManager.Instance.HumanCardTouched(captured));
            }
        }

        void RenderAIBack(Transform container, int count)
        {
            if (!container || !CardBackPrefab) return;
            foreach (Transform c in container) Destroy(c.gameObject);
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(CardBackPrefab, container);
                go.GetComponent<CardView>()?.SetCardBack();
            }
        }

        void PlaceCardOnTable(int seat, Card card)
        {
            if (TrickSlots == null || seat >= TrickSlots.Length || !TrickSlots[seat]) return;
            var slot = TrickSlots[seat];
            foreach (Transform c in slot) Destroy(c.gameObject);
            var prefab = TrickCardPrefab ? TrickCardPrefab : CardPrefab;
            if (!prefab) return;
            var go = Instantiate(prefab, slot);
            var cv  = go.GetComponent<CardView>();
            if (cv  != null) { cv.SetCard(card, GameManager.Instance.State.Trump); cv.SetPlayable(false); }
            else { var t = go.GetComponentInChildren<Text>(); if (t) t.text = card.ToString(); }
        }

        void ClearTrick()
        {
            if (TrickSlots == null) return;
            foreach (var s in TrickSlots) if (s) foreach (Transform c in s) Destroy(c.gameObject);
        }

        void SetInfoBar(string text) { if (InfoBarText) InfoBarText.text = text; }

        IEnumerator ClearTrickDelayed(float d) { yield return new WaitForSeconds(d); ClearTrick(); }
        IEnumerator HideAfter(GameObject go, float d) { yield return new WaitForSeconds(d); go?.SetActive(false); }

        public static string PlayerName(int seat) =>
            seat switch { 0=>"Vous", 1=>"Ouest", 2=>"Nord", 3=>"Est", _=>"?" };

        static string SuitSymbol(Suit s) =>
            s switch { Suit.Spades=>"♠", Suit.Hearts=>"♥", Suit.Diamonds=>"♦", Suit.Clubs=>"♣", _=>"?" };

        static string SuitName(Suit s) =>
            s switch { Suit.Spades=>"Pique", Suit.Hearts=>"Coeur", Suit.Diamonds=>"Carreau", Suit.Clubs=>"Trefle", _=>"?" };
    }
}
