using BeloteFreeze.Core;

namespace BeloteFreeze.Rules
{
    /// <summary>
    /// Cas 6 — Belote : Dame d'atout jouée en premier, annonce automatique.
    /// Cas 7 — Rebelote : Roi d'atout joué après, validation et bonus 20 pts.
    /// Le Roi peut aussi être joué avant la Dame.
    /// </summary>
    public class BeloteTracker
    {
        public int  HolderSeat      { get; private set; } = -1; // qui a les deux cartes
        public bool BeloteAnnounced  { get; private set; }
        public bool RebeloteAnnounced{ get; private set; }
        public bool IsComplete => BeloteAnnounced && RebeloteAnnounced;

        private Suit _trump;
        private bool _kingPlayed;
        private bool _queenPlayed;

        public void Reset(Suit trump)
        {
            _trump            = trump;
            HolderSeat        = -1;
            BeloteAnnounced   = false;
            RebeloteAnnounced = false;
            _kingPlayed       = false;
            _queenPlayed      = false;
        }

        /// <summary>
        /// À appeler après chaque distribution pour détecter qui a Roi + Dame d'atout.
        /// </summary>
        public void DetectHolder(Player[] players)
        {
            HolderSeat = -1;
            foreach (var p in players)
            {
                bool hasKing  = p.Hand.Exists(c => c.Suit == _trump && c.Rank == Rank.King);
                bool hasQueen = p.Hand.Exists(c => c.Suit == _trump && c.Rank == Rank.Queen);
                if (hasKing && hasQueen)
                {
                    HolderSeat = (int)p.Seat;
                    break;
                }
            }
        }

        /// <summary>
        /// Notifie qu'une carte d'atout vient d'être jouée.
        /// Retourne true si une annonce doit être affichée ("Belote" ou "Rebelote").
        /// </summary>
        public bool OnCardPlayed(int playerSeat, Card card, out string announcement)
        {
            announcement = null;
            if (HolderSeat < 0 || playerSeat != HolderSeat) return false;
            if (card.Suit != _trump) return false;

            if (card.Rank == Rank.Queen && !_queenPlayed)
            {
                _queenPlayed = true;
                if (!BeloteAnnounced) { BeloteAnnounced = true; announcement = "Belote !"; return true; }
                if (!RebeloteAnnounced) { RebeloteAnnounced = true; announcement = "Rebelote !"; return true; }
            }

            if (card.Rank == Rank.King && !_kingPlayed)
            {
                _kingPlayed = true;
                if (!BeloteAnnounced) { BeloteAnnounced = true; announcement = "Belote !"; return true; }
                if (!RebeloteAnnounced) { RebeloteAnnounced = true; announcement = "Rebelote !"; return true; }
            }

            return false;
        }

        public int BeloteTeam => HolderSeat >= 0
            ? ((HolderSeat == 0 || HolderSeat == 2) ? 0 : 1)
            : -1;
    }
}
