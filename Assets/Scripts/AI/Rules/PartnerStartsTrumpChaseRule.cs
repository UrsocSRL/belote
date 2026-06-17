using System.Linq;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Bible IA — Soutien au preneur après récupération de la main.
    /// Si je suis le partenaire du preneur, que j'entame un pli après l'avoir
    /// remporté, et qu'aucun tour d'atout n'a encore été joué depuis le début
    /// de la manche, j'initie immédiatement la chasse à l'atout (le preneur
    /// ne doit pas être seul à la mener).
    /// Cartes a eviter : Valet et 9 d'atout (à conserver autant que possible).
    /// Ordre de préférence : Roi > Dame > 8 > 7 > As.
    /// </summary>
    public class PartnerStartsTrumpChaseRule : ICardSelectionRule
    {
        public string Name => "Soutien atout du partenaire (entame)";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            chosen = null;
            if (!ctx.IsLeading || ctx.IsTaker) return false;
            if (!RuleEngine.IsPartner(ctx.MySeat, ctx.TakerSeat)) return false;

            int trumpsPlayed = ctx.PlayedCards.Count(c => c.Suit == ctx.Trump);
            if (trumpsPlayed > 0) return false; // la chasse est déjà engagée

            var myTrumps = ctx.LegalCards.Where(c => c.Suit == ctx.Trump).ToList();
            if (myTrumps.Count == 0) return false;

            Rank[] priority = { Rank.King, Rank.Queen, Rank.Eight, Rank.Seven, Rank.Ace };
            foreach (var rank in priority)
            {
                var card = myTrumps.FirstOrDefault(c => c.Rank == rank);
                if (card != null) { chosen = card; return true; }
            }

            // Que Valet/9 en atout : on en garde un (le moins précieux des deux).
            chosen = myTrumps.OrderBy(c => c.TrumpOrder()).First();
            return true;
        }
    }
}
