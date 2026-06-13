using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>Situation : c'est au joueur d'entamer le pli.</summary>
    public class LeadCardRule : ICardSelectionRule
    {
        public string Name => "Entame";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            if (!ctx.IsLeading) { chosen = null; return false; }
            chosen = CardHeuristics.ChooseLead(ctx, ctx.LegalCards);
            return true;
        }
    }
}
