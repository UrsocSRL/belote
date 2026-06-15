using System.Linq;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Bible IA — Defenseur qui entame.
    /// Ordre de priorite : carte seche (hors atout) > As (hors atout) >
    /// couleur la plus courte (hors atout) > atout en dernier recours.
    /// </summary>
    public class DefenderOpeningRule : ICardSelectionRule
    {
        public string Name => "Entame defenseur";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            if (!ctx.IsLeading || ctx.IsTaker) { chosen = null; return false; }

            var hand      = ctx.LegalCards;
            var nonTrump  = hand.Where(c => c.Suit != ctx.Trump).ToList();
            var bySuit    = nonTrump.GroupBy(c => c.Suit).ToList();

            // Priorite 1 : carte seche (seule dans sa couleur, hors atout, hors As)
            var dryCard = bySuit
                .Where(g => g.Count() == 1 && g.First().Rank != Rank.Ace)
                .Select(g => g.First())
                .FirstOrDefault();
            if (dryCard != null) { chosen = dryCard; return true; }

            // Priorite 2 : As (hors atout)
            var ace = nonTrump.FirstOrDefault(c => c.Rank == Rank.Ace);
            if (ace != null) { chosen = ace; return true; }

            // Priorite 3/4 : couleur la plus courte (hors atout)
            if (bySuit.Count > 0)
            {
                var shortestSuit = bySuit.OrderBy(g => g.Count()).First();
                chosen = shortestSuit.OrderBy(c => c.Value(ctx.Trump)).First();
                return true;
            }

            // Priorite 5 : que de l'atout en main -> dernier recours
            chosen = hand.OrderBy(c => c.TrumpOrder()).First();
            return true;
        }
    }
}
