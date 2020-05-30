using Core;
using System.Collections.Generic;
using System.Linq;
using WebUI.Services.DTOs;

namespace WebUI.Models.GamePlayer
{
    public class GameModel
    {
        public string Name { get; set; }

        public GameType GameType { get; set; }

        public List<PlayerModel> Players { get; set; }

        public static GameModel FromEntity(GameDTO entity) =>
            new GameModel {
                Name = entity.Name,
                GameType = entity.GameType,
                Players = entity.Players.Select(player => PlayerModel.FromEntity(player, entity.GameType)).ToList() };
    }
}
