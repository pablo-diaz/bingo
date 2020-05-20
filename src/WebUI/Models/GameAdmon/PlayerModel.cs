using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core;

namespace WebUI.Models.GameAdmon
{
    public class PlayerModel
    {
        [Required(ErrorMessage = "Por favor escribe un nombre del jugador")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo. Intenta con uno más corto")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Por favor escribe un login para el jugador")]
        [StringLength(30, ErrorMessage = "El login es muy largo. Intenta con uno más corto")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Por favor escribe una contraseña para el jugador")]
        [StringLength(100, ErrorMessage = "La contraseña es muy larga. Intenta con una más corta")]
        public string Password { get; set; }

        public int BoardsCount { get; set; }

        public int WinningBoardCount { get; set; }

        public bool IsTheWinner { get; set; }

        public Player PlayerEntity { get; set; }

        public static PlayerModel FromEntity(Player entity, bool isTheWinner) =>
            new PlayerModel { Name = entity.Name, 
                              Login = entity.Security.Login, 
                              Password = entity.Security.Password, 
                              BoardsCount = entity.Boards.Count,
                              PlayerEntity = entity,
                              WinningBoardCount = entity.Boards.Count(board => board.State == BoardState.Winner),
                              IsTheWinner = isTheWinner
            };
    }
}
