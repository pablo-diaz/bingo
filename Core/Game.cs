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
        public short WithNBallsMaxPerBoardBucket { get; }
        public GameState State { get; private set; }
        public Maybe<Player> Winner { get; private set; }

        private HashSet<Ball> _ballsConfigured;

        public IReadOnlyCollection<Ball> BallsConfigured { get => this._ballsConfigured.ToList(); }

        private HashSet<Ball> _ballsPlayed;
        public IReadOnlyCollection<Ball> BallsPlayed { get => this._ballsPlayed.ToList(); }

        private HashSet<Player> _players;
        public IReadOnlyCollection<Player> Players { get => this._players.ToList(); }

        #endregion

        #region Constructors

        private Game(string name, HashSet<Ball> withBalls, short withNBallsMaxPerBoardBucket)
        {
            this.Name = name;
            this.State = GameState.Draft;
            this.Winner = Maybe<Player>.None;
            this.WithNBallsMaxPerBoardBucket = withNBallsMaxPerBoardBucket;

            this._players = new HashSet<Player>();
            this._ballsConfigured = withBalls;
            this._ballsPlayed = new HashSet<Ball>();
        }

        #endregion

        #region Builders

        public static Result<Game> Create(string name, short withNBallsTotal, short withNBallsMaxPerBoardBucket)
        {
            if (string.IsNullOrEmpty(name))
                return Result.Failure<Game>("Name should be valid");

            if(withNBallsTotal <= 0 || withNBallsTotal % 5 != 0)
                return Result.Failure<Game>("Provide enough balls to play game");

            if(withNBallsMaxPerBoardBucket <= 1)
                return Result.Failure<Game>("Provide a valid number for balls per board bucket, so we can randomly create boards");

            if (withNBallsMaxPerBoardBucket > ((withNBallsTotal / 5) - 1))
                return Result.Failure<Game>("Provide enough balls per board bucket, so we can randomly create boards");

            return Result.Ok(new Game(name, CreateNBallsSet(withNBallsTotal), withNBallsMaxPerBoardBucket));
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

        public Result Start()
        {
            if (this._players.Count() < 2)
                return Result.Failure("There should be at least 2 Players to start Game");

            if(this._players.Any(player => !player.Boards.Any()))
                return Result.Failure("There should be at least 1 board setup for each Player to start Game");

            this.State = GameState.Started;

            return Result.Ok();
        }

        public Result PlayBall(Ball ballToPlay)
        {
            if(State != GameState.Started)
                return Result.Failure("Game has not been started yet");

            if (!this._ballsConfigured.Contains(ballToPlay))
                return Result.Failure("Ball is not in the possible set");

            if(this.BallsPlayed.Contains(ballToPlay))
                return Result.Failure("Ball has already been played");

            foreach(var player in this._players)
                player.PlayBall(ballToPlay);

            this._ballsPlayed.Add(ballToPlay);

            return Result.Ok();
        }

        public Result<Board> AddBoardToPlayer(Random randomizer, Player player)
        {
            if (State != GameState.Draft)
                return Result.Failure<Board>("Game is in wrong state");

            if(!this._players.Contains(player))
                return Result.Failure<Board>("Player is not part of Game");

            var newBoardResult = TryCreatingNewBoard(randomizer, tryUpToNTimes: 10);
            if (newBoardResult.IsFailure)
                return newBoardResult;

            player.AddBoard(newBoardResult.Value);

            return newBoardResult;
        }

        public Result SetWinner(Player winner)
        {
            if (winner == null)
                return Result.Failure("Player is Null");

            if(!this._players.Contains(winner))
                return Result.Failure("Player is not part of the game");

            if(this.State != GameState.Started)
                return Result.Failure("Game has not started yet");

            if(!winner.Boards.Any(board => board.State == BoardState.Winner))
                return Result.Failure("This Player does not have a winning Board");

            this.Winner = winner;
            this.State = GameState.Finished;

            return Result.Ok();
        }

        public Result<Ball> RadmonlyPlayBall(Random randomizer)
        {
            var randomBallResult = GetRandomBall(randomizer);
            if (randomBallResult.IsFailure)
                return randomBallResult;
            var playBallResult = PlayBall(randomBallResult.Value);
            if (playBallResult.IsFailure)
                return Result.Failure<Ball>(playBallResult.Error);
            return randomBallResult;
        }

        public Result<List<(Player potentialWinner, List<Board> winningBoards)>> GetPotentialWinners()
        {
            if (State != GameState.Started)
                return Result.Failure<List<(Player potentialWinner, List<Board> winningBoards)>>("Game is not started");

            var winners = this._players
                .Where(player => player.Boards.Any(board => board.State == BoardState.Winner))
                .Select(player => (player, player.Boards.Where(board => board.State == BoardState.Winner).ToList()))
                .ToList();

            return Result.Ok(winners);
        }

        #endregion

        #region Helpers

        private static HashSet<Ball> CreateNBallsSet(short nBalls)
        {
            var ballList = Enumerable.Range(1, nBalls)
                .Select(number =>
                {
                    var letter = GetBallLetterForMaxBallCount((short)number, nBalls);
                    var newBallResult = Ball.Create(letter, (short)number);
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

        private Result<Board> TryCreatingNewBoard(Random randomizer, short tryUpToNTimes)
        {
            var tryCount = 1;
            while(tryCount <= tryUpToNTimes)
            {
                var newBoardResult = Board.RandonmlyCreateFromBallSet(randomizer, this._ballsConfigured, this.WithNBallsMaxPerBoardBucket);
                if (newBoardResult.IsFailure)
                    return newBoardResult;

                if(!this._players.Any(otherPlayer => otherPlayer.Boards.Contains(newBoardResult.Value)))
                    return newBoardResult;

                tryCount++;
            }

            return Result.Failure<Board>("We were not able to randomly create a Board");
        }

        private Result<Ball> GetRandomBall(Random randomizer)
        {
            if (randomizer == null)
                return Result.Failure<Ball>("Randomizer cannot be null");

            var pendingBallsToBePlayed = this._ballsConfigured.Except(this._ballsPlayed).ToList();
            if(pendingBallsToBePlayed.Count == 0)
                return Result.Failure<Ball>("There are no more pending balls to be played");

            var randomIndex = randomizer.Next(0, pendingBallsToBePlayed.Count - 1);
            return Result.Ok(pendingBallsToBePlayed[randomIndex]);
        }

        #endregion
    }
}
