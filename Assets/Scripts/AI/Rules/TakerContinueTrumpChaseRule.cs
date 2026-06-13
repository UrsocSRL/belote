using System.Linq;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Situation : le preneur entame un pli alors que la chasse à l'atout est
    /// déjà engagée (au moins un atout est déjà tombé) et que des atouts
    /// adverses sont encore en circulation.
    /// Décision : jouer le maître-atout actuel (le plus haut atout restant
    /// dans notre main) pour poursuivre la chasse — il est garanti de
    /// remporter le pli puisqu'aucun atout plus fort n'est en circulation.
    /// </summary>
    public class TakerContinueTrumpChaseRule : ICardSelectionRule
    {
        public const int TotalTrumpCount = 8;

        public string Name => "Poursuite de la chasse à l'atout (preneur)";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            chosen = null;
            if (!ctx.IsLeading || !ctx.IsTaker) return false;

            int trumpsPlayed = ctx.PlayedCards.Count(c => c.Suit == ctx.Trump);
            if (trumpsPlayed == 0) return false; // la chasse n'a pas encore commencé (entame initiale)

            var myTrumps = ctx.Hand.Where(c => c.Suit == ctx.Trump).ToList();
            if (myTrumps.Count == 0) return false;

            int adverseTrumpsRemaining = TotalTrumpCount - myTrumps.Count - trumpsPlayed;
            if (adverseTrumpsRemaining <= 0) return false; // contrôle total : voir TakerFullTrumpControlRule

            chosen = myTrumps.OrderByDescending(c => c.TrumpOrder()).First();
            return true;
        }
    }
}
