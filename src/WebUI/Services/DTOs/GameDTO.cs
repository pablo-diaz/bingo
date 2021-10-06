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
        private Game _game;
        
        public GameStatus GameStatus { get; private set; } = GameStatus.DRAFT;

        public string Name => this._game.Name;

        public GameType GameType => this._game.GameType;

        public Maybe<Player> Winner => this._game.Winner;

        public IReadOnlyCollection<Ball> BallsConfigured => this._game.BallsConfigured;

        public IReadOnlyCollection<Ball> BallsPlayed => this._game.BallsPlayed;

        public IReadOnlyCollection<Player> Players => this._game.Players;

        public bool IsItPlayable => this.GameStatus == GameStatus.ACTIVE;

        private GameDTO(Game game)
        {
            this.GameStatus = GameStatus.DRAFT;
            this._game = game;
        }

        internal static GameDTO CreateFromGame(Game game) => new GameDTO(game);

        internal Result AddPlayer(string withName) => this._game.AddPlayer(withName);

        internal Result UpdatePlayer(Player playerToUpdate, string withNewPlayerName) =>
            this._game.UpdatePlayerInfo(playerToUpdate, newName: withNewPlayerName);

        internal Result AddBoardToPlayer(Player toPlayer) => this._game.AddBoardToPlayer(toPlayer);

        internal Result RemoveBoardFromPlayer(Player fromPlayer, Board boardRoRemove) =>
            this._game.RemoveBoardFromPlayer(fromPlayer, boardRoRemove);

        internal Result RemovePlayer(Player player) => this._game.RemovePlayer(player);

        internal Result Start()
        {
            var startResult = this._game.Start();
            if (startResult.IsFailure)
                return startResult;

            this._game = startResult.Value;
            this.GameStatus = GameStatus.ACTIVE;

            return Result.Success();
        }

        internal Maybe<Ball> FindBallConfigured(string ballName) =>
            this._game.BallsConfigured.FirstOrDefault(ball => ball.Name == ballName)
            ?? Maybe<Ball>.None;

        internal Result PlayBall(Ball ball) => this._game.PlayBall(ball);

        internal Result<Ball> RadmonlyPlayBall() => this._game.RadmonlyPlayBall();

        internal Result SetWinner(Player winner)
        {
            var setWinnerResult = this._game.SetWinner(winner);
            if (setWinnerResult.IsFailure)
                return setWinnerResult;

            this._game = setWinnerResult.Value;
            this.GameStatus = GameStatus.FINISHED;

            return Result.Success();
        }
    }
}
