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
}