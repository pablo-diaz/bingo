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
            name = name.Trim();

            var maybeGame = this._games.FirstOrDefault(game => game.Name == name);
            if (maybeGame != null)
                return Result.Failure("There is already a game with the same name. Please try with a different one");

            var draftGameResult = GameServices.CreateGame(
                withName: name, gameType: gameType,
                withNBallsTotal: STANDARD_BALLS_VERSION_TOTAL,
                withNBallsMaxPerColumn: STANDARD_BALLS_VERSION_PER_COLUMN_COUNT);
            if (draftGameResult.IsFailure)
                return draftGameResult;

            this._games.Add(GameState.CreateFromGame(draftGameResult.Value));

            return Result.Success();
        }

        public Result CopyPlayersFromGame(GameState fromGame, GameState toGame)
        {
            foreach (var sourcePlayer in fromGame.Players.Except(toGame.Players))
            {
                var addPlayerResult = toGame.AddPlayer(sourcePlayer.Name);
                if (addPlayerResult.IsFailure)
                    return Result.Failure<GameState>(addPlayerResult.Error);
            }

            return Result.Success();
        }

        public Result AddNewPlayerToGame(GameState game, string playerName) =>
            game.AddPlayer(playerName.Trim());

        public Result UpdatePlayerInfoInGame(GameState game,
                Player existingPlayer, string newPlayerName) =>
            game.UpdatePlayer(existingPlayer, newPlayerName.Trim());

        public Result AddBoardToPlayer(GameState inGame, Player toPlayer) =>
            inGame.AddBoardToPlayer(toPlayer);

        public Result RemoveBoardFromPlayer(GameState inGame, Player fromPlayer) =>
            inGame.RemoveBoardFromPlayer(fromPlayer, fromPlayer.Boards.Last());

        public Result RemovePlayer(GameState inGame, Player playerToRemove) =>
            inGame.RemovePlayer(playerToRemove);

        public Result StartGame(GameState game) =>
            game.Start();

        public async Task<Result> PlayBall(GameState inGame, string ballName)
        {
            var maybeBall = inGame.FindBallConfigured(ballName);
            if(maybeBall.HasNoValue)
                return Result.Failure("Ball has not been found by its name");

            var ball = maybeBall.GetValueOrThrow();

            var playBallResult = inGame.PlayBall(ball);
            if (playBallResult.IsFailure)
                return Result.Failure(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGame.Name,
                new Infrastructure.DTOs.BallDTO { Name = ball.Name });

            return Result.Success();
        }

        public async Task<Result> RandomlyPlayBall(GameState inGame)
        {
            var playBallResult = inGame.RadmonlyPlayBall();
            if (playBallResult.IsFailure)
                return Result.Failure(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGame.Name,
                new Infrastructure.DTOs.BallDTO { Name = playBallResult.Value.Name });

            return Result.Success();
        }

        public async Task<Result> SetWinner(GameState inGame, Player winner)
        {
            var settingWinnerResult = inGame.SetWinner(winner);
            if (settingWinnerResult.IsFailure)
                return Result.Failure(settingWinnerResult.Error);

            await this._bingoHub.SendWinnerMessage(inGame.Name, winner.Name);

            return Result.Success();
        }

        public Result<LoginResultDTO> PerformLogIn(GameState inGame, Player forPlayer)
        {
            var jwtPlayerToken = this._bingoSecurity.CreateJWTTokenForPlayer(inGame.Name);
            var loginResult = new LoginResultDTO(forPlayer, inGame.BallsPlayed, jwtPlayerToken);
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

        public Result DeleteGame(GameState game)
        {
            var gameIndexFound = this._games.FindIndex(g => g == game);
            if (gameIndexFound <= -1)
                return Result.Failure<GameState>("Game has not been found by its name");

            this._games.RemoveAt(gameIndexFound);

            return Result.Success();
        }
    }
}
