using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public static class GameServices
    {
        private static BallLeter[] ColumnsInGame = { BallLeter.B, BallLeter.I, BallLeter.N, BallLeter.G, BallLeter.O };

        public static Result<Game> CreateGame(string withName, GameType gameType,
            short withNBallsTotal, short withNBallsMaxPerColumn)
        {
            if (string.IsNullOrEmpty(withName))
                return Result.Failure<Game>("Name should be valid");

            if (!IsBallsCountEnoughForABingoGame(withNBallsTotal))
                return Result.Failure<Game>("Provide enough balls to play game");

            if (withNBallsMaxPerColumn <= 0)
                return Result.Failure<Game>("Provide a valid number for balls per column, so we can randomly create boards");

            if (!IsBallsCountEnoughToBeEvenlyDistributedInAllColumns(
                    ballsCount: withNBallsTotal, ballsPerColumn: withNBallsMaxPerColumn))
                return Result.Failure<Game>("Provide enough balls, so we can randomly distribute them in all columns");

            var ballSetResult = CreateBallsSet(withNBallsTotal);
            if(ballSetResult.IsFailure)
                return Result.Failure<Game>(ballSetResult.Error);

            return new DraftGame(withName, gameType, ballSetResult.Value, withNBallsMaxPerColumn);
        }

        #region Helpers

        private static bool IsBallsCountEnoughForABingoGame(int ballsCount) =>
                ballsCount > 0 && ballsCount % ColumnsInGame.Length == 0;

        private static bool IsBallsCountEnoughToBeEvenlyDistributedInAllColumns(int ballsCount, int ballsPerColumn) =>
            ballsPerColumn <= (ballsCount / ColumnsInGame.Length);

        private static Result<HashSet<Ball>> CreateBallsSet(short forNBalls)
        {
            var ballResultList = Enumerable.Range(1, forNBalls)
                .Select(number => CreateBall(ballNumber: (short)number, ballsCount: forNBalls))
                .ToList();

            if (ballResultList.Any(b => b.IsFailure))
                return Result.Failure<HashSet<Ball>>(ballResultList.First(b => b.IsFailure).Error);

            return new HashSet<Ball>(ballResultList.Select(b => b.Value));
        }

        private static Result<Ball> CreateBall(short ballNumber, short ballsCount)
        {
            var letter = GetBallLetterForMaxBallCount(ballNumber: ballNumber, ballsCount: ballsCount);
            return Ball.Create(letter, ballNumber);
        }

        private static BallLeter GetBallLetterForMaxBallCount(short ballNumber, short ballsCount)
        {
            var maxItemsPerColumn = ballsCount / ColumnsInGame.Length;
            var columnIndex = (ballNumber - 1) / maxItemsPerColumn;
            return ColumnsInGame[columnIndex];
        }

        #endregion
    }
}
