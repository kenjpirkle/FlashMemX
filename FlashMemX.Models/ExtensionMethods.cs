using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FlashMemX.Core
{
    public static class ExtensionMethods
    {
        public static T FromBack<T>(this IList<T> list, int index)
        {
            return list[list.Count - 1 - index];
        }

        public static void Randomize<T>(this IList<T> list)
        {
            var rand = new Random();

            int j;

            for(int i = 0; i < list.Count; ++i)
            {
                j = rand.Next(i, list.Count);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public static byte[] GetBytes(this SQLiteDataReader dataReader, int index)
        {
            return (byte[])dataReader[index];
        }
    }
}
