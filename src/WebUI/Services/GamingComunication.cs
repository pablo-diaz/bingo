using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebUI.Services.DTOs;
using WebUI.Infrastructure;

using Core;

using CSharpFunctionalExtensions;

namespace WebUI.Services
{
    public class GamingComunication
    {
        private const short STANDARD_BALLS_VERSION_TOTAL = 75;
        private const short STANDARD_BALLS_VERSION_PER_BUCKET_COUNT = 5;

        private readonly List<GameDTO> _games;
        private readonly Random _randomizer;
        private readonly BingoHub _bingoHub;
        private readonly BingoSecurity _bingoSecurity;

        public GamingComunication(BingoHub bingoHub, BingoSecurity bingoSecurity)
        {
            this._games = new List<GameDTO>();
            this._randomizer = new Random();
            this._bingoHub = bingoHub;
            this._bingoSecurity = bingoSecurity;
        }

        public Result AddStandardGame(string name, GameType gameType)
        {
            name = name.Trim();
            var existingGameFound = this._games.FirstOrDefault(game => game.Name == name);
            if (existingGameFound != null)
                return Result.Failure("There is already a game with the same name. Please try with a different one");

            var draftGameResult = DraftGame.Create(name, gameType, STANDARD_BALLS_VERSION_TOTAL, STANDARD_BALLS_VERSION_PER_BUCKET_COUNT);
            if (draftGameResult.IsFailure)
                return draftGameResult;

            this._games.Add(GameDTO.CreateFromDraftGame(draftGameResult.Value));

            return Result.Ok();
        }

        public Result<GameDTO> AddNewPlayerToGame(string gameName, string playerName)
        {
            gameName = gameName.Trim();
            playerName = playerName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == gameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var newPlayerResult = Player.Create(playerName);
            if (newPlayerResult.IsFailure)
                return Result.Failure<GameDTO>(newPlayerResult.Error);
            
            var addPlayerResult = gameFound.AddPlayer(newPlayerResult.Value);
            if (addPlayerResult.IsFailure)
                return Result.Failure<GameDTO>(addPlayerResult.Error);

            return Result.Ok(gameFound);
        }

        public Result<GameDTO> UpdatePlayerInfoInGame(string gameName, Player existingPlayer, 
            string newPlayerName)
        {
            gameName = gameName.Trim();
            newPlayerName = newPlayerName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == gameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var existingPlayerFound = gameFound.FindPlayer(existingPlayer);
            if (existingPlayerFound == null)
                return Result.Failure<GameDTO>("Existing player was not found in game");

            var newPlayerInfoResult = Player.Create(newPlayerName);
            if (newPlayerInfoResult.IsFailure)
                return Result.Failure<GameDTO>(newPlayerInfoResult.Error);

            var updatePlayerResult = gameFound.UpdatePlayer(existingPlayerFound, newPlayerInfoResult.Value);
            if (updatePlayerResult.IsFailure)
                return Result.Failure<GameDTO>(updatePlayerResult.Error);

            return Result.Ok(gameFound);
        }

        public Result<GameDTO> AddBoardToPlayer(string inGameName, string toPlayerName)
        {
            inGameName = inGameName.Trim();
            toPlayerName = toPlayerName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == inGameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var playerFound = gameFound.FindPlayer(toPlayerName);
            if (playerFound == null)
                return Result.Failure<GameDTO>("Player has not been found by its name");

            var addBoardResult = gameFound.AddBoardToPlayer(this._randomizer, playerFound);
            if (addBoardResult.IsFailure)
                return Result.Failure<GameDTO>(addBoardResult.Error);

            return Result.Ok(gameFound);
        }

