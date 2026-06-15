using System.Linq;
using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Bible IA — Priorite de prise avec l'As.
    /// Si un adversaire est actuellement maitre du pli avec une carte de la
    /// couleur demandee (hors atout) et que le defenseur possede l'As de
    /// cette couleur, il doit prendre avec l'As : empecher le preneur de
    /// realiser sa carte, reprendre l'initiative, eviter les points gratuits.
    /// </summary>
    public class DefenderAceTakeRule : ICardSelectionRule
    {
        public string Name => "Prise avec l'As";

        public bool TryChoose(TrickContext ctx, out Card chosen)
        {
            if (ctx.IsLeading || ctx.IsTaker || !ctx.OpponentIsMaster) { chosen = null; return false; }

            Suit winningSuit = ctx.CurrentWinningCard.Suit;
            if (winningSuit == ctx.Trump) { chosen = null; return false; }

            var ace = ctx.LegalCards.FirstOrDefault(c => c.Suit == winningSuit && c.Rank == Rank.Ace);
            if (ace == null) { chosen = null; return false; }

            chosen = ace;
            return true;
        }
    }
}
