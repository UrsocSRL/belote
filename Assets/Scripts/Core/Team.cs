namespace BeloteFreeze.Core
{
    public class Team
    {
        public int Index;        // 0 = Nous (Sud+Nord), 1 = Eux (Ouest+Est)
        public string Name;
        public int TotalScore;   // Score cumulé sur toutes les manches
        public int RoundPoints;  // Points marqués dans la manche en cours
        public int TricksWon;    // Plis remportés dans la manche en cours
        public bool IsTaker;     // Est le preneur cette manche

        public Team(int index)
        {
            Index = index;
            Name = index == 0 ? "Nous" : "Eux";
        }

        public void ResetRound()
        {
            RoundPoints = 0;
            TricksWon = 0;
            IsTaker = false;
        }

        public void AddPoints(int pts) => RoundPoints += pts;
        public void AddTotalScore(int pts) => TotalScore += pts;
    }
}
