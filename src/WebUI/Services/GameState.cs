using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebUI.Services.DTOs;
using WebUI.Infrastructure;

using Core;

using CSharpFunctionalExtensions;

namespace WebUI.Services
{
    public class GameState
    {
        private const short STANDARD_BALLS_VERSION_TOTAL = 75;
        private const short STANDARD_BALLS_VERSION_PER_COLUMN_COUNT = 5;

        private readonly List<GameDTO> _games;
        private readonly BingoHub _bingoHub;
        private readonly BingoSecurity _bingoSecurity;

        public GameState(BingoHub bingoHub, BingoSecurity bingoSecurity)
        {
            this._games = new List<GameDTO>();
            this._bingoHub = bingoHub;
            this._bingoSecurity = bingoSecurity;
        }

        private (Maybe<GameDTO> maybeGame, GameDTO game) FindGame(string withName)
        {
            var maybeGame = this._games.FirstOrDefault(game => game.Name == withName.Trim());
            return maybeGame != null
                ? (Maybe<GameDTO>.From(maybeGame), maybeGame)
                : (Maybe<GameDTO>.None, null);
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

            this._games.Add(GameDTO.CreateFromGame(draftGameResult.Value));

            return Result.Success();
        }

        public Result<GameDTO> CopyPlayersFromGame(string fromGameName, string toGameName)
        {
            (var maybeSourceGame, var sourceGame) = this.FindGame(fromGameName);
            if (maybeSourceGame.HasNoValue)
                return Result.Failure<GameDTO>("Source Game has not been found by its name");

            (var maybeTargetGame, var targetGame) = this.FindGame(toGameName);
            if (maybeTargetGame.HasNoValue)
                return Result.Failure<GameDTO>("Target Game has not been found by its name");

            foreach (var sourcePlayer in sourceGame.Players.Except(targetGame.Players))
            {
                var addPlayerResult = targetGame.AddPlayer(sourcePlayer.Name);
                if (addPlayerResult.IsFailure)
                    return Result.Failure<GameDTO>(addPlayerResult.Error);
            }

            return Result.Success(targetGame);
        }

        public Result<GameDTO> AddNewPlayerToGame(string gameName, string playerName)
        {
            (var maybeGame, var game) = this.FindGame(gameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var addPlayerResult = game.AddPlayer(playerName.Trim());
            if (addPlayerResult.IsFailure)
                return Result.Failure<GameDTO>(addPlayerResult.Error);

            return Result.Success(game);
        }

        public Result<GameDTO> UpdatePlayerInfoInGame(string gameName,
            Player existingPlayer, string newPlayerName)
        {
            (var maybeGame, var game) = this.FindGame(gameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var updatePlayerResult = game.UpdatePlayer(existingPlayer, newPlayerName.Trim());
            if (updatePlayerResult.IsFailure)
                return Result.Failure<GameDTO>(updatePlayerResult.Error);

            return Result.Success(game);
        }

        public Result<GameDTO> AddBoardToPlayer(string inGameName, Player toPlayer)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var addBoardResult = game.AddBoardToPlayer(toPlayer);
            if (addBoardResult.IsFailure)
                return Result.Failure<GameDTO>(addBoardResult.Error);

            return Result.Success(game);
        }

        public Result<GameDTO> RemoveBoardFromPlayer(string inGameName, Player fromPlayer)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var lastBoardFound = fromPlayer.Boards.LastOrDefault();
            if(lastBoardFound == null)
                return Result.Failure<GameDTO>("Player does not have any boards left");

            var removeBoardResult = game.RemoveBoardFromPlayer(fromPlayer, lastBoardFound);
            if (removeBoardResult.IsFailure)
                return Result.Failure<GameDTO>(removeBoardResult.Error);

            return Result.Success(game);
        }

        public Result<GameDTO> RemovePlayer(string inGameName, Player playerToRemove)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var removePlayerResult = game.RemovePlayer(playerToRemove);
            if (removePlayerResult.IsFailure)
                return Result.Failure<GameDTO>(removePlayerResult.Error);

            return Result.Success(game);
        }

        public Result<GameDTO> StartGame(string gameName)
        {
            (var maybeGame, var game) = this.FindGame(gameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var gameStartResult = game.Start();
            if(gameStartResult.IsFailure)
                return Result.Failure<GameDTO>(gameStartResult.Error);

            return Result.Success(game);
        }

        public async Task<Result<GameDTO>> PlayBall(string inGameName, string ballName)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var maybeBall = game.FindBallConfigured(ballName);
            if(maybeBall.HasNoValue)
                return Result.Failure<GameDTO>("Ball has not been found by its name");

            var ball = maybeBall.GetValueOrThrow();

            var playBallResult = game.PlayBall(ball);
            if (playBallResult.IsFailure)
                return Result.Failure<GameDTO>(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGameName,
                new Infrastructure.DTOs.BallDTO { Name = ball.Name });

            return Result.Success(game);
        }

        public async Task<Result<GameDTO>> RandomlyPlayBall(string inGameName)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var playBallResult = game.RadmonlyPlayBall();
            if (playBallResult.IsFailure)
                return Result.Failure<GameDTO>(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGameName,
                new Infrastructure.DTOs.BallDTO { Name = playBallResult.Value.Name });

            return Result.Success(game);
        }

        public async Task<Result<GameDTO>> SetWinner(string inGameName, Player winner)
        {
            (var maybeGame, var game) = this.FindGame(inGameName);
            if (maybeGame.HasNoValue)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var settingWinnerResult = game.SetWinner(winner);
            if (settingWinnerResult.IsFailure)
                return Result.Failure<GameDTO>(settingWinnerResult.Error);

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

        public IReadOnlyCollection<GameDTO> GetAllGames() =>
            this._games
                .OrderBy(game => game.Name)
                .ToList()
                .AsReadOnly();

        public IReadOnlyCollection<GameDTO> GetPlayableGames() =>
            this._games
                .Where(game => game.IsItPlayable)
                .OrderBy(game => game.Name)
                .ToList()
                .AsReadOnly();

        public Result DeleteGame(string gameName)
        {
            var gameIndexFound = this._games.FindIndex(g => g.Name == gameName.Trim());
            if (gameIndexFound <= -1)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            this._games.RemoveAt(gameIndexFound);

            return Result.Success();
        }
    }
}
