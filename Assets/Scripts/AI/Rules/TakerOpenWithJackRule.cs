using System.Linq;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Situation : le preneur entame un pli, possède le Valet d'atout et au
    /// moins 3 atouts au total. La chasse à l'atout prime sur toute autre
    /// considération (encaissement d'As extérieurs, couleur maîtresse, etc.) :
    /// l'ouverture standard est le Valet lui-même.
    /// </summary>
    public class TakerOpenWithJackRule : ICardSelectionRule
    {
        public string Name => "Ouverture Valet d'atout (preneur)";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            chosen = null;
            if (!ctx.IsLeading || !ctx.IsTaker) return false;

            var trumps = ctx.Hand.Where(c => c.Suit == ctx.Trump).ToList();
            if (trumps.Count < 3) return false;

            var jack = trumps.FirstOrDefault(c => c.Rank == Rank.Jack);
            if (jack == null) return false;

            chosen = jack;
            return true;
        }
    }
}
