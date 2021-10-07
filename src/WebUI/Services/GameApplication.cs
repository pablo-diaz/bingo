using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebUI.Services.DTOs;
using WebUI.Infrastructure;

using Core;

using CSharpFunctionalExtensions;

namespace WebUI.Services
{
    public class GameApplication
    {
        private const short STANDARD_BALLS_VERSION_TOTAL = 75;
        private const short STANDARD_BALLS_VERSION_PER_COLUMN_COUNT = 5;

        private readonly List<GameState> _games;
        private readonly BingoHub _bingoHub;
        private readonly BingoSecurity _bingoSecurity;

        public GameApplication(BingoHub bingoHub, BingoSecurity bingoSecurity)
        {
            this._games = new List<GameState>();
            this._bingoHub = bingoHub;
            this._bingoSecurity = bingoSecurity;
        }

        public Result AddGame(string name, GameType gameType)
        {
            (var maybeGameFound, var _) = this.FindGame(name);
            if (maybeGameFound.HasValue)
                return Result.Failure("There is already a game with the same name. Please try with a different one");

            var draftGameResult = GameServices.CreateGame(
                withName: name.Trim(), gameType: gameType,
                withNBallsTotal: STANDARD_BALLS_VERSION_TOTAL,
                withNBallsMaxPerColumn: STANDARD_BALLS_VERSION_PER_COLUMN_COUNT);
            if (draftGameResult.IsFailure)
                return draftGameResult;

            this._games.Add(GameState.CreateFromGame(draftGameResult.Value));

            return Result.Success();
        }

        public Result<GameState> CopyPlayersFromGame(string fromGameName, string toGameName)
        {
            (var maybeSourceGame, var sourceGame) = this.FindGame(fromGameName);
            if (maybeSourceGame.HasNoValue)
                return Result.Failure<GameState>("Source Game has not been found by its name");

            (var maybeTargetGame, var targetGame) = this.FindGame(toGameName);
            if (maybeTargetGame.HasNoValue)
                return Result.Failure<GameState>("Target Game has not been found by its name");

            foreach (var sourcePlayer in sourceGame.Players.Except(targetGame.Players))
            {
                var addPlayerResult = targetGame.AddPlayer(sourcePlayer.Name);
                if (addPlayerResult.IsFailure)
                    return Result.Failure<GameState>(addPlayerResult.Error);
            }

            return Result.Success(targetGame);
        }

        public Result<GameState> AddNewPlayerToGame(string gameName, string playerName)
        {
            (var maybeGame, var game) = this.FindGame(gameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var addPlayerResult = game.AddPlayer(playerName.Trim());
            if (addPlayerResult.IsFailure)
                return Result.Failure<GameState>(addPlayerResult.Error);

            return Result.Success(game);
        }

        public Result<GameState> UpdatePlayerInfoInGame(string gameName,
            Player existingPlayer, string newPlayerName)
        {
            (var maybeGame, var game) = this.FindGame(gameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var updatePlayerResult = game.UpdatePlayer(existingPlayer, newPlayerName.Trim());
            if (updatePlayerResult.IsFailure)
                return Result.Failure<GameState>(updatePlayerResult.Error);

            return Result.Success(game);
        }

        public Result<GameState> AddBoardToPlayer(string inGameName, Player toPlayer)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var addBoardResult = game.AddBoardToPlayer(toPlayer);
            if (addBoardResult.IsFailure)
                return Result.Failure<GameState>(addBoardResult.Error);

            return Result.Success(game);
        }

        public Result<GameState> RemoveBoardFromPlayer(string inGameName, Player fromPlayer)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var lastBoardFound = fromPlayer.Boards.LastOrDefault();
            if(lastBoardFound == null)
                return Result.Failure<GameState>("Player does not have any boards left");

            var removeBoardResult = game.RemoveBoardFromPlayer(fromPlayer, lastBoardFound);
            if (removeBoardResult.IsFailure)
                return Result.Failure<GameState>(removeBoardResult.Error);

            return Result.Success(game);
        }

        public Result<GameState> RemovePlayer(string inGameName, Player playerToRemove)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var removePlayerResult = game.RemovePlayer(playerToRemove);
            if (removePlayerResult.IsFailure)
                return Result.Failure<GameState>(removePlayerResult.Error);

            return Result.Success(game);
        }

        public Result<GameState> StartGame(string gameName)
        {
            (var maybeGame, var game) = this.FindGame(gameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var gameStartResult = game.Start();
            if(gameStartResult.IsFailure)
                return Result.Failure<GameState>(gameStartResult.Error);

            return Result.Success(game);
        }

        public async Task<Result<GameState>> PlayBall(string inGameName, string ballName)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var maybeBall = game.FindBallConfigured(ballName);
            if(maybeBall.HasNoValue)
                return Result.Failure<GameState>("Ball has not been found by its name");

            var ball = maybeBall.GetValueOrThrow();

            var playBallResult = game.PlayBall(ball);
            if (playBallResult.IsFailure)
                return Result.Failure<GameState>(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGameName,
                new Infrastructure.DTOs.BallDTO { Name = ball.Name });

            return Result.Success(game);
        }

        public async Task<Result<GameState>> RandomlyPlayBall(string inGameName)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var playBallResult = game.RadmonlyPlayBall();
            if (playBallResult.IsFailure)
                return Result.Failure<GameState>(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGameName,
                new Infrastructure.DTOs.BallDTO { Name = playBallResult.Value.Name });

            return Result.Success(game);
        }

        public async Task<Result<GameState>> SetWinner(string inGameName, Player winner)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameState>("Game has not been found by its name");

            var settingWinnerResult = game.SetWinner(winner);
            if (settingWinnerResult.IsFailure)
                return Result.Failure<GameState>(settingWinnerResult.Error);

            await this._bingoHub.SendWinnerMessage(inGameName, winner.Name);

            return Result.Success(game);
        }

        public Result<LoginResultDTO> PerformLogIn(string inGameName, Player forPlayer)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<LoginResultDTO>("Game has not been found by its name");

            var jwtPlayerToken = this._bingoSecurity.CreateJWTTokenForPlayer(inGameName);
            var loginResult = new LoginResultDTO(forPlayer, game.BallsPlayed, jwtPlayerToken);
            return Result.Success(loginResult);
        }

        public IReadOnlyCollection<GameState> GetAllGames() =>
            this._games
                .OrderBy(game => game.Name)
                .ToList()
                .AsReadOnly();

        public IReadOnlyCollection<GameState> GetPlayableGames() =>
            this._games
                .Where(game => game.IsItPlayable)
                .OrderBy(game => game.Name)
                .ToList()
                .AsReadOnly();

        public Result DeleteGame(string gameName)
        {
            var gameIndexFound = this._games.FindIndex(g => g.Name == gameName.Trim());
            if (gameIndexFound <= -1)
                return Result.Failure<GameState>("Game has not been found by its name");

            this._games.RemoveAt(gameIndexFound);

            return Result.Success();
        }

        private (Maybe<GameState> maybeGame, GameState game) FindGame(string withName)
        {
            var maybeGame = this._games.FirstOrDefault(game => game.Name == withName.Trim());
            return maybeGame != null
                ? (Maybe<GameState>.From(maybeGame), maybeGame)
                : (Maybe<GameState>.None, null);
        }
    }
}
