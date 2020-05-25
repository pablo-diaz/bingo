using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebUI.Services;
using WebUI.Models.GamePlayer;

using Blazored.Toast.Services;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using WebUI.Infrastructure.DTOs;
using Microsoft.Extensions.Configuration;

namespace WebUI.ViewModels
{
    public class GamePlayerViewModel
    {
        public enum State
        {
            SELECTING_GAME,
            AUTHENTICATING_IN_GAME,
            LOGGED_IN
        }

        private readonly IToastService _toastService;
        private readonly GamingComunication _gamingComunication;
        private readonly NavigationManager _navigationManager;
        private readonly IConfiguration _configuration;
        private State _currentState;
        private HubConnection _bingoHubConnection;
        private Action _triggerUIRefresh;

        public List<GameModel> PlayableGames => 
            this._gamingComunication.GetPlayableGames()
                .Select(game => GameModel.FromEntity(game))
                .ToList();
        public GameModel GameSelected { get; set; }
        public LoginModel LoginModel { get; set; }
        public PlayerModel PlayerModel { get; set; }
        public List<BallDTO> PlayedBalls { get; private set; }
        
        public bool CanSelectGameSectionBeShown => this._currentState == State.SELECTING_GAME;
        public bool CanAuthenticateInGameSectionBeShown => this._currentState == State.AUTHENTICATING_IN_GAME;
        public bool CanLoggedInSectionBeShown => this._currentState == State.LOGGED_IN;

        public GamePlayerViewModel(IToastService toastService, GamingComunication gamingComunication,
            NavigationManager navigationManager, IConfiguration configuration)
        {
            this._toastService = toastService;
            this._gamingComunication = gamingComunication;
            this._navigationManager = navigationManager;
            this._configuration = configuration;
        }

        public Task InitializeComponent(Action triggerUIRefresh)
        {
            this._triggerUIRefresh = triggerUIRefresh;
            this.TransitionToSelectingGame();
            return Task.CompletedTask;
        }

        public void TransitionToSelectingGame()
        {
            this._currentState = State.SELECTING_GAME;
            this.GameSelected = null;
            this.LoginModel = null;
            this.PlayerModel = null;
            this.PlayedBalls = null;
        }

        public Task SelectGame(GameModel game)
        {
            this.GameSelected = game;
            this.LoginModel = new LoginModel();
            this._currentState = State.AUTHENTICATING_IN_GAME;
            return Task.CompletedTask;
        }

        public async Task Login()
        {
            var loginResult = this._gamingComunication.PerformLogIn(this.GameSelected.Name, this.LoginModel.Login, this.LoginModel.Passwd);
            if(loginResult.IsFailure)
            {
                this._toastService.ShowError(loginResult.Error);
                return;
            }

            (var authSuccess, var resultInfo) = loginResult.Value;
            if(!authSuccess)
            {
                this._toastService.ShowError("Usuario o Contraseña equivocadas. Intenta nuevamente");
                return;
            }

            DisplayBallsPlayedRecentlyBeforeLoggin(resultInfo.BallsPlayedRecently);
            await this.StartBingoHubConnection(resultInfo.JWTPlayerToken);

            this.PlayerModel = PlayerModel.FromEntity(resultInfo.LoggedInPlayer, this.GameSelected.GameType);
            this._currentState = State.LOGGED_IN;
            return;
        }

        public Task CancelLogin()
        {
            this.TransitionToSelectingGame();
            return Task.CompletedTask;
        }

        private async Task StartBingoHubConnection(string jwtPlayerToken)
        {
            var specificWebRootFolder = this._configuration["Bingo.Security:SpecificWebRootFolder"];
            var serverURI = this._navigationManager.ToAbsoluteUri($"{specificWebRootFolder}/bingoHub");
            this._bingoHubConnection = new HubConnectionBuilder()
                .WithUrl(serverURI, options => {
                    options.AccessTokenProvider = () => Task.FromResult(jwtPlayerToken);
                })
                .Build();

            this._bingoHubConnection.On<BallDTO>("OnBallPlayedMessage", OnBallPlayed);
            this._bingoHubConnection.On<string>("OnSetWinnerMessage", SetWinnerMessageHandler);

            await this._bingoHubConnection.StartAsync();
        }

        private void DisplayBallsPlayedRecentlyBeforeLoggin(IReadOnlyCollection<BallDTO> balls)
        {
            this.PlayedBalls = new List<BallDTO>();
            foreach (var ball in balls)
                OnBallPlayed(ball);
        }

        private void OnBallPlayed(BallDTO ball)
        {
            if (this.PlayedBalls.Any(b => b.Name == ball.Name))
                return;

            this.PlayedBalls.Add(ball);

            if(this.PlayerModel != null)
                this.PlayerModel.AdjustBoardsState(ball);

            this._triggerUIRefresh?.Invoke();
        }

        private void SetWinnerMessageHandler(string winnerName)
        {
            if(this.PlayerModel != null && this.PlayerModel.Name == winnerName)
            {
                this._toastService.ShowSuccess($"Tú has ganado el juego. FELICITACIONES !!", "Eres el Ganador");
            }
            else
            {
                this._toastService.ShowSuccess($"El jugador {winnerName} ha ganado el juego !!");
            }
        }
    }
}
