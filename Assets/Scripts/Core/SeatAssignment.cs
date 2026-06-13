using System;

namespace BeloteFreeze.Core
{
    /// <summary>
    /// Placement des joueurs (Sud/Ouest/Nord/Est) selon le nombre de joueurs
    /// humains disponibles. Priorité absolue aux humains, complétés par des IA.
    /// Index du tableau retourné : 0=Sud, 1=Ouest, 2=Nord, 3=Est.
    /// </summary>
    public static class SeatAssignment
    {
        public static bool[] GetHumanSeats(int humanCount)
        {
            humanCount = Math.Clamp(humanCount, 1, 4);
            return humanCount switch
            {
                // Cas 1 — 4 humains, aucune IA.
                4 => new[] { true, true, true, true },

                // Cas 2 — 3 humains, l'IA occupe la place restante (Est).
                3 => new[] { true, true, true, false },

                // Cas 3 — 2 humains, placés sur des équipes opposées
                // (Sud+Nord vs Ouest+Est) pour éviter toute collusion.
                2 => new[] { true, false, false, true },

                // Cas 4 — 1 humain (Sud), 3 IA.
                _ => new[] { true, false, false, false },
            };
        }
    }
}
