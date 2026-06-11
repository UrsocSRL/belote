using System.Collections.Generic;
using UnityEngine;
using BeloteFreeze.Core;

namespace BeloteFreeze.Rules
{
    /// <summary>
    /// Gère le déroulement d'un pli : cartes jouées, gagnant, attribution des points.
    /// </summary>
    public class TrickManager
    {
        public List<TrickPlay> CurrentTrick { get; private set; } = new();
        public List<TrickPlay> LastTrick    { get; private set; } = new();

        private Suit _trump;
        private int  _trickCount = 0;

        public int TrickCount => _trickCount;
        public bool IsLastTrick => _trickCount == 7; // 0-indexé : le 8ème pli

        public void Reset(Suit trump)
        {
            _trump = trump;
            _trickCount = 0;
            CurrentTrick.Clear();
            LastTrick.Clear();
        }

        /// <summary>
        /// Joue une carte dans le pli courant.
        /// </summary>
        public void PlayCard(int playerSeat, Card card)
        {
            CurrentTrick.Add(new TrickPlay(playerSeat, card));
        }

        /// <summary>
        /// Vérifie si le pli est complet (4 cartes jouées).
        /// </summary>
        public bool IsTrickComplete => CurrentTrick.Count == 4;

        /// <summary>
        /// Résout le pli : retourne le siège du gagnant et les points.
        /// Cas 8 — Dernier pli : +10 points.
        /// Cas 15 — Attribution selon règles d'atout.
        /// </summary>
        public (int winnerSeat, int points) ResolveTrick()
        {
            int winnerSeat = RuleEngine.GetTrickWinner(CurrentTrick, _trump);
            int points = RuleEngine.GetTrickPoints(CurrentTrick, _trump);

            // Cas 8 — Dernier pli : bonus 10 points
            if (IsLastTrick) points += 10;

            LastTrick = new List<TrickPlay>(CurrentTrick);
            CurrentTrick.Clear();
            _trickCount++;

            return (winnerSeat, points);
        }

        /// <summary>
        /// Retourne le gagnant actuel du pli en cours (avant qu'il soit terminé).
        /// </summary>
        public int GetCurrentLeader()
        {
            return RuleEngine.GetTrickWinner(CurrentTrick, _trump);
        }

        /// <summary>
        /// Retourne les cartes autorisées pour un joueur dans le pli actuel.
        /// </summary>
        public List<Card> GetAllowedCards(Player player)
        {
            return RuleEngine.GetAllowedCards(player, CurrentTrick, _trump);
        }
    }
}
