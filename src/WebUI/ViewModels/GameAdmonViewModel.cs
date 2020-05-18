using System.Threading.Tasks;
using System.Collections.Generic;

using Core;

using WebUI.Models.GameAdmon;

using Blazored.Toast.Services;
using System.Linq;

namespace WebUI.ViewModels
{
    public class GameAdmonViewModel
    {
        public enum State
        {
            BROWSING,
            CREATING_GAME,
            EDITING_GAME,
            CREATING_PLAYER,
            EDITING_PLAYER
        }

        private const short STANDARD_BALLS_VERSION_TOTAL = 75;
        private const short STANDARD_BALLS_VERSION_PER_BUCKET_COUNT = 5;
        private readonly IToastService _toastService;
        private State _currentState;

        public List<GameModel> Games { get; private set; }
        public GameModel GameModel { get; set; }
        public PlayerModel PlayerModel { get; set; }

        public bool CanLandingBeShown => this._currentState == State.BROWSING;
        public bool CanNewGameSectionBeShown => this._currentState == State.CREATING_GAME;
        public bool CanEditGameSectionBeShown => this._currentState == State.EDITING_GAME;
        public bool CanNewPlayerSectionBeShown => this._currentState == State.CREATING_PLAYER;

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
            this.PlayerModel = null;
        }

        public Task TransitionToNewGame()
        {
            this._currentState = State.CREATING_GAME;
            this.GameModel = new GameModel();
            return Task.CompletedTask;
        }

        public Task CreateNewGame()
        {
            var existingGameFound = this.Games.FirstOrDefault(game => game.Name == this.GameModel.Name.Trim());
            if(existingGameFound != null)
            {
                this._toastService.ShowWarning("Actualmente existe un juego con el mismo nombre. Por favor, intenta crear un nombre diferente");
                return Task.CompletedTask;
            }

            var newGameResult = Game.Create(this.GameModel.Name.Trim(), STANDARD_BALLS_VERSION_TOTAL, STANDARD_BALLS_VERSION_PER_BUCKET_COUNT);
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

        public Task EditGame(GameModel game)
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
            var newPlayerSecurityResult = PlayerSecurity.Create(this.PlayerModel.Login.Trim().ToLower(), this.PlayerModel.Password.Trim());
            if(newPlayerSecurityResult.IsFailure)
            {
                this._toastService.ShowError(newPlayerSecurityResult.Error);
                return Task.CompletedTask;
            }

            var newPlayerResult = Player.Create(this.PlayerModel.Name.Trim(), newPlayerSecurityResult.Value);
            if (newPlayerResult.IsFailure)
            {
                this._toastService.ShowError(newPlayerResult.Error);
                return Task.CompletedTask;
            }

            var addPlayerResult = this.GameModel.GameEntity.AddPlayer(newPlayerResult.Value);
            if(addPlayerResult.IsFailure)
            {
                this._toastService.ShowError(addPlayerResult.Error);
                return Task.CompletedTask;
            }

            var updatedGame = this.Games.First(game => game.Name == this.GameModel.Name);
            updatedGame = GameModel.FromEntity(this.GameModel.GameEntity);
            this.GameModel = updatedGame;

            this._toastService.ShowSuccess("El jugador se ha creado exitosamente");

            this.EditGame(this.GameModel);
            return Task.CompletedTask;
        }

        public Task CancelCreatingNewPlayer()
        {
            EditGame(this.GameModel);

            return Task.CompletedTask;
        }

        public Task EditPlayer(PlayerModel player)
        {
            this._currentState = State.EDITING_PLAYER;
            this.PlayerModel = player;

            return Task.CompletedTask;
        }
    }
}
