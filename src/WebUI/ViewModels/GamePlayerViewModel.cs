using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebUI.Services;
using WebUI.Models.GamePlayer;

using Blazored.Toast.Services;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;

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
        private State _currentState;
        private HubConnection _bingoHubConnection;

        public List<GameModel> PlayableGames => 
            this._gamingComunication.GetPlayableGames()
                .Select(game => GameModel.FromEntity(game))
                .ToList();
        public GameModel GameSelected { get; set; }
        public LoginModel LoginModel { get; set; }
        public PlayerModel PlayerModel { get; set; }

        public bool CanSelectGameSectionBeShown => this._currentState == State.SELECTING_GAME;
        public bool CanAuthenticateInGameSectionBeShown => this._currentState == State.AUTHENTICATING_IN_GAME;
        public bool CanLoggedInSectionBeShown => this._currentState == State.LOGGED_IN;

        public GamePlayerViewModel(IToastService toastService, GamingComunication gamingComunication,
            NavigationManager navigationManager)
        {
            this._toastService = toastService;
            this._gamingComunication = gamingComunication;
            this._navigationManager = navigationManager;
        }

        public Task InitializeComponent()
        {
            this.TransitionToSelectingGame();
            return Task.CompletedTask;
        }

        public void TransitionToSelectingGame()
        {
            this._currentState = State.SELECTING_GAME;
            this.GameSelected = null;
            this.LoginModel = null;
            this.PlayerModel = null;
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

            this.PlayerModel = PlayerModel.FromEntity(resultInfo.LoggedInPlayer);
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
            this._bingoHubConnection = new HubConnectionBuilder()
                .WithUrl(this._navigationManager.ToAbsoluteUri("/bingoHub"), options => {
                    options.AccessTokenProvider = () => Task.FromResult(jwtPlayerToken);
                })
                .Build();

            this._bingoHubConnection.On<Infrastructure.DTOs.BallDTO>("OnBallPlayedMessage", ballPlayed =>
            {
                OnBallPlayed(ballPlayed);
            });

            await this._bingoHubConnection.StartAsync();
        }

        private void DisplayBallsPlayedRecentlyBeforeLoggin(IReadOnlyCollection<Infrastructure.DTOs.BallDTO> balls)
        {
            Console.WriteLine($"The following {balls.Count} balls have been played already: ");
            foreach (var ball in balls)
                OnBallPlayed(ball);
        }

        private void OnBallPlayed(Infrastructure.DTOs.BallDTO ball)
        {
            Console.WriteLine($"Ball {ball.Name} has been played");
        }
    }
}
