using System.Collections.Generic;
using UnityEngine;

namespace BeloteFreeze.Core
{
    public class Deck
    {
        private List<Card> _cards = new();

        public void Build()
        {
            _cards.Clear();
            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                    _cards.Add(new Card(suit, rank));
        }

        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public Card Draw() 
        {
            if (_cards.Count == 0) return null;
            var c = _cards[_cards.Count - 1];
            _cards.RemoveAt(_cards.Count - 1);
            return c;
        }

        public int Count => _cards.Count;

        /// <summary>
        /// Distribution officielle belote : 3+2 puis carte retournée puis 3
        /// dealerIndex : index du donneur (0=Sud/Joueur, 1=Ouest, 2=Nord, 3=Est)
        /// Retourne les 4 mains et la carte retournée
        /// </summary>
        public (List<Card>[] hands, Card trumpCard) Deal(int dealerIndex)
        {
            Build();
            Shuffle();

            var hands = new List<Card>[4];
            for (int i = 0; i < 4; i++) hands[i] = new List<Card>();

            // Ordre de distribution : à partir du joueur après le donneur
            int[] order = new int[4];
            for (int i = 0; i < 4; i++) order[i] = (dealerIndex + 1 + i) % 4;

            // Tour 1 : 3 cartes chacun
            foreach (int p in order)
                for (int i = 0; i < 3; i++) hands[p].Add(Draw());

            // Tour 2 : 2 cartes chacun
            foreach (int p in order)
                for (int i = 0; i < 2; i++) hands[p].Add(Draw());

            // Carte retournée
            Card trumpCard = Draw();

            // Tour 3 : 3 cartes chacun
            foreach (int p in order)
                for (int i = 0; i < 3; i++) hands[p].Add(Draw());

            return (hands, trumpCard);
        }
    }
}
