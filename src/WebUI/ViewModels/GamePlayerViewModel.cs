using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebUI.Services;
using WebUI.Models.GamePlayer;
using WebUI.Infrastructure.DTOs;

using Blazored.Toast.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace WebUI.ViewModels
{
    public class GamePlayerViewModel: IDisposable
    {
        public enum State
        {
            SELECTING_GAME,
            SELECTING_PLAYER,
            PLAYER_SELECTED
        }

        private readonly IToastService _toastService;
        private readonly GameApplication _gamingComunication;
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
        public PlayerModel PlayerModel { get; set; }
        public List<BallDTO> PlayedBalls { get; private set; }
        
        public bool CanSelectGameSectionBeShown => this._currentState == State.SELECTING_GAME;
        public bool CanSelectPlayerInGameSectionBeShown => this._currentState == State.SELECTING_PLAYER;
        public bool CanPlayerSelectedSectionBeShown => this._currentState == State.PLAYER_SELECTED;

        public GamePlayerViewModel(IToastService toastService, GameApplication gamingComunication,
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
            this.PlayerModel = null;
            this.PlayedBalls = null;
        }

        public Task SelectGame(GameModel game)
        {
            this.GameSelected = game;
            this._currentState = State.SELECTING_PLAYER;
            return Task.CompletedTask;
        }

        public async Task SelectPlayer(PlayerModel player)
        {
            var loginResult = this._gamingComunication.PerformLogIn(this.GameSelected.Name, player.PlayerEntity);
            if(loginResult.IsFailure)
            {
                this._toastService.ShowError(loginResult.Error);
                return;
            }

            DisplayBallsPlayedRecentlyBeforeLoggin(loginResult.Value.BallsPlayedRecently);
            await this.StartBingoHubConnection(loginResult.Value.JWTPlayerToken);

            this.PlayerModel = PlayerModel.FromEntity(loginResult.Value.LoggedInPlayer, this.GameSelected.GameType);
            this._currentState = State.PLAYER_SELECTED;
            return;
        }

        private async Task StartBingoHubConnection(string jwtPlayerToken)
        {
            if (this._bingoHubConnection != null)
                await this.DisconnectFromServer();

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

            this.PlayedBalls.Insert(0, ball);

            if(this.PlayerModel != null)
                this.PlayerModel.AdjustBoardsState(ball);

            this._triggerUIRefresh?.Invoke();
        }

        private async Task SetWinnerMessageHandler(string winnerName)
        {
            var isItMeTheWinner = this.PlayerModel != null && this.PlayerModel.Name == winnerName;
            if (isItMeTheWinner)
                this._toastService.ShowSuccess($"Tú has ganado el juego. FELICITACIONES !!", "Eres el Ganador");
            else
                this._toastService.ShowSuccess($"El jugador {winnerName} ha ganado el juego !!");

            await this.DisconnectFromServer();
        }

        public void Dispose()
        {
            Task.Run(() => this.DisconnectFromServer());
        }

        private async Task DisconnectFromServer()
        {
            if (this._bingoHubConnection == null || this._bingoHubConnection.State == HubConnectionState.Disconnected)
                return;

            await this._bingoHubConnection.StopAsync();
        }
    }
}
