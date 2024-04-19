using System;

namespace MegaCorps.Core.Model
{
    public static class RandomHelper
    {
        static Random rnd = new Random();

        public static int Next()
        {
            return rnd.Next();
        }

        internal static int Next(int v)
        {
            return rnd.Next(v);
        }
    }
}