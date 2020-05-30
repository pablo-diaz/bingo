﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Core;

using WebUI.Models.GameAdmon;
using WebUI.Services;

using Blazored.Toast.Services;

using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using System;
using WebUI.Services.DTOs;

namespace WebUI.ViewModels
{
    public class GameAdmonViewModel
    {
        public enum State
        {
            AUTHENTICATING_ADMIN,
            BROWSING,
            CREATING_GAME,
            EDITING_GAME,
            CREATING_PLAYER,
            EDITING_PLAYER,
            COPYING_PLAYERS_FROM_OTHER_GAME
        }

        private readonly IToastService _toastService;
        private readonly GamingComunication _gamingComunication;
        private readonly IConfiguration _configuration;
        private State _currentState;

        public AdminLoginModel AdminLoginModel { get; set; }
        public GameModel CurrentGame { get; set; }
        public PlayerModel PlayerModel { get; set; }
        public List<GameModel> OtherGames =>
            this._gamingComunication.GetAllGames()
                .Where(game => game.Name != this.CurrentGame.Name)
                .Select(game => GameModel.FromEntity(game))
                .ToList();

        public List<GameModel> Games => this._gamingComunication.GetAllGames()
            .Select(game => GameModel.FromEntity(game))
            .ToList();

        public bool CanAuthenticatingAdminSectionBeShown => this._currentState == State.AUTHENTICATING_ADMIN;
        public bool CanLandingBeShown => this._currentState == State.BROWSING;
        public bool CanNewGameSectionBeShown => this._currentState == State.CREATING_GAME;
        public bool CanEditGameSectionBeShown => this._currentState == State.EDITING_GAME;
        public bool CanNewPlayerSectionBeShown => this._currentState == State.CREATING_PLAYER;
        public bool CanEditPlayerSectionBeShown => this._currentState == State.EDITING_PLAYER;
        public bool CanCopyPlayersFromOtherGameSectionBeShown => this._currentState == State.COPYING_PLAYERS_FROM_OTHER_GAME;
        public bool IsItInDevMode => Convert.ToBoolean(this._configuration["Bingo.Security:DevMode"]);

        public bool IsThereAWinnerAlready => this.CurrentGame.GameEntity.Winner.HasValue;

        public GameAdmonViewModel(IToastService toastService, GamingComunication gamingComunication,
            IConfiguration configuration)
        {
            this._toastService = toastService;
            this._gamingComunication = gamingComunication;
            this._configuration = configuration;
        }

        public Task InitializeComponent()
        {
            this.TransitionToAuthenticateAdmin();
            return Task.CompletedTask;
        }

        private void TransitionToAuthenticateAdmin()
        {
            this._currentState = State.AUTHENTICATING_ADMIN;
            this.AdminLoginModel = new AdminLoginModel();
            this.CurrentGame = null;
            this.PlayerModel = null;
        }

        public void TransitionToBrowsing()
        {
            this._currentState = State.BROWSING;
            this.CurrentGame = null;
            this.PlayerModel = null;
        }

        public Task TransitionToNewGame()
        {
            this._currentState = State.CREATING_GAME;
            this.CurrentGame = new GameModel();
            return Task.CompletedTask;
        }

        public Task AuthenticateAdmin()
        {
            var adminPassword = this._configuration["Bingo.Security:AdminPassword"];
            if(adminPassword != this.AdminLoginModel.Passwd.Trim())
            {
                this._toastService.ShowError("Contraseña equivocada. Intenta nuevamente");
                return Task.CompletedTask;
            }

            this.TransitionToBrowsing();

            return Task.CompletedTask;
        }

