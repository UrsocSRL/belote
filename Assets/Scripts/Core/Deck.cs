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
        /// Distribution initiale officielle : 3 puis 2 cartes par joueur (5 au total),
        /// puis une carte est retournée pour la phase de prise.
        /// dealerIndex : index du donneur (0=Sud/Joueur, 1=Ouest, 2=Nord, 3=Est)
        /// Le paquet conserve les 11 cartes restantes pour la distribution complémentaire.
        /// </summary>
        public (List<Card>[] hands, Card trumpCard) DealInitial(int dealerIndex)
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

            // Tour 2 : 2 cartes chacun (5 cartes au total par joueur)
            foreach (int p in order)
                for (int i = 0; i < 2; i++) hands[p].Add(Draw());

            // Carte retournée, proposée comme atout
            Card trumpCard = Draw();

            return (hands, trumpCard);
        }

        /// <summary>
        /// Distribution complémentaire après la prise : le preneur reçoit la carte
        /// retournée + 2 cartes, les autres joueurs reçoivent 3 cartes, pour
        /// atteindre 8 cartes chacun (épuise les 11 cartes restantes du paquet).
        /// </summary>
        public List<Card>[] DealComplement(int dealerIndex, int takerSeat, Card trumpCard)
        {
            var additions = new List<Card>[4];
            for (int i = 0; i < 4; i++) additions[i] = new List<Card>();

            int[] order = new int[4];
            for (int i = 0; i < 4; i++) order[i] = (dealerIndex + 1 + i) % 4;

            foreach (int seat in order)
            {
                if (seat == takerSeat) additions[seat].Add(trumpCard);
                int need = seat == takerSeat ? 2 : 3;
                for (int i = 0; i < need; i++) additions[seat].Add(Draw());
            }

            return additions;
        }
    }
}
