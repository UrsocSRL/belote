using UnityEngine;
using BeloteFreeze.Core;

namespace BeloteFreeze.Rules
{
    public enum RoundOutcome
    {
        Normal,
        Dedans,
        Capot,
        CapotAdverse,
        Litige
    }

    public struct RoundResult
    {
        public RoundOutcome Outcome;
        public int PointsUs;
        public int PointsThem;
        public int EarnedUs;
        public int EarnedThem;
        public int PendingPoints;
        public bool BeloteRebelote;
        public string Description;
    }

    /// <summary>
    /// Calcule les scores en fin de manche selon toutes les règles belote.
    /// Bible Demo V0.1 : Capot, Litige, Dedans, Belote/Rebelote, Dernier pli.
    /// </summary>
    public class ScoreManager
    {
        public int TotalScoreUs   { get; private set; }
        public int TotalScoreThem { get; private set; }
        public int PendingPoints  { get; private set; } // Cas 10/11 — Litiges accumulés

        public void Reset()
        {
            TotalScoreUs   = 0;
            TotalScoreThem = 0;
            PendingPoints  = 0;
        }

        /// <summary>
        /// Calcule et applique le score de fin de manche.
        /// </summary>
        public RoundResult ComputeRound(
            int pointsUs,
            int pointsThem,
            int tricksUs,
            int tricksThem,
            int takerTeam,       // 0=Nous, 1=Eux
            bool beloteRebelote,
            int beloteTeam)      // 0=Nous, 1=Eux (si belote)
        {
            var result = new RoundResult
            {
                PointsUs   = pointsUs,
                PointsThem = pointsThem,
                BeloteRebelote = beloteRebelote
            };

            // Belote/Rebelote : +20 pts pour l'équipe qui l'a
            if (beloteRebelote)
            {
                if (beloteTeam == 0) pointsUs   += 20;
                else                 pointsThem += 20;
                result.PointsUs   = pointsUs;
                result.PointsThem = pointsThem;
            }

            // Cas 9 — Capot
            bool capotUs   = tricksUs   == 8;
            bool capotThem = tricksThem == 8;

            if (capotUs || capotThem)
            {
                int capotTeam = capotUs ? 0 : 1;

                if (capotTeam == takerTeam)
                {
                    // Capot par le preneur
                    result.Outcome = RoundOutcome.Capot;
                    if (takerTeam == 0)
                    {
                        result.EarnedUs   = 252;
                        result.EarnedThem = 0;
                        result.Description = "🎉 Capot ! Nous gagnons tout (252 pts)";
                    }
                    else
                    {
                        result.EarnedUs   = 0;
                        result.EarnedThem = 252;
                        result.Description = "😔 Capot ! Eux gagnent tout (252 pts)";
                    }
                }
                else
                {
                    // Capot par les défenseurs (preneur est dedans de pire façon)
                    result.Outcome = RoundOutcome.CapotAdverse;
                    if (takerTeam == 0)
                    {
                        result.EarnedUs   = 0;
                        result.EarnedThem = 252;
                        result.Description = "😮 Capot adverse ! Eux gagnent tout (252 pts)";
                    }
                    else
                    {
                        result.EarnedUs   = 252;
                        result.EarnedThem = 0;
                        result.Description = "🎉 Capot adverse ! Nous gagnons tout (252 pts)";
                    }
                }

                Apply(result);
                return result;
            }

            // Cas 10 — Litige : 81-81
            if (pointsUs == 81 && pointsThem == 81)
            {
                PendingPoints += 81;
                result.Outcome      = RoundOutcome.Litige;
                result.EarnedUs     = 0;
                result.EarnedThem   = 0;
                result.PendingPoints = PendingPoints;
                result.Description  = $"⚖️ Litige ! 81-81\n{PendingPoints} pts suspendus.";
                // Cas 11 — les litiges s'accumulent, on n'applique pas de score
                return result;
            }

            // Preneur dedans ?
            int takerPoints = takerTeam == 0 ? pointsUs   : pointsThem;
            int defPoints   = takerTeam == 0 ? pointsThem : pointsUs;

            if (takerPoints <= defPoints)
            {
                // Cas dedans : l'équipe adverse marque 162 + litiges accumulés
                result.Outcome = RoundOutcome.Dedans;
                int total = 162 + PendingPoints;
                PendingPoints = 0;

                if (takerTeam == 0)
                {
                    result.EarnedUs   = 0;
                    result.EarnedThem = total;
                    result.Description = $"😔 Dedans ! Eux marquent {total} pts";
                }
                else
                {
                    result.EarnedUs   = total;
                    result.EarnedThem = 0;
                    result.Description = $"🎉 Ils sont dedans ! Nous marquons {total} pts";
                }
            }
            else
            {
                // Manche normale : chaque équipe marque ses points
                result.Outcome = RoundOutcome.Normal;
                result.EarnedUs   = pointsUs   + (takerTeam == 0 ? PendingPoints : 0);
                result.EarnedThem = pointsThem + (takerTeam == 1 ? PendingPoints : 0);
                PendingPoints = 0;
                result.Description = "✅ Manche terminée";
            }

            Apply(result);
            return result;
        }

        private void Apply(RoundResult r)
        {
            TotalScoreUs   += r.EarnedUs;
            TotalScoreThem += r.EarnedThem;
        }
    }
}
