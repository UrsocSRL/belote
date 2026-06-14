using System.Collections.Generic;
using UnityEngine;
using BeloteFreeze.Core;

namespace BeloteFreeze.Rules
{
    /// <summary>
    /// Rule Engine officiel — Bible Rule Engine V1
    /// Détermine les cartes autorisées selon les 15 cas officiels.
    /// </summary>
    public static class RuleEngine
    {
        /// <summary>
        /// Retourne la liste des cartes autorisées pour un joueur donné.
        /// Respecte les 15 cas de la bible Rule Engine V1.
        /// </summary>
        public static List<Card> GetAllowedCards(
            Player player,
            List<TrickPlay> currentTrick,
            Suit trump)
        {
            var hand = player.Hand;
            if (hand.Count == 0) return new List<Card>();

            // Cas 12/13/14 — Phase prise d'atout : toutes les cartes autorisées
            // (géré par TrumpManager, pas ici)

            // Entame libre
            if (currentTrick.Count == 0)
                return new List<Card>(hand);

            var leadCard = currentTrick[0].Card;
            var leadSuit = leadCard.Suit;
            bool isTrumpLead = leadSuit == trump;

            // Qui est maître du pli ?
            int currentWinner = GetTrickWinner(currentTrick, trump);
            int partnerSeat = GetPartnerSeat(player.Seat);
            bool partnerIsMaster = currentWinner == partnerSeat;

            List<Card> result;
            if (isTrumpLead)
            {
                // Atout demandé
                var myTrumps = player.GetTrumps(trump);
                if (myTrumps.Count == 0)
                {
                    result = new List<Card>(hand); // Pas d'atout : libre
                }
                // Cas 1 — Fourniture obligatoire à l'atout : on a de l'atout, on DOIT
                // en jouer, même si le partenaire est maître (cf. Cas 5 ci-dessous,
                // qui ne dispense que de l'obligation de SURCOUPE, pas de fourniture).
                else if (partnerIsMaster)
                {
                    // Cas 5 — Partenaire maître : pas de surcoupe obligatoire,
                    // mais l'atout reste obligatoire (fourniture).
                    result = myTrumps;
                }
                else
                {
                    // Cas 3 — Surcoupe obligatoire sur adversaire maître
                    var highestTrump = GetHighestTrumpInTrick(currentTrick, trump);
                    if (highestTrump != null)
                    {
                        var overcuts = myTrumps.FindAll(c => c.TrumpOrder() > highestTrump.TrumpOrder());
                        // Cas 3 si surcoupe possible, sinon Cas 4 (atout quand même)
                        result = overcuts.Count > 0 ? overcuts : myTrumps;
                    }
                    else
                    {
                        result = myTrumps;
                    }
                }
            }
            else
            {
                // Couleur normale demandée

                // Cas 1 — Fourniture obligatoire
                var mySuit = player.GetSuit(leadSuit);
                if (mySuit.Count > 0)
                {
                    result = mySuit;
                }
                else
                {
                    // Pas de la couleur demandée
                    var myTrumps = player.GetTrumps(trump);
                    if (myTrumps.Count == 0)
                    {
                        result = new List<Card>(hand); // Pas d'atout : libre
                    }
                    else if (partnerIsMaster)
                    {
                        // Cas 5 — Partenaire maître : pisse autorisée (pas de coupe obligatoire)
                        result = new List<Card>(hand);
                    }
                    else
                    {
                        // Cas 2 — Coupe obligatoire / Cas 3 — Surcoupe obligatoire
                        var highestTrump = GetHighestTrumpInTrick(currentTrick, trump);
                        if (highestTrump != null)
                        {
                            // Cas 3 — Surcoupe obligatoire sur adversaire maître
                            var overcuts = myTrumps.FindAll(c => c.TrumpOrder() > highestTrump.TrumpOrder());
                            // Cas 3 si surcoupe possible, sinon Cas 4 (atout quand même)
                            result = overcuts.Count > 0 ? overcuts : myTrumps;
                        }
                        else
                        {
                            // Cas 2 — Coupe obligatoire (pas encore d'atout dans le pli)
                            result = myTrumps;
                        }
                    }
                }
            }

            Debug.Log($"[RuleEngine] Joueur {player.Seat} | Couleur demandée : {leadSuit} | "
                + $"Atouts en main : {player.GetTrumps(trump).Count} | "
                + $"Cartes légales : [{string.Join(", ", result)}]");

            return result;
        }

        /// <summary>
        /// Cas 15 — Détermine le gagnant du pli selon les règles d'atout.
        /// Retourne le PlayerSeat du gagnant.
        /// </summary>
        public static int GetTrickWinner(List<TrickPlay> trick, Suit trump)
        {
            if (trick.Count == 0) return -1;
            var best = trick[0];
            for (int i = 1; i < trick.Count; i++)
            {
                if (trick[i].Card.Beats(best.Card, trump))
                    best = trick[i];
            }
            return best.PlayerSeat;
        }

        /// <summary>
        /// Retourne la carte d'atout la plus haute dans le pli actuel.
        /// </summary>
        public static Card GetHighestTrumpInTrick(List<TrickPlay> trick, Suit trump)
        {
            Card highest = null;
            foreach (var play in trick)
            {
                if (play.Card.Suit == trump)
                {
                    if (highest == null || play.Card.TrumpOrder() > highest.TrumpOrder())
                        highest = play.Card;
                }
            }
            return highest;
        }

        /// <summary>
        /// Calcule la valeur totale des cartes d'un pli.
        /// </summary>
        public static int GetTrickPoints(List<TrickPlay> trick, Suit trump)
        {
            int total = 0;
            foreach (var play in trick)
                total += play.Card.Value(trump);
            return total;
        }

        /// <summary>
        /// Retourne le siège du partenaire (opposé diagonal).
        /// </summary>
        public static int GetPartnerSeat(PlayerSeat seat)
        {
            return ((int)seat + 2) % 4;
        }

        /// <summary>
        /// Indique si deux sièges forment une paire de partenaires (0↔2, 1↔3).
        /// </summary>
        public static bool IsPartner(int seatA, int seatB)
        {
            return GetPartnerSeat((PlayerSeat)seatA) == seatB;
        }

        /// <summary>
        /// Retourne la carte actuellement maîtresse du pli (respecte couleur demandée et atout).
        /// </summary>
        public static Card GetWinningCard(List<TrickPlay> trick, Suit trump)
        {
            if (trick.Count == 0) return null;
            var best = trick[0];
            for (int i = 1; i < trick.Count; i++)
                if (trick[i].Card.Beats(best.Card, trump)) best = trick[i];
            return best.Card;
        }
    }

    /// <summary>
    /// Représente une carte jouée dans un pli avec le siège du joueur.
    /// </summary>
    public class TrickPlay
    {
        public int PlayerSeat;
        public Card Card;

        public TrickPlay(int playerSeat, Card card)
        {
            PlayerSeat = playerSeat;
            Card = card;
        }
    }
}
