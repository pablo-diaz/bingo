using Core;
using System.Linq;
using System.Collections.Generic;

using WebUI.Services.DTOs;

namespace WebUI.Models.GamePlayer
{
    public class GameModel
    {
        public string Name { get; set; }

        public GameType GameType => GameEntity.GameType;

        public GameState GameEntity { get; set; }

        public List<PlayerModel> Players { get; set; }

        public static GameModel FromEntity(GameState entity) =>
            new GameModel {
                Name = entity.Name,
                GameEntity = entity,
                Players = entity.Players.Select(player => PlayerModel.FromEntity(player, entity.GameType)).ToList() };
    }
}
