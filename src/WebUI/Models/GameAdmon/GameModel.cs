using System.ComponentModel.DataAnnotations;

using Core;

namespace WebUI.Models.GameAdmon
{
    public class GameModel
    {
        [Required(ErrorMessage = "Por favor escribe un nombre del juego")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo. Intenta con uno más corto")]
        public string Name { get; set; }

        public int PlayerCount { get; set; }

        public static GameModel FromEntity(Game entity) =>
            new GameModel { Name = entity.Name, PlayerCount = entity.Players.Count };
    }
}
