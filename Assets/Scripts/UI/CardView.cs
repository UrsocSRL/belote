using UnityEngine;
using UnityEngine.UI;
using BeloteFreeze.Core;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// Composant visuel d'une carte — UnityEngine.UI uniquement.
    /// Affiche le sprite illustre (CardArtSet) si disponible, sinon un libelle texte.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text  CardLabel;
        public Image CardBackground;
        public Image CardFaceImage;
        public Image TrumpHighlight;

        [Header("Colors")]
        public Color RedSuitColor   = new Color(0.80f, 0.10f, 0.10f);
        public Color BlackSuitColor = new Color(0.10f, 0.10f, 0.10f);
        public Color NormalBgColor  = Color.white;

        /// <summary>
        /// Bibliotheque de sprites partagee, assignee une fois par UIManager au demarrage.
        /// </summary>
        public static CardArtSet ArtSet;

        public void SetCard(Card card, Suit trump)
        {
            var sprite = ArtSet != null ? ArtSet.GetCardSprite(card.Suit, card.Rank) : null;

            if (CardFaceImage != null && sprite != null)
            {
                CardFaceImage.sprite = sprite;
                CardFaceImage.gameObject.SetActive(true);
                if (CardLabel != null) CardLabel.gameObject.SetActive(false);
            }
            else
            {
                if (CardFaceImage != null) CardFaceImage.gameObject.SetActive(false);
                if (CardLabel != null)
                {
                    CardLabel.gameObject.SetActive(true);
                    CardLabel.text  = card.RankSymbol() + card.SuitSymbol();
                    CardLabel.color = IsRed(card.Suit) ? RedSuitColor : BlackSuitColor;
                }
            }

            if (CardBackground != null)
                CardBackground.color = NormalBgColor;
            if (TrumpHighlight != null)
                TrumpHighlight.gameObject.SetActive(false);
        }

        /// <summary>
        /// Affiche le dos de carte (mains IA, pli en cours avant resolution, etc.)
        /// </summary>
        public void SetCardBack()
        {
            if (CardLabel != null) CardLabel.gameObject.SetActive(false);
            if (TrumpHighlight != null) TrumpHighlight.gameObject.SetActive(false);

            if (CardFaceImage != null && ArtSet != null && ArtSet.CardBack != null)
            {
                CardFaceImage.sprite = ArtSet.CardBack;
                CardFaceImage.gameObject.SetActive(true);
            }
            else if (CardBackground != null)
            {
                CardBackground.color = new Color(0.12f, 0.25f, 0.60f);
            }
        }

        [Tooltip("Decalage vertical applique aux cartes jouables")]
        public float LiftAmount = 30f;

        bool _lifted;

        public void SetPlayable(bool playable)
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.interactable = playable;

            if (transform is RectTransform rt)
            {
                if (playable && !_lifted)      { rt.anchoredPosition += new Vector2(0, LiftAmount); _lifted = true;  }
                else if (!playable && _lifted) { rt.anchoredPosition -= new Vector2(0, LiftAmount); _lifted = false; }
            }
        }

        static bool IsRed(Suit s) => s == Suit.Hearts || s == Suit.Diamonds;
    }
}
