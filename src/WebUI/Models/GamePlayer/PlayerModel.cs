using System.Linq;
using System.Collections.Generic;

using Core;

namespace WebUI.Models.GamePlayer
{
    public class PlayerModel
    {
        public string Name { get; set; }
        public GameType GameType { get; set; }
        public Player PlayerEntity { get; set; }

        public List<BoardModel> Boards { get; set; }

        public void AdjustBoardsState(Infrastructure.DTOs.BallDTO newBall)
        {
            foreach(var board in Boards)
            {
                var ballFound = board.Balls.FirstOrDefault(ball => ball.Name == newBall.Name);
                if (ballFound != null)
                    ballFound.IsItPossibleToSelect = true;
            }
        }

        public static PlayerModel FromEntity(Player entity, GameType gameType) =>
            new PlayerModel { 
                Name = entity.Name,
                GameType = gameType,
                PlayerEntity = entity,
                Boards = entity.Boards
                               .Select(board => new BoardModel {
                    Balls = board.BallsConfigured
                                 .Select(ball => new BallModel { 
                                     Leter = ball.Letter.ToString(), 
                                     Number = ball.Number,
                                     IsItPossibleToSelect = board.BallsPlayed.Contains(ball) })
                                 .ToList() })
                               .ToList() };
    }
}
