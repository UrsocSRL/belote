using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Situation : le partenaire est actuellement maître du pli.
    /// Anti-comportement : ne pas couper son partenaire, ne pas gaspiller
    /// un gros atout ou un As inutilement — on défausse.
    /// </summary>
    public class PartnerMasterRule : ICardSelectionRule
    {
        public string Name => "Partenaire maître";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            if (ctx.IsLeading || !ctx.PartnerIsMaster) { chosen = null; return false; }
            chosen = CardHeuristics.Discard(ctx, ctx.LegalCards);
            return true;
        }
    }
}
