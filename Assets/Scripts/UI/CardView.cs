using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BeloteFreeze.Core;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// Composant visuel attaché à chaque prefab de carte.
    /// Affiche le rang, la couleur, et gère l'état (jouable / grisé / atout).
    /// </summary>
    public class CardView : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI RankText;
        public TextMeshProUGUI SuitText;
        public Image           CardBackground;
        public Image           TrumpHighlight; // bordure dorée si atout

        [Header("Colors")]
        public Color RedSuitColor   = new Color(0.8f, 0.1f, 0.1f);
        public Color BlackSuitColor = new Color(0.1f, 0.1f, 0.1f);
        public Color TrumpBgColor   = new Color(1f, 0.98f, 0.85f);
        public Color NormalBgColor  = Color.white;
        public Color ForbiddenAlpha = new Color(1, 1, 1, 0.35f);

        private Card _card;
        private bool _isTrump;

        public void SetCard(Card card, Suit trump)
        {
            _card   = card;
            _isTrump = card.Suit == trump;

            if (RankText != null) RankText.text = card.RankSymbol();
            if (SuitText != null)
            {
                SuitText.text  = card.SuitSymbol();
                SuitText.color = IsRed(card.Suit) ? RedSuitColor : BlackSuitColor;
            }
            if (RankText != null)
                RankText.color = IsRed(card.Suit) ? RedSuitColor : BlackSuitColor;

            if (CardBackground != null)
                CardBackground.color = _isTrump ? TrumpBgColor : NormalBgColor;

            if (TrumpHighlight != null)
                TrumpHighlight.gameObject.SetActive(_isTrump);
        }

        public void SetPlayable(bool playable)
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = playable ? 1f : ForbiddenAlpha.a;
            var btn = GetComponent<Button>();
            if (btn != null) btn.interactable = playable;
        }

        private static bool IsRed(Suit suit) => suit == Suit.Hearts || suit == Suit.Diamonds;
    }
}
