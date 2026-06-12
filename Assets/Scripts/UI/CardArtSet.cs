using UnityEngine;
using BeloteFreeze.Core;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// Bibliotheque de sprites du jeu (cartes, dos, decor, avatars, icones).
    /// Genere/rempli automatiquement par SceneBuilder (BeloteFreeze/Build GameScene).
    /// </summary>
    [CreateAssetMenu(fileName = "CardArtSet", menuName = "BeloteFreeze/Card Art Set")]
    public class CardArtSet : ScriptableObject
    {
        [System.Serializable]
        public struct CardSprite
        {
            public Suit Suit;
            public Rank Rank;
            public Sprite Sprite;
        }

        [Header("Cartes (32)")]
        public CardSprite[] Cards = new CardSprite[32];

        [Header("Dos de carte")]
        public Sprite CardBack;

        [Header("Decor")]
        public Sprite Background;
        public Sprite Table;

        [Header("Avatars IA")]
        public Sprite AvatarMale;
        public Sprite AvatarFemale;

        [Header("Icones couleurs")]
        public Sprite IconSpades;
        public Sprite IconHearts;
        public Sprite IconDiamonds;
        public Sprite IconClubs;

        public Sprite GetCardSprite(Suit suit, Rank rank)
        {
            foreach (var c in Cards)
                if (c.Suit == suit && c.Rank == rank) return c.Sprite;
            return null;
        }

        public Sprite GetSuitIcon(Suit suit) => suit switch
        {
            Suit.Spades   => IconSpades,
            Suit.Hearts   => IconHearts,
            Suit.Diamonds => IconDiamonds,
            Suit.Clubs    => IconClubs,
            _             => null
        };
    }
}
