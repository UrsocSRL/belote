using System.Collections.Generic;
using System.Linq;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// IA Officielle V1 — Bible IA Officielle V1
    /// Priorités :
    ///   1. Respect absolu des règles (cartes autorisées par RuleEngine)
    ///   2. Gagner un pli au coût minimal
    ///   3. Conserver les gros atouts : Valet, 9, As, 10
    ///   4. Préserver les As
    ///   5. Préserver les Dix
    /// Anti-comportements : ne jamais couper son partenaire,
    ///   ne pas jeter un As inutilement, ne pas gaspiller un gros atout.
    /// </summary>
    public class AIPlayer
    {
        // Mémoire IA
        private readonly List<Card>       _playedCards = new();
        private readonly HashSet<Suit>[]  _suitVoid;   // couleurs épuisées par joueur
        private Suit                      _trump;

        public AIPlayer()
        {
            _suitVoid = new HashSet<Suit>[4];
            for (int i = 0; i < 4; i++) _suitVoid[i] = new HashSet<Suit>();
        }

        public void Reset(Suit trump)
        {
            _trump = trump;
            _playedCards.Clear();
            for (int i = 0; i < 4; i++) _suitVoid[i].Clear();
        }

        /// <summary>
        /// Mémorise une carte jouée et met à jour les couleurs épuisées.
        /// </summary>
        public void RecordPlay(int playerSeat, Card card, Suit leadSuit)
        {
            _playedCards.Add(card);

            // Si le joueur n'a pas suivi la couleur demandée (et n'a pas coupé),
            // on note qu'il est "void" dans cette couleur.
            if (card.Suit != leadSuit && card.Suit != _trump)
                _suitVoid[playerSeat].Add(leadSuit);
        }

        /// <summary>
        /// Priorité 1 — Décision prise d'atout tour 1.
        /// Prendre si la main est suffisamment forte.
        /// </summary>
        public bool ShouldTakeTrump1(Player player, Card trumpCard)
        {
            var trumpInHand = player.GetTrumps(trumpCard.Suit);
            int score = trumpInHand.Count;
            score += trumpInHand.Any(c => c.Rank == Rank.Jack)  ? 2 : 0;
            score += trumpInHand.Any(c => c.Rank == Rank.Nine)  ? 1 : 0;
            score += trumpInHand.Any(c => c.Rank == Rank.Ace)   ? 1 : 0;
            return score >= 2;
        }

        /// <summary>
        /// Priorité 1 — Décision prise d'atout tour 2.
        /// Évaluer les couleurs et prendre si main suffisamment forte.
        /// </summary>
        public (bool take, Suit suit) ShouldTakeTrump2(Player player, Suit excluded)
        {
            Suit bestSuit = excluded;
            int bestScore = 0;
            foreach (Suit s in System.Enum.GetValues(typeof(Suit)))
            {
                if (s == excluded) continue;
                var inSuit = player.GetSuit(s);
                int score = inSuit.Count;
                score += inSuit.Any(c => c.Rank == Rank.Jack)  ? 2 : 0;
                score += inSuit.Any(c => c.Rank == Rank.Nine)  ? 1 : 0;
                score += inSuit.Any(c => c.Rank == Rank.Ace)   ? 1 : 0;
                if (score > bestScore) { bestScore = score; bestSuit = s; }
            }
            return bestScore >= 2 ? (true, bestSuit) : (false, excluded);
        }

        /// <summary>
        /// Choisit la meilleure carte à jouer parmi les cartes autorisées.
        /// Respecte toutes les priorités de la bible IA V1.
        /// </summary>
        public Card ChooseCard(
            Player player,
            List<Card> allowedCards,
            List<TrickPlay> currentTrick)
        {
            if (allowedCards.Count == 1) return allowedCards[0];

            int mySeat    = (int)player.Seat;
            int partner   = RuleEngine.GetPartnerSeat(player.Seat);
            bool isLeading = currentTrick.Count == 0;

            // Gagnant actuel du pli
            int curWinner = isLeading ? -1 : RuleEngine.GetTrickWinner(currentTrick, _trump);
            bool partnerIsMaster = !isLeading && curWinner == partner;
            bool enemyIsMaster   = !isLeading && curWinner >= 0 && curWinner != mySeat && curWinner != partner;

            // ── ENTAME ──────────────────────────────────────────────────────
            if (isLeading) return ChooseLeadCard(player, allowedCards);

            // ── PARTENAIRE MAÎTRE ────────────────────────────────────────────
            // Anti-comportement : ne pas couper son partenaire
            if (partnerIsMaster) return ChargeOrDiscard(player, allowedCards);

            // ── ADVERSAIRE MAÎTRE ────────────────────────────────────────────
            if (enemyIsMaster) return TryWinOrDiscard(player, allowedCards, currentTrick);

            // ── DÉFAUT ───────────────────────────────────────────────────────
            return Discard(allowedCards);
        }

        // ── ENTAME ──────────────────────────────────────────────────────────
        private Card ChooseLeadCard(Player player, List<Card> allowed)
        {
            // Priorité 4 — As maître hors atout
            var masterAces = allowed.Where(c =>
                c.Rank == Rank.Ace &&
                c.Suit != _trump &&
                !_playedCards.Any(p => p.Suit == c.Suit && p.Rank == Rank.Ace) &&
                !_suitVoid.Where((_, i) => i != (int)player.Seat).Any(v => v.Contains(c.Suit))
            ).ToList();
            if (masterAces.Any()) return masterAces[0];

            // Tirer atout si main forte en atout (Priorité 3)
            var myTrumps = allowed.Where(c => c.Suit == _trump).ToList();
            if (myTrumps.Count >= 3)
            {
                // Tirer avec le plus petit atout pour économiser les gros
                return myTrumps.OrderBy(c => c.TrumpOrder()).First();
            }

            // Priorité 5 — Éviter de jouer les Dix
            // Jouer petite carte inutile
            var cheap = allowed
                .Where(c => c.Suit != _trump && c.Rank != Rank.Ace && c.Rank != Rank.Ten)
                .OrderBy(c => c.NormalOrder())
                .ToList();
            if (cheap.Any()) return cheap[0];

            // Défausse minimum
            return Discard(allowed);
        }

        // ── CHARGER (partenaire maître) ──────────────────────────────────────
        private Card ChargeOrDiscard(Player player, List<Card> allowed)
        {
            // Charger avec 10 ou As si possible (Priorité 4/5)
            var big = allowed
                .Where(c => c.Suit != _trump && (c.Rank == Rank.Ace || c.Rank == Rank.Ten))
                .OrderByDescending(c => c.NormalValue())
                .ToList();
            if (big.Any()) return big[0];

            // Pisser : jeter petite carte inutile (pas d'atout précieux)
            var cheap = allowed
                .Where(c => c.Suit != _trump && c.Rank != Rank.Ace && c.Rank != Rank.Ten)
                .OrderBy(c => c.NormalOrder())
                .ToList();
            if (cheap.Any()) return cheap[0];

            // En dernier recours, la moins chère
            return Discard(allowed);
        }

        // ── TENTER DE GAGNER (adversaire maître) ────────────────────────────
        private Card TryWinOrDiscard(Player player, List<Card> allowed, List<TrickPlay> trick)
        {
            // Trouver les cartes qui gagnent le pli
            var winning = allowed.Where(c =>
            {
                var mockTrick = new List<TrickPlay>(trick) { new TrickPlay((int)player.Seat, c) };
                return RuleEngine.GetTrickWinner(mockTrick, _trump) == (int)player.Seat;
            }).ToList();

            if (winning.Any())
            {
                // Priorité 2 — Gagner au coût minimal
                // Priorité 3 — Éviter Valet/9 d'atout si possible
                var cheapWin = winning
                    .OrderBy(c => c.Value(_trump))
                    .ToList();

                // Anti-comportement : ne pas gaspiller un gros atout si un cheap suffit
                var nonBigTrump = cheapWin.FirstOrDefault(c =>
                    !(c.Suit == _trump && (c.Rank == Rank.Jack || c.Rank == Rank.Nine)));
                return nonBigTrump ?? cheapWin[0];
            }

            // Ne peut pas gagner : défausser le moins cher
            return Discard(allowed);
        }

        // ── DÉFAUSSE (perdre au moindre coût) ───────────────────────────────
        private Card Discard(List<Card> allowed)
        {
            // Priorité 5 — préserver les Dix
            // Priorité 4 — préserver les As
            // Priorité 3 — préserver les gros atouts
            return allowed
                .OrderBy(c => c.Value(_trump))        // moindre valeur d'abord
                .ThenBy(c => c.Suit == _trump ? 1 : 0) // éviter atout si possible
                .First();
        }

        // ── MÉMOIRE ──────────────────────────────────────────────────────────
        public bool IsTrumpPlayed(Rank rank) =>
            _playedCards.Any(c => c.Suit == _trump && c.Rank == rank);

        public List<Card> GetRemainingTrumps(Player player) =>
            player.GetTrumps(_trump)
                  .Where(c => !_playedCards.Any(p => p.Suit == c.Suit && p.Rank == c.Rank))
                  .ToList();
    }
}
