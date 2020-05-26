using Core;

using WebUI.Services.DTOs;

namespace WebUI.Models.GamePlayer
{
    public class GameModel
    {
        public string Name { get; set; }

        public GameType GameType { get; set; }

        public static GameModel FromEntity(GameDTO entity) =>
            new GameModel { Name = entity.Name, GameType = entity.GameType };
    }
}
