using System.Linq;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Situation : le preneur entame un pli sans posséder le Valet d'atout
    /// et souhaite ouvrir à l'atout pour faire tomber l'atout adverse.
    /// Le 9, l'As et le 10 d'atout sont des cartes stratégiques à conserver
    /// et ne doivent jamais servir d'ouverture : on joue la plus forte carte
    /// d'atout parmi Roi, Dame, 8, 7 (ordre Roi > Dame > 8 > 7).
    /// </summary>
    public class OpenTrumpWithoutJackRule : ICardSelectionRule
    {
        public string Name => "Ouverture atout sans le Valet (preneur)";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            chosen = null;
            if (!ctx.IsLeading || !ctx.IsTaker) return false;

            bool hasJackOfTrump = ctx.Hand.Any(c => c.Suit == ctx.Trump && c.Rank == Rank.Jack);
            if (hasJackOfTrump) return false;

            var openers = ctx.LegalCards
                .Where(c => c.Suit == ctx.Trump &&
                            (c.Rank == Rank.King || c.Rank == Rank.Queen ||
                             c.Rank == Rank.Eight || c.Rank == Rank.Seven))
                .ToList();
            if (openers.Count == 0) return false;

            // Roi > Dame > 8 > 7 — correspond exactement à l'ordre décroissant de TrumpOrder
            // pour ce sous-ensemble (Roi=3, Dame=2, 8=1, 7=0).
            chosen = openers.OrderByDescending(c => c.TrumpOrder()).First();
            return true;
        }
    }
}
