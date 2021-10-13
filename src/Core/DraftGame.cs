using System;
using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    internal class DraftGame: Game
    {
        #region Constructors

        internal DraftGame(string name, GameType gameType,
                HashSet<Ball> withBalls, short withMaxNBallsPerColumn):
            base(name: name, gameType: gameType, balls: withBalls,
                maxNBallsPerColumn: withMaxNBallsPerColumn)
        {
        }

        #endregion

        #region Public Methods

        public override Result AddPlayer(string withName)
        {
            if (string.IsNullOrEmpty(withName))
                return Result.Failure("New player name is null");

            if(this._players.Any(p => p.Name == withName))
                return Result.Failure("There is already another Player with the same name");

            var newPlayerResult = Player.Create(name: withName, withBallSet: this._ballsConfigured,
                withNBallsPerColumn: this.MaxNBallsPerColumn, gameType: this.GameType);
            if (newPlayerResult.IsFailure)
                return Result.Failure(newPlayerResult.Error);

            this._players.Add(newPlayerResult.Value);

            return Result.Success();
        }

        public override Result UpdatePlayerInfo(Player playerToUpdate, string withName)
        {
            if (!this._players.Any(player => player == playerToUpdate))
                return Result.Failure("Game does not contain player that is to be updated");

            if (IsThereAnyOtherPlayerWithSameName(playerToUpdate, withName))
                return Result.Failure("Game already contains another player with the same new info");

            var result = playerToUpdate.SetInfo(withName);
            if (result.IsFailure)
                return result;

            return Result.Success();
        }

        public override Result RemovePlayer(Player playerToRemove)
        {
            if (playerToRemove == null)
                return Result.Failure("Player is null");

            if (!this._players.Any(player => player == playerToRemove))
                return Result.Failure("Player does not exist in Game");

            if(this.Players.Count == 2)
                return Result.Failure("There sould exist at least 2 players in this game");

            this._players.Remove(playerToRemove);

            return Result.Success();
        }

        public override Result<Game> Start()
        {
            if (this._players.Count() < 2)
                return Result.Failure<Game>("There should be at least 2 Players to start Game");

            return new ActiveGame(this.Name, this.GameType, this._ballsConfigured,
                this.MaxNBallsPerColumn, this._players);
        }

        public override Result<Board> AddBoardToPlayer(Player player)
        {
            if(!this._players.Any(p => p == player))
                return Result.Failure<Board>("Player is not part of Game");

            var newBoardResult = TryCreatingNewBoard(tryUpToNTimes: 10, this.GameType);
            if (newBoardResult.IsFailure)
                return newBoardResult;

            player.AddBoard(newBoardResult.Value);

            return newBoardResult;
        }

        public override Result RemoveBoardFromPlayer(Player player, Board board)
        {
            if (!this._players.Any(p => p == player))
                return Result.Failure<Board>("Player is not part of Game");

            return player.RemoveBoard(board);
        }

        #endregion

        #region Helpers

        private bool IsThereAnyOtherPlayerWithSameName(Player playerToCheck, string nameToCheckInOthers) =>
            this._players.Except(new Player[] { playerToCheck })
                .Any(player => player.Name == nameToCheckInOthers);

        private Result<Board> TryCreatingNewBoard(short tryUpToNTimes, GameType gameType)
        {
            var tryCount = 1;
            while(tryCount <= tryUpToNTimes)
            {
                var newBoardResult = Board.Create(this._ballsConfigured, 
                    this.MaxNBallsPerColumn, gameType);
                if (newBoardResult.IsFailure)
                    return newBoardResult;

                if(!this._players.Any(otherPlayer => otherPlayer.Boards.Contains(newBoardResult.Value)))
                    return newBoardResult;

                tryCount++;
            }

            return Result.Failure<Board>("We were not able to randomly create a Board");
        }

        #endregion

        #region Methods that are not allowed for this Game State

        public override Result PlayBall(Ball ballToPlay) =>
            Result.Failure("A Draft Game cannot Play any Ball, because it has not been Started yet");

        public override Result<Ball> RadmonlyPlayBall() =>
            Result.Failure<Ball>("A Draft Game cannot Play any Ball, because it has not been Started yet");

        public override Result<Game> SetWinner(Player winner) =>
            Result.Failure<Game>("A Draft Game cannot set a Winner");

        #endregion
    }
}
