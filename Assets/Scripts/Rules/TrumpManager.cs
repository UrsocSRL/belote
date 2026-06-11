using System;
using BeloteFreeze.Core;

namespace BeloteFreeze.Rules
{
    public enum TrumpPhase { Round1, Round2, Done, Redeal }

    /// <summary>
    /// Gère la prise d'atout selon les cas 12, 13, 14 de la bible Rule Engine V1.
    /// Cas 12 — Tour 1 : prendre ou passer (couleur de la carte retournée).
    /// Cas 13 — Tour 2 : choisir une autre couleur.
    /// Cas 14 — Nouvelle donne si tout le monde passe.
    /// </summary>
    public class TrumpManager
    {
        public TrumpPhase Phase   { get; private set; } = TrumpPhase.Round1;
        public Card TrumpCard     { get; private set; }
        public Suit? ChosenTrump  { get; private set; }
        public int  TakerSeat     { get; private set; } = -1;

        private int _dealerSeat;
        private int _askerIndex; // 0..3 dans l'ordre de distribution

        public int CurrentAskerSeat => (_dealerSeat + 1 + _askerIndex) % 4;

        public void StartTrump(Card trumpCard, int dealerSeat)
        {
            TrumpCard    = trumpCard;
            _dealerSeat  = dealerSeat;
            _askerIndex  = 0;
            Phase        = TrumpPhase.Round1;
            ChosenTrump  = null;
            TakerSeat    = -1;
        }

        /// <summary>
        /// Cas 12 — Tour 1 : le joueur currentAsker prend ou passe.
        /// </summary>
        public void Take(int playerSeat)
        {
            ChosenTrump = TrumpCard.Suit;
            TakerSeat   = playerSeat;
            Phase       = TrumpPhase.Done;
        }

        /// <summary>
        /// Cas 13 — Tour 2 : le joueur choisit une autre couleur.
        /// </summary>
        public void TakeSuit(int playerSeat, Suit suit)
        {
            if (suit == TrumpCard.Suit)
                throw new InvalidOperationException("Impossible de choisir la même couleur qu'au tour 1.");
            ChosenTrump = suit;
            TakerSeat   = playerSeat;
            Phase       = TrumpPhase.Done;
        }

        /// <summary>
        /// Le joueur passe.
        /// </summary>
        public void Pass()
        {
            _askerIndex++;
            if (_askerIndex >= 4)
            {
                if (Phase == TrumpPhase.Round1)
                {
                    // Passer au tour 2
                    Phase       = TrumpPhase.Round2;
                    _askerIndex = 0;
                }
                else
                {
                    // Cas 14 — Tout le monde a passé : nouvelle donne
                    Phase = TrumpPhase.Redeal;
                }
            }
        }

        public bool IsComplete => Phase == TrumpPhase.Done || Phase == TrumpPhase.Redeal;
    }
}
