using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public enum GameType
    {
        STANDARD,
        T,
        L,
        O,
        X
    }

    public enum GameStatus
    {
        Draft,
        Playing,
        Finished
    }

    public abstract class Game
    {
        #region Properties

        public string Name { get; }
        public GameType GameType { get; }
        public GameStatus Status { get; }
        public short MaxNBallsPerColumn { get; }
        public Maybe<Player> Winner { get; }

        protected HashSet<Ball> _ballsConfigured;
        public IReadOnlyCollection<Ball> BallsConfigured { get => this._ballsConfigured.ToList(); }

        protected HashSet<Ball> _ballsPlayed;
        public IReadOnlyCollection<Ball> BallsPlayed { get => this._ballsPlayed.ToList(); }

        protected HashSet<Player> _players;
        public IReadOnlyCollection<Player> Players { get => this._players.ToList(); }

        #endregion

        #region Constructors

        private Game()
        {
            _players = new HashSet<Player>();
            _ballsPlayed = new HashSet<Ball>();
            Winner = Maybe<Player>.None;
            Status = GameStatus.Draft;
        }

        protected Game(string name, GameType gameType,
            HashSet<Ball> balls, short maxNBallsPerColumn): this()
        {
            Name = name;
            GameType = gameType;
            MaxNBallsPerColumn = maxNBallsPerColumn;
            _ballsConfigured = balls;
        }

        protected Game(string name, GameType gameType,
            HashSet<Ball> balls, short maxNBallsPerColumn,
            HashSet<Player> players, GameStatus status) : this()
        {
            Name = name;
            GameType = gameType;
            Status = status;
            MaxNBallsPerColumn = maxNBallsPerColumn;
            _ballsConfigured = balls;
            _players = players;
        }

        protected Game(string name, GameType gameType,
            HashSet<Ball> balls, short maxNBallsPerColumn,
            HashSet<Player> players, Player winner,
            HashSet<Ball> ballsPlayed, GameStatus status) : this()
        {
            Name = name;
            GameType = gameType;
            Status = status;
            MaxNBallsPerColumn = maxNBallsPerColumn;
            _ballsConfigured = balls;
            _ballsPlayed = ballsPlayed;
            _players = players;
            Winner = winner;
        }

        #endregion

        #region Behaviour

        public abstract Result AddPlayer(string withName);

        public abstract Result UpdatePlayerInfo(Player playerToUpdate, string newName);

        public abstract Result<Board> AddBoardToPlayer(Player player);

        public abstract Result RemoveBoardFromPlayer(Player player, Board board);

        public abstract Result RemovePlayer(Player playerToRemove);

        public abstract Result<Game> Start();

        public abstract Result PlayBall(Ball ballToPlay);

        public abstract Result<Ball> RadmonlyPlayBall();

        public abstract Result<Game> SetWinner(Player winner);

        #endregion
    }
}
