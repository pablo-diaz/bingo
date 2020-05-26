using System;
using System.Linq;
using System.Collections.Generic;

using Core;

using CSharpFunctionalExtensions;

namespace WebUI.Services.DTOs
{
    public enum GameStatus
    {
        DRAFT,
        ACTIVE,
        FINISHED
    }

    public class GameDTO
    {
        private DraftGame _draftGame;
        private ActiveGame _activeGame;
        private FinishedGame _finishedGame;
        
        public GameStatus GameStatus { get; private set; } = GameStatus.DRAFT;

        public string Name => this.GameStatus switch { 
            GameStatus.DRAFT => this._draftGame.Name,
            GameStatus.ACTIVE => this._activeGame.Name,
            GameStatus.FINISHED => this._finishedGame.Name,
            _ => throw new ApplicationException("Wrong game status")
        };

        public GameType GameType => this.GameStatus switch
        {
            GameStatus.DRAFT => this._draftGame.GameType,
            GameStatus.ACTIVE => this._activeGame.GameType,
            GameStatus.FINISHED => this._finishedGame.GameType,
            _ => throw new ApplicationException("Wrong game status")
        };

        public Maybe<Player> Winner => this.GameStatus switch
        {
            GameStatus.DRAFT => Maybe<Player>.None,
            GameStatus.ACTIVE => Maybe<Player>.None,
            GameStatus.FINISHED => this._finishedGame.Winner,
            _ => throw new ApplicationException("Wrong game status")
        };

        public IReadOnlyCollection<Ball> BallsConfigured => this.GameStatus switch
        {
            GameStatus.DRAFT => this._draftGame.BallsConfigured,
            GameStatus.ACTIVE => this._activeGame.BallsConfigured,
            GameStatus.FINISHED => this._finishedGame.BallsConfigured,
            _ => throw new ApplicationException("Wrong game status")
        };

        public IReadOnlyCollection<Ball> BallsPlayed => this.GameStatus switch
        {
            GameStatus.DRAFT => new List<Ball>(),
            GameStatus.ACTIVE => this._activeGame.BallsPlayed,
            GameStatus.FINISHED => this._finishedGame.BallsPlayed,
            _ => throw new ApplicationException("Wrong game status")
        };

        public IReadOnlyCollection<Player> Players => this.GameStatus switch
        {
            GameStatus.DRAFT => this._draftGame.Players,
            GameStatus.ACTIVE => this._activeGame.Players,
            GameStatus.FINISHED => this._finishedGame.Players,
            _ => throw new ApplicationException("Wrong game status")
        };

        public bool IsItPlayable => this.GameStatus == GameStatus.ACTIVE;

        private GameDTO()
        {

        }

        public static GameDTO CreateFromDraftGame(DraftGame game) =>
            new GameDTO { 
                GameStatus = GameStatus.DRAFT,
                _draftGame = game
            };

        public Result AddPlayer(Player newPlayer)
        {
            if (this.GameStatus != GameStatus.DRAFT)
                return Result.Failure("Game is not in draft state");
            return this._draftGame.AddPlayer(newPlayer);
        }

        public Player FindPlayer(Player existingPlayer) =>
            this.GameStatus switch {
                GameStatus.DRAFT => this._draftGame.Players.FirstOrDefault(player => player == existingPlayer),
                GameStatus.ACTIVE => this._activeGame.Players.FirstOrDefault(player => player == existingPlayer),
                GameStatus.FINISHED => this._finishedGame.Players.FirstOrDefault(player => player == existingPlayer),
                _ => throw new ApplicationException("Wrong game status")
            };

        public Player FindPlayer(string playerName) =>
            this.GameStatus switch
            {
                GameStatus.DRAFT => this._draftGame.Players.FirstOrDefault(player => player.Name == playerName),
                GameStatus.ACTIVE => this._activeGame.Players.FirstOrDefault(player => player.Name == playerName),
                GameStatus.FINISHED => this._finishedGame.Players.FirstOrDefault(player => player.Name == playerName),
                _ => throw new ApplicationException("Wrong game status")
            };

