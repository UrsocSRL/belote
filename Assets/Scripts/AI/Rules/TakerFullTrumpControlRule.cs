using System.Linq;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Situation : le preneur entame un pli, la chasse à l'atout est terminée
    /// et il a le contrôle total (aucun atout adverse ne circule encore).
    /// Décision : ne plus jouer atout (il sera utile en défense plus tard) et
    /// entamer une couleur non-atout, en respectant l'ordre de préférence :
    /// 1) carte intermédiaire (ni As ni 10) — la plus forte d'entre elles,
    /// 2) un 10 seulement si aucune intermédiaire n'est disponible,
    /// 3) un As en dernier recours, pour préserver le "10 de der".
    /// </summary>
    public class TakerFullTrumpControlRule : ICardSelectionRule
    {
        public const int TotalTrumpCount = 8;

        public string Name => "Contrôle total de l'atout (preneur)";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            chosen = null;
            if (!ctx.IsLeading || !ctx.IsTaker) return false;

            int trumpsPlayed = ctx.PlayedCards.Count(c => c.Suit == ctx.Trump);
            if (trumpsPlayed == 0) return false; // la chasse n'a pas encore commencé

            var myTrumps = ctx.Hand.Where(c => c.Suit == ctx.Trump).ToList();
            if (myTrumps.Count == 0) return false;

            int adverseTrumpsRemaining = TotalTrumpCount - myTrumps.Count - trumpsPlayed;
            if (adverseTrumpsRemaining > 0) return false; // chasse toujours en cours : voir TakerContinueTrumpChaseRule

            var nonTrump = ctx.LegalCards.Where(c => c.Suit != ctx.Trump).ToList();
            if (nonTrump.Count == 0) return false; // que de l'atout en main, pas d'alternative

            var intermediates = nonTrump.Where(c => c.Rank != Rank.Ace && c.Rank != Rank.Ten).ToList();
            if (intermediates.Count > 0)
            {
                chosen = intermediates.OrderByDescending(c => c.NormalOrder()).First();
                return true;
            }

            var tens = nonTrump.Where(c => c.Rank == Rank.Ten).ToList();
            if (tens.Count > 0)
            {
                chosen = tens.First();
                return true;
            }

            chosen = nonTrump.First(); // plus que des As
            return true;
        }
    }
}
