using System;
using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public enum GameState
    {
        Draft,
        Started,
        Finished
    }

    public class Game
    {
        #region Properties

        public string Name { get; }

        public GameState State { get; private set; }

        private HashSet<Ball> _ballsConfigured;
        public IReadOnlyCollection<Ball> BallsConfigured { get => this._ballsConfigured.ToList(); }

        private HashSet<Ball> _ballsPlayed;
        public IReadOnlyCollection<Ball> BallsPlayed { get => this._ballsPlayed.ToList(); }

        private HashSet<Player> _players;
        public IReadOnlyCollection<Player> Players { get => this._players.ToList(); }

        #endregion

        #region Constructors

        private Game(string name, HashSet<Ball> withBalls)
        {
            this.Name = name;
            this.State = GameState.Draft;
            this._players = new HashSet<Player>();
            this._ballsConfigured = withBalls;
            this._ballsPlayed = new HashSet<Ball>();
        }

        #endregion

        #region Builders

        public static Result<Game> Create(string name, short withNBalls)
        {
            if (string.IsNullOrEmpty(name))
                return Result.Failure<Game>("Name should be valid");

            if(withNBalls <= 0 || withNBalls % 5 != 0)
                return Result.Failure<Game>("Provide enough balls to play game");

            return Result.Ok(new Game(name, CreateNBallsSet(withNBalls)));
        }

        #endregion

        #region Public Methods

        public Result AddPlayer(Player newPlayer)
        {
            if (newPlayer == null)
                return Result.Failure("New player is null");

            if (State != GameState.Draft)
                return Result.Failure("Game has started already, thus no more new players are allowed");

            if (this._players.Contains(newPlayer))
                return Result.Failure("Game already contains the same player");

            if(this._players.Any(player => player.Security == newPlayer.Security))
                return Result.Failure("Game already contains a player with same security (i.e. same Login)");

            this._players.Add(newPlayer);

            return Result.Ok();
        }

        public void Start()
        {
            this.State = GameState.Started;
        }

        #endregion

        #region Helpers

        private static HashSet<Ball> CreateNBallsSet(short nBalls)
        {
            var ballList = Enumerable.Range(1, nBalls)
                .Select(number =>
                {
                    var letter = GetBallLetterForMaxBallCount((short)number, nBalls);
                    var newBallResult = Ball.Create((short)number, letter);
                    if (newBallResult.IsFailure)
                        throw new ApplicationException(newBallResult.Error);
                    return newBallResult.Value;
                })
                .ToList();

            return new HashSet<Ball>(ballList);
        }

        private static BallLeter GetBallLetterForMaxBallCount(short ballNumber, short maxBallCount)
        {
            var maxItemsPerBucket = maxBallCount / 5;
            var bucketIndex = (ballNumber - 1) / maxItemsPerBucket;
            return new BallLeter[] { BallLeter.B, BallLeter.I, BallLeter.N, BallLeter.G, BallLeter.O }[bucketIndex];
        }

        #endregion
    }
}
