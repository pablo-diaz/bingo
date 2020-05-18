using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Core;

namespace WebUI.Models.GameAdmon
{
    public class GameModel
    {
        [Required(ErrorMessage = "Por favor escribe un nombre del juego")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo. Intenta con uno más corto")]
        public string Name { get; set; }

        public int PlayerCount { get => Players.Count; }

        public string State { get; set; }

        public bool CanStartButtonBeShown { get => GameEntity.State == GameState.Draft; }

        public List<PlayerModel> Players { get; set; }

        public Game GameEntity { get; set; }

        public static GameModel FromEntity(Game entity) =>
            new GameModel { Name = entity.Name, 
                            Players = entity.Players.Select(player => PlayerModel.FromEntity(player)).ToList(),
                            GameEntity = entity,
                            State = entity.State switch {
                                GameState.Draft => "Borrador [No Iniciado]",
                                GameState.Started => "Iniciado [Jugando]",
                                GameState.Finished => $"Finalizado [Ganador: {entity.Winner.Value.Name}]",
                                _ => "Estado desconocido"
                            }
            };
    }
}
