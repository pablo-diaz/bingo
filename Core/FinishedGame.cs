using System.Collections.Generic;

namespace Core
{
    public class FinishedGame
    {
        #region Properties

        public string Name { get; }
        public GameType GameType { get; }
        public Player Winner { get; }
        public IReadOnlyCollection<Ball> BallsConfigured { get; }
        public IReadOnlyCollection<Ball> BallsPlayed { get; }
        public IReadOnlyCollection<Player> Players { get; }

        #endregion

        #region Constructors

        internal FinishedGame(string name, GameType gameType, IReadOnlyCollection<Ball> withBallsConfigured, 
            IReadOnlyCollection<Player> withPlayers, IReadOnlyCollection<Ball> withBallsPlayed, Player withWinner)
        {
            this.Name = name;
            this.GameType = gameType;
            this.Winner = withWinner;
            this.Players = withPlayers;
            this.BallsConfigured = withBallsConfigured;
            this.BallsPlayed = withBallsPlayed;
        }

        #endregion
    }
}
