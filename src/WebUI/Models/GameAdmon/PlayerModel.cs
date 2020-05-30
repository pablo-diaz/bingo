using System.Linq;
using System.ComponentModel.DataAnnotations;

using Core;

namespace WebUI.Models.GameAdmon
{
    public class PlayerModel
    {
        [Required(ErrorMessage = "Por favor escribe un nombre del jugador")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo. Intenta con uno más corto")]
        public string Name { get; set; }

        public int BoardsCount { get; set; }

        public int WinningBoardCount { get; set; }

        public bool IsTheWinner { get; set; }

        public Player PlayerEntity { get; set; }

        public static PlayerModel FromEntity(Player entity, bool isTheWinner) =>
            new PlayerModel { Name = entity.Name, 
                              BoardsCount = entity.Boards.Count,
                              PlayerEntity = entity,
                              WinningBoardCount = entity.Boards.Count(board => board.State == BoardState.Winner),
                              IsTheWinner = isTheWinner
            };
    }
}
