using System.Linq;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Core
{
    public enum BallLeter
    {
        B,I,N,G,O
    }

    public class Ball: ValueObject
    {
        public short Number { get; }
        public BallLeter Letter { get; }
        public string Name { get => $"{this.Letter}{this.Number}"; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Letter;
            yield return this.Number;
        }

        private Ball(short number, BallLeter letter)
        {
            Number = number;
            Letter = letter;
        }

        internal static Result<Ball> Create(BallLeter letter, short number)
        {
            if (number <= 0)
                return Result.Failure<Ball>("Wrong number value");

            return Result.Success(new Ball(number, letter));
        }
    }

    internal static class BallSetExtensions
    {
        public static List<Ball> RandomlyRemoveBallsUntilDesiredCount(this List<Ball> from,
            int desiredCount)
        {
            if (from.Count == desiredCount)
                return from;

            var randomIndex = RandomizingUtilities.GetRandomValue(from.Count);
            from.RemoveAt(randomIndex);
            return from.RandomlyRemoveBallsUntilDesiredCount(desiredCount);
        }

        public static HashSet<Ball> AdjustBallsToDesiredCount(this HashSet<Ball> balls,
            Dictionary<BallLeter, int> desiredCountPerColumn)
        {
            var fullBallSet = new List<Ball>();
            foreach (var kvp in desiredCountPerColumn)
            {
                var set = balls.Where(ball => ball.Letter == kvp.Key)
                               .ToList()
                               .RandomlyRemoveBallsUntilDesiredCount(desiredCount: kvp.Value);

                fullBallSet.AddRange(set);
            }

            return new HashSet<Ball>(fullBallSet);
        }
    }
}