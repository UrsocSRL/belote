using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Situation : un adversaire est actuellement maître du pli.
    /// On tente de remporter le pli au moindre coût ; à défaut, on défausse.
    /// </summary>
    public class OpponentMasterRule : ICardSelectionRule
    {
        public string Name => "Adversaire maître";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            if (ctx.IsLeading || !ctx.OpponentIsMaster) { chosen = null; return false; }
            chosen = CardHeuristics.TryWinOrDiscard(ctx, ctx.LegalCards);
            return true;
        }
    }
}
