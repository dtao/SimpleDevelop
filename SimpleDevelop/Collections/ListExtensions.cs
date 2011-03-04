using System.Collections.Generic;

using SimpleDevelop.Core;

namespace SimpleDevelop.Collections
{
    public static class ListExtensions
    {
        public static int BinarySearch<T>(this IList<T> list, int index, int length, T value, IComparer<T> comparer = null)
        {
            list.ThrowIfNull("list");
            index.ThrowIfOutside(0, list.Count - length + 1, "index");
            length.ThrowIfOutside(0, list.Count - index + 1, "length");
            
            comparer = comparer ?? Comparer<T>.Default;

            int lower = index;
            int upper = (index + length) - 1;

            while (lower <= upper)
            {
                int adjustedIndex = lower + ((upper - lower) >> 1);
                int comparison = comparer.Compare(list[adjustedIndex], value);
                if (comparison == 0)
                    return adjustedIndex;
                else if (comparison < 0)
                    lower = adjustedIndex + 1;
                else
                    upper = adjustedIndex - 1;
            }

            return lower;
        }

        public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T> comparer = null)
        {
            return list.BinarySearch(0, list.Count, value, comparer);
        }
    }
}
