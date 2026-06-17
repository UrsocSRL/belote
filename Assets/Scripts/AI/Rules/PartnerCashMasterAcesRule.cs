using System.Linq;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Bible IA — Encaissement des As du partenaire.
    /// Si je suis le partenaire du preneur, que j'ai la main et que les
    /// atouts adverses ont disparu ou sont sous contrôle (plus aucun atout
    /// adverse en circulation), mes As maîtres ne servent plus à rien en
    /// réserve : il faut les transformer immédiatement en points plutôt que
    /// de jouer une autre couleur.
    /// </summary>
    public class PartnerCashMasterAcesRule : ICardSelectionRule
    {
        public const int TotalTrumpCount = 8;

        public string Name => "Encaissement des As du partenaire";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            chosen = null;
            if (!ctx.IsLeading || ctx.IsTaker) return false;
            if (!RuleEngine.IsPartner(ctx.MySeat, ctx.TakerSeat)) return false;

            int trumpsPlayed = ctx.PlayedCards.Count(c => c.Suit == ctx.Trump);
            var myTrumps      = ctx.Hand.Where(c => c.Suit == ctx.Trump).ToList();

            int adverseTrumpsRemaining = TotalTrumpCount - myTrumps.Count - trumpsPlayed;
            if (adverseTrumpsRemaining > 0) return false; // atouts adverses encore actifs

            var ace = ctx.LegalCards.FirstOrDefault(c => c.Suit != ctx.Trump && c.Rank == Rank.Ace);
            if (ace == null) return false;

            chosen = ace;
            return true;
        }
    }
}
