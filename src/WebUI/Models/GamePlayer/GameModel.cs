using Core;

namespace WebUI.Models.GamePlayer
{
    public class GameModel
    {
        public string Name { get; set; }

        public static GameModel FromEntity(Game entity) =>
            new GameModel { Name = entity.Name };
    }
}