        public Task CreateNewGame()
        {
            var addNewGameResult = this._gamingComunication.AddStandardGame(this.CurrentGame.Name, this.CurrentGame.GameType.Value);
            if(addNewGameResult.IsFailure)
            {
                this._toastService.ShowError(addNewGameResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess($"El juego '{this.CurrentGame.Name}' se ha creado exitosamente");

            this.TransitionToBrowsing();
            return Task.CompletedTask;
        }

        public Task CancelCreatingNewGame()
        {
            TransitionToBrowsing();

            return Task.CompletedTask;
        }

        public Task TransitionToEditGame(GameModel game)
        {
            this._currentState = State.EDITING_GAME;
            this.CurrentGame = game;
            this.PlayerModel = null;

            return Task.CompletedTask;
        }

        public Task TransitionToCopyPlayersFromOtherGame()
        {
            this._currentState = State.COPYING_PLAYERS_FROM_OTHER_GAME;
            return Task.CompletedTask;
        }

        public void CopyPlayersFromGame(GameModel sourceGame)
        {
            var copyPlayersResult = this._gamingComunication.CopyPlayersFromGame(sourceGame.Name, this.CurrentGame.Name);
            if (copyPlayersResult.IsFailure)
            {
                this._toastService.ShowError(copyPlayersResult.Error);
                return;
            }

            this._toastService.ShowSuccess($"Se han copiado los jugadores exitosamente");

            TransitionToEditGame(GameModel.FromEntity(copyPlayersResult.Value));
        }

        public Task CancelCopyPlayersFromGame()
        {
            TransitionToEditGame(this.CurrentGame);

            return Task.CompletedTask;
        }

        public Task TransitionToNewPlayer()
        {
            this._currentState = State.CREATING_PLAYER;
            this.PlayerModel = new PlayerModel();

            return Task.CompletedTask;
        }

        public Task SavePlayerInfo()
        {
            GameDTO game = null;

            if(this._currentState == State.CREATING_PLAYER)
            {
                var newPlayerResult = this._gamingComunication.AddNewPlayerToGame(
                    this.CurrentGame.Name, this.PlayerModel.Name);
                if(newPlayerResult.IsFailure)
                {
                    this._toastService.ShowError(newPlayerResult.Error);
                    return Task.CompletedTask;
                }

                game = newPlayerResult.Value;
                this._toastService.ShowSuccess("El jugador se ha creado exitosamente");
            }
            else if(this._currentState == State.EDITING_PLAYER)
            {
                var updatePlayerResult = this._gamingComunication.UpdatePlayerInfoInGame(
                    this.CurrentGame.Name, this.PlayerModel.PlayerEntity, this.PlayerModel.Name);

                if (updatePlayerResult.IsFailure)
                {
                    this._toastService.ShowError(updatePlayerResult.Error);
                    return Task.CompletedTask;
                }

                game = updatePlayerResult.Value;
                this._toastService.ShowSuccess("El jugador se ha actualizado exitosamente");
            }
            else
            {
                this._toastService.ShowError("Game is in weird state :(");
                return Task.CompletedTask;
            }

            this.TransitionToEditGame(GameModel.FromEntity(game));
            return Task.CompletedTask;
        }

        public Task CancelAddingOrEditingPlayerInfo()
        {
            TransitionToEditGame(this.CurrentGame);

            return Task.CompletedTask;
        }

        public Task TransitionToEditPlayerInfo(PlayerModel player)
        {
            this._currentState = State.EDITING_PLAYER;
            this.PlayerModel = player;
            return Task.CompletedTask;
        }

        public Task AddBoardToPlayer(PlayerModel player)
        {
            var addBoardToPlayerResult = this._gamingComunication.AddBoardToPlayer(this.CurrentGame.Name, player.Name);
            if(addBoardToPlayerResult.IsFailure)
            {
                this._toastService.ShowError(addBoardToPlayerResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess($"Una tabla más ha sido exitosamente agregada a {player.Name}");

            this.TransitionToEditGame(GameModel.FromEntity(addBoardToPlayerResult.Value));
            return Task.CompletedTask;
        }

        public Task RemoveBoardFromPlayer(PlayerModel player)
        {
            var removeBoardFromPlayerResult = this._gamingComunication.RemoveBoardFromPlayer(this.CurrentGame.Name, player.Name);
            if (removeBoardFromPlayerResult.IsFailure)
            {
                this._toastService.ShowError(removeBoardFromPlayerResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess($"Se ha removido una tabla del jugador {player.Name}");

            this.TransitionToEditGame(GameModel.FromEntity(removeBoardFromPlayerResult.Value));
            return Task.CompletedTask;
        }

        public Task RemovePlayer(PlayerModel player)
        {
            var removePlayerResult = this._gamingComunication.RemovePlayer(this.CurrentGame.Name, player.Name);
            if (removePlayerResult.IsFailure)
            {
                this._toastService.ShowError(removePlayerResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess($"Se ha eliminado el jugador {player.Name}");

            this.TransitionToEditGame(GameModel.FromEntity(removePlayerResult.Value));
            return Task.CompletedTask;
        }

        public Task StartGame()
        {
            var startGameResult = this._gamingComunication.StartGame(this.CurrentGame.Name);
            if(startGameResult.IsFailure)
            {
                this._toastService.ShowError(startGameResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess("El juego ha empezado");

            this.TransitionToEditGame(GameModel.FromEntity(startGameResult.Value));
            return Task.CompletedTask;
        }

        public async Task PlayBall(BallModel ball)
        {
            var playBallResult = await this._gamingComunication.PlayBall(this.CurrentGame.Name, ball.Entity.Name);
            HandleBallPlayedResult(playBallResult);
        }

        public async Task PlayBallRandomly()
        {
            var playBallResult = await this._gamingComunication.RandomlyPlayBall(this.CurrentGame.Name);
            HandleBallPlayedResult(playBallResult);
        }

        private void HandleBallPlayedResult(Result<GameDTO> playBallResult)
        {
            if (playBallResult.IsFailure)
            {
                this._toastService.ShowError(playBallResult.Error);
                return;
            }

            var potentialWinners = playBallResult.Value.Players
                .Where(player => player.Boards.Any(board => board.State == BoardState.Winner))
                .Select(player => player.Name)
                .ToList();

            if (potentialWinners.Count > 0)
                this._toastService.ShowWarning($"Potenciales ganadores: {string.Join(" - ", potentialWinners)}");

            this._toastService.ShowSuccess("Bola ha sido jugada exitosamente");

            this.TransitionToEditGame(GameModel.FromEntity(playBallResult.Value));
        }

        public async Task SetWinner(PlayerModel player)
        {
            var setWinnerResult = await this._gamingComunication.SetWinner(this.CurrentGame.Name, player.Name);
            if (setWinnerResult.IsFailure)
            {
                this._toastService.ShowError(setWinnerResult.Error);
                return;
            }

            this._toastService.ShowSuccess($"Se ha establecido a {player.Name} como el Ganador del juego");

            await this.TransitionToEditGame(GameModel.FromEntity(setWinnerResult.Value));
        }

        public void DeleteGame(GameModel game)
        {
            var deleteGameResult = this._gamingComunication.DeleteGame(game.Name);
            if (deleteGameResult.IsFailure)
            {
                this._toastService.ShowError(deleteGameResult.Error);
                return;
            }

            this._toastService.ShowSuccess($"El juego se ha eliminado exitosamente");

            TransitionToBrowsing();
        }

        public Task AddTestGames()
        {
            var gameTypes = new GameType[] { GameType.L, GameType.O, GameType.STANDARD, GameType.T, GameType.X };
            Enumerable.Range(1, 10)
                .ToList()
                .ForEach(testGameId => {
                    var gameType = gameTypes[testGameId % gameTypes.Length];
                    var addNewGameResult = this._gamingComunication.AddStandardGame($"Game_{testGameId}", gameType);
                });

            return Task.CompletedTask;
        }

        public Task AddTestPlayers()
        {
            GameDTO gameInContext = null;
            Enumerable.Range(1, 7)
                .ToList()
                .ForEach(testPlayerNumber => {
                    var newPlayerResult = this._gamingComunication.AddNewPlayerToGame(
                        this.CurrentGame.Name, $"Name_{testPlayerNumber}");
                    gameInContext = newPlayerResult.Value;
                });

            this.TransitionToEditGame(GameModel.FromEntity(gameInContext));

            return Task.CompletedTask;
        }
    }
}
