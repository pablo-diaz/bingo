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

        public BoardState State { get; private set; }

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

        internal static Result<Board> RandonmlyCreateFromBallSet(Random randomizer, 
            HashSet<Ball> balls, int countPerColumn, GameType gameType)
        {
            if (balls == null || balls.Count() == 0)
                return Result.Failure<Board>("You must provide a valid balls array");

            if(!AreAllRequiredBallsPresent(balls, countPerColumn))
                return Result.Failure<Board>("There are pending ball types that must be provided");

            var ballsToPlayWith = RandomlyCreateBallSet(randomizer, balls, countPerColumn);
            ballsToPlayWith = AdjustBallSetToGameType(gameType, randomizer, ballsToPlayWith);
            return Result.Ok(new Board(ballsToPlayWith));
        }

        #endregion

        #region Helpers

        private static HashSet<Ball> RandomlyCreateBallSet(Random randomGenerator, ISet<Ball> balls, int countPerColumn)
        {
            (var bColumn, var iColumn, var nColumn, var gColumn, var oColumn) = TransformIntoColumns(balls);
            
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

            var randomIndex = randomGenerator.Next(0, balls.Count - 1);
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

        private static HashSet<Ball> AdjustBallSetToGameType(GameType gameType, Random randomizer, 
                HashSet<Ball> originalBallSet) =>
            gameType switch { 
                GameType.STANDARD => RandomlyRemoveBallFromNColumn(randomizer, originalBallSet),
                GameType.T => RandomlyRemoveBallsForTStructure(randomizer, originalBallSet),
                GameType.L => RandomlyRemoveBallsForLStructure(randomizer, originalBallSet),
                GameType.O => RandomlyRemoveBallsForOStructure(randomizer, originalBallSet),
                GameType.X => RandomlyRemoveBallsForXStructure(randomizer, originalBallSet),
                _ => throw new ApplicationException($"Unknown game type '{gameType}'")
            };

        private static HashSet<Ball> RandomlyRemoveBallFromNColumn(Random randomizer, HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(randomizer, new Dictionary<BallLeter, int> {
                { BallLeter.B, 5 },
                { BallLeter.I, 5 },
                { BallLeter.N, 4 },
                { BallLeter.G, 5 },
                { BallLeter.O, 5 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForTStructure(Random randomizer, HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(randomizer, new Dictionary<BallLeter, int> {
                { BallLeter.B, 1 },
                { BallLeter.I, 1 },
                { BallLeter.N, 5 },
                { BallLeter.G, 1 },
                { BallLeter.O, 1 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForLStructure(Random randomizer, HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(randomizer, new Dictionary<BallLeter, int> {
                { BallLeter.B, 5 },
                { BallLeter.I, 1 },
                { BallLeter.N, 1 },
                { BallLeter.G, 1 },
                { BallLeter.O, 1 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForOStructure(Random randomizer, HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(randomizer, new Dictionary<BallLeter, int> {
                { BallLeter.B, 5 },
                { BallLeter.I, 2 },
                { BallLeter.N, 2 },
                { BallLeter.G, 2 },
                { BallLeter.O, 5 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForXStructure(Random randomizer, HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(randomizer, new Dictionary<BallLeter, int> {
                { BallLeter.B, 2 },
                { BallLeter.I, 2 },
                { BallLeter.N, 1 },
                { BallLeter.G, 2 },
                { BallLeter.O, 2 },
            });

        #endregion

        #region Internal methods

        internal void PlayBall(Ball ballToPlay)
        {
            if (!this._ballsConfigured.Contains(ballToPlay))
                return;

            this._ballsPlayed.Add(ballToPlay);
            if (this._ballsPlayed.SetEquals(this._ballsConfigured))
                this.State = BoardState.Winner;
        }

        #endregion
    }

    internal static class BallSetExtensions
    {
        public static List<Ball> RandomlyRemoveBallsUntilDesiredCount(this List<Ball> from, Random randomizer, 
            int desiredCount)
        {
            if (from.Count == desiredCount)
                return from;

            var randomIndex = randomizer.Next(0, from.Count - 1);
            from.RemoveAt(randomIndex);
            return from.RandomlyRemoveBallsUntilDesiredCount(randomizer, desiredCount);
        }

        public static HashSet<Ball> AdjustBallsToDesiredCount(this HashSet<Ball> balls, Random randomizer,
            Dictionary<BallLeter, int> desiredCountPerColumn)
        {
            var fullBallSet = new List<Ball>();
            foreach (var kvp in desiredCountPerColumn)
            {
                var set = balls.Where(ball => ball.Letter == kvp.Key)
                               .ToList()
                               .RandomlyRemoveBallsUntilDesiredCount(randomizer, desiredCount: kvp.Value);

                fullBallSet.AddRange(set);
            }

            return new HashSet<Ball>(fullBallSet);
        }
    }
}