        public Result<GameDTO> RemoveBoardFromPlayer(string inGameName, string fromPlayerName)
        {
            inGameName = inGameName.Trim();
            fromPlayerName = fromPlayerName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == inGameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var playerFound = gameFound.FindPlayer(fromPlayerName);
            if (playerFound == null)
                return Result.Failure<GameDTO>("Player has not been found by its name");

            var lastBoardFound = playerFound.Boards.LastOrDefault();
            if(lastBoardFound == null)
                return Result.Failure<GameDTO>("Player does not have any boards left");

            var removeBoardResult = gameFound.RemoveBoardFromPlayer(playerFound, lastBoardFound);
            if (removeBoardResult.IsFailure)
                return Result.Failure<GameDTO>(removeBoardResult.Error);

            return Result.Ok(gameFound);
        }

        public Result<GameDTO> RemovePlayer(string inGameName, string playerNameToRemove)
        {
            inGameName = inGameName.Trim();
            playerNameToRemove = playerNameToRemove.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == inGameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var playerFound = gameFound.FindPlayer(playerNameToRemove);
            if (playerFound == null)
                return Result.Failure<GameDTO>("Player has not been found by its name");

            var removePlayerResult = gameFound.RemovePlayer(playerFound);
            if (removePlayerResult.IsFailure)
                return Result.Failure<GameDTO>(removePlayerResult.Error);

            return Result.Ok(gameFound);
        }

        public Result<GameDTO> StartGame(string gameName)
        {
            gameName = gameName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == gameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var gameStartResult = gameFound.Start();
            if(gameStartResult.IsFailure)
                return Result.Failure<GameDTO>(gameStartResult.Error);

            return Result.Ok(gameFound);
        }

        public async Task<Result<GameDTO>> PlayBall(string inGameName, string ballName)
        {
            inGameName = inGameName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == inGameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var ballFound = gameFound.FindBallConfigured(ballName);
            if(ballFound == null)
                return Result.Failure<GameDTO>("Ball has not been found by its name");

            var playBallResult = gameFound.PlayBall(ballFound);
            if (playBallResult.IsFailure)
                return Result.Failure<GameDTO>(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGameName, new Infrastructure.DTOs.BallDTO { Name = ballFound.Name });

            return Result.Ok(gameFound);
        }

        public async Task<Result<GameDTO>> RandomlyPlayBall(string inGameName)
        {
            inGameName = inGameName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == inGameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var playBallResult = gameFound.RadmonlyPlayBall(this._randomizer);
            if (playBallResult.IsFailure)
                return Result.Failure<GameDTO>(playBallResult.Error);

            await this._bingoHub.SendBallPlayedMessage(inGameName, new Infrastructure.DTOs.BallDTO { Name = playBallResult.Value.Name });

            return Result.Ok(gameFound);
        }

        public async Task<Result<GameDTO>> SetWinner(string inGameName, string winnerName)
        {
            inGameName = inGameName.Trim();
            winnerName = winnerName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == inGameName);
            if (gameFound == null)
                return Result.Failure<GameDTO>("Game has not been found by its name");

            var playerFound = gameFound.FindPlayer(winnerName);
            if (playerFound == null)
                return Result.Failure<GameDTO>("Player has not been found by its name");

            var settingWinnerResult = gameFound.SetWinner(playerFound);
            if (settingWinnerResult.IsFailure)
                return Result.Failure<GameDTO>(settingWinnerResult.Error);

            await this._bingoHub.SendWinnerMessage(inGameName, winnerName);

            return Result.Ok(gameFound);
        }

        public Result<LoginResultDTO> PerformLogIn(string inGameName, string forPlayerName)
        {
            inGameName = inGameName.Trim();
            forPlayerName = forPlayerName.Trim();

            var gameFound = this._games.FirstOrDefault(g => g.Name == inGameName);
            if (gameFound == null)
                return Result.Failure<LoginResultDTO>("Game has not been found by its name");

            var playerFound = gameFound.FindPlayer(forPlayerName);
            if(playerFound == null)
                return Result.Failure<LoginResultDTO>("Player does not exist in Game");

            var jwtPlayerToken = this._bingoSecurity.CreateJWTTokenForPlayer(inGameName);
            var loginResult = new LoginResultDTO(playerFound, gameFound.BallsPlayed, jwtPlayerToken);
            return Result.Ok(loginResult);
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
    }
}
