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

        public int BoardsCount => PlayerEntity.Boards.Count;

        public int WinningBoardCount => PlayerEntity.Boards.Count(board => board.State == BoardState.Winner);

        public bool IsTheWinner { get; }

        public Player PlayerEntity { get; }

        private PlayerModel(string name, Player entity, bool isTheWinner)
        {
            this.Name = name;
            this.PlayerEntity = entity;
            this.IsTheWinner = isTheWinner;
        }

        public static PlayerModel CreateAsEmptyForNewPlayer() =>
            new PlayerModel(null, null, false);

        public static PlayerModel FromEntity(Player entity, bool isTheWinner) =>
            new PlayerModel(entity.Name, entity, isTheWinner);
    }
}
