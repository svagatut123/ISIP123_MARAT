using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISIP123_MARAT.game;

        public static class RandomGenerator
        {
            private static Random _random = new Random();
            private static object _lock = new object();

            public static int Next()
            {
                lock (_lock)
                {
                    return _random.Next();
                }
            }

            public static int Next(int maxValue)
            {
                lock (_lock)
                {
                    return _random.Next(maxValue);
                }
            }

            public static int Next(int minValue, int maxValue)
            {
                lock (_lock)
                {
                    return _random.Next(minValue, maxValue);
                }
            }

            public static double NextDouble()
            {
                lock (_lock)
                {
                    return _random.NextDouble();
                }
            }

            public static bool NextBool()
            {
                lock (_lock)
                {
                    return _random.Next(2) == 0;
                }
            }

            public static T GetRandomItem<T>(T[] array)
            {
                lock (_lock)
                {
                    return array[_random.Next(array.Length)];
                }
            }

            public static T GetRandomItem<T>(List<T> list)
            {
                lock (_lock)
                {
                    return list[_random.Next(list.Count)];
                }
            }
        }
    