        public Player FindPlayer(string playerLogin, string playerPasswd) =>
            this.GameStatus switch
            {
                GameStatus.DRAFT => this._draftGame.Players
                    .FirstOrDefault(player => player.Security.Login == playerLogin && player.Security.Password == playerPasswd),
                GameStatus.ACTIVE => this._activeGame.Players
                    .FirstOrDefault(player => player.Security.Login == playerLogin && player.Security.Password == playerPasswd),
                GameStatus.FINISHED => this._finishedGame.Players
                    .FirstOrDefault(player => player.Security.Login == playerLogin && player.Security.Password == playerPasswd),
                _ => throw new ApplicationException("Wrong game status")
            };

        public Result UpdatePlayer(Player playerToUpdate, Player newPlayerInfo)
        {
            if (this.GameStatus != GameStatus.DRAFT)
                return Result.Failure("Game is not in draft state");
            return this._draftGame.UpdatePlayer(playerToUpdate, newPlayerInfo);
        }

        public Result AddBoardToPlayer(Random randomizer, Player toPlayer)
        {
            if (this.GameStatus != GameStatus.DRAFT)
                return Result.Failure("Game is not in draft state");
            return this._draftGame.AddBoardToPlayer(randomizer, toPlayer);
        }

        public Result RemoveBoardFromPlayer(Player fromPlayer, Board boardRoRemove)
        {
            if (this.GameStatus != GameStatus.DRAFT)
                return Result.Failure("Game is not in draft state");
            return this._draftGame.RemoveBoardFromPlayer(fromPlayer, boardRoRemove);
        }

        public Result RemovePlayer(Player player)
        {
            if (this.GameStatus != GameStatus.DRAFT)
                return Result.Failure("Game is not in draft state");
            return this._draftGame.RemovePlayer(player);
        }

        public Result Start()
        {
            if (this.GameStatus != GameStatus.DRAFT)
                return Result.Failure("Game is not in draft state");
            
            var activeGameResult = this._draftGame.Start();
            if (activeGameResult.IsFailure)
                return activeGameResult;

            this._draftGame = null;
            this._activeGame = activeGameResult.Value;
            this.GameStatus = GameStatus.ACTIVE;

            return Result.Ok();
        }

        public Ball FindBallConfigured(string ballName) =>
            this.GameStatus switch
            {
                GameStatus.DRAFT => this._draftGame.BallsConfigured.FirstOrDefault(ball => ball.Name == ballName),
                GameStatus.ACTIVE => this._activeGame.BallsConfigured.FirstOrDefault(ball => ball.Name == ballName),
                GameStatus.FINISHED => this._finishedGame.BallsConfigured.FirstOrDefault(ball => ball.Name == ballName),
                _ => throw new ApplicationException("Wrong game status")
            };

        public Result PlayBall(Ball ball)
        {
            if (this.GameStatus != GameStatus.ACTIVE)
                return Result.Failure("Game is not Active");
            return this._activeGame.PlayBall(ball);
        }

        public Result<Ball> RadmonlyPlayBall(Random randomizer)
        {
            if (this.GameStatus != GameStatus.ACTIVE)
                return Result.Failure<Ball>("Game is not Active");
            return this._activeGame.RadmonlyPlayBall(randomizer);
        }

        public Result SetWinner(Player winner)
        {
            if (this.GameStatus != GameStatus.ACTIVE)
                return Result.Failure("Game is not Active");

            var finishedGameResult = this._activeGame.SetWinner(winner);
            if (finishedGameResult.IsFailure)
                return finishedGameResult;

            this._activeGame = null;
            this._finishedGame = finishedGameResult.Value;
            this.GameStatus = GameStatus.FINISHED;

            return Result.Ok();
        }
    }
}
