using Core;

namespace WebUI.Models.GamePlayer
{
    public class PlayerModel
    {
        public string Name { get; set; }

        public static PlayerModel FromEntity(Player entity) =>
            new PlayerModel { Name = entity.Name };
    }
}
