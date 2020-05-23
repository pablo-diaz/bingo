using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Core;

using WebUI.Models.GameAdmon;
using WebUI.Services;

using Blazored.Toast.Services;

using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using System;

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
            CREATING_PLAYER
        }

        private readonly IToastService _toastService;
        private readonly GamingComunication _gamingComunication;
        private readonly IConfiguration _configuration;
        private State _currentState;

        public AdminLoginModel AdminLoginModel { get; set; }
        public GameModel GameModel { get; set; }
        public PlayerModel PlayerModel { get; set; }

        public List<GameModel> Games => this._gamingComunication.GetAllGames()
            .Select(game => GameModel.FromEntity(game))
            .ToList();

        public bool CanAuthenticatingAdminSectionBeShown => this._currentState == State.AUTHENTICATING_ADMIN;
        public bool CanLandingBeShown => this._currentState == State.BROWSING;
        public bool CanNewGameSectionBeShown => this._currentState == State.CREATING_GAME;
        public bool CanEditGameSectionBeShown => this._currentState == State.EDITING_GAME;
        public bool CanNewPlayerSectionBeShown => this._currentState == State.CREATING_PLAYER;
        public bool IsItInDevMode => Convert.ToBoolean(this._configuration["Bingo.Security:DevMode"]);

        public bool IsThereAWinnerAlready => this.GameModel.GameEntity.Winner.HasValue;

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
            this.GameModel = null;
            this.PlayerModel = null;
        }

        public void TransitionToBrowsing()
        {
            this._currentState = State.BROWSING;
            this.GameModel = null;
            this.PlayerModel = null;
        }

        public Task TransitionToNewGame()
        {
            this._currentState = State.CREATING_GAME;
            this.GameModel = new GameModel();
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
            var addNewGameResult = this._gamingComunication.AddStandardGame(this.GameModel.Name);
            if(addNewGameResult.IsFailure)
            {
                this._toastService.ShowError(addNewGameResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess($"El juego '{this.GameModel.Name}' se ha creado exitosamente");

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
            this.GameModel = game;
            this.PlayerModel = null;

            return Task.CompletedTask;
        }

        public Task TransitionToNewPlayer()
        {
            this._currentState = State.CREATING_PLAYER;
            this.PlayerModel = new PlayerModel();

            return Task.CompletedTask;
        }

        public Task CreateNewPlayer()
        {
            var newPlayerResult = this._gamingComunication.AddNewPlayerToGame(this.GameModel.Name, 
                this.PlayerModel.Name, this.PlayerModel.Login, this.PlayerModel.Password);

            if(newPlayerResult.IsFailure)
            {
                this._toastService.ShowError(newPlayerResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess("El jugador se ha creado exitosamente");

            this.TransitionToEditGame(GameModel.FromEntity(newPlayerResult.Value));
            return Task.CompletedTask;
        }

        public Task CancelCreatingNewPlayer()
        {
            TransitionToEditGame(this.GameModel);

            return Task.CompletedTask;
        }

        public Task AddBoardToPlayer(PlayerModel player)
        {
            var addBoardToPlayerResult = this._gamingComunication.AddBoardToPlayer(this.GameModel.Name, player.Name);
            if(addBoardToPlayerResult.IsFailure)
            {
                this._toastService.ShowError(addBoardToPlayerResult.Error);
                return Task.CompletedTask;
            }

            this._toastService.ShowSuccess($"Una tabla más ha sido exitosamente agregada a {player.Name}");

            this.TransitionToEditGame(GameModel.FromEntity(addBoardToPlayerResult.Value));
            return Task.CompletedTask;
        }

        public Task StartGame()
        {
            var startGameResult = this._gamingComunication.StartGame(this.GameModel.Name);
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
            var playBallResult = await this._gamingComunication.PlayBall(this.GameModel.Name, ball.Entity.Name);
            HandleBallPlayedResult(playBallResult);
        }

        public async Task PlayBallRandomly()
        {
            var playBallResult = await this._gamingComunication.RandomlyPlayBall(this.GameModel.Name);
            HandleBallPlayedResult(playBallResult);
        }

        private void HandleBallPlayedResult(Result<Game> playBallResult)
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

        public void SetWinner(PlayerModel player)
        {
            var setWinnerResult = this._gamingComunication.SetWinner(this.GameModel.Name, player.Name);
            if (setWinnerResult.IsFailure)
            {
                this._toastService.ShowError(setWinnerResult.Error);
                return;
            }

            this._toastService.ShowSuccess($"Se ha establecido a {player.Name} como el Ganador del juego");

            this.TransitionToEditGame(GameModel.FromEntity(setWinnerResult.Value));
        }

        public Task AddTestGames()
        {
            Enumerable.Range(1, 10)
                .ToList()
                .ForEach(testGameId => {
                    var addNewGameResult = this._gamingComunication.AddStandardGame($"Game_{testGameId}");
                });

            return Task.CompletedTask;
        }

        public Task AddTestPlayers()
        {
            Game gameInContext = null;
            Enumerable.Range(1, 7)
                .ToList()
                .ForEach(testPlayerNumber => {
                    var newPlayerResult = this._gamingComunication.AddNewPlayerToGame(this.GameModel.Name,
                        $"Name_{testPlayerNumber}", $"login_{testPlayerNumber}", $"passwd_{testPlayerNumber}");
                    gameInContext = newPlayerResult.Value;
                });

            this.TransitionToEditGame(GameModel.FromEntity(gameInContext));

            return Task.CompletedTask;
        }
    }
}
