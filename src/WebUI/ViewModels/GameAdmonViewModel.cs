using System.Threading.Tasks;
using System.Collections.Generic;

using Core;

using WebUI.Models.GameAdmon;

using Blazored.Toast.Services;

namespace WebUI.ViewModels
{
    public class GameAdmonViewModel
    {
        public enum State
        {
            BROWSING,
            CREATING_GAME
        }

        private const short STANDARD_BALLS_VERSION_TOTAL = 75;
        private const short STANDARD_BALLS_VERSION_PER_BUCKET_COUNT = 5;
        private readonly IToastService _toastService;
        private State _currentState;

        public List<GameModel> Games { get; private set; }
        public GameModel GameModel { get; set; }

        public bool CanLandingBeShown => this._currentState == State.BROWSING;
        public bool CanNewGameSectionBeShown => this._currentState == State.CREATING_GAME;

        public GameAdmonViewModel(IToastService toastService)
        {
            this._toastService = toastService;
        }

        public Task InitializeComponent()
        {
            this.Games = new List<GameModel>();
            this.TransitionToBrowsing();

            return Task.CompletedTask;
        }

        public void TransitionToBrowsing()
        {
            this._currentState = State.BROWSING;
            this.GameModel = null;
        }

        public Task TransitionToNewGame()
        {
            this._currentState = State.CREATING_GAME;
            this.GameModel = new GameModel();
            return Task.CompletedTask;
        }

        public Task CreateNewGame()
        {
            var newGameResult = Game.Create(this.GameModel.Name, STANDARD_BALLS_VERSION_TOTAL, STANDARD_BALLS_VERSION_PER_BUCKET_COUNT);
            if(newGameResult.IsFailure)
            {
                this._toastService.ShowError(newGameResult.Error);
                return Task.CompletedTask;
            }

            this.Games.Add(GameModel.FromEntity(newGameResult.Value));

            this._toastService.ShowSuccess("El juego se ha creado exitosamente");

            this.TransitionToBrowsing();
            return Task.CompletedTask;
        }

        public Task CancelCreatingNewGame()
        {
            TransitionToBrowsing();

            return Task.CompletedTask;
        }
    }
}
