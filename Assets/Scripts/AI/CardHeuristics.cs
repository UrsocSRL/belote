using System.Collections.Generic;
using System.Linq;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Briques de base réutilisables par les règles de <see cref="ICardSelectionRule"/>.
    /// Regroupe les heuristiques élémentaires (entame, défausse, tentative de gain)
    /// sur lesquelles s'appuieront les futures situations de la bible IA experte.
    /// </summary>
    public static class CardHeuristics
    {
        /// <summary>
        /// Défausse au moindre coût : carte de plus faible valeur, atout évité si possible.
        /// </summary>
        public static Card Discard(TrickContext ctx, List<Card> candidates)
        {
            return candidates
                .OrderBy(c => c.Value(ctx.Trump))
                .ThenBy(c => c.Suit == ctx.Trump ? 1 : 0)
                .First();
        }

        /// <summary>
        /// Choix d'entame : As maître connu, sinon tirer l'atout si la main est forte,
        /// sinon petite carte non précieuse.
        /// </summary>
        public static Card ChooseLead(TrickContext ctx, List<Card> candidates)
        {
            var masterAces = candidates.Where(c =>
                c.Rank == Rank.Ace &&
                c.Suit != ctx.Trump &&
                !ctx.PlayedCards.Any(p => p.Suit == c.Suit && p.Rank == Rank.Ace) &&
                !ctx.SuitVoids.Where((_, seat) => seat != ctx.MySeat).Any(v => v.Contains(c.Suit))
            ).ToList();
            if (masterAces.Any()) return masterAces[0];

            var myTrumps = candidates.Where(c => c.Suit == ctx.Trump).ToList();
            if (myTrumps.Count >= 3)
                return myTrumps.OrderBy(c => c.TrumpOrder()).First();

            var cheap = candidates
                .Where(c => c.Suit != ctx.Trump && c.Rank != Rank.Ace && c.Rank != Rank.Ten)
                .OrderBy(c => c.NormalOrder())
                .ToList();
            if (cheap.Any()) return cheap[0];

            return Discard(ctx, candidates);
        }

        /// <summary>
        /// Tente de remporter le pli au moindre coût ; à défaut, défausse.
        /// </summary>
        public static Card TryWinOrDiscard(TrickContext ctx, List<Card> candidates)
        {
            var winning = candidates.Where(c =>
            {
                var mockTrick = new List<TrickPlay>(ctx.CurrentTrick) { new TrickPlay(ctx.MySeat, c) };
                return RuleEngine.GetTrickWinner(mockTrick, ctx.Trump) == ctx.MySeat;
            }).ToList();

            if (winning.Any())
            {
                var cheapWin = winning.OrderBy(c => c.Value(ctx.Trump)).ToList();

                // Ne pas gaspiller un gros atout (Valet/9) si une carte plus faible suffit.
                var nonBigTrump = cheapWin.FirstOrDefault(c =>
                    !(c.Suit == ctx.Trump && (c.Rank == Rank.Jack || c.Rank == Rank.Nine)));
                return nonBigTrump ?? cheapWin[0];
            }

            return Discard(ctx, candidates);
        }
    }
}
