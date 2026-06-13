using BeloteFreeze.Core;

namespace BeloteFreeze.UI
{
    public enum GamePhase
    {
        Distribution,
        FirstBiddingRound,
        SecondBiddingRound,
        CompletingDeal,
        Playing,
        TrickEnd,
        Scoring
    }

    /// <summary>
    /// État centralisé de la partie — lu par GameManager et UIManager.
    /// </summary>
    public class GameState
    {
        public GamePhase Phase;
        public Suit      Trump;
        public int       DealerSeat;
        public int       TakerSeat;
        public int       TakerTeam;
        public int       CurrentPlayerSeat;
        public int       TrickLeaderSeat;

        // Scores de manche
        public int[]     RoundPoints  = new int[2]; // [0]=Nous, [1]=Eux
        public int[]     TricksWon    = new int[4]; // par siège
        public int       PendingPoints;

        // Belote
        public int       BeloteHolderSeat = -1;
        public bool      BeloteAnnounced;
        public bool      RebeloteAnnounced;

        public void ResetRound()
        {
            Phase             = GamePhase.Distribution;
            Trump             = default;
            TakerSeat         = -1;
            TakerTeam         = -1;
            CurrentPlayerSeat = -1;
            TrickLeaderSeat   = -1;
            RoundPoints[0]    = 0;
            RoundPoints[1]    = 0;
            for (int i = 0; i < 4; i++) TricksWon[i] = 0;
            BeloteHolderSeat  = -1;
            BeloteAnnounced   = false;
            RebeloteAnnounced = false;
        }
    }
}
