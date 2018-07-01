using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CS380
{
    public static class Rand
    {
        private static ThreadLocal<Random> rand;
        private static int counter;
        private static object lockObj;

        static Rand()
        {
            counter = 0;
            lockObj = new object();

            rand = new ThreadLocal<Random>(() =>
            {
                lock (lockObj)
                {
                    counter++;
                    var seed = (int)(DateTime.Now.Ticks & (Int64)Int32.MaxValue) ^ Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId ^ counter;

                    return new Random(seed);
                }
            });
        }

        public static int NextInt(int max)
        {
            return rand.Value.Next(max);
        }


        public static int NextInt(int min, int max)
        {
            return rand.Value.Next(min, max);
        }

        public static double NextDouble()
        {
            return rand.Value.NextDouble();
        }

        public static double SampleNormalBoxMuller()
        {
            // http://en.wikipedia.org/wiki/Box-Muller_transform

            while (true)
            {
                var u = NextDouble() * 2.0 - 1.0;
                var v = NextDouble() * 2.0 - 1.0;

                var s = u * u + v * v;

                if (s <= 0 || s >= 1)
                {
                    // Invalid s. Try again.
                    continue;
                }

                return u * Math.Sqrt(-2.0 * (Math.Log(s) / s));
            }
        }

        public static double SampleNormal(double mean, double sd)
        {
            return SampleNormalBoxMuller() * sd + mean;
        }
    }
}
