using System;
using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public class DraftGame
    {
        #region Properties

        public string Name { get; }
        public GameType GameType { get; }
        public short WithNBallsMaxPerBoardBucket { get; }

        private HashSet<Ball> _ballsConfigured;

        public IReadOnlyCollection<Ball> BallsConfigured { get => this._ballsConfigured.ToList(); }

        private HashSet<Player> _players;
        public IReadOnlyCollection<Player> Players { get => this._players.ToList(); }

        #endregion

        #region Constructors

        private DraftGame(string name, GameType gameType, HashSet<Ball> withBalls, short withNBallsMaxPerBoardBucket)
        {
            this.Name = name;
            this.GameType = gameType;
            this.WithNBallsMaxPerBoardBucket = withNBallsMaxPerBoardBucket;

            this._players = new HashSet<Player>();
            this._ballsConfigured = withBalls;
        }

        #endregion

        #region Builders

        public static Result<DraftGame> Create(string name, GameType gameType, short withNBallsTotal, short withNBallsMaxPerBoardBucket)
        {
            if (string.IsNullOrEmpty(name))
                return Result.Failure<DraftGame>("Name should be valid");

            if(withNBallsTotal <= 0 || withNBallsTotal % 5 != 0)
                return Result.Failure<DraftGame>("Provide enough balls to play game");

            if(withNBallsMaxPerBoardBucket <= 1)
                return Result.Failure<DraftGame>("Provide a valid number for balls per board bucket, so we can randomly create boards");

            if (withNBallsMaxPerBoardBucket > ((withNBallsTotal / 5) - 1))
                return Result.Failure<DraftGame>("Provide enough balls per board bucket, so we can randomly create boards");

            return Result.Ok(new DraftGame(name, gameType, CreateNBallsSet(withNBallsTotal), withNBallsMaxPerBoardBucket));
        }

        #endregion

        #region Public Methods

        public Result AddPlayer(Player newPlayer)
        {
            if (newPlayer == null)
                return Result.Failure("New player is null");

            if (this._players.Any(player => player.Name == newPlayer.Name))
                return Result.Failure("Game already contains the same player");

            this._players.Add(newPlayer);

            return Result.Ok();
        }

        public Result UpdatePlayer(Player playerToUpdate, Player newPlayerInfo)
        {
            if (playerToUpdate == null)
                return Result.Failure("Player is null");

            if (newPlayerInfo == null)
                return Result.Failure("New player info is null");

            if (!this._players.Any(player => player.Name == playerToUpdate.Name))
                return Result.Failure("Game does not contain player that is to be updated");

            var playerWithSameNameExists = this._players.Except(new Player[] { playerToUpdate })
                .Any(player => player.Name == newPlayerInfo.Name);

            if (playerWithSameNameExists)
                return Result.Failure("Game already contains another player with the same new info");

            playerToUpdate.CopyInfoFromPlayer(newPlayerInfo);
            
            return Result.Ok();
        }

        public Result RemovePlayer(Player playerToRemove)
        {
            if (playerToRemove == null)
                return Result.Failure("Player is null");

            if (!this._players.Any(player => player.Name == playerToRemove.Name))
                return Result.Failure("Player does not exist in Game");

            this._players.Remove(playerToRemove);

            return Result.Ok();
        }

        public Result<ActiveGame> Start()
        {
            if (this._players.Count() < 2)
                return Result.Failure<ActiveGame>("There should be at least 2 Players to start Game");

            if(this._players.Any(player => !player.Boards.Any()))
                return Result.Failure<ActiveGame>("There should be at least 1 board setup for each Player to start Game");

            var newActiveGame = new ActiveGame(this.Name, this.GameType, this.Players, this.BallsConfigured);
            return Result.Ok(newActiveGame);
        }

        public Result<Board> AddBoardToPlayer(Random randomizer, Player player)
        {
            if(!this._players.Any(p => p.Name == player.Name))
                return Result.Failure<Board>("Player is not part of Game");

            var newBoardResult = TryCreatingNewBoard(randomizer, tryUpToNTimes: 10, this.GameType);
            if (newBoardResult.IsFailure)
                return newBoardResult;

            player.AddBoard(newBoardResult.Value);

            return newBoardResult;
        }

        public Result RemoveBoardFromPlayer(Player player, Board board)
        {
            if (!this._players.Any(p => p.Name == player.Name))
                return Result.Failure<Board>("Player is not part of Game");

            return player.RemoveBoard(board);
        }

        #endregion

        #region Helpers

        private static HashSet<Ball> CreateNBallsSet(short nBalls)
        {
            var ballList = Enumerable.Range(1, nBalls)
                .Select(number =>
                {
                    var letter = GetBallLetterForMaxBallCount((short)number, nBalls);
                    var newBallResult = Ball.Create(letter, (short)number);
                    if (newBallResult.IsFailure)
                        throw new ApplicationException(newBallResult.Error);
                    return newBallResult.Value;
                })
                .ToList();

            return new HashSet<Ball>(ballList);
        }

        private static BallLeter GetBallLetterForMaxBallCount(short ballNumber, short maxBallCount)
        {
            var maxItemsPerBucket = maxBallCount / 5;
            var bucketIndex = (ballNumber - 1) / maxItemsPerBucket;
            return new BallLeter[] { BallLeter.B, BallLeter.I, BallLeter.N, BallLeter.G, BallLeter.O }[bucketIndex];
        }

        private Result<Board> TryCreatingNewBoard(Random randomizer, short tryUpToNTimes, GameType gameType)
        {
            var tryCount = 1;
            while(tryCount <= tryUpToNTimes)
            {
                var newBoardResult = Board.RandonmlyCreateFromBallSet(randomizer, this._ballsConfigured, 
                    this.WithNBallsMaxPerBoardBucket, gameType);
                if (newBoardResult.IsFailure)
                    return newBoardResult;

                if(!this._players.Any(otherPlayer => otherPlayer.Boards.Contains(newBoardResult.Value)))
                    return newBoardResult;

                tryCount++;
            }

            return Result.Failure<Board>("We were not able to randomly create a Board");
        }

        #endregion
    }
}
