using System.Linq;
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
            CREATING_GAME,
            EDITING_GAME,
            CREATING_PLAYER
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
            this.Games = new List<GameModel>();
        }

        public Task InitializeComponent()
        {
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

            var updatingGameModelIndex = this.Games.FindIndex(game => game.Name == this.GameModel.Name);
            this.Games[updatingGameModelIndex] = GameModel.FromEntity(this.GameModel.GameEntity);

            this._toastService.ShowSuccess("El jugador se ha creado exitosamente");

            this.TransitionToEditGame(this.Games[updatingGameModelIndex]);
            return Task.CompletedTask;
        }

        public Task CancelCreatingNewPlayer()
        {
            TransitionToEditGame(this.GameModel);

            return Task.CompletedTask;
        }

        public void AddBoardToPlayer(PlayerModel player)
        {
            var addBoardToPlayerResult = this.GameModel.GameEntity.AddBoardToPlayer(player.PlayerEntity);
            if(addBoardToPlayerResult.IsFailure)
            {
                this._toastService.ShowError(addBoardToPlayerResult.Error);
                return;
            }

            var currentGameIndex = this.Games.FindIndex(game => game.Name == this.GameModel.Name);
            var currentPlayerIndex = this.Games[currentGameIndex].Players.FindIndex(p => p.Name == player.Name);
            var updatedPlayer = this.GameModel.GameEntity.Players.First(p => p.Name == player.Name);
            this.Games[currentGameIndex].Players[currentPlayerIndex] = PlayerModel.FromEntity(updatedPlayer);

            this._toastService.ShowSuccess($"Una tabla más ha sido exitosamente agregada a {player.Name}. Ahora tiene en total {updatedPlayer.Boards.Count} tablas");
        }

        public Task StartGame()
        {
            var startGameResult = this.GameModel.GameEntity.Start();
            if(startGameResult.IsFailure)
            {
                this._toastService.ShowError(startGameResult.Error);
                return Task.CompletedTask;
            }

            var currentGameIndex = this.Games.FindIndex(game => game.Name == this.GameModel.Name);
            this.GameModel = GameModel.FromEntity(this.GameModel.GameEntity);
            this.Games[currentGameIndex] = this.GameModel;

            this._toastService.ShowSuccess("El juego ha empezado");

            return Task.CompletedTask;
        }
    }
}
