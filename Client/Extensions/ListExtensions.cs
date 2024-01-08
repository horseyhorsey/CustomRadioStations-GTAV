using System;
using System.Collections.Generic;

namespace CustomRadioStations.Extensions
{
    public static class ListExtensions
    {
        public static void ShuffleMe<T>(this IList<T> list)
        {
            Random random = new Random();
            int n = list.Count;

            for (int i = list.Count - 1; i > 1; i--)
            {
                int rnd = random.Next(i + 1);

                T value = list[rnd];
                list[rnd] = list[i];
                list[i] = value;
            }
        }

        public static T GetNext<T>(this List<T> list, T current)
        {
            var index = list.IndexOf(current);
            return index < list.Count - 1 ? list[index + 1] : list[0];
        }

        public static T GetPrevious<T>(this List<T> list, T current)
        {
            var index = list.IndexOf(current);
            return index > 0 ? list[index - 1] : list[list.Count - 1];
        }
    }
}
