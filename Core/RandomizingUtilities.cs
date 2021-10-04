using System;

namespace Core
{
    internal static class RandomizingUtilities
    {
        private static readonly Random _randomizer;

        static RandomizingUtilities()
        {
            _randomizer = new Random();
        }

        public static int GetRandomValue(int max) =>
            _randomizer.Next(0, max);
    }
}
