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

        public int PlayerCount => Players.Count;

        public string State => GameEntity.GameStatus switch {
            GameStatus.Draft => "Borrador [No Iniciado]",
            GameStatus.Playing => "Iniciado [Jugando]",
            GameStatus.Finished => $"Finalizado [Ganador: {GameEntity.Winner.GetValueOrThrow().Name}]",
            _ => "Estado desconocido"
        };

        public bool IsItADraftGame => GameEntity != null
                                      ? GameEntity.GameStatus == GameStatus.Draft
                                      : true;

        public List<PlayerModel> Players => GameEntity != null
                                            ? GameEntity.Players
                                                .Select(player => PlayerModel.FromEntity(player,
                                                                    isTheWinner: GameEntity.Winner.HasValue
                                                                                 ? player == GameEntity.Winner.GetValueOrThrow()
                                                                                 : false))
                                                .ToList()
                                            : new List<PlayerModel>();

        public Dictionary<string, List<BallModel>> MasterBoard => GameEntity != null
                                                                  ? BuildMasterBoardState(GameEntity)
                                                                  : new Dictionary<string, List<BallModel>>();

        public List<BallModel> BallsPlayed => GameEntity != null
                                              ? GameEntity.BallsPlayed.Select(ball => new BallModel {
                                                                               Letter = ball.Letter.ToString(),
                                                                               Number = ball.Number,
                                                                               Entity = ball
                                                                             })
                                                                      .ToList()
                                              : new List<BallModel>();

        public GameState GameEntity { get; }

        private GameModel(string name, GameType? gameType, GameState entity)
        {
            this.Name = name;
            this.GameType = gameType;
            this.GameEntity = entity;
        }

        public static GameModel CreateAsEmptyForNewGame() =>
            new GameModel(null, null, null);

        public static GameModel FromEntity(GameState entity) =>
            new GameModel(entity.Name, entity.GameType, entity);

        private static Dictionary<string, List<BallModel>> BuildMasterBoardState(GameState game) =>
            new Dictionary<string, List<BallModel>>() {
                { "B", GetBallsByLetter(game, BallLeter.B) },
                { "I", GetBallsByLetter(game, BallLeter.I) },
                { "N", GetBallsByLetter(game, BallLeter.N) },
                { "G", GetBallsByLetter(game, BallLeter.G) },
                { "O", GetBallsByLetter(game, BallLeter.O) }
            };

        private static List<BallModel> GetBallsByLetter(GameState game, BallLeter letter) =>
            game.BallsConfigured.Where(ball => ball.Letter == letter)
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
