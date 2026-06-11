using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// UIManager — Gère tout l'affichage Unity UI.
    /// À connecter dans l'Inspector Unity avec les références de la scène.
    /// Bible Demo V0.1 : Mode portrait ET paysage, rotation dynamique,
    /// aucun bouton Jouer, carte touchée = jouée immédiatement.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Score ────────────────────────────────────────────────────────────
        [Header("Scores")]
        public TextMeshProUGUI ScoreUsText;
        public TextMeshProUGUI ScoreThemText;
        public TextMeshProUGUI TrumpIndicatorText;

        // ── Prise d'atout ────────────────────────────────────────────────────
        [Header("Trump Panel")]
        public GameObject  TrumpPanel;
        public TextMeshProUGUI TrumpPanelTitle;
        public Image       TrumpCardImage;
        public TextMeshProUGUI TrumpCardText;
        public Button      TakeButton;
        public Button      PassButton;
        public GameObject  SuitButtonsContainer;
        public Button[]    SuitButtons; // 4 boutons : ♠ ♥ ♦ ♣

        // ── Mains des joueurs ─────────────────────────────────────────────────
        [Header("Hands")]
        public Transform   HumanHandContainer;    // Sud
        public Transform[] AIHandContainers;       // [0]=Ouest [1]=Nord [2]=Est
        public GameObject  CardPrefab;
        public GameObject  CardBackPrefab;

        // ── Table (pli en cours) ──────────────────────────────────────────────
        [Header("Table")]
        public Transform[] TrickSlots;   // 4 emplacements : [0]=Sud [1]=Ouest [2]=Nord [3]=Est

        // ── Messages ─────────────────────────────────────────────────────────
        [Header("Messages")]
        public GameObject       MessagePanel;
        public TextMeshProUGUI  MessageText;
        public GameObject       BeloteAnnouncePanel;
        public TextMeshProUGUI  BeloteAnnounceText;

        // ── Fin de manche ─────────────────────────────────────────────────────
        [Header("End Panel")]
        public GameObject       EndPanel;
        public TextMeshProUGUI  EndTitleText;
        public TextMeshProUGUI  EndDetailsText;
        public Button           NextHandButton;

        // ── Info bar ──────────────────────────────────────────────────────────
        [Header("Info")]
        public TextMeshProUGUI InfoBarText;
        public Button          LastTrickButton;

        // ── Internes ──────────────────────────────────────────────────────────
        private List<GameObject> _humanCardObjects = new();
        private List<Card>       _currentHumanHand = new();

        // ────────────────────────────────────────────────────────────────────
        void Start()
        {
            TrumpPanel?.SetActive(false);
            EndPanel?.SetActive(false);
            MessagePanel?.SetActive(false);
            BeloteAnnouncePanel?.SetActive(false);

            NextHandButton?.onClick.AddListener(() => GameManager.Instance.OnNextHandRequested());
            TakeButton?.onClick.AddListener(() => GameManager.Instance.HumanTake());
            PassButton?.onClick.AddListener(() => GameManager.Instance.HumanPass());

            // Boutons couleurs pour le tour 2
            Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            for (int i = 0; i < SuitButtons.Length && i < 4; i++)
            {
                Suit s = suits[i];
                SuitButtons[i].onClick.AddListener(() => GameManager.Instance.HumanTakeSuit(s));
            }
        }

        // ── Distribution ─────────────────────────────────────────────────────
        public void OnDeal(Player[] players, Card trumpCard)
        {
            // Afficher les dos de cartes pour les IA
            for (int i = 0; i < 3; i++)
            {
                int seat = i + 1; // 1=Ouest 2=Nord 3=Est
                int aiIdx = i;
                if (AIHandContainers != null && aiIdx < AIHandContainers.Length)
                    RenderAIBack(AIHandContainers[aiIdx], players[seat].Hand.Count);
            }

            // Afficher la main du joueur humain
            _currentHumanHand = new List<Card>(players[0].Hand);
            RenderHumanHand(_currentHumanHand, new List<Card>());

            // Nettoyer la table
            ClearTrick();
        }

        // ── Prise d'atout ─────────────────────────────────────────────────────
        public void OnTrumpAsk(int askerSeat, TrumpPhase phase, Card trumpCard)
        {
            TrumpPanel?.SetActive(true);

            // Afficher la carte retournée
            if (TrumpCardText != null)
                TrumpCardText.text = trumpCard.ToString();

            if (askerSeat == 0)
            {
                // Tour humain
                if (phase == TrumpPhase.Round1)
                {
                    TrumpPanelTitle.text = $"Prendre à {trumpCard.SuitSymbol()} ?";
                    TakeButton?.gameObject.SetActive(true);
                    SuitButtonsContainer?.SetActive(false);
                }
                else
                {
                    TrumpPanelTitle.text = $"Choisir l'atout (sauf {trumpCard.SuitSymbol()})";
                    TakeButton?.gameObject.SetActive(false);
                    SuitButtonsContainer?.SetActive(true);

                    // Désactiver la couleur de la carte retournée
                    Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
                    for (int i = 0; i < SuitButtons.Length && i < 4; i++)
                        SuitButtons[i].interactable = suits[i] != trumpCard.Suit;
                }
                PassButton?.gameObject.SetActive(true);
            }
            else
            {
                TrumpPanelTitle.text = $"{PlayerName(askerSeat)} réfléchit...";
                TakeButton?.gameObject.SetActive(false);
                PassButton?.gameObject.SetActive(false);
                SuitButtonsContainer?.SetActive(false);
            }
        }

        public void OnTrumpChosen(Suit trump, int takerSeat)
        {
            TrumpPanel?.SetActive(false);
            if (TrumpIndicatorText != null)
                TrumpIndicatorText.text = $"Atout: {SuitSymbol(trump)}";
            SetInfoBar($"{PlayerName(takerSeat)} prend à {SuitSymbol(trump)}");
        }

        // ── Tour de jeu ───────────────────────────────────────────────────────
        public void OnPlayerTurn(int seat)
        {
            SetInfoBar(seat == 0 ? "À vous de jouer" : $"{PlayerName(seat)} joue...");
        }

        public void HighlightAllowedCards(List<Card> allowedCards)
        {
            // Mettre en évidence les cartes jouables, griser les autres
            for (int i = 0; i < _humanCardObjects.Count && i < _currentHumanHand.Count; i++)
            {
                var go = _humanCardObjects[i];
                var card = _currentHumanHand[i];
                bool isAllowed = allowedCards.Contains(card);
                var cg = go.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = isAllowed ? 1f : 0.4f;
            }
        }

        public void OnCardPlayed(int seat, Card card, List<TrickPlay> trick)
        {
            // Placer la carte sur la table
            PlaceCardOnTable(seat, card);

            // Mettre à jour la main
            if (seat == 0)
            {
                _currentHumanHand.Remove(card);
                RenderHumanHand(_currentHumanHand, new List<Card>());
            }
            else
            {
                int aiIdx = seat - 1;
                if (AIHandContainers != null && aiIdx < AIHandContainers.Length)
                    RenderAIBack(AIHandContainers[aiIdx], GameManager.Instance.State.TricksWon.Length);
            }
        }

        public void OnTrickEnd(int winnerSeat, int pts)
        {
            SetInfoBar($"{PlayerName(winnerSeat)} remporte le pli ({pts} pts)");
            StartCoroutine(ClearTrickDelayed(0.5f));
        }

        // ── Fin de manche ─────────────────────────────────────────────────────
        public void OnRoundEnd(RoundResult result, int totalUs, int totalThem)
        {
            EndPanel?.SetActive(true);

            if (EndTitleText != null)
                EndTitleText.text = result.Outcome switch
                {
                    RoundOutcome.Capot       => "Capot !",
                    RoundOutcome.CapotAdverse=> "Capot adverse !",
                    RoundOutcome.Dedans      => "Dedans !",
                    RoundOutcome.Litige      => "Litige !",
                    _                        => "Fin de manche"
                };

            if (EndDetailsText != null)
                EndDetailsText.text = result.Description +
                    $"\n\nTotal — Nous : {totalUs} | Eux : {totalThem}";

            UpdateScoreDisplay(totalUs, totalThem);
        }

        // ── Belote / Rebelote ─────────────────────────────────────────────────
        public void ShowBeloteAnnounce(int seat, string announcement)
        {
            if (BeloteAnnouncePanel == null) return;
            BeloteAnnouncePanel.SetActive(true);
            if (BeloteAnnounceText != null)
                BeloteAnnounceText.text = $"{PlayerName(seat)} : {announcement}";
            StartCoroutine(HideAfter(BeloteAnnouncePanel, 1.2f));
        }

        // ── Messages ─────────────────────────────────────────────────────────
        public void ShowMessage(string text, float duration)
        {
            if (MessagePanel == null) return;
            MessagePanel.SetActive(true);
            if (MessageText != null) MessageText.text = text;
            StartCoroutine(HideAfter(MessagePanel, duration));
        }

        // ── Helpers render ───────────────────────────────────────────────────
        void RenderHumanHand(List<Card> hand, List<Card> allowed)
        {
            if (HumanHandContainer == null) return;

            foreach (var go in _humanCardObjects) Destroy(go);
            _humanCardObjects.Clear();

            // Trier : couleurs groupées, atout en dernier
            var sorted = new List<Card>(hand);
            sorted.Sort((a, b) =>
            {
                var trump = GameManager.Instance.State.Trump;
                if (a.Suit == trump && b.Suit != trump) return 1;
                if (a.Suit != trump && b.Suit == trump) return -1;
                int si = (int)a.Suit - (int)b.Suit;
                return si != 0 ? si : a.NormalOrder() - b.NormalOrder();
            });

            foreach (var card in sorted)
            {
                if (CardPrefab == null) continue;
                var go = Instantiate(CardPrefab, HumanHandContainer);
                _humanCardObjects.Add(go);

                // Texte de la carte
                var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = card.ToString();

                // Interaction — Bible Demo : carte touchée = jouée immédiatement
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    Card captured = card;
                    btn.onClick.AddListener(() => GameManager.Instance.HumanCardTouched(captured));
                }

                // Tag pour identifier la carte
                go.name = $"Card_{card}";
            }
        }

        void RenderAIBack(Transform container, int count)
        {
            if (container == null || CardBackPrefab == null) return;
            foreach (Transform child in container) Destroy(child.gameObject);
            for (int i = 0; i < count; i++)
                Instantiate(CardBackPrefab, container);
        }

        void PlaceCardOnTable(int seat, Card card)
        {
            if (TrickSlots == null || seat >= TrickSlots.Length || CardPrefab == null) return;
            var slot = TrickSlots[seat];
            if (slot == null) return;

            foreach (Transform child in slot) Destroy(child.gameObject);

            var go = Instantiate(CardPrefab, slot);
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = card.ToString();
        }

        void ClearTrick()
        {
            if (TrickSlots == null) return;
            foreach (var slot in TrickSlots)
                if (slot != null)
                    foreach (Transform child in slot) Destroy(child.gameObject);
        }

        IEnumerator ClearTrickDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            ClearTrick();
        }

        void UpdateScoreDisplay(int us, int them)
        {
            if (ScoreUsText   != null) ScoreUsText.text   = us.ToString();
            if (ScoreThemText != null) ScoreThemText.text = them.ToString();
        }

        void SetInfoBar(string text)
        {
            if (InfoBarText != null) InfoBarText.text = text;
        }

        IEnumerator HideAfter(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            go?.SetActive(false);
        }

        static string PlayerName(int seat) => seat switch
        {
            0 => "Vous", 1 => "Ouest", 2 => "Nord", 3 => "Est", _ => "?"
        };

        static string SuitSymbol(Suit suit) => suit switch
        {
            Suit.Spades   => "♠", Suit.Hearts  => "♥",
            Suit.Diamonds => "♦", Suit.Clubs   => "♣", _ => "?"
        };
    }
}
