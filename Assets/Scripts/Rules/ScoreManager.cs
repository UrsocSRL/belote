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
        public int TrickPointsUs;
        public int TrickPointsThem;
        public int EarnedUs;
        public int EarnedThem;
        public int PendingPoints;
        public bool BeloteRebelote;
        public int BeloteTeam;
        public string Description;
    }

    /// <summary>
    /// Calcule les scores en fin de manche selon les règles officielles
    /// de la Fédération Française de Belote (Capot, Litige, Chute,
    /// Belote/Rebelote, Dix de der).
    /// </summary>
    public class ScoreManager
    {
        public int TotalScoreUs   { get; private set; }
        public int TotalScoreThem { get; private set; }
        public int PendingPoints  { get; private set; } // Litiges accumulés (10.1.c)

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
            int trickPointsUs,
            int trickPointsThem,
            int tricksUs,
            int tricksThem,
            int takerTeam,       // 0=Nous, 1=Eux
            bool beloteRebelote,
            int beloteTeam)      // 0=Nous, 1=Eux (si belote)
        {
            // Belote/Rebelote : +20 pts pour l'équipe qui l'a, imprenable (7.)
            int beloteBonusUs   = (beloteRebelote && beloteTeam == 0) ? 20 : 0;
            int beloteBonusThem = (beloteRebelote && beloteTeam == 1) ? 20 : 0;

            int pointsUs   = trickPointsUs   + beloteBonusUs;
            int pointsThem = trickPointsThem + beloteBonusThem;

            var result = new RoundResult
            {
                PointsUs         = pointsUs,
                PointsThem       = pointsThem,
                TrickPointsUs    = trickPointsUs,
                TrickPointsThem  = trickPointsThem,
                BeloteRebelote   = beloteRebelote,
                BeloteTeam       = beloteRebelote ? beloteTeam : -1
            };

            // 9.2 — Capot : les 8 plis remportés par la même équipe.
            bool capotUs   = tricksUs   == 8;
            bool capotThem = tricksThem == 8;

            if (capotUs || capotThem)
            {
                int capotTeam = capotUs ? 0 : 1;
                result.Outcome = capotTeam == takerTeam ? RoundOutcome.Capot : RoundOutcome.CapotAdverse;

                // Le camp qui réalise le capot empoche 252 pts, sauf la belote
                // adverse éventuelle qui reste imprenable (10.1.a).
                int pendingBonus = PendingPoints;
                PendingPoints = 0;

                if (capotTeam == 0)
                {
                    result.EarnedThem = beloteBonusThem;
                    result.EarnedUs   = 252 - beloteBonusThem + pendingBonus;
                    result.Description = takerTeam == 0
                        ? "🎉 Capot ! Nous gagnons tout (252 pts)"
                        : "🎉 Capot adverse ! Nous gagnons tout (252 pts)";
                }
                else
                {
                    result.EarnedUs   = beloteBonusUs;
                    result.EarnedThem = 252 - beloteBonusUs + pendingBonus;
                    result.Description = takerTeam == 1
                        ? "😔 Capot ! Eux gagnent tout (252 pts)"
                        : "😮 Capot adverse ! Eux gagnent tout (252 pts)";
                }

                Apply(result);
                return result;
            }

            // 10.1.c — Litige : totaux identiques (ex. 81-81, ou avec belote 91-91...).
            if (pointsUs == pointsThem)
            {
                result.Outcome = RoundOutcome.Litige;

                int takerTotal = takerTeam == 0 ? pointsUs   : pointsThem;
                int defTotal   = takerTeam == 0 ? pointsThem : pointsUs;

                // La défense marque son total ; le total des preneurs est
                // remis en jeu pour la donne suivante.
                if (takerTeam == 0)
                {
                    result.EarnedUs   = 0;
                    result.EarnedThem = defTotal;
                }
                else
                {
                    result.EarnedThem = 0;
                    result.EarnedUs   = defTotal;
                }

                PendingPoints += takerTotal;
                result.PendingPoints = PendingPoints;
                result.Description = $"⚖️ Litige ! {pointsUs}-{pointsThem}\n{PendingPoints} pts suspendus.";
                Apply(result);
                return result;
            }

            int pendingBonus2 = PendingPoints;
            PendingPoints = 0;

            // Preneur dedans ? (Chute, 10.1.b)
            int takerPoints = takerTeam == 0 ? pointsUs   : pointsThem;
            int defPoints   = takerTeam == 0 ? pointsThem : pointsUs;

            if (takerPoints < defPoints)
            {
                // 10.1.b — Chute : la défense marque 162 + ses annonces + sa
                // belote + le bonus de litige éventuel. Le preneur ne marque
                // que sa belote (imprenable).
                result.Outcome = RoundOutcome.Dedans;

                int defBelote   = takerTeam == 0 ? beloteBonusThem : beloteBonusUs;
                int takerBelote = takerTeam == 0 ? beloteBonusUs   : beloteBonusThem;
                int defEarned   = 162 + pendingBonus2 + defBelote;

                if (takerTeam == 0)
                {
                    result.EarnedUs   = takerBelote;
                    result.EarnedThem = defEarned;
                    result.Description = $"😔 Dedans ! Eux marquent {defEarned} pts";
                }
                else
                {
                    result.EarnedThem = takerBelote;
                    result.EarnedUs   = defEarned;
                    result.Description = $"🎉 Ils sont dedans ! Nous marquons {defEarned} pts";
                }
            }
            else
            {
                // 10.1.a — Contrat réussi : chaque camp marque son total ;
                // le bonus de litige éventuel revient au camp gagnant (le preneur).
                result.Outcome = RoundOutcome.Normal;
                result.EarnedUs   = pointsUs   + (takerTeam == 0 ? pendingBonus2 : 0);
                result.EarnedThem = pointsThem + (takerTeam == 1 ? pendingBonus2 : 0);
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
