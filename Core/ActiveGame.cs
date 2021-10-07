using System;
using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    internal class ActiveGame: Game
    {
        #region Constructors

        internal ActiveGame(string name, GameType gameType,
                HashSet<Ball> withBalls, short withMaxNBallsPerColumn,
                HashSet<Player> players) :
            base(name: name, gameType: gameType, balls: withBalls,
                maxNBallsPerColumn: withMaxNBallsPerColumn, players: players,
                status: GameStatus.Playing)
        {
        }

        #endregion

        #region Public Methods
        
        public override Result PlayBall(Ball ballToPlay)
        {
            if (!this.BallsConfigured.Contains(ballToPlay))
                return Result.Failure("Ball is not in the possible set");

            if(this.BallsPlayed.Contains(ballToPlay))
                return Result.Failure("Ball has already been played");

            foreach(var player in this.Players)
                player.PlayBall(ballToPlay);

            this._ballsPlayed.Add(ballToPlay);

            return Result.Success();
        }

        public override Result<Ball> RadmonlyPlayBall()
        {
            var randomBallResult = GetRandomBallPendingToBePlayed();
            if (randomBallResult.IsFailure)
                return randomBallResult;

            var playBallResult = PlayBall(randomBallResult.Value);
            if (playBallResult.IsFailure)
                return Result.Failure<Ball>(playBallResult.Error);

            return randomBallResult;
        }

        public override Result<Game> SetWinner(Player winner)
        {
            if (winner == null)
                return Result.Failure<Game>("Player is Null");

            if(!this.Players.Any(player => player == winner))
                return Result.Failure<Game>("Player is not part of the game");

            if(!winner.Boards.Any(board => board.State == BoardState.Winner))
                return Result.Failure<Game>("This Player does not have a winning Board");

            return new FinishedGame(this.Name, this.GameType, this._ballsConfigured,
                this.MaxNBallsPerColumn, this._players, winner, this._ballsPlayed);
        }

        #endregion

        #region Helpers

        private Result<Ball> GetRandomBallPendingToBePlayed()
        {
            var pendingBallsToBePlayed = this.BallsConfigured.Except(this._ballsPlayed).ToList();
            if(pendingBallsToBePlayed.Count == 0)
                return Result.Failure<Ball>("There are no more pending balls to be played");

            var randomIndex = RandomizingUtilities.GetRandomValue(pendingBallsToBePlayed.Count);
            return pendingBallsToBePlayed[randomIndex];
        }

        #endregion

        #region Methods that are not allowed for this Game State

        public override Result AddPlayer(string withName) =>
            Result.Failure("An Active Game cannot Add Players");

        public override Result<Board> AddBoardToPlayer(Player player) =>
            Result.Failure<Board>("An Active Game cannot Add more Boards to Players");

        public override Result RemoveBoardFromPlayer(Player player, Board board) =>
            Result.Failure("An Active Game cannot Remove any Board from Players");

        public override Result UpdatePlayerInfo(Player playerToUpdate, string newName) =>
            Result.Failure("An Active Game cannot Update Players");

        public override Result RemovePlayer(Player playerToRemove) =>
            Result.Failure("An Active Game cannot Remove Players");

        public override Result<Game> Start() =>
            Result.Failure<Game>("An Active Game cannot Start the Game once again");

        #endregion
    }
}
