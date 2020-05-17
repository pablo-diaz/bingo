using System;
using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public enum BoardState
    {
        Playing,
        Winner
    }

    public class Board
    {
        #region Properties

        public BoardState State { get; }

        private HashSet<Ball> _ballsConfigured { get; }
        public IReadOnlyCollection<Ball> BallsConfigured { get => this._ballsConfigured.ToList(); }

        private HashSet<Ball> _ballsPlayed { get; }
        public IReadOnlyCollection<Ball> BallsPlayed { get => this._ballsPlayed.ToList(); }

        #endregion

        #region Equality

        public override bool Equals(object obj)
        {
            var other = obj as Board;
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            
            return this._ballsConfigured.SetEquals(other._ballsConfigured);
        }

        public static bool operator ==(Board a, Board b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(Board a, Board b) => !(a == b);

        public override int GetHashCode()
        {
            int hashCode = _ballsConfigured.Count();
            foreach(var ball in _ballsConfigured)
                hashCode = unchecked(hashCode * 314159 + ball.Name.GetHashCode());
            return hashCode;
        }

        #endregion

        #region Constructors

        private Board(HashSet<Ball> ballsConfigured)
        {
            this.State = BoardState.Playing;
            this._ballsPlayed = new HashSet<Ball>();
            this._ballsConfigured = ballsConfigured;
        }

        #endregion

        #region Builders

        public static Result<Board> RandonmlyCreateFromBallSet(ISet<Ball> balls, int countPerColumn)
        {
            if (balls == null || balls.Count() == 0)
                return Result.Failure<Board>("You must provide a valid balls array");

            if(!AreAllRequiredBallsPresent(balls, countPerColumn))
                return Result.Failure<Board>("There are pending ball types that must be provided");

            HashSet<Ball> ballsToPlayWith = RandomlyCreateBallSet(balls, countPerColumn);
            return Result.Ok(new Board(ballsToPlayWith));
        }

        #endregion

        #region Helpers

        private static HashSet<Ball> RandomlyCreateBallSet(ISet<Ball> balls, int countPerColumn)
        {
            (var bColumn, var iColumn, var nColumn, var gColumn, var oColumn) = TransformIntoColumns(balls);
            
            var randomGenerator = new Random();
            var result = new HashSet<Ball>();

            Randomize(bColumn, countPerColumn, randomGenerator).ForEach(ball => result.Add(ball));
            Randomize(iColumn, countPerColumn, randomGenerator).ForEach(ball => result.Add(ball));
            Randomize(nColumn, countPerColumn, randomGenerator).ForEach(ball => result.Add(ball));
            Randomize(gColumn, countPerColumn, randomGenerator).ForEach(ball => result.Add(ball));
            Randomize(oColumn, countPerColumn, randomGenerator).ForEach(ball => result.Add(ball));

            return result;
        }

        private static List<Ball> Randomize(IList<Ball> balls, int expectedCount, Random randomGenerator, List<Ball> randomizedList = null)
        {
            if (randomizedList == null)
                randomizedList = new List<Ball>();

            if (randomizedList.Count() == expectedCount)
                return randomizedList;

            var randomIndex = randomGenerator.Next(Math.Min(balls.Count, expectedCount));
            var randomBall = balls[randomIndex];
            balls.RemoveAt(randomIndex);
            randomizedList.Add(randomBall);
            return Randomize(balls, expectedCount, randomGenerator, randomizedList);
        }

        private static bool AreAllRequiredBallsPresent(ISet<Ball> balls, int countPerColumn)
        {
            (var bColumn, var iColumn, var nColumn, var gColumn, var oColumn) = TransformIntoColumns(balls);
            return bColumn.Count() >= countPerColumn
                && iColumn.Count() >= countPerColumn
                && nColumn.Count() >= countPerColumn
                && gColumn.Count() >= countPerColumn
                && oColumn.Count() >= countPerColumn;
        }

        private static (IList<Ball> bColumn, IList<Ball> iColumn, IList<Ball> nColumn, IList<Ball> gColumn, IList<Ball> oColumn)
            TransformIntoColumns(ISet<Ball> balls) =>
            (balls.Where(ball => ball.Letter == BallLeter.B).ToList(),
             balls.Where(ball => ball.Letter == BallLeter.I).ToList(),
             balls.Where(ball => ball.Letter == BallLeter.N).ToList(),
             balls.Where(ball => ball.Letter == BallLeter.G).ToList(),
             balls.Where(ball => ball.Letter == BallLeter.O).ToList());

        #endregion
    }
}