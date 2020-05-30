using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Core;

using WebUI.Services.DTOs;

namespace WebUI.Models.GameAdmon
{
    public class GameModel
    {
        [Required(ErrorMessage = "Por favor escribe un nombre del juego")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo. Intenta con uno más corto")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Por favor selecciona el tipo de juego")]
        public GameType? GameType { get; set; }

        public int PlayerCount { get => Players.Count; }

        public string State { get; set; }

        public bool IsItADraftGame { get => GameEntity.GameStatus == GameStatus.DRAFT; }

        public List<PlayerModel> Players { get; set; }

        public Dictionary<string, List<BallModel>> MasterBoard { get; set; }

        public List<BallModel> BallsPlayed { get; set; }

        public GameDTO GameEntity { get; set; }

        public static GameModel FromEntity(GameDTO entity) =>
            new GameModel { Name = entity.Name, 
                            GameType = entity.GameType,
                            Players = entity.Players.Select(player => PlayerModel.FromEntity(player, entity.Winner.HasValue ? player == entity.Winner.Value : false)).ToList(),
                            GameEntity = entity,
                            State = entity.GameStatus switch {
                                GameStatus.DRAFT => "Borrador [No Iniciado]",
                                GameStatus.ACTIVE => "Iniciado [Jugando]",
                                GameStatus.FINISHED => $"Finalizado [Ganador: {entity.Winner.Value.Name}]",
                                _ => "Estado desconocido"
                            },
                            MasterBoard = BuildMasterBoardState(entity),
                            BallsPlayed = entity.BallsPlayed.Select(ball => new BallModel { 
                                                                    Letter = ball.Letter.ToString(), 
                                                                    Number = ball.Number, 
                                                                    Entity = ball })
                                                            .ToList()
            };

        private static Dictionary<string, List<BallModel>> BuildMasterBoardState(GameDTO game)
        {
            var result = new Dictionary<string, List<BallModel>>() {
                { "B", GetBallsByLetter(game, BallLeter.B) },
                { "I", GetBallsByLetter(game, BallLeter.I) },
                { "N", GetBallsByLetter(game, BallLeter.N) },
                { "G", GetBallsByLetter(game, BallLeter.G) },
                { "O", GetBallsByLetter(game, BallLeter.O) }
            };

            return result;
        }

        private static List<BallModel> GetBallsByLetter(GameDTO game, BallLeter letter)
        {
            return game.BallsConfigured.Where(ball => ball.Letter == letter)
                .Select(ball => new BallModel { 
                    Letter = ball.Letter.ToString(), 
                    Number = ball.Number, 
                    WasItPlayed = game.BallsPlayed.Contains(ball),
                    Entity = ball
                })
                .OrderBy(ball => ball.Number)
                .ToList();
        }
    }
}
