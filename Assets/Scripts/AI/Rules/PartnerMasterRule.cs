using System.Linq;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Situation : le partenaire est actuellement maître du pli.
    /// Anti-comportement : ne jamais couper (surcouper) son partenaire —
    /// on défausse en évitant l'atout, sauf si l'atout est la seule
    /// possibilité (main composée uniquement d'atouts).
    /// </summary>
    public class PartnerMasterRule : ICardSelectionRule
    {
        public string Name => "Partenaire maître";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            if (ctx.IsLeading || !ctx.PartnerIsMaster) { chosen = null; return false; }

            var nonTrump = ctx.LegalCards.Where(c => c.Suit != ctx.Trump).ToList();
            chosen = CardHeuristics.Discard(ctx, nonTrump.Count > 0 ? nonTrump : ctx.LegalCards);
            return true;
        }
    }
}
