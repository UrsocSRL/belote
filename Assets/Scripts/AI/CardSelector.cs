using System.Collections.Generic;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Point d'entrée unique de l'IA experte. Évalue une liste ordonnée de
    /// règles (situations de référence) et retourne la carte produite par la
    /// première règle applicable. Une règle de repli inconditionnelle doit
    /// toujours être enregistrée en dernière position pour garantir un retour.
    /// </summary>
    public class CardSelector
    {
        private readonly List<ICardSelectionRule> _rules;

        public CardSelector(IEnumerable<ICardSelectionRule> rules)
        {
            _rules = new List<ICardSelectionRule>(rules);
        }

        /// <summary>
        /// Choisit la meilleure carte à jouer pour le contexte donné.
        /// Ne joue jamais une carte au hasard : à défaut de règle applicable,
        /// la première carte légale est retournée.
        /// </summary>
        public Card ChooseBestCard(TrickContext ctx)
        {
            if (ctx.LegalCards.Count == 1) return ctx.LegalCards[0];

            foreach (var rule in _rules)
                if (rule.TryChoose(ctx, out var card))
                    return card;

            return ctx.LegalCards[0];
        }
    }
}
