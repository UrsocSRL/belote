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
    /// Bible Demo V0.1 : Mode portrait ET paysage, rotation dynamique,
    /// aucun bouton Jouer, carte touchée = jouée immédiatement.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Scores ───────────────────────────────────────────────────────────
        [Header("Scores")]
        public TextMeshProUGUI ScoreUsText;
        public TextMeshProUGUI ScoreThemText;
        public TextMeshProUGUI TrumpIndicatorText;

        // ── Prise d'atout ─────────────────────────────────────────────────────
        [Header("Trump Panel")]
        public GameObject      TrumpPanel;
        public TextMeshProUGUI TrumpPanelTitle;
        public Image           TrumpCardImage;
        public TextMeshProUGUI TrumpCardText;
        public Button          TakeButton;
        public Button          PassButton;
        public GameObject      SuitButtonsContainer;
        public Button[]        SuitButtons;   // 4 boutons ♠ ♥ ♦ ♣ dans l'Inspector

        // ── Mains ─────────────────────────────────────────────────────────────
        [Header("Hands")]
        public Transform   HumanHandContainer;
        public Transform[] AIHandContainers;   // [0]=Ouest [1]=Nord [2]=Est
        public GameObject  CardPrefab;
        public GameObject  CardBackPrefab;
        public GameObject  TrickCardPrefab;    // prefab pour les cartes posées sur la table

        // ── Table ─────────────────────────────────────────────────────────────
        [Header("Table — 4 slots : [0]=Sud [1]=Ouest [2]=Nord [3]=Est")]
        public Transform[] TrickSlots;

        // ── Messages ──────────────────────────────────────────────────────────
        [Header("Messages")]
        public GameObject      MessagePanel;
        public TextMeshProUGUI MessageText;
        public GameObject      BeloteAnnouncePanel;
        public TextMeshProUGUI BeloteAnnounceText;

        // ── Fin de manche ─────────────────────────────────────────────────────
        [Header("End Panel")]
        public GameObject      EndPanel;
        public TextMeshProUGUI EndTitleText;
        public TextMeshProUGUI EndDetailsText;
        public Button          NextHandButton;

        // ── Info bar + Dernier pli ────────────────────────────────────────────
        [Header("Info")]
        public TextMeshProUGUI InfoBarText;
        public Button          LastTrickButton;   // FIX : branché dans Start()
        public GameObject      LastTrickPanel;    // panneau affichage dernier pli
        public TextMeshProUGUI LastTrickText;

        // ── Internes ──────────────────────────────────────────────────────────
        private readonly List<GameObject> _humanCardObjects = new();
        private List<Card>                _currentHumanHand = new();
        private List<TrickPlay>           _lastTrickData    = new();

        // ─────────────────────────────────────────────────────────────────────
        void Start()
        {
            // Initialiser les panneaux
            TrumpPanel?.SetActive(false);
            EndPanel?.SetActive(false);
            MessagePanel?.SetActive(false);
            BeloteAnnouncePanel?.SetActive(false);
            LastTrickPanel?.SetActive(false);

            // Boutons principaux
            NextHandButton?.onClick.AddListener(() => GameManager.Instance.OnNextHandRequested());
            TakeButton?.onClick.AddListener(()    => GameManager.Instance.HumanTake());
            PassButton?.onClick.AddListener(()    => GameManager.Instance.HumanPass());

            // FIX — Boutons couleurs tour 2 : capture locale pour éviter closure bug
            Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            if (SuitButtons != null)
            {
                for (int i = 0; i < SuitButtons.Length && i < 4; i++)
                {
                    Suit captured = suits[i];   // capture locale explicite
                    SuitButtons[i].onClick.AddListener(() => GameManager.Instance.HumanTakeSuit(captured));
                }
            }

            // FIX — LastTrickButton : branchement du listener
            LastTrickButton?.onClick.AddListener(ShowLastTrick);
            LastTrickButton?.gameObject.SetActive(false); // masqué jusqu'au premier pli terminé
        }

        // ── Distribution ──────────────────────────────────────────────────────
        public void OnDeal(Player[] players, Card trumpCard)
        {
            // Dos de cartes pour les 3 IA
            for (int aiIdx = 0; aiIdx < 3; aiIdx++)
            {
                int seat = aiIdx + 1;   // 1=Ouest 2=Nord 3=Est
                if (AIHandContainers != null && aiIdx < AIHandContainers.Length)
                    RenderAIBack(AIHandContainers[aiIdx], players[seat].Hand.Count);
            }

            // Main du joueur humain
            _currentHumanHand = new List<Card>(players[0].Hand);
            RenderHumanHand(_currentHumanHand);

            // Nettoyer la table et réinitialiser le bouton dernier pli
            ClearTrick();
            _lastTrickData.Clear();
            LastTrickButton?.gameObject.SetActive(false);
        }

        // ── Prise d'atout ─────────────────────────────────────────────────────
        public void OnTrumpAsk(int askerSeat, TrumpPhase phase, Card trumpCard)
        {
            TrumpPanel?.SetActive(true);

            if (TrumpCardText != null)
                TrumpCardText.text = trumpCard.ToString();

            if (askerSeat == 0)
            {
                // Tour du joueur humain
                if (phase == TrumpPhase.Round1)
                {
                    if (TrumpPanelTitle != null)
                        TrumpPanelTitle.text = $"Prendre à {trumpCard.SuitSymbol()} ?";
                    TakeButton?.gameObject.SetActive(true);
                    SuitButtonsContainer?.SetActive(false);
                }
                else
                {
                    if (TrumpPanelTitle != null)
                        TrumpPanelTitle.text = $"Choisir l'atout (sauf {trumpCard.SuitSymbol()})";
                    TakeButton?.gameObject.SetActive(false);
                    SuitButtonsContainer?.SetActive(true);

                    // Désactiver la couleur de la carte retournée
                    Suit[] suits = { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
                    if (SuitButtons != null)
                        for (int i = 0; i < SuitButtons.Length && i < 4; i++)
                            SuitButtons[i].interactable = suits[i] != trumpCard.Suit;
                }
                PassButton?.gameObject.SetActive(true);
            }
            else
            {
                // Tour IA
                if (TrumpPanelTitle != null)
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
                TrumpIndicatorText.text = $"Atout : {SuitSymbol(trump)}";
            SetInfoBar($"{PlayerName(takerSeat)} prend à {SuitSymbol(trump)}");
        }

        // ── Tour de jeu ───────────────────────────────────────────────────────
        public void OnPlayerTurn(int seat)
        {
            SetInfoBar(seat == 0 ? "À vous de jouer" : $"{PlayerName(seat)} joue...");
        }

        /// <summary>
        /// FIX : grisage des cartes non jouables, mise en avant des cartes autorisées.
        /// Utilise CardView.SetPlayable() si disponible, sinon CanvasGroup alpha.
        /// </summary>
        public void HighlightAllowedCards(List<Card> allowedCards)
        {
            Suit trump = GameManager.Instance.State.Trump;
            for (int i = 0; i < _humanCardObjects.Count && i < _currentHumanHand.Count; i++)
            {
                var go   = _humanCardObjects[i];
                var card = _currentHumanHand[i];
                if (go == null) continue;

                bool isAllowed = allowedCards.Contains(card);

                // FIX — Intégration CardView : déléguer à SetPlayable si le composant existe
                var cv = go.GetComponent<CardView>();
                if (cv != null)
                {
                    cv.SetPlayable(isAllowed);
                }
                else
                {
                    // Fallback : CanvasGroup alpha
                    var cg = go.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = isAllowed ? 1f : 0.35f;
                    var btn = go.GetComponent<Button>();
                    if (btn != null) btn.interactable = isAllowed;
                }
            }
        }

        /// <summary>
        /// FIX ligne 193 : passe le vrai nombre de cartes restantes en main de l'IA.
        /// </summary>
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
                // FIX : utiliser la vraie taille de la main de l'IA, pas TricksWon.Length
                int aiIdx = seat - 1;
                if (AIHandContainers != null && aiIdx < AIHandContainers.Length)
                {
                    int cardsLeft = GameManager.Instance.GetPlayerHandCount(seat);
                    RenderAIBack(AIHandContainers[aiIdx], cardsLeft);
                }
            }
        }

        public void OnTrickEnd(int winnerSeat, int pts)
        {
            // Sauvegarder le dernier pli et activer le bouton
            _lastTrickData = new List<TrickPlay>(GameManager.Instance.GetLastTrick());
            LastTrickButton?.gameObject.SetActive(_lastTrickData.Count > 0);

            SetInfoBar($"{PlayerName(winnerSeat)} remporte le pli ({pts} pts)");
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
                EndDetailsText.text = result.Description
                    + $"\n\nTotal — Nous : {totalUs} | Eux : {totalThem}";

            UpdateScoreDisplay(totalUs, totalThem);
        }

        // ── Belote / Rebelote ─────────────────────────────────────────────────
        public void ShowBeloteAnnounce(int seat, string announcement)
        {
            if (BeloteAnnouncePanel == null) return;
            BeloteAnnouncePanel.SetActive(true);
            if (BeloteAnnounceText != null)
                BeloteAnnounceText.text = $"{PlayerName(seat)} : {announcement}";
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

        // ── FIX — Dernier pli ─────────────────────────────────────────────────
        void ShowLastTrick()
        {
            if (_lastTrickData == null || _lastTrickData.Count == 0) return;

            if (LastTrickPanel != null)
            {
                LastTrickPanel.SetActive(true);
                if (LastTrickText != null)
                {
                    var lines = new System.Text.StringBuilder();
                    foreach (var play in _lastTrickData)
                        lines.AppendLine($"{PlayerName(play.PlayerSeat)} : {play.Card}");
                    LastTrickText.text = lines.ToString().TrimEnd();
                }
                StartCoroutine(HideAfter(LastTrickPanel, 2.5f));
            }
            else
            {
                // Fallback : afficher dans le MessagePanel
                var sb = new System.Text.StringBuilder("Dernier pli :\n");
                foreach (var play in _lastTrickData)
                    sb.Append($"{PlayerName(play.PlayerSeat)}: {play.Card}  ");
                ShowMessage(sb.ToString(), 2.5f);
            }
        }

        // ── Render helpers ────────────────────────────────────────────────────

        /// <summary>
        /// FIX : signature simplifiée — le allowed est géré par HighlightAllowedCards séparément.
        /// FIX CardView : SetCard() appelé sur chaque carte instanciée.
        /// </summary>
        void RenderHumanHand(List<Card> hand)
        {
            if (HumanHandContainer == null) return;

            // Détruire les anciens objets
            foreach (var go in _humanCardObjects) if (go != null) Destroy(go);
            _humanCardObjects.Clear();

            // Trier : couleurs groupées, atout en dernier
            Suit trump = GameManager.Instance.State.Trump;
            var sorted = new List<Card>(hand);
            sorted.Sort((a, b) =>
            {
                if (a.Suit == trump && b.Suit != trump) return 1;
                if (a.Suit != trump && b.Suit == trump) return -1;
                int si = (int)a.Suit - (int)b.Suit;
                return si != 0 ? si : a.NormalOrder() - b.NormalOrder();
            });

            // Mettre à jour la liste triée (nécessaire pour HighlightAllowedCards)
            _currentHumanHand = sorted;

            foreach (var card in sorted)
            {
                if (CardPrefab == null) continue;
                var go = Instantiate(CardPrefab, HumanHandContainer);
                _humanCardObjects.Add(go);
                go.name = $"Card_{card}";

                // FIX — Intégration CardView : appeler SetCard() en priorité
                var cv = go.GetComponent<CardView>();
                if (cv != null)
                {
                    cv.SetCard(card, trump);
                }
                else
                {
                    // Fallback texte brut si pas de CardView
                    var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = card.ToString();
                }

                // Bible Demo : carte touchée = jouée immédiatement
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    Card captured = card;
                    btn.onClick.AddListener(() => GameManager.Instance.HumanCardTouched(captured));
                }
            }
        }

        void RenderAIBack(Transform container, int count)
        {
            if (container == null || CardBackPrefab == null) return;
            foreach (Transform child in container) Destroy(child.gameObject);
            for (int i = 0; i < count; i++)
                Instantiate(CardBackPrefab, container);
        }

        /// <summary>
        /// FIX — Utilise TrickCardPrefab si défini, sinon CardPrefab en fallback.
        /// Appelle CardView.SetCard() si disponible.
        /// </summary>
        void PlaceCardOnTable(int seat, Card card)
        {
            if (TrickSlots == null || seat >= TrickSlots.Length) return;
            var slot = TrickSlots[seat];
            if (slot == null) return;

            foreach (Transform child in slot) Destroy(child.gameObject);

            // Utiliser TrickCardPrefab de préférence, sinon CardPrefab
            GameObject prefabToUse = TrickCardPrefab != null ? TrickCardPrefab : CardPrefab;
            if (prefabToUse == null) return;

            var go = Instantiate(prefabToUse, slot);
            go.name = $"Trick_{card}";

            // FIX — Intégration CardView
            var cv = go.GetComponent<CardView>();
            Suit trump = GameManager.Instance.State.Trump;
            if (cv != null)
            {
                cv.SetCard(card, trump);
                cv.SetPlayable(false); // les cartes sur la table ne sont pas cliquables
            }
            else
            {
                var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = card.ToString();
            }
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
