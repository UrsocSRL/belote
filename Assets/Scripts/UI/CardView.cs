using UnityEngine;
using UnityEngine.UI;
using BeloteFreeze.Core;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// Composant visuel d'une carte — UnityEngine.UI uniquement.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text  CardLabel;
        public Image CardBackground;
        public Image TrumpHighlight;

        [Header("Colors")]
        public Color RedSuitColor   = new Color(0.80f, 0.10f, 0.10f);
        public Color BlackSuitColor = new Color(0.10f, 0.10f, 0.10f);
        public Color TrumpBgColor   = new Color(1f, 0.98f, 0.85f);
        public Color NormalBgColor  = Color.white;

        public void SetCard(Card card, Suit trump)
        {
            bool isTrump = card.Suit == trump;
            if (CardLabel != null)
            {
                CardLabel.text  = card.RankSymbol() + card.SuitSymbol();
                CardLabel.color = IsRed(card.Suit) ? RedSuitColor : BlackSuitColor;
            }
            if (CardBackground != null)
                CardBackground.color = isTrump ? TrumpBgColor : NormalBgColor;
            if (TrumpHighlight != null)
                TrumpHighlight.gameObject.SetActive(isTrump);
        }

        public void SetPlayable(bool playable)
        {
            var cg  = GetComponent<CanvasGroup>();
            if (cg  != null) cg.alpha = playable ? 1f : 0.35f;
            var btn = GetComponent<Button>();
            if (btn != null) btn.interactable = playable;
        }

        static bool IsRed(Suit s) => s == Suit.Hearts || s == Suit.Diamonds;
    }
}
