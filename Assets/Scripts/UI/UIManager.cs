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
    /// UIManager — UnityEngine.UI.Text uniquement (pas de TMPro requis).
    /// Bible Demo V0.1 : portrait ET paysage, carte touchée = jouée immédiatement.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Scores ───────────────────────────────────────────────────────────
        [Header("Scores")]
        public Text ScoreUsText;
        public Text ScoreThemText;
        public Text TrumpIndicatorText;

        // ── Prise d'atout ─────────────────────────────────────────────────────
        [Header("Trump Panel")]
        public GameObject TrumpPanel;
        public Text       TrumpPanelTitle;
        public Text       TrumpCardText;
        public Button     TakeButton;
        public Button     PassButton;
        public GameObject SuitButtonsContainer;
        public Button[]   SuitButtons;

        // ── Mains ─────────────────────────────────────────────────────────────
        [Header("Hands")]
        public Transform   HumanHandContainer;
        public Transform[] AIHandContainers;      // [0]=Ouest [1]=Nord [2]=Est
        public GameObject  CardPrefab;
        public GameObject  CardBackPrefab;
        public GameObject  TrickCardPrefab;

        // ── Table ─────────────────────────────────────────────────────────────
        [Header("Table — [0]=Sud [1]=Ouest [2]=Nord [3]=Est")]
        public Transform[] TrickSlots;

        // ── Messages ──────────────────────────────────────────────────────────
        [Header("Messages")]
        public GameObject MessagePanel;
        public Text       MessageText;
        public GameObject BeloteAnnouncePanel;
        public Text       BeloteAnnounceText;

        // ── Fin de manche ─────────────────────────────────────────────────────
        [Header("End Panel")]
        public GameObject EndPanel;
        public Text       EndTitleText;
        public Text       EndDetailsText;
        public Button     NextHandButton;

        // ── Info + Dernier pli ────────────────────────────────────────────────
        [Header("Info")]
        public Text       InfoBarText;
        public Button     LastTrickButton;
        public GameObject LastTrickPanel;
        public Text       LastTrickText;

        // ── Internes ──────────────────────────────────────────────────────────
        private readonly List<GameObject> _humanCardObjects = new();
        private List<Card>                _currentHumanHand = new();
        private List<TrickPlay>           _lastTrickData    = new();

        // ─────────────────────────────────────────────────────────────────────
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

            // Boutons couleurs tour 2 — capture locale explicite (pas de closure bug)
            Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            if (SuitButtons != null)
                for (int i = 0; i < SuitButtons.Length && i < 4; i++)
                {
                    Suit captured = suits[i];
                    SuitButtons[i].onClick.AddListener(() => GameManager.Instance.HumanTakeSuit(captured));
                }

            LastTrickButton?.onClick.AddListener(ShowLastTrick);
            LastTrickButton?.gameObject.SetActive(false);
        }

        // ── Distribution ──────────────────────────────────────────────────────
        public void OnDeal(Player[] players, Card trumpCard)
        {
            for (int ai = 0; ai < 3; ai++)
                if (AIHandContainers != null && ai < AIHandContainers.Length)
                    RenderAIBack(AIHandContainers[ai], players[ai + 1].Hand.Count);

            _currentHumanHand = new List<Card>(players[0].Hand);
            RenderHumanHand(_currentHumanHand);

            ClearTrick();
            _lastTrickData.Clear();
            LastTrickButton?.gameObject.SetActive(false);
        }

        // ── Prise d'atout ─────────────────────────────────────────────────────
        public void OnTrumpAsk(int askerSeat, TrumpPhase phase, Card trumpCard)
        {
            TrumpPanel?.SetActive(true);
            if (TrumpCardText != null) TrumpCardText.text = trumpCard.ToString();

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
            if (TrumpIndicatorText != null) TrumpIndicatorText.text = "Atout : " + SuitSymbol(trump);
            SetInfoBar(PlayerName(takerSeat) + " prend a " + SuitSymbol(trump));
        }

        // ── Tour de jeu ───────────────────────────────────────────────────────
        public void OnPlayerTurn(int seat)
        {
            SetInfoBar(seat == 0 ? "A vous de jouer" : PlayerName(seat) + " joue...");
        }

        public void HighlightAllowedCards(List<Card> allowedCards)
        {
            for (int i = 0; i < _humanCardObjects.Count && i < _currentHumanHand.Count; i++)
            {
                var go = _humanCardObjects[i];
                if (go == null) continue;
                bool ok = allowedCards.Contains(_currentHumanHand[i]);

                var cv = go.GetComponent<CardView>();
                if (cv != null) { cv.SetPlayable(ok); continue; }

                var cg = go.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = ok ? 1f : 0.35f;
                var btn = go.GetComponent<Button>();
                if (btn != null) btn.interactable = ok;
            }
        }

        public void OnCardPlayed(int seat, Card card, List<TrickPlay> trick)
        {
            PlaceCardOnTable(seat, card);
            if (seat == 0)
            {
                _currentHumanHand.Remove(card);
                RenderHumanHand(_currentHumanHand);
            }
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

        // ── Fin de manche ─────────────────────────────────────────────────────
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
            UpdateScoreDisplay(totalUs, totalThem);
        }

        // ── Belote / Rebelote ─────────────────────────────────────────────────
        public void ShowBeloteAnnounce(int seat, string announcement)
        {
            if (BeloteAnnouncePanel == null) return;
            BeloteAnnouncePanel.SetActive(true);
            if (BeloteAnnounceText != null)
                BeloteAnnounceText.text = PlayerName(seat) + " : " + announcement;
            StartCoroutine(HideAfter(BeloteAnnouncePanel, 1.4f));
        }

        // ── Messages flash ────────────────────────────────────────────────────
        public void ShowMessage(string text, float duration)
        {
            if (MessagePanel == null) return;
            MessagePanel.SetActive(true);
            if (MessageText != null) MessageText.text = text;
            StartCoroutine(HideAfter(MessagePanel, duration));
        }

        // ── Dernier pli ───────────────────────────────────────────────────────
        void ShowLastTrick()
        {
            if (_lastTrickData == null || _lastTrickData.Count == 0) return;
            var sb = new StringBuilder();
            foreach (var p in _lastTrickData)
                sb.AppendLine(PlayerName(p.PlayerSeat) + " : " + p.Card);

            if (LastTrickPanel != null)
            {
                LastTrickPanel.SetActive(true);
                if (LastTrickText != null) LastTrickText.text = sb.ToString().TrimEnd();
                StartCoroutine(HideAfter(LastTrickPanel, 2.5f));
            }
            else
            {
                ShowMessage("Dernier pli :\n" + sb, 2.5f);
            }
        }

        // ── Render ────────────────────────────────────────────────────────────
        void RenderHumanHand(List<Card> hand)
        {
            if (HumanHandContainer == null) return;
            foreach (var go in _humanCardObjects) if (go) Destroy(go);
            _humanCardObjects.Clear();

            Suit trump = GameManager.Instance.State.Trump;
            var sorted = new List<Card>(hand);
            sorted.Sort((a, b) =>
            {
                if (a.Suit == trump && b.Suit != trump) return 1;
                if (a.Suit != trump && b.Suit == trump) return -1;
                int si = (int)a.Suit - (int)b.Suit;
                return si != 0 ? si : a.NormalOrder() - b.NormalOrder();
            });
            _currentHumanHand = sorted;

            foreach (var card in sorted)
            {
                if (CardPrefab == null) continue;
                var go = Instantiate(CardPrefab, HumanHandContainer);
                _humanCardObjects.Add(go);
                go.name = "Card_" + card;

                var cv = go.GetComponent<CardView>();
                if (cv != null)
                {
                    cv.SetCard(card, trump);
                }
                else
                {
                    var t = go.GetComponentInChildren<Text>();
                    if (t != null) t.text = card.ToString();
                }

                Card captured = card;
                var btn = go.GetComponent<Button>();
                btn?.onClick.AddListener(() => GameManager.Instance.HumanCardTouched(captured));
            }
        }

        void RenderAIBack(Transform container, int count)
        {
            if (container == null || CardBackPrefab == null) return;
            foreach (Transform c in container) Destroy(c.gameObject);
            for (int i = 0; i < count; i++) Instantiate(CardBackPrefab, container);
        }

        void PlaceCardOnTable(int seat, Card card)
        {
            if (TrickSlots == null || seat >= TrickSlots.Length) return;
            var slot = TrickSlots[seat];
            if (slot == null) return;
            foreach (Transform c in slot) Destroy(c.gameObject);

            GameObject prefab = TrickCardPrefab != null ? TrickCardPrefab : CardPrefab;
            if (prefab == null) return;
            var go = Instantiate(prefab, slot);
            go.name = "Trick_" + card;

            var cv = go.GetComponent<CardView>();
            if (cv != null) { cv.SetCard(card, GameManager.Instance.State.Trump); cv.SetPlayable(false); }
            else { var t = go.GetComponentInChildren<Text>(); if (t != null) t.text = card.ToString(); }
        }

        void ClearTrick()
        {
            if (TrickSlots == null) return;
            foreach (var slot in TrickSlots)
                if (slot != null)
                    foreach (Transform c in slot) Destroy(c.gameObject);
        }

        IEnumerator ClearTrickDelayed(float d) { yield return new WaitForSeconds(d); ClearTrick(); }

        void UpdateScoreDisplay(int us, int them)
        {
            if (ScoreUsText   != null) ScoreUsText.text   = us.ToString();
            if (ScoreThemText != null) ScoreThemText.text = them.ToString();
        }

        void SetInfoBar(string text) { if (InfoBarText != null) InfoBarText.text = text; }

        IEnumerator HideAfter(GameObject go, float delay)
        { yield return new WaitForSeconds(delay); go?.SetActive(false); }

        public static string PlayerName(int seat) => seat switch
        { 0 => "Vous", 1 => "Ouest", 2 => "Nord", 3 => "Est", _ => "?" };

        static string SuitSymbol(Suit suit) => suit switch
        { Suit.Spades => "S", Suit.Hearts => "H", Suit.Diamonds => "D", Suit.Clubs => "C", _ => "?" };
    }
}
