using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Core;
using System;

namespace WebUI.Models.GameAdmon
{
    public class GameModel
    {
        [Required(ErrorMessage = "Por favor escribe un nombre del juego")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo. Intenta con uno más corto")]
        public string Name { get; set; }

        public int PlayerCount { get => Players.Count; }

        public string State { get; set; }

        public bool IsItADraftGame { get => GameEntity.State == GameState.Draft; }

        public List<PlayerModel> Players { get; set; }

        public Dictionary<string, List<BallModel>> MasterBoard { get; set; }

        public List<BallModel> BallsPlayed { get; set; }

        public Game GameEntity { get; set; }

        public static GameModel FromEntity(Game entity) =>
            new GameModel { Name = entity.Name, 
                            Players = entity.Players.Select(player => PlayerModel.FromEntity(player, entity.Winner.HasValue ? player == entity.Winner.Value : false)).ToList(),
                            GameEntity = entity,
                            State = entity.State switch {
                                GameState.Draft => "Borrador [No Iniciado]",
                                GameState.Started => "Iniciado [Jugando]",
                                GameState.Finished => $"Finalizado [Ganador: {entity.Winner.Value.Name}]",
                                _ => "Estado desconocido"
                            },
                            MasterBoard = BuildMasterBoardState(entity),
                            BallsPlayed = entity.BallsPlayed.Select(ball => new BallModel { 
                                                                    Letter = ball.Letter.ToString(), 
                                                                    Number = ball.Number, 
                                                                    Entity = ball })
                                                            .ToList()
            };

        private static Dictionary<string, List<BallModel>> BuildMasterBoardState(Game game)
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

        private static List<BallModel> GetBallsByLetter(Game game, BallLeter letter)
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
