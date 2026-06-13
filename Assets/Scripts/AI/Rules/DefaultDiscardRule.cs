using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Règle de repli inconditionnelle : doit toujours être enregistrée en
    /// dernière position dans <see cref="CardSelector"/> pour garantir un retour.
    /// </summary>
    public class DefaultDiscardRule : ICardSelectionRule
    {
        public string Name => "Défausse par défaut";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            chosen = CardHeuristics.Discard(ctx, ctx.LegalCards);
            return true;
        }
    }
}
