using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    internal class FinishedGame: Game
    {
        #region Constructors

        internal FinishedGame(string name, GameType gameType,
                HashSet<Ball> balls, short maxNBallsPerColumn,
                HashSet<Player> players, Player withWinner,
                HashSet<Ball> withBallsPlayed) :
            base(name: name, gameType: gameType, balls: balls,
                maxNBallsPerColumn: maxNBallsPerColumn,
                players: players, winner: withWinner,
                ballsPlayed: withBallsPlayed, status: GameStatus.Finished)
        {
        }

        #endregion

        #region Methods that are not allowed for this Game State

        public override Result AddPlayer(string withName) =>
            Result.Failure("Operation cannot be performed, due to the state of this game");

        public override Result UpdatePlayerInfo(Player playerToUpdate, string newName) =>
            Result.Failure("Operation cannot be performed, due to the state of this game");

        public override Result<Board> AddBoardToPlayer(Player player) =>
            Result.Failure<Board>("Operation cannot be performed, due to the state of this game");

        public override Result RemoveBoardFromPlayer(Player player, Board board) =>
            Result.Failure("Operation cannot be performed, due to the state of this game");

        public override Result RemovePlayer(Player playerToRemove) =>
            Result.Failure("Operation cannot be performed, due to the state of this game");

        public override Result<Game> Start() =>
            Result.Failure<Game>("Operation cannot be performed, due to the state of this game");

        public override Result PlayBall(Ball ballToPlay) =>
            Result.Failure("Operation cannot be performed, due to the state of this game");

        public override Result<Ball> RadmonlyPlayBall() =>
            Result.Failure<Ball>("Operation cannot be performed, due to the state of this game");

        public override Result<Game> SetWinner(Player winner) =>
            Result.Failure<Game>("Operation cannot be performed, due to the state of this game");

        #endregion
    }
}
