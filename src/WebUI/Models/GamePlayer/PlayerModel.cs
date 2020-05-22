using System.Collections.Generic;
using System.Linq;
using Core;

namespace WebUI.Models.GamePlayer
{
    public class PlayerModel
    {
        public string Name { get; set; }

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

        public static PlayerModel FromEntity(Player entity) =>
            new PlayerModel { 
                Name = entity.Name, 
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
