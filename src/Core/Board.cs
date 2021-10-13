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

        internal static Result<Board> Create( 
            HashSet<Ball> withBallSet, int withNBallsPerColumn, GameType gameType)
        {
            if (withBallSet == null || withBallSet.Count() == 0)
                return Result.Failure<Board>("You must provide a valid balls array");

            if(!AreAllRequiredBallsPresent(withBallSet, withNBallsPerColumn))
                return Result.Failure<Board>("There are pending ball types that must be provided");

            var ballsToPlayWith = RandomlyCreateBallSet(withBallSet, withNBallsPerColumn);
            ballsToPlayWith = AdjustBallSetToGameType(gameType, ballsToPlayWith);
            return Result.Success(new Board(ballsToPlayWith));
        }

        #endregion

        #region Helpers

        private static HashSet<Ball> RandomlyCreateBallSet(ISet<Ball> balls, int countPerColumn)
        {
            (var bColumn, var iColumn, var nColumn, var gColumn, var oColumn) = TransformIntoColumns(balls);
            
            var result = new HashSet<Ball>();

            Randomize(bColumn, countPerColumn).ForEach(ball => result.Add(ball));
            Randomize(iColumn, countPerColumn).ForEach(ball => result.Add(ball));
            Randomize(nColumn, countPerColumn).ForEach(ball => result.Add(ball));
            Randomize(gColumn, countPerColumn).ForEach(ball => result.Add(ball));
            Randomize(oColumn, countPerColumn).ForEach(ball => result.Add(ball));

            return result;
        }

        private static List<Ball> Randomize(IList<Ball> balls, int expectedCount, List<Ball> randomizedList = null)
        {
            if (randomizedList == null)
                randomizedList = new List<Ball>();

            if (randomizedList.Count() == expectedCount)
                return randomizedList;

            var randomIndex = RandomizingUtilities.GetRandomValue(balls.Count);
            var randomBall = balls[randomIndex];
            balls.RemoveAt(randomIndex);
            randomizedList.Add(randomBall);

            return Randomize(balls, expectedCount, randomizedList);
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

        private static HashSet<Ball> AdjustBallSetToGameType(GameType gameType,
                HashSet<Ball> originalBallSet) =>
            gameType switch { 
                GameType.STANDARD => RandomlyRemoveBallFromNColumn(originalBallSet),
                GameType.T => RandomlyRemoveBallsForTStructure(originalBallSet),
                GameType.L => RandomlyRemoveBallsForLStructure(originalBallSet),
                GameType.O => RandomlyRemoveBallsForOStructure(originalBallSet),
                GameType.X => RandomlyRemoveBallsForXStructure(originalBallSet),
                _ => throw new ApplicationException($"Unknown game type '{gameType}'")
            };

        private static HashSet<Ball> RandomlyRemoveBallFromNColumn(HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(new Dictionary<BallLeter, int> {
                { BallLeter.B, 5 },
                { BallLeter.I, 5 },
                { BallLeter.N, 4 },
                { BallLeter.G, 5 },
                { BallLeter.O, 5 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForTStructure(HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(new Dictionary<BallLeter, int> {
                { BallLeter.B, 1 },
                { BallLeter.I, 1 },
                { BallLeter.N, 5 },
                { BallLeter.G, 1 },
                { BallLeter.O, 1 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForLStructure(HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(new Dictionary<BallLeter, int> {
                { BallLeter.B, 5 },
                { BallLeter.I, 1 },
                { BallLeter.N, 1 },
                { BallLeter.G, 1 },
                { BallLeter.O, 1 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForOStructure(HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(new Dictionary<BallLeter, int> {
                { BallLeter.B, 5 },
                { BallLeter.I, 2 },
                { BallLeter.N, 2 },
                { BallLeter.G, 2 },
                { BallLeter.O, 5 },
            });

        private static HashSet<Ball> RandomlyRemoveBallsForXStructure(HashSet<Ball> balls) =>
            balls.AdjustBallsToDesiredCount(new Dictionary<BallLeter, int> {
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
}