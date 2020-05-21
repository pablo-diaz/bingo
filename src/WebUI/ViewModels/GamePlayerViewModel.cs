using System.Linq;
using System.Collections.Generic;

using WebUI.Services;
using WebUI.Models.GamePlayer;

namespace WebUI.ViewModels
{
    public class GamePlayerViewModel
    {
        private readonly GamingComunication _gamingComunication;

        public List<GameModel> PlayableGames => 
            this._gamingComunication.GetPlayableGames()
                .Select(game => GameModel.FromEntity(game))
                .ToList();

        public GamePlayerViewModel(GamingComunication gamingComunication)
        {
            this._gamingComunication = gamingComunication;
        }
    }
}
