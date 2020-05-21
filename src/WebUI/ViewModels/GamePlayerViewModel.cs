using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using WebUI.Services;
using WebUI.Models.GamePlayer;

using Blazored.Toast.Services;

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
        private State _currentState;

        public List<GameModel> PlayableGames => 
            this._gamingComunication.GetPlayableGames()
                .Select(game => GameModel.FromEntity(game))
                .ToList();
        public GameModel GameSelected { get; set; }
        public LoginModel LoginModel { get; set; }

        public bool CanSelectGameSectionBeShown => this._currentState == State.SELECTING_GAME;
        public bool CanAuthenticateInGameSectionBeShown => this._currentState == State.AUTHENTICATING_IN_GAME;
        public bool CanLoggedInSectionBeShown => this._currentState == State.LOGGED_IN;

        public GamePlayerViewModel(IToastService toastService, GamingComunication gamingComunication)
        {
            this._toastService = toastService;
            this._gamingComunication = gamingComunication;
        }

        public Task InitializeComponent()
        {
            this.TransitionToSelectingGame();
            return Task.CompletedTask;
        }

        public Task TransitionToSelectingGame()
        {
            this._currentState = State.SELECTING_GAME;
            this.GameSelected = null;
            this.LoginModel = null;
            return Task.CompletedTask;
        }

        public Task SelectGame(GameModel game)
        {
            this.GameSelected = game;
            this.LoginModel = new LoginModel();
            this._currentState = State.AUTHENTICATING_IN_GAME;
            return Task.CompletedTask;
        }

        public Task Login()
        {
            var loginResult = this._gamingComunication.PerformLogIn(this.GameSelected.Name, this.LoginModel.Login, this.LoginModel.Passwd);
            if(loginResult.IsFailure)
            {
                this._toastService.ShowError(loginResult.Error);
                return Task.CompletedTask;
            }

            (var authSuccess, var player) = loginResult.Value;
            if(!authSuccess)
            {
                this._toastService.ShowError("Usuario o Contraseña equivocadas. Intenta nuevamente");
                return Task.CompletedTask;
            }

            this._currentState = State.LOGGED_IN;
            return Task.CompletedTask;
        }

        public Task CancelLogin()
        {
            TransitionToSelectingGame();
            return Task.CompletedTask;
        }
    }
}
