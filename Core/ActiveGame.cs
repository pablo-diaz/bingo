using System;
using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public class ActiveGame
    {
        #region Properties

        public string Name { get; }
        public GameType GameType { get; }

        public IReadOnlyCollection<Ball> BallsConfigured { get ; }

        private HashSet<Ball> _ballsPlayed;
        public IReadOnlyCollection<Ball> BallsPlayed { get => this._ballsPlayed.ToList(); }

        public IReadOnlyCollection<Player> Players { get; }

        #endregion

        #region Constructors

        internal ActiveGame(string name, GameType gameType, IReadOnlyCollection<Player> withPlayers, 
            IReadOnlyCollection<Ball> withBallsConfigured)
        {
            this.Name = name;
            this.GameType = gameType;
            this.Players = withPlayers;
            this.BallsConfigured = withBallsConfigured;
            this._ballsPlayed = new HashSet<Ball>();
        }

        #endregion

        #region Public Methods
        
        public Result PlayBall(Ball ballToPlay)
        {
            if (!this.BallsConfigured.Contains(ballToPlay))
                return Result.Failure("Ball is not in the possible set");

            if(this.BallsPlayed.Contains(ballToPlay))
                return Result.Failure("Ball has already been played");

            foreach(var player in this.Players)
                player.PlayBall(ballToPlay);

            this._ballsPlayed.Add(ballToPlay);

            return Result.Ok();
        }

        public Result<FinishedGame> SetWinner(Player winner)
        {
            if (winner == null)
                return Result.Failure<FinishedGame>("Player is Null");

            if(!this.Players.Any(player => player.Name == winner.Name))
                return Result.Failure<FinishedGame>("Player is not part of the game");

            if(!winner.Boards.Any(board => board.State == BoardState.Winner))
                return Result.Failure<FinishedGame>("This Player does not have a winning Board");

            var newFinishedGame = new FinishedGame(this.Name, this.GameType, this.BallsConfigured, this.Players, this.BallsPlayed, winner);
            return Result.Ok(newFinishedGame);
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
            var winners = this.Players
                .Where(player => player.Boards.Any(board => board.State == BoardState.Winner))
                .Select(player => (player, player.Boards.Where(board => board.State == BoardState.Winner).ToList()))
                .ToList();

            return Result.Ok(winners);
        }

        #endregion

        #region Helpers

        private Result<Ball> GetRandomBall(Random randomizer)
        {
            if (randomizer == null)
                return Result.Failure<Ball>("Randomizer cannot be null");

            var pendingBallsToBePlayed = this.BallsConfigured.Except(this._ballsPlayed).ToList();
            if(pendingBallsToBePlayed.Count == 0)
                return Result.Failure<Ball>("There are no more pending balls to be played");

            var randomIndex = randomizer.Next(0, pendingBallsToBePlayed.Count - 1);
            return Result.Ok(pendingBallsToBePlayed[randomIndex]);
        }

        #endregion
    }
}
