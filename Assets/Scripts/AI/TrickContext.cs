using System.Collections.Generic;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Contexte complet d'une décision de carte, calculé une fois par tour de jeu.
    /// Bible IA Experte : toute règle de sélection (<see cref="ICardSelectionRule"/>)
    /// ne doit raisonner qu'à partir de ce contexte — jamais d'état caché.
    /// </summary>
    public class TrickContext
    {
        /// <summary>Le joueur (IA) qui doit choisir une carte.</summary>
        public Player Player;
        public int    MySeat;
        public int    PartnerSeat;

        /// <summary>Main complète du joueur.</summary>
        public List<Card> Hand;

        /// <summary>Cartes légales pour ce coup (déjà filtrées par RuleEngine).</summary>
        public List<Card> LegalCards;

        /// <summary>Cartes posées dans le pli en cours (vide si entame).</summary>
        public List<TrickPlay> CurrentTrick;

        public Suit Trump;

        /// <summary>Toutes les cartes jouées depuis le début de la manche (mémoire IA).</summary>
        public List<Card> PlayedCards;

        /// <summary>Couleurs dont chaque siège est connu comme étant dépourvu.</summary>
        public HashSet<Suit>[] SuitVoids;

        /// <summary>Siège du preneur de la manche en cours.</summary>
        public int TakerSeat;

        /// <summary>Vrai si ce joueur est le preneur de la manche en cours.</summary>
        public bool IsTaker;

        /// <summary>Vrai si le joueur entame le pli (CurrentTrick vide).</summary>
        public bool IsLeading;

        /// <summary>Siège actuellement maître du pli, -1 si entame.</summary>
        public int CurrentWinnerSeat;

        /// <summary>Carte actuellement maîtresse du pli, null si entame.</summary>
        public Card CurrentWinningCard;

        /// <summary>Vrai si le partenaire est actuellement maître du pli.</summary>
        public bool PartnerIsMaster;

        /// <summary>Vrai si un adversaire est actuellement maître du pli.</summary>
        public bool OpponentIsMaster;

        /// <summary>
        /// Construit le contexte de décision à partir de l'état courant de la manche.
        /// </summary>
        public static TrickContext Build(
            Player player,
            List<Card> legalCards,
            List<TrickPlay> currentTrick,
            Suit trump,
            List<Card> playedCards,
            HashSet<Suit>[] suitVoids,
            int takerSeat)
        {
            var ctx = new TrickContext
            {
                Player       = player,
                MySeat       = (int)player.Seat,
                PartnerSeat  = RuleEngine.GetPartnerSeat(player.Seat),
                Hand         = player.Hand,
                LegalCards   = legalCards,
                CurrentTrick = currentTrick,
                Trump        = trump,
                PlayedCards  = playedCards,
                SuitVoids    = suitVoids,
                TakerSeat    = takerSeat,
                IsTaker      = (int)player.Seat == takerSeat,
                IsLeading    = currentTrick.Count == 0,
            };

            if (ctx.IsLeading)
            {
                ctx.CurrentWinnerSeat  = -1;
                ctx.CurrentWinningCard = null;
            }
            else
            {
                ctx.CurrentWinnerSeat  = RuleEngine.GetTrickWinner(currentTrick, trump);
                ctx.CurrentWinningCard = RuleEngine.GetWinningCard(currentTrick, trump);
                ctx.PartnerIsMaster    = ctx.CurrentWinnerSeat == ctx.PartnerSeat;
                ctx.OpponentIsMaster   = ctx.CurrentWinnerSeat != ctx.MySeat
                                         && !RuleEngine.IsPartner(ctx.MySeat, ctx.CurrentWinnerSeat);
            }

            return ctx;
        }
    }
}
