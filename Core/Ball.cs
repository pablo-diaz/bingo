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
        private short _number;
        public BallLeter Letter { get; }
        public string Name { get => $"{this.Letter}{this._number}"; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Letter;
            yield return this._number;
        }

        private Ball(short number, BallLeter letter)
        {
            _number = number;
            Letter = letter;
        }

        public static Result<Ball> Create(short number, BallLeter letter)
        {
            if (number <= 0)
                return Result.Failure<Ball>("Wrong number value");

            return Result.Ok(new Ball(number, letter));
        }
    }
